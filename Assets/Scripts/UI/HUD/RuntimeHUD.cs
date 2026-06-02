using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonArchitect.Core;
using DungeonArchitect.Systems;
using DungeonArchitect.Data;

namespace DungeonArchitect.UI
{
    public class RuntimeHUD : MonoBehaviour
    {
        private TextMeshProUGUI hpText;
        private TextMeshProUGUI timeText;
        private TextMeshProUGUI goldText;
        private TextMeshProUGUI keysText;
        private TextMeshProUGUI gemsText;
        private TextMeshProUGUI floorText;
        private TextMeshProUGUI roomsText;

        private ResourceManager resources;
        private Canvas hudCanvas;

        private void Start()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnStateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            UnsubscribeAll();
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.Exploring && hudCanvas == null)
                BuildHUD();

            if (hudCanvas != null)
                hudCanvas.gameObject.SetActive(state != GameState.MainMenu);
        }

        private void BuildHUD()
        {
            resources = GameManager.Instance.Resources;

            var canvasGO = new GameObject("HUDCanvas");
            canvasGO.transform.SetParent(transform);
            hudCanvas = canvasGO.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 95;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            var panelGO = new GameObject("HUDPanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelRT = panelGO.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0f, 1f);
            panelRT.anchorMax = new Vector2(1f, 1f);
            panelRT.pivot = new Vector2(0.5f, 1f);
            panelRT.anchoredPosition = Vector2.zero;
            panelRT.sizeDelta = new Vector2(0, 50);
            panelGO.AddComponent<CanvasRenderer>();
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.05f, 0.1f, 0.85f);
            panelImg.raycastTarget = false;

            var layout = panelGO.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 20;
            layout.padding = new RectOffset(20, 20, 8, 8);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            hpText = CreateLabel(panelGO.transform, "HP", new Color(0.9f, 0.3f, 0.3f));
            timeText = CreateLabel(panelGO.transform, "Time", new Color(0.3f, 0.7f, 0.9f));
            goldText = CreateLabel(panelGO.transform, "Gold", new Color(1f, 0.85f, 0.2f));
            keysText = CreateLabel(panelGO.transform, "Keys", new Color(0.7f, 0.7f, 0.7f));
            gemsText = CreateLabel(panelGO.transform, "Gems", new Color(0.6f, 0.3f, 0.9f));
            floorText = CreateLabel(panelGO.transform, "Floor", new Color(0.9f, 0.9f, 0.9f));
            roomsText = CreateLabel(panelGO.transform, "Rooms", new Color(0.6f, 0.8f, 0.6f));

            SubscribeAll();
            RefreshAll();
        }

        private TextMeshProUGUI CreateLabel(Transform parent, string prefix, Color color)
        {
            var go = new GameObject(prefix);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(140, 34);
            go.AddComponent<CanvasRenderer>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 18;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;
            tmp.text = $"{prefix}: --";
            return tmp;
        }

        private void SubscribeAll()
        {
            if (resources == null) return;
            resources.OnHPChanged += UpdateHP;
            resources.OnTimeChanged += UpdateTime;
            resources.OnGoldChanged += UpdateGold;
            resources.OnKeysChanged += UpdateKeys;
            resources.OnGemsChanged += UpdateGems;
            GameManager.Instance.OnFloorChanged += UpdateFloor;
        }

        private void UnsubscribeAll()
        {
            if (resources != null)
            {
                resources.OnHPChanged -= UpdateHP;
                resources.OnTimeChanged -= UpdateTime;
                resources.OnGoldChanged -= UpdateGold;
                resources.OnKeysChanged -= UpdateKeys;
                resources.OnGemsChanged -= UpdateGems;
            }
            if (GameManager.Instance != null)
                GameManager.Instance.OnFloorChanged -= UpdateFloor;
        }

        private void RefreshAll()
        {
            if (resources == null) return;
            UpdateHP(resources.CurrentHP, resources.MaxHP);
            UpdateTime(resources.CurrentTime, resources.MaxTime);
            UpdateGold(resources.Gold);
            UpdateKeys(resources.Keys);
            UpdateGems(resources.Gems);
            UpdateFloor(GameManager.Instance.CurrentFloor);
            UpdateRooms();
        }

        private void LateUpdate()
        {
            if (resources != null && roomsText != null)
                UpdateRooms();
        }

        private void UpdateHP(int current, int max) { if (hpText != null) hpText.text = $"HP: {current}/{max}"; }
        private void UpdateTime(int current, int max) { if (timeText != null) timeText.text = $"Time: {current}/{max}"; }
        private void UpdateGold(int amount) { if (goldText != null) goldText.text = $"Gold: {amount}"; }
        private void UpdateKeys(int amount) { if (keysText != null) keysText.text = $"Keys: {amount}"; }
        private void UpdateGems(int amount) { if (gemsText != null) gemsText.text = $"Gems: {amount}"; }
        private void UpdateFloor(int floor) { if (floorText != null) floorText.text = $"Floor: {floor}"; }
        private void UpdateRooms() { if (roomsText != null) roomsText.text = $"Rooms: {GameManager.Instance.RoomsPlacedThisFloor}"; }
    }
}
