using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DungeonArchitect.EditorTools
{
    public class SceneSetup : EditorWindow
    {
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
