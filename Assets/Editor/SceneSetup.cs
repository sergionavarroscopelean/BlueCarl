using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DungeonArchitect.Systems;
using DungeonArchitect.UI;

namespace DungeonArchitect.EditorTools
{
    public class SceneSetup : EditorWindow
    {
        [MenuItem("Dungeon Architect/Setup - Empty Slot Prefab")]
        public static void CreateEmptySlotPrefab()
        {
            var go = new GameObject("EmptySlotIndicator");
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<EmptySlotIndicator>();
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.9f, 1.9f);

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Rooms"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "Rooms");

            string path = "Assets/Prefabs/Rooms/EmptySlotIndicator.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            AssetDatabase.Refresh();
            Debug.Log($"EmptySlotIndicator prefab saved to {path}");
        }

        [MenuItem("Dungeon Architect/Setup - Valid Slots Manager")]
        public static void AddValidSlotsManager()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Rooms/EmptySlotIndicator.prefab");
            if (slotPrefab == null) { Debug.LogError("Run 'Setup - Empty Slot Prefab' first."); return; }

            var gridManager = Object.FindObjectOfType<DungeonGridManager>();
            if (gridManager == null) { Debug.LogError("DungeonGridManager not found in scene."); return; }

            var go = new GameObject("ValidSlotsManager");
            var vsm = go.AddComponent<ValidSlotsManager>();
            var so = new SerializedObject(vsm);
            so.FindProperty("gridManager").objectReferenceValue = gridManager;
            so.FindProperty("emptySlotPrefab").objectReferenceValue = slotPrefab;
            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("ValidSlotsManager added to scene.");
            Selection.activeGameObject = go;
        }

        [MenuItem("Dungeon Architect/Setup - Draft Panel UI")]
        public static void CreateDraftPanel()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var cgo = new GameObject("Canvas");
                canvas = cgo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                cgo.AddComponent<CanvasScaler>();
                cgo.AddComponent<GraphicRaycaster>();
            }

            // Panel root
            var panelRoot = new GameObject("DraftPanel");
            panelRoot.transform.SetParent(canvas.transform, false);
            var panelRT = panelRoot.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.05f, 0.02f);
            panelRT.anchorMax = new Vector2(0.95f, 0.42f);
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;
            var panelImg = panelRoot.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.05f, 0.1f, 0.93f);

            // Card container
            var container = new GameObject("CardContainer");
            container.transform.SetParent(panelRoot.transform, false);
            var containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.02f, 0.05f);
            containerRT.anchorMax = new Vector2(0.98f, 0.95f);
            containerRT.offsetMin = containerRT.offsetMax = Vector2.zero;
            var layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 24;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(12, 12, 12, 12);

            var cardPrefab = CreateRoomCardPrefab();

            var draftPanel = panelRoot.AddComponent<DraftPanelUI>();
            var dso = new SerializedObject(draftPanel);
            dso.FindProperty("cardContainer").objectReferenceValue = container.transform;
            dso.FindProperty("roomCardPrefab").objectReferenceValue = cardPrefab;
            dso.FindProperty("panelRoot").objectReferenceValue = panelRoot;
            dso.ApplyModifiedProperties();

            panelRoot.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("DraftPanel UI created and hidden. It shows automatically on draft.");
            Selection.activeGameObject = panelRoot;
        }

        private static GameObject CreateRoomCardPrefab()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

            string path = "Assets/Prefabs/UI/RoomCard.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var card = new GameObject("RoomCard");
            var cardRT = card.AddComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(200, 280);
            var bg = card.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.25f, 1f);
            card.AddComponent<RoomCardUI>();

            GameObject MakeChild(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
            {
                var c = new GameObject(name);
                c.transform.SetParent(parent, false);
                var rt = c.AddComponent<RectTransform>();
                rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                return c;
            }

            var border = MakeChild("RarityBorder", card.transform, new Vector2(-0.02f, -0.02f), new Vector2(1.02f, 1.02f));
            var borderImg = border.AddComponent<Image>(); borderImg.color = Color.gray;

            var icon = MakeChild("RoomIcon", card.transform, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.95f));
            icon.AddComponent<Image>();

            TextMeshProUGUI MakeTMP(string name, Transform parent, Vector2 aMin, Vector2 aMax, int size, Color col, string def)
            {
                var g = MakeChild(name, parent, aMin, aMax);
                var t = g.AddComponent<TextMeshProUGUI>();
                t.fontSize = size; t.color = col; t.alignment = TextAlignmentOptions.Center; t.text = def;
                return t;
            }

            var nameTMP   = MakeTMP("RoomName", card.transform, new Vector2(0.05f,0.42f), new Vector2(0.95f,0.57f), 14, Color.white,  "Room Name");
            var typeTMP   = MakeTMP("RoomType", card.transform, new Vector2(0.05f,0.32f), new Vector2(0.95f,0.43f), 11, new Color(0.7f,0.7f,0.7f), "Type");
            var costTMP   = MakeTMP("Cost",     card.transform, new Vector2(0.05f,0.22f), new Vector2(0.95f,0.33f), 11, Color.white,  "Free");
            var rewardTMP = MakeTMP("Reward",   card.transform, new Vector2(0.05f,0.12f), new Vector2(0.95f,0.23f), 10, new Color(0.6f,1f,0.6f), "");
            var doorsTMP  = MakeTMP("Doors",    card.transform, new Vector2(0.05f,0.02f), new Vector2(0.95f,0.13f), 10, new Color(0.8f,0.8f,0.5f), "N S");

            var cardUI = card.GetComponent<RoomCardUI>();
            var cso = new SerializedObject(cardUI);
            cso.FindProperty("cardBackground").objectReferenceValue  = bg;
            cso.FindProperty("roomIcon").objectReferenceValue        = icon.GetComponent<Image>();
            cso.FindProperty("roomNameText").objectReferenceValue    = nameTMP;
            cso.FindProperty("roomTypeText").objectReferenceValue    = typeTMP;
            cso.FindProperty("costText").objectReferenceValue        = costTMP;
            cso.FindProperty("rewardText").objectReferenceValue      = rewardTMP;
            cso.FindProperty("doorsText").objectReferenceValue       = doorsTMP;
            cso.FindProperty("rarityBorder").objectReferenceValue    = borderImg;
            cso.ApplyModifiedProperties();

            var prefab = PrefabUtility.SaveAsPrefabAsset(card, path);
            DestroyImmediate(card);
            return prefab;
        }
        [MenuItem("Dungeon Architect/Setup Gameplay Scene")]
        public static void SetupGameplayScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);

            // GameManager
            var gmGO = new GameObject("GameManager");
            gmGO.AddComponent<Core.GameManager>();
            gmGO.AddComponent<Systems.ResourceManager>();
            gmGO.AddComponent<Systems.DungeonGridManager>();
            gmGO.AddComponent<Systems.RoomDraftManager>();
            gmGO.AddComponent<Systems.DeckManager>();
            gmGO.AddComponent<Systems.CombatManager>();
            gmGO.AddComponent<Systems.StairManager>();
            gmGO.AddComponent<Systems.ProgressionManager>();
            gmGO.AddComponent<Systems.RoomResolver>();
            gmGO.AddComponent<Systems.RelicManager>();
            gmGO.AddComponent<Systems.ShopManager>();
            gmGO.AddComponent<Systems.EventManager>();
            gmGO.AddComponent<Systems.GridInputHandler>();

            // Grid Parent
            var gridParent = new GameObject("DungeonGrid");
            gridParent.AddComponent<Systems.GridVisualizer>();

            // Camera setup
            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 7f;
                cam.transform.position = new Vector3(0, 0, -10);
                cam.gameObject.AddComponent<Core.CameraController>();
            }

            // Audio
            var audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<Core.AudioManager>();
            var musicSource = audioGO.AddComponent<AudioSource>();
            musicSource.loop = true;
            audioGO.AddComponent<AudioSource>();

            // UI Canvas
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Save scene
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Gameplay.unity");
            Debug.Log("Gameplay scene created at Assets/Scenes/Gameplay.unity");
        }

        [MenuItem("Dungeon Architect/Setup Main Menu Scene")]
        public static void SetupMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);

            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var menuGO = new GameObject("MainMenu");
            menuGO.transform.SetParent(canvasGO.transform);
            menuGO.AddComponent<UI.MainMenuUI>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
            Debug.Log("Main Menu scene created at Assets/Scenes/MainMenu.unity");
        }
    }
}
