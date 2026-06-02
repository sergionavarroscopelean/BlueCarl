using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonArchitect.Systems
{
    public class FloorCompletePopupUI : MonoBehaviour
    {
        private System.Action onConfirm;

        public void Initialize(int floor, int roomsPlaced, Vector3 worldPos, Camera cam, System.Action confirmCallback)
        {
            onConfirm = confirmCallback;
            CreatePopup(floor, roomsPlaced, worldPos, cam);
        }

        private void CreatePopup(int floor, int roomsPlaced, Vector3 worldPos, Camera cam)
        {
            var canvasGO = new GameObject("FloorCompleteCanvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = cam;
            canvas.sortingOrder = 100;

            var rt = canvasGO.GetComponent<RectTransform>();
            rt.position = worldPos;
            rt.sizeDelta = new Vector2(400, 300);
            rt.localScale = Vector3.one * 0.012f;

            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
            bgGO.AddComponent<CanvasRenderer>();
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.05f, 0.12f, 0.97f);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(canvasGO.transform, false);
            var titleRT = titleGO.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.05f, 0.7f);
            titleRT.anchorMax = new Vector2(0.95f, 0.95f);
            titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;
            titleGO.AddComponent<CanvasRenderer>();
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = $"PISO {floor} COMPLETADO";
            titleTMP.fontSize = 28;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = new Color(1f, 0.85f, 0.2f);
            titleTMP.alignment = TextAlignmentOptions.Center;

            var infoGO = new GameObject("Info");
            infoGO.transform.SetParent(canvasGO.transform, false);
            var infoRT = infoGO.AddComponent<RectTransform>();
            infoRT.anchorMin = new Vector2(0.1f, 0.4f);
            infoRT.anchorMax = new Vector2(0.9f, 0.7f);
            infoRT.offsetMin = infoRT.offsetMax = Vector2.zero;
            infoGO.AddComponent<CanvasRenderer>();
            var infoTMP = infoGO.AddComponent<TextMeshProUGUI>();
            infoTMP.text = $"Habitaciones exploradas: {roomsPlaced}\nDescendiendo al piso {floor + 1}...";
            infoTMP.fontSize = 16;
            infoTMP.color = Color.white;
            infoTMP.alignment = TextAlignmentOptions.Center;

            var btnGO = new GameObject("ContinueButton");
            btnGO.transform.SetParent(canvasGO.transform, false);
            var btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.25f, 0.08f);
            btnRT.anchorMax = new Vector2(0.75f, 0.3f);
            btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
            btnGO.AddComponent<CanvasRenderer>();
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.15f, 0.5f, 0.15f, 1f);

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(OnContinueClicked);

            var btnTextGO = new GameObject("Text");
            btnTextGO.transform.SetParent(btnGO.transform, false);
            var btnTextRT = btnTextGO.AddComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.offsetMin = btnTextRT.offsetMax = Vector2.zero;
            btnTextGO.AddComponent<CanvasRenderer>();
            var btnTMP = btnTextGO.AddComponent<TextMeshProUGUI>();
            btnTMP.text = "CONTINUAR";
            btnTMP.fontSize = 20;
            btnTMP.fontStyle = FontStyles.Bold;
            btnTMP.color = Color.white;
            btnTMP.alignment = TextAlignmentOptions.Center;
        }

        private void OnContinueClicked()
        {
            onConfirm?.Invoke();
        }
    }
}
