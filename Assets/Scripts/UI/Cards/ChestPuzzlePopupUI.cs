using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonArchitect.Systems
{
    public class ChestPuzzlePopupUI : MonoBehaviour
    {
        private Canvas canvas;
        private System.Action<bool> onResolved;
        private GameObject panelGO;
        private int correctChest;
        private int puzzleIndex;

        private static readonly PuzzleData[] puzzles =
        {
            new PuzzleData(0,
                "El tesoro esta en este cofre.",
                "El tesoro NO esta en el cofre 1.",
                "El cofre 2 miente."),
            new PuzzleData(1,
                "Este cofre esta vacio.",
                "El tesoro esta aqui.",
                "El cofre 1 dice la verdad."),
            new PuzzleData(2,
                "El tesoro no esta en el cofre 2.",
                "El tesoro no esta en el cofre 3.",
                "El tesoro esta en este cofre."),
            new PuzzleData(0,
                "Al menos una de las otras inscripciones miente.",
                "Este cofre esta vacio.",
                "El cofre 1 miente."),
            new PuzzleData(1,
                "El cofre 3 dice la verdad.",
                "Exactamente una inscripcion es verdadera.",
                "El tesoro esta en el cofre 1."),
            new PuzzleData(2,
                "El tesoro no esta aqui.",
                "El tesoro no esta aqui.",
                "Exactamente una de estas frases es falsa."),
            new PuzzleData(0,
                "El tesoro esta en un cofre con numero impar.",
                "Este cofre esta vacio.",
                "El cofre 1 miente."),
            new PuzzleData(1,
                "Todas las inscripciones son falsas.",
                "El tesoro no esta en el cofre 3.",
                "El tesoro esta en el cofre 1."),
            new PuzzleData(2,
                "El cofre 2 dice la verdad.",
                "El tesoro no esta en este cofre.",
                "El cofre 1 miente."),
            new PuzzleData(0,
                "Exactamente dos inscripciones son verdaderas.",
                "El tesoro esta en el cofre 3.",
                "El cofre 2 miente."),
            new PuzzleData(1,
                "El tesoro no esta en este cofre.",
                "El cofre 3 miente.",
                "El tesoro esta en el cofre 1."),
            new PuzzleData(2,
                "Este cofre esta vacio.",
                "Este cofre tambien esta vacio.",
                "Los otros dos dicen la verdad."),
            new PuzzleData(0,
                "Solo una inscripcion es verdadera.",
                "El tesoro no esta en el cofre 1.",
                "El tesoro esta en el cofre 2."),
            new PuzzleData(1,
                "El cofre 2 y el 3 no pueden ambos decir la verdad.",
                "El tesoro no esta en el cofre 3.",
                "El tesoro no esta en el cofre 2."),
            new PuzzleData(2,
                "El tesoro esta en el cofre 2.",
                "El cofre 1 dice la verdad.",
                "Los otros dos cofres mienten."),
            new PuzzleData(0,
                "Ninguna inscripcion es falsa.",
                "El tesoro no esta en el cofre 3.",
                "El tesoro no esta en el cofre 2."),
            new PuzzleData(1,
                "Este cofre no tiene nada.",
                "El cofre 1 dice la verdad.",
                "El cofre 1 dice mentira."),
            new PuzzleData(2,
                "El cofre 2 tiene el tesoro.",
                "El cofre 1 miente.",
                "El cofre 1 dice la verdad."),
            new PuzzleData(0,
                "Al menos dos inscripciones son verdaderas.",
                "El tesoro no esta en el cofre 3.",
                "El cofre 2 miente."),
            new PuzzleData(1,
                "El tesoro no esta aqui.",
                "Todas las inscripciones son verdaderas.",
                "El cofre 2 miente."),
        };

        public static int PuzzleCount => puzzles.Length;

        public void Initialize(Vector3 worldPos, Camera cam, System.Action<bool> callback, int forcePuzzleIndex = -1)
        {
            onResolved = callback;
            puzzleIndex = forcePuzzleIndex >= 0 && forcePuzzleIndex < puzzles.Length
                ? forcePuzzleIndex
                : Random.Range(0, puzzles.Length);

            var puzzle = puzzles[puzzleIndex];
            correctChest = puzzle.correctChest;

            CreateCanvas();
            CreateContent(puzzle);
        }

        private void CreateCanvas()
        {
            var canvasGO = new GameObject("ChestPuzzleCanvas");
            canvasGO.transform.SetParent(transform);
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 95;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
            bgGO.AddComponent<CanvasRenderer>();
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.02f, 0.02f, 0.05f, 0.88f);
            bgImg.raycastTarget = true;
        }

        private void CreateContent(PuzzleData puzzle)
        {
            panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(canvas.transform, false);
            var panelRT = panelGO.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.08f, 0.1f);
            panelRT.anchorMax = new Vector2(0.92f, 0.9f);
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;
            panelGO.AddComponent<CanvasRenderer>();
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.06f, 0.05f, 0.1f, 0.95f);

            var titleGO = CreateText(panelGO.transform,
                "El Santuario revela tres cofres...",
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f),
                36, new Color(0.85f, 0.7f, 0.2f), FontStyles.Bold);

            var hintGO = CreateText(panelGO.transform,
                "Cada cofre tiene una inscripcion. Solo uno contiene el tesoro.\nAlguna inscripcion puede mentir...",
                new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.88f),
                22, new Color(0.7f, 0.7f, 0.7f), FontStyles.Italic);

            float chestWidth = 0.28f;
            float spacing = (1f - 3f * chestWidth) / 4f;

            for (int i = 0; i < 3; i++)
            {
                float xMin = spacing + i * (chestWidth + spacing);
                float xMax = xMin + chestWidth;

                CreateChest(panelGO.transform, i, puzzle.inscriptions[i], xMin, xMax);
            }
        }

        private void CreateChest(Transform parent, int index, string inscription, float xMin, float xMax)
        {
            var chestGO = new GameObject($"Chest_{index}");
            chestGO.transform.SetParent(parent, false);
            var chestRT = chestGO.AddComponent<RectTransform>();
            chestRT.anchorMin = new Vector2(xMin, 0.12f);
            chestRT.anchorMax = new Vector2(xMax, 0.76f);
            chestRT.offsetMin = chestRT.offsetMax = Vector2.zero;
            chestGO.AddComponent<CanvasRenderer>();
            var chestImg = chestGO.AddComponent<Image>();
            chestImg.color = new Color(0.12f, 0.1f, 0.18f, 1f);

            var btn = chestGO.AddComponent<Button>();
            btn.targetGraphic = chestImg;
            var colors = btn.colors;
            colors.normalColor = new Color(0.12f, 0.1f, 0.18f, 1f);
            colors.highlightedColor = new Color(0.2f, 0.17f, 0.3f, 1f);
            colors.pressedColor = new Color(0.25f, 0.2f, 0.35f, 1f);
            btn.colors = colors;

            int captured = index;
            btn.onClick.AddListener(() => OnChestClicked(captured));

            var borderGO = new GameObject("Border");
            borderGO.transform.SetParent(chestGO.transform, false);
            var borderRT = borderGO.AddComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = new Vector2(-2f, -2f);
            borderRT.offsetMax = new Vector2(2f, 2f);
            borderGO.AddComponent<CanvasRenderer>();
            var borderImg = borderGO.AddComponent<Image>();
            borderImg.color = new Color(0.5f, 0.4f, 0.2f, 0.8f);
            borderImg.raycastTarget = false;
            borderGO.transform.SetAsFirstSibling();

            CreateText(chestGO.transform,
                $"Cofre {index + 1}",
                new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.97f),
                26, new Color(0.9f, 0.8f, 0.4f), FontStyles.Bold);

            var iconGO = new GameObject("ChestIcon");
            iconGO.transform.SetParent(chestGO.transform, false);
            var iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.25f, 0.55f);
            iconRT.anchorMax = new Vector2(0.75f, 0.85f);
            iconRT.offsetMin = iconRT.offsetMax = Vector2.zero;
            iconGO.AddComponent<CanvasRenderer>();
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.color = new Color(0.6f, 0.45f, 0.15f, 1f);
            iconImg.raycastTarget = false;

            CreateText(chestGO.transform,
                $"\"{inscription}\"",
                new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.55f),
                20, new Color(0.9f, 0.9f, 0.85f), FontStyles.Italic);
        }

        private void OnChestClicked(int index)
        {
            bool correct = index == correctChest;
            Destroy(panelGO);
            ShowResultPanel(correct, index);
        }

        private void ShowResultPanel(bool correct, int chosenIndex)
        {
            var resultGO = new GameObject("ResultPanel");
            resultGO.transform.SetParent(canvas.transform, false);
            var resultRT = resultGO.AddComponent<RectTransform>();
            resultRT.anchorMin = new Vector2(0.2f, 0.2f);
            resultRT.anchorMax = new Vector2(0.8f, 0.8f);
            resultRT.offsetMin = resultRT.offsetMax = Vector2.zero;
            resultGO.AddComponent<CanvasRenderer>();
            var resultImg = resultGO.AddComponent<Image>();
            resultImg.color = correct
                ? new Color(0.05f, 0.15f, 0.05f, 0.95f)
                : new Color(0.2f, 0.05f, 0.05f, 0.95f);

            CreateText(resultGO.transform,
                correct ? "Has elegido bien!" : "El cofre estaba vacio...",
                new Vector2(0.05f, 0.7f), new Vector2(0.95f, 0.9f),
                36, correct ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.4f, 0.4f), FontStyles.Bold);

            string flavor = correct
                ? "El cofre se abre revelando riquezas ancestrales."
                : $"El tesoro estaba en el cofre {correctChest + 1}.";

            CreateText(resultGO.transform,
                flavor,
                new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.68f),
                22, new Color(0.85f, 0.85f, 0.85f), FontStyles.Normal);

            string rewardText = correct
                ? "<color=#FFD700>+8 Oro</color>  <color=#66FF66>+25 XP</color>  <color=#88CCFF>+1 Gema</color>"
                : "<color=#FF6666>-5 HP</color>";

            CreateText(resultGO.transform,
                rewardText,
                new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.5f),
                26, Color.white, FontStyles.Normal, true);

            var btnGO = new GameObject("ContinueBtn");
            btnGO.transform.SetParent(resultGO.transform, false);
            var btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.25f, 0.08f);
            btnRT.anchorMax = new Vector2(0.75f, 0.24f);
            btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
            btnGO.AddComponent<CanvasRenderer>();
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.2f, 0.3f, 1f);

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var btnColors = btn.colors;
            btnColors.highlightedColor = new Color(0.35f, 0.35f, 0.5f, 1f);
            btn.colors = btnColors;

            bool capturedCorrect = correct;
            btn.onClick.AddListener(() => onResolved?.Invoke(capturedCorrect));

            CreateText(btnGO.transform,
                "Continuar",
                Vector2.zero, Vector2.one,
                24, Color.white, FontStyles.Normal);
        }

        private GameObject CreateText(Transform parent, string text,
            Vector2 anchorMin, Vector2 anchorMax,
            float fontSize, Color color, FontStyles style, bool richText = false)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.AddComponent<CanvasRenderer>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            tmp.richText = richText || text.Contains("<color");
            tmp.raycastTarget = false;
            return go;
        }

        private struct PuzzleData
        {
            public int correctChest;
            public string[] inscriptions;

            public PuzzleData(int correct, string chest1, string chest2, string chest3)
            {
                correctChest = correct;
                inscriptions = new[] { chest1, chest2, chest3 };
            }
        }
    }
}
