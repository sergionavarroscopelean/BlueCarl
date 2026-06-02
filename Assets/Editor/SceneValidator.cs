using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DungeonArchitect.Core;
using DungeonArchitect.Systems;
using DungeonArchitect.Data;
using DungeonArchitect.Utils;

namespace DungeonArchitect.EditorTools
{
    public class SceneValidator : EditorWindow
    {
        private Vector2 scrollPos;
        private string log = "";
        private int errors;
        private int fixes;

        [MenuItem("Dungeon Architect/Validate & Fix Scene")]
        public static void ShowWindow()
        {
            GetWindow<SceneValidator>("Scene Validator");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Validate & Fix Current Scene", GUILayout.Height(30)))
            {
                log = "";
                errors = 0;
                fixes = 0;
                ValidateCurrentScene();
                log += $"\n--- Done: {errors} issues found, {fixes} fixed ---\n";
            }

            if (GUILayout.Button("Validate & Fix ALL Game Scenes", GUILayout.Height(30)))
            {
                log = "";
                errors = 0;
                fixes = 0;
                ValidateAllScenes();
                log += $"\n--- Done: {errors} issues found, {fixes} fixed ---\n";
            }

            EditorGUILayout.Space();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.TextArea(log, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void ValidateAllScenes()
        {
            string[] scenePaths = new[]
            {
                "Assets/GameScene.unity",
                "Assets/GameplayScene.unity"
            };

            foreach (var path in scenePaths)
            {
                if (!System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, "..", path)))
                {
                    Log($"[SKIP] Scene not found: {path}");
                    continue;
                }
                Log($"\n=== Validating: {path} ===");
                var scene = EditorSceneManager.OpenScene(path);
                ValidateCurrentScene();
                EditorSceneManager.SaveScene(scene);
            }
        }

        private void ValidateCurrentScene()
        {
            ValidateGameManager();
            ValidateCamera();
            ValidateAudioManager();
            ValidateCanvas();
            ValidateEventSystem();
            ValidateDungeonGrid();
            ValidateGridInputHandler();
            ValidateGridVisualizer();
            ValidateDoorInteractionManager();
            ValidateBuildSettings();
            ValidateStartingRoom();
        }

        private void ValidateGameManager()
        {
            var gm = Object.FindObjectOfType<GameManager>();
            if (gm == null)
            {
                Error("GameManager not found in scene");
                return;
            }

            var so = new SerializedObject(gm);

            FixReference<ResourceManager>(so, "resourceManager", gm.gameObject);
            FixReference<DungeonGridManager>(so, "gridManager", gm.gameObject);
            FixReference<RoomDraftManager>(so, "draftManager", gm.gameObject);
            FixReference<DeckManager>(so, "deckManager", gm.gameObject);
            FixReference<CombatManager>(so, "combatManager", gm.gameObject);
            FixReference<StairManager>(so, "stairManager", gm.gameObject);
            FixReference<ProgressionManager>(so, "progressionManager", gm.gameObject);

            var menuCanvas = so.FindProperty("menuCanvas");
            if (menuCanvas != null && menuCanvas.objectReferenceValue == null)
            {
                var canvas = Object.FindObjectOfType<Canvas>(true);
                if (canvas != null)
                {
                    menuCanvas.objectReferenceValue = canvas.gameObject;
                    Fix("GameManager.menuCanvas -> " + canvas.gameObject.name);
                }
            }

            var selectedClass = so.FindProperty("selectedClass");
            if (selectedClass != null && selectedClass.objectReferenceValue == null)
            {
                var classAsset = FindAsset<ClassData>();
                if (classAsset != null)
                {
                    selectedClass.objectReferenceValue = classAsset;
                    Fix("GameManager.selectedClass -> " + classAsset.name);
                }
                else Error("GameManager.selectedClass is null and no ClassData found");
            }

            var debugStartRoom = so.FindProperty("debugStartingRoom");
            if (debugStartRoom != null && debugStartRoom.objectReferenceValue == null)
            {
                var roomAsset = AssetDatabase.LoadAssetAtPath<RoomData>("Assets/ScriptableObjects/Rooms/01_Escaleras_de_Entrada.asset");
                if (roomAsset != null)
                {
                    debugStartRoom.objectReferenceValue = roomAsset;
                    Fix("GameManager.debugStartingRoom -> " + roomAsset.name);
                }
            }

            so.ApplyModifiedProperties();

            var deck = gm.GetComponent<DeckManager>();
            if (deck != null)
            {
                var deckSO = new SerializedObject(deck);
                var srcDeck = deckSO.FindProperty("sourceDeck");
                if (srcDeck != null && srcDeck.objectReferenceValue == null)
                {
                    var deckAsset = AssetDatabase.LoadAssetAtPath<DeckData>("Assets/ScriptableObjects/Decks/Mazo_Inicial.asset");
                    if (deckAsset != null)
                    {
                        srcDeck.objectReferenceValue = deckAsset;
                        deckSO.ApplyModifiedProperties();
                        Fix("DeckManager.sourceDeck -> " + deckAsset.name);
                    }
                    else Error("DeckManager.sourceDeck is null and no DeckData found");
                }
                else deckSO.ApplyModifiedProperties();
            }
        }

        private void ValidateCamera()
        {
            var cam = Object.FindObjectOfType<Camera>();
            if (cam == null)
            {
                Error("No Camera found in scene");
                return;
            }

            var cc = cam.GetComponent<CameraController>();
            if (cc == null)
            {
                cc = cam.gameObject.AddComponent<CameraController>();
                Fix("Added CameraController to Main Camera");
            }

            var so = new SerializedObject(cc);
            var gridProp = so.FindProperty("gridManager");
            if (gridProp != null && gridProp.objectReferenceValue == null)
            {
                var grid = Object.FindObjectOfType<DungeonGridManager>();
                if (grid != null)
                {
                    gridProp.objectReferenceValue = grid;
                    Fix("CameraController.gridManager linked");
                }
            }
            so.ApplyModifiedProperties();

            if (!cam.orthographic)
            {
                cam.orthographic = true;
                cam.orthographicSize = 7f;
                Fix("Camera set to orthographic (size 7)");
            }
        }

        private void ValidateAudioManager()
        {
            var am = Object.FindObjectOfType<AudioManager>();
            if (am == null)
            {
                Error("AudioManager not found in scene");
                return;
            }

            var so = new SerializedObject(am);
            var musicProp = so.FindProperty("musicSource");
            var sfxProp = so.FindProperty("sfxSource");

            var sources = am.GetComponents<AudioSource>();
            if (sources.Length < 2)
            {
                while (am.GetComponents<AudioSource>().Length < 2)
                    am.gameObject.AddComponent<AudioSource>();
                sources = am.GetComponents<AudioSource>();
                Fix("Added AudioSource components to AudioManager");
            }

            if (musicProp != null && musicProp.objectReferenceValue == null)
            {
                sources[0].loop = true;
                musicProp.objectReferenceValue = sources[0];
                Fix("AudioManager.musicSource linked");
            }
            if (sfxProp != null && sfxProp.objectReferenceValue == null)
            {
                sfxProp.objectReferenceValue = sources.Length > 1 ? sources[1] : sources[0];
                Fix("AudioManager.sfxSource linked");
            }
            so.ApplyModifiedProperties();
        }

        private void ValidateCanvas()
        {
            var canvas = Object.FindObjectOfType<Canvas>(true);
            if (canvas == null)
            {
                var go = new GameObject("Canvas");
                canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                go.AddComponent<CanvasScaler>();
                go.AddComponent<GraphicRaycaster>();
                Fix("Created Canvas (Screen Space - Overlay)");
            }

            if (!canvas.gameObject.activeInHierarchy)
            {
                canvas.gameObject.SetActive(true);
                Fix("Canvas was inactive - activated");
            }
        }

        private void ValidateEventSystem()
        {
            var es = Object.FindObjectOfType<EventSystem>();
            if (es == null)
            {
                var go = new GameObject("EventSystem");
                go.AddComponent<EventSystem>();
                go.AddComponent<StandaloneInputModule>();
                Fix("Created EventSystem");
            }
        }

        private void ValidateDungeonGrid()
        {
            var grid = Object.FindObjectOfType<DungeonGridManager>();
            if (grid == null)
            {
                Error("DungeonGridManager not found in scene");
                return;
            }

            var so = new SerializedObject(grid);

            var gridParent = so.FindProperty("gridParent");
            if (gridParent != null && gridParent.objectReferenceValue == null)
            {
                var gv = Object.FindObjectOfType<GridVisualizer>();
                if (gv != null)
                {
                    gridParent.objectReferenceValue = gv.transform;
                    Fix("DungeonGridManager.gridParent -> DungeonGrid transform");
                }
                else
                {
                    var go = new GameObject("DungeonGrid");
                    go.AddComponent<GridVisualizer>();
                    gridParent.objectReferenceValue = go.transform;
                    Fix("Created DungeonGrid and linked as gridParent");
                }
            }

            var roomPrefab = so.FindProperty("roomPrefab");
            if (roomPrefab != null && roomPrefab.objectReferenceValue == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Rooms/RoomVisual.prefab");
                if (prefab != null)
                {
                    roomPrefab.objectReferenceValue = prefab;
                    Fix("DungeonGridManager.roomPrefab -> RoomVisual.prefab");
                }
                else Error("DungeonGridManager.roomPrefab is null and RoomVisual.prefab not found");
            }

            var spriteMapper = so.FindProperty("spriteMapper");
            if (spriteMapper != null && spriteMapper.objectReferenceValue == null)
            {
                var mapper = AssetDatabase.LoadAssetAtPath<RoomSpriteMapper>("Assets/ScriptableObjects/RoomSpriteMapper.asset");
                if (mapper != null)
                {
                    spriteMapper.objectReferenceValue = mapper;
                    Fix("DungeonGridManager.spriteMapper -> RoomSpriteMapper.asset");
                }
                else Error("DungeonGridManager.spriteMapper is null and no RoomSpriteMapper found");
            }

            var debugStart = so.FindProperty("debugStartRoom");
            if (debugStart != null && debugStart.objectReferenceValue == null)
            {
                var room = AssetDatabase.LoadAssetAtPath<RoomData>("Assets/ScriptableObjects/Rooms/01_Escaleras_de_Entrada.asset");
                if (room != null)
                {
                    debugStart.objectReferenceValue = room;
                    Fix("DungeonGridManager.debugStartRoom -> 01_Escaleras_de_Entrada");
                }
            }

            so.ApplyModifiedProperties();
        }

        private void ValidateGridInputHandler()
        {
            var handler = Object.FindObjectOfType<GridInputHandler>();
            if (handler == null)
            {
                Error("GridInputHandler not found in scene");
                return;
            }

            var so = new SerializedObject(handler);

            var gridProp = so.FindProperty("gridManager");
            if (gridProp != null && gridProp.objectReferenceValue == null)
            {
                var grid = Object.FindObjectOfType<DungeonGridManager>();
                if (grid != null)
                {
                    gridProp.objectReferenceValue = grid;
                    Fix("GridInputHandler.gridManager linked");
                }
            }

            var camProp = so.FindProperty("mainCamera");
            if (camProp != null && camProp.objectReferenceValue == null)
            {
                var cam = Object.FindObjectOfType<Camera>();
                if (cam != null)
                {
                    camProp.objectReferenceValue = cam;
                    Fix("GridInputHandler.mainCamera linked");
                }
            }

            var highlightProp = so.FindProperty("placementHighlightPrefab");
            if (highlightProp != null && highlightProp.objectReferenceValue == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Rooms/PlacementHighlight.prefab");
                if (prefab != null)
                {
                    highlightProp.objectReferenceValue = prefab;
                    Fix("GridInputHandler.placementHighlightPrefab -> PlacementHighlight.prefab");
                }
                else Error("GridInputHandler.placementHighlightPrefab is null and prefab not found");
            }

            so.ApplyModifiedProperties();
        }

        private void ValidateGridVisualizer()
        {
            var vis = Object.FindObjectOfType<GridVisualizer>();
            if (vis == null) return;

            var so = new SerializedObject(vis);
            var gridProp = so.FindProperty("gridManager");
            if (gridProp != null && gridProp.objectReferenceValue == null)
            {
                var grid = Object.FindObjectOfType<DungeonGridManager>();
                if (grid != null)
                {
                    gridProp.objectReferenceValue = grid;
                    Fix("GridVisualizer.gridManager linked");
                }
            }
            so.ApplyModifiedProperties();
        }

        private void ValidateDoorInteractionManager()
        {
            var dim = Object.FindObjectOfType<DoorInteractionManager>();
            if (dim == null)
            {
                var gm = Object.FindObjectOfType<GameManager>();
                if (gm != null)
                {
                    dim = gm.gameObject.AddComponent<DoorInteractionManager>();
                    Fix("Added DoorInteractionManager to GameManager");
                }
                else return;
            }

            var so = new SerializedObject(dim);

            var gridProp = so.FindProperty("gridManager");
            if (gridProp != null && gridProp.objectReferenceValue == null)
            {
                var grid = Object.FindObjectOfType<DungeonGridManager>();
                if (grid != null) { gridProp.objectReferenceValue = grid; Fix("DoorInteractionManager.gridManager linked"); }
            }

            var draftProp = so.FindProperty("draftManager");
            if (draftProp != null && draftProp.objectReferenceValue == null)
            {
                var draft = Object.FindObjectOfType<RoomDraftManager>();
                if (draft != null) { draftProp.objectReferenceValue = draft; Fix("DoorInteractionManager.draftManager linked"); }
            }

            var camProp = so.FindProperty("mainCamera");
            if (camProp != null && camProp.objectReferenceValue == null)
            {
                var cam = Object.FindObjectOfType<Camera>();
                if (cam != null) { camProp.objectReferenceValue = cam; Fix("DoorInteractionManager.mainCamera linked"); }
            }

            so.ApplyModifiedProperties();
        }

        private void ValidateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            bool hasGameplay = false;
            foreach (var s in scenes)
            {
                if (s.path.Contains("GameplayScene") || s.path.Contains("GameScene"))
                    hasGameplay = true;
            }

            if (!hasGameplay || scenes.Length == 0)
            {
                var newScenes = new EditorBuildSettingsScene[]
                {
                    new EditorBuildSettingsScene("Assets/GameplayScene.unity", true),
                    new EditorBuildSettingsScene("Assets/GameScene.unity", true),
                };
                EditorBuildSettings.scenes = newScenes;
                Fix("Build Settings updated: GameplayScene (index 0), GameScene (index 1)");
            }
        }

        private void ValidateStartingRoom()
        {
            var room = AssetDatabase.LoadAssetAtPath<RoomData>("Assets/ScriptableObjects/Rooms/01_Escaleras_de_Entrada.asset");
            if (room == null)
            {
                Error("Starting room asset (01_Escaleras_de_Entrada) not found!");
                return;
            }

            var so = new SerializedObject(room);
            var doors = so.FindProperty("doors");
            if (doors != null && doors.arraySize == 0)
            {
                Error("Starting room has NO doors! Adding South and East for connectivity.");
                doors.arraySize = 2;
                doors.GetArrayElementAtIndex(0).intValue = (int)Direction.South;
                doors.GetArrayElementAtIndex(1).intValue = (int)Direction.East;
                so.ApplyModifiedProperties();
                Fix("Added doors (South, East) to starting room");
            }
            else if (doors != null && doors.arraySize > 0)
            {
                string doorList = "";
                for (int i = 0; i < doors.arraySize; i++)
                    doorList += ((Direction)doors.GetArrayElementAtIndex(i).intValue).ToString() + " ";
                Log($"[OK] Starting room doors: {doorList.Trim()}");
            }

            var sprite = so.FindProperty("roomSprite");
            if (sprite != null && sprite.objectReferenceValue == null)
            {
                var mapper = AssetDatabase.LoadAssetAtPath<RoomSpriteMapper>("Assets/ScriptableObjects/RoomSpriteMapper.asset");
                if (mapper != null && mapper.entries.Count > 0 && mapper.entries[0].sprite != null)
                {
                    sprite.objectReferenceValue = mapper.entries[0].sprite;
                    so.ApplyModifiedProperties();
                    Fix("Starting room roomSprite assigned from RoomSpriteMapper");
                }
            }
        }

        private void FixReference<T>(SerializedObject so, string propertyName, GameObject owner) where T : Component
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) return;
            if (prop.objectReferenceValue != null) return;

            var comp = owner.GetComponent<T>();
            if (comp == null)
                comp = Object.FindObjectOfType<T>();

            if (comp != null)
            {
                prop.objectReferenceValue = comp;
                Fix($"{so.targetObject.GetType().Name}.{propertyName} -> {comp.GetType().Name}");
            }
            else
            {
                Error($"{so.targetObject.GetType().Name}.{propertyName} is null and no {typeof(T).Name} found");
            }
        }

        private T FindAsset<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return null;
        }

        private void Log(string msg) { log += msg + "\n"; }
        private void Error(string msg) { log += $"[ERROR] {msg}\n"; errors++; }
        private void Fix(string msg) { log += $"[FIXED] {msg}\n"; fixes++; }
    }
}
