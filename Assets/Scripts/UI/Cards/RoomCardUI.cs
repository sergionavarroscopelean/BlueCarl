using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DungeonArchitect.Data;

namespace DungeonArchitect.UI
{
    public class RoomCardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Card Elements")]
        [SerializeField] private Image cardBackground;
        [SerializeField] private Image roomIcon;
        [SerializeField] private TextMeshProUGUI roomNameText;
        [SerializeField] private TextMeshProUGUI roomTypeText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private TextMeshProUGUI doorsText;
        [SerializeField] private Image rarityBorder;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = Color.gray;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = new Color(0.6f, 0f, 0.8f);
        [SerializeField] private Color legendaryColor = new Color(1f, 0.84f, 0f);

        [Header("Hover")]
        [SerializeField] private float hoverScale = 1.1f;

        private RoomData roomData;
        private bool isSelected;
        private Vector3 originalScale;

        public RoomData Data => roomData;
        public event System.Action<RoomCardUI> OnCardClicked;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        public void Setup(RoomData data)
        {
            roomData = data;
            isSelected = false;

            if (roomNameText != null) roomNameText.text = data.roomName;
            if (roomTypeText != null) roomTypeText.text = data.roomType.ToString();
            if (costText != null) costText.text = data.gemCost > 0 ? $"{data.gemCost} Gems" : "Free";
            if (roomIcon != null && data.cardSprite != null) roomIcon.sprite = data.cardSprite;

            UpdateRewardText(data);
            UpdateDoorsText(data);
            UpdateRarityVisual(data.rarity);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (cardBackground != null)
                cardBackground.color = selected ? Color.yellow : Color.white;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnCardClicked?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.localScale = originalScale * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = originalScale;
        }

        private void UpdateRewardText(RoomData data)
        {
            if (rewardText == null) return;

            var rewards = new System.Text.StringBuilder();
            if (data.goldReward > 0) rewards.Append($"+{data.goldReward}G ");
            if (data.xpReward > 0) rewards.Append($"+{data.xpReward}XP ");
            if (data.keyReward > 0) rewards.Append($"+{data.keyReward}Key ");
            if (data.gemReward > 0) rewards.Append($"+{data.gemReward}Gem ");
            rewardText.text = rewards.ToString().TrimEnd();
        }

        private void UpdateDoorsText(RoomData data)
        {
            if (doorsText == null) return;

            string doors = "";
            foreach (var door in data.doors)
            {
                doors += door switch
                {
                    Direction.North => "N ",
                    Direction.South => "S ",
                    Direction.East => "E ",
                    Direction.West => "W ",
                    _ => ""
                };
            }
            doorsText.text = doors.TrimEnd();
        }

        private void UpdateRarityVisual(RoomRarity rarity)
        {
            if (rarityBorder == null) return;

            rarityBorder.color = rarity switch
            {
                RoomRarity.Common => commonColor,
                RoomRarity.Rare => rareColor,
                RoomRarity.Epic => epicColor,
                RoomRarity.Legendary => legendaryColor,
                _ => commonColor
            };
        }
    }
}
