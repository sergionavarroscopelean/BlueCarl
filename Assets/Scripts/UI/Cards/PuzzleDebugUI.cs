using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonArchitect.Core;

namespace DungeonArchitect.Systems
{
    public class PuzzleDebugUI : MonoBehaviour
    {
        private Canvas canvas;
        private GameObject currentPuzzlePopup;
        private int currentChestIndex;
        private int currentRiddleIndex;

        private void Update()
        {
            if (!Input.GetKey(KeyCode.LeftShift)) return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
                ShowDebugPanel();
        }

        private void ShowDebugPanel()
        {
            if (canvas != null)
            {
                Destroy(canvas.gameObject);
                canvas = null;
                return;
            }

            var canvasGO = new GameObject("PuzzleDebugCanvas");
            canvasGO.transform.SetParent(transform);
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            var panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelRT = panelGO.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0f, 0.6f);
            panelRT.anchorMax = new Vector2(0.3f, 1f);
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;
            panelGO.AddComponent<CanvasRenderer>();
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            CreateLabel(panelGO.transform, "DEBUG - Puzzles",
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.98f), 20, Color.yellow);

            CreateLabel(panelGO.transform, $"Cofre Puzzle (Shift+F2)\nIndex: {currentChestIndex} / {ChestPuzzlePopupUI.PuzzleCount - 1}",
                new Vector2(0.05f, 0.62f), new Vector2(0.95f, 0.82f), 16, Color.white);

            CreateButton(panelGO.transform, "<", new Vector2(0.05f, 0.5f), new Vector2(0.25f, 0.6f), () => {
                currentChestIndex = Mathf.Max(0, currentChestIndex - 1);
                RefreshDebugPanel();
            });
            CreateButton(panelGO.transform, ">", new Vector2(0.28f, 0.5f), new Vector2(0.48f, 0.6f), () => {
                currentChestIndex = Mathf.Min(ChestPuzzlePopupUI.PuzzleCount - 1, currentChestIndex + 1);
                RefreshDebugPanel();
            });
            CreateButton(panelGO.transform, "Launch", new Vector2(0.52f, 0.5f), new Vector2(0.95f, 0.6f), () => {
                LaunchChestPuzzle(currentChestIndex);
            });

            CreateLabel(panelGO.transform, $"Riddle (Shift+F3)\nIndex: {currentRiddleIndex} / 39",
                new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.45f), 16, Color.white);

            CreateButton(panelGO.transform, "<", new Vector2(0.05f, 0.13f), new Vector2(0.25f, 0.23f), () => {
                currentRiddleIndex = Mathf.Max(0, currentRiddleIndex - 1);
                RefreshDebugPanel();
            });
            CreateButton(panelGO.transform, ">", new Vector2(0.28f, 0.13f), new Vector2(0.48f, 0.23f), () => {
                currentRiddleIndex = Mathf.Min(39, currentRiddleIndex + 1);
                RefreshDebugPanel();
            });
            CreateButton(panelGO.transform, "Launch", new Vector2(0.52f, 0.13f), new Vector2(0.95f, 0.23f), () => {
                LaunchRiddlePopup(currentRiddleIndex);
            });

            CreateButton(panelGO.transform, "Close", new Vector2(0.3f, 0.02f), new Vector2(0.7f, 0.11f), () => {
                Destroy(canvas.gameObject);
                canvas = null;
            });
        }

        private void RefreshDebugPanel()
        {
            if (canvas != null)
                Destroy(canvas.gameObject);
            canvas = null;
            ShowDebugPanel();
        }

        private void LaunchChestPuzzle(int index)
        {
            if (currentPuzzlePopup != null) Destroy(currentPuzzlePopup);
            if (canvas != null) { Destroy(canvas.gameObject); canvas = null; }

            var cam = Camera.main;
            var go = new GameObject("DebugChestPuzzle");
            var popup = go.AddComponent<ChestPuzzlePopupUI>();
            popup.Initialize(Vector3.zero, cam, OnDebugResolved, index);
            currentPuzzlePopup = go;
        }

        private void LaunchRiddlePopup(int index)
        {
            if (currentPuzzlePopup != null) Destroy(currentPuzzlePopup);
            if (canvas != null) { Destroy(canvas.gameObject); canvas = null; }

            var cam = Camera.main;
            var go = new GameObject("DebugRiddlePopup");
            var popup = go.AddComponent<RiddlePopupUI>();
            popup.Initialize(Vector3.zero, cam, OnDebugResolved, index);
            currentPuzzlePopup = go;
        }

        private void OnDebugResolved(bool success)
        {
            if (currentPuzzlePopup != null)
            {
                Destroy(currentPuzzlePopup);
                currentPuzzlePopup = null;
            }
            Debug.Log($"[PuzzleDebug] Result: {(success ? "SUCCESS" : "FAIL")}");
        }

        private void CreateLabel(Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, float size, Color color)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.AddComponent<CanvasRenderer>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;
        }

        private void CreateButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject($"Btn_{label}");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.AddComponent<CanvasRenderer>();

            var img = go.AddComponent<Image>();
            img.color = new Color(0.25f, 0.25f, 0.35f, 1f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(action);

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = textRT.offsetMax = Vector2.zero;
            textGO.AddComponent<CanvasRenderer>();
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 16;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }
}
