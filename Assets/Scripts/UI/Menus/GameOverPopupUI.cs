using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace DungeonArchitect.Systems
{
    public class GameOverPopupUI : MonoBehaviour
    {
        public void Initialize(string reason, int floor, int roomsPlaced, Vector3 worldPos, Camera cam)
        {
            var canvasGO = new GameObject("GameOverCanvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

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
            bgImg.color = new Color(0.12f, 0.02f, 0.02f, 0.97f);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(canvasGO.transform, false);
            var titleRT = titleGO.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.05f, 0.72f);
            titleRT.anchorMax = new Vector2(0.95f, 0.95f);
            titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;
            titleGO.AddComponent<CanvasRenderer>();
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "GAME OVER";
            titleTMP.fontSize = 52;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = new Color(0.9f, 0.2f, 0.2f);
            titleTMP.alignment = TextAlignmentOptions.Center;

            var reasonGO = new GameObject("Reason");
            reasonGO.transform.SetParent(canvasGO.transform, false);
            var reasonRT = reasonGO.AddComponent<RectTransform>();
            reasonRT.anchorMin = new Vector2(0.1f, 0.5f);
            reasonRT.anchorMax = new Vector2(0.9f, 0.72f);
            reasonRT.offsetMin = reasonRT.offsetMax = Vector2.zero;
            reasonGO.AddComponent<CanvasRenderer>();
            var reasonTMP = reasonGO.AddComponent<TextMeshProUGUI>();
            reasonTMP.text = reason;
            reasonTMP.fontSize = 24;
            reasonTMP.color = new Color(1f, 0.7f, 0.7f);
            reasonTMP.alignment = TextAlignmentOptions.Center;

            var statsGO = new GameObject("Stats");
            statsGO.transform.SetParent(canvasGO.transform, false);
            var statsRT = statsGO.AddComponent<RectTransform>();
            statsRT.anchorMin = new Vector2(0.1f, 0.3f);
            statsRT.anchorMax = new Vector2(0.9f, 0.5f);
            statsRT.offsetMin = statsRT.offsetMax = Vector2.zero;
            statsGO.AddComponent<CanvasRenderer>();
            var statsTMP = statsGO.AddComponent<TextMeshProUGUI>();
            statsTMP.text = $"Piso alcanzado: {floor}\nHabitaciones exploradas: {roomsPlaced}";
            statsTMP.fontSize = 20;
            statsTMP.color = Color.white;
            statsTMP.alignment = TextAlignmentOptions.Center;

            var btnGO = new GameObject("RetryButton");
            btnGO.transform.SetParent(canvasGO.transform, false);
            var btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.25f, 0.06f);
            btnRT.anchorMax = new Vector2(0.75f, 0.25f);
            btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
            btnGO.AddComponent<CanvasRenderer>();
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.5f, 0.15f, 0.15f, 1f);

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(OnRetryClicked);

            var btnTextGO = new GameObject("Text");
            btnTextGO.transform.SetParent(btnGO.transform, false);
            var btnTextRT = btnTextGO.AddComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.offsetMin = btnTextRT.offsetMax = Vector2.zero;
            btnTextGO.AddComponent<CanvasRenderer>();
            var btnTMP = btnTextGO.AddComponent<TextMeshProUGUI>();
            btnTMP.text = "REINTENTAR";
            btnTMP.fontSize = 24;
            btnTMP.fontStyle = FontStyles.Bold;
            btnTMP.color = Color.white;
            btnTMP.alignment = TextAlignmentOptions.Center;
        }

        private void OnRetryClicked()
        {
            PlayerBuffs.Reset();

            if (Core.GameManager.Instance != null)
                Destroy(Core.GameManager.Instance.gameObject);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
