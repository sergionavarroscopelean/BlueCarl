using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DungeonArchitect.Data;
using DungeonArchitect.Utils;

namespace DungeonArchitect.Systems
{
    public enum RoomCostType { Free, Key, Gems }

    public struct RoomOffer
    {
        public RoomData room;
        public RoomCostType costType;
        public int costAmount;
        public int goldReward;
        public int gemReward;
        public int keyReward;
    }

    public class DraftPopupUI : MonoBehaviour
    {
        private Canvas canvas;
        private System.Action<RoomOffer> onRoomChosen;
        private List<GameObject> cardObjects = new List<GameObject>();
        private List<RoomOffer> offers = new List<RoomOffer>();
        private RoomSpriteMapper spriteMapper;

        public void Initialize(IReadOnlyList<RoomData> rooms, Vector3 worldPos, Camera cam, System.Action<RoomOffer> callback)
        {
            onRoomChosen = callback;
            spriteMapper = UnityEngine.Resources.FindObjectsOfTypeAll<RoomSpriteMapper>().Length > 0
                ? UnityEngine.Resources.FindObjectsOfTypeAll<RoomSpriteMapper>()[0]
                : null;

            offers.Clear();
            foreach (var room in rooms)
                offers.Add(GenerateOffer(room));

            CreatePopupCanvas(worldPos, cam);
            CreateOfferCards();
        }

        private void CreatePopupCanvas(Vector3 worldPos, Camera cam)
        {
            var canvasGO = new GameObject("PopupCanvas");
            canvasGO.transform.SetParent(transform);
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = cam;

            var rt = canvasGO.GetComponent<RectTransform>();
            rt.position = worldPos + Vector3.back * 0.1f;
            rt.sizeDelta = new Vector2(620, 280);
            rt.localScale = Vector3.one * 0.01f;

            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            bgGO.AddComponent<CanvasRenderer>();
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.08f, 0.15f, 0.95f);
        }

        private void CreateOfferCards()
        {
            var canvasRT = canvas.GetComponent<RectTransform>();
            float cardWidth = 180f;
            float cardHeight = 280f;
            float spacing = 15f;
            float totalWidth = offers.Count * cardWidth + (offers.Count - 1) * spacing;
            float startX = -totalWidth / 2f + cardWidth / 2f;

            for (int i = 0; i < offers.Count; i++)
            {
                var offer = offers[i];
                var cardGO = CreateCard(offer, canvasRT.transform);
                var cardRT = cardGO.GetComponent<RectTransform>();
                cardRT.anchoredPosition = new Vector2(startX + i * (cardWidth + spacing), 0);
                cardRT.sizeDelta = new Vector2(cardWidth, cardHeight);
                cardObjects.Add(cardGO);
            }
        }

        private GameObject CreateCard(RoomOffer offer, Transform parent)
        {
            var room = offer.room;
            var cardGO = new GameObject($"Card_{room.roomName}");
            cardGO.transform.SetParent(parent, false);

            var rt = cardGO.AddComponent<RectTransform>();
            cardGO.AddComponent<CanvasRenderer>();

            var img = cardGO.AddComponent<Image>();
            img.color = GetTypeColor(room.roomType);

            var btn = cardGO.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            btn.colors = colors;

            var capturedOffer = offer;
            btn.onClick.AddListener(() => OnCardClick(capturedOffer));

            var iconGO = new GameObject("RoomImage");
            iconGO.transform.SetParent(cardGO.transform, false);
            var iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.1f, 0.45f);
            iconRT.anchorMax = new Vector2(0.9f, 0.95f);
            iconRT.offsetMin = iconRT.offsetMax = Vector2.zero;
            iconGO.AddComponent<CanvasRenderer>();
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            var roomSprite = GetRoomSprite(room);
            if (roomSprite != null)
                iconImg.sprite = roomSprite;
            else
                iconImg.color = new Color(0.2f, 0.2f, 0.3f, 0.5f);

            var nameGO = new GameObject("Name");
            nameGO.transform.SetParent(cardGO.transform, false);
            var nameRT = nameGO.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.05f, 0.32f);
            nameRT.anchorMax = new Vector2(0.95f, 0.46f);
            nameRT.offsetMin = nameRT.offsetMax = Vector2.zero;
            nameGO.AddComponent<CanvasRenderer>();
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text = room.roomName;
            nameTMP.fontSize = 12;
            nameTMP.color = Color.white;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.enableWordWrapping = true;

            var typeGO = new GameObject("Type");
            typeGO.transform.SetParent(cardGO.transform, false);
            var typeRT = typeGO.AddComponent<RectTransform>();
            typeRT.anchorMin = new Vector2(0.05f, 0.22f);
            typeRT.anchorMax = new Vector2(0.95f, 0.33f);
            typeRT.offsetMin = typeRT.offsetMax = Vector2.zero;
            typeGO.AddComponent<CanvasRenderer>();
            var typeTMP = typeGO.AddComponent<TextMeshProUGUI>();
            typeTMP.text = room.roomType.ToString();
            typeTMP.fontSize = 10;
            typeTMP.color = new Color(0.8f, 0.8f, 0.8f);
            typeTMP.alignment = TextAlignmentOptions.Center;

            var doorsGO = new GameObject("Doors");
            doorsGO.transform.SetParent(cardGO.transform, false);
            var doorsRT = doorsGO.AddComponent<RectTransform>();
            doorsRT.anchorMin = new Vector2(0.05f, 0.15f);
            doorsRT.anchorMax = new Vector2(0.95f, 0.23f);
            doorsRT.offsetMin = doorsRT.offsetMax = Vector2.zero;
            doorsGO.AddComponent<CanvasRenderer>();
            var doorsTMP = doorsGO.AddComponent<TextMeshProUGUI>();
            doorsTMP.text = "Salidas: " + GetDoorsLabel(room);
            doorsTMP.fontSize = 9;
            doorsTMP.color = new Color(0.9f, 0.9f, 0.5f);
            doorsTMP.alignment = TextAlignmentOptions.Center;

            var costGO = new GameObject("Cost");
            costGO.transform.SetParent(cardGO.transform, false);
            var costRT = costGO.AddComponent<RectTransform>();
            costRT.anchorMin = new Vector2(0.05f, 0.08f);
            costRT.anchorMax = new Vector2(0.95f, 0.16f);
            costRT.offsetMin = costRT.offsetMax = Vector2.zero;
            costGO.AddComponent<CanvasRenderer>();
            var costTMP = costGO.AddComponent<TextMeshProUGUI>();
            costTMP.fontSize = 9;
            costTMP.alignment = TextAlignmentOptions.Center;

            string costLabel = offer.costType switch
            {
                RoomCostType.Key => "Coste: 1 Llave",
                RoomCostType.Gems => $"Coste: {offer.costAmount} Gemas",
                _ => "Gratis"
            };
            costTMP.text = costLabel;
            costTMP.color = offer.costType == RoomCostType.Free
                ? new Color(0.5f, 0.9f, 0.5f)
                : new Color(1f, 0.7f, 0.3f);

            var infoGO = new GameObject("Info");
            infoGO.transform.SetParent(cardGO.transform, false);
            var infoRT = infoGO.AddComponent<RectTransform>();
            infoRT.anchorMin = new Vector2(0.05f, 0.01f);
            infoRT.anchorMax = new Vector2(0.95f, 0.09f);
            infoRT.offsetMin = infoRT.offsetMax = Vector2.zero;
            infoGO.AddComponent<CanvasRenderer>();
            var infoTMP = infoGO.AddComponent<TextMeshProUGUI>();
            infoTMP.fontSize = 9;
            infoTMP.alignment = TextAlignmentOptions.Center;

            GetDamageRange(room, out int minDmg, out int maxDmg);
            var infoText = new System.Text.StringBuilder();
            if (maxDmg > 0)
                infoText.Append($"<color=#FF6666>HP: -{minDmg} a -{maxDmg}</color>");

            string rewardStr = GetRewardLabel(offer);
            if (!string.IsNullOrEmpty(rewardStr))
            {
                if (infoText.Length > 0) infoText.Append("  ");
                infoText.Append($"<color=#66FF66>{rewardStr}</color>");
            }

            if (infoText.Length == 0)
                infoText.Append("<color=#88FF88>Segura</color>");

            infoTMP.text = infoText.ToString();
            infoTMP.richText = true;
            infoTMP.color = Color.white;

            return cardGO;
        }

        private void OnCardClick(RoomOffer offer)
        {
            onRoomChosen?.Invoke(offer);
        }

        private static RoomOffer GenerateOffer(RoomData room)
        {
            var offer = new RoomOffer { room = room };

            bool isValuable = room.roomType == RoomType.Treasure
                           || room.roomType == RoomType.Shrine
                           || room.roomType == RoomType.Shop
                           || room.roomType == RoomType.Rest;

            float costChance = isValuable ? 0.5f : 0.15f;
            costChance += (int)room.rarity * 0.1f;

            float roll = Random.value;
            if (roll < costChance)
            {
                if (Random.value < 0.4f)
                {
                    offer.costType = RoomCostType.Key;
                    offer.costAmount = 1;
                }
                else
                {
                    offer.costType = RoomCostType.Gems;
                    offer.costAmount = 1 + (int)room.rarity + Random.Range(0, 2);
                }
            }
            else
            {
                offer.costType = RoomCostType.Free;
                offer.costAmount = 0;
            }

            bool hasCombat = room.roomType == RoomType.Combat
                          || room.roomType == RoomType.Elite
                          || room.roomType == RoomType.Trap
                          || room.roomType == RoomType.Boss;

            if (hasCombat)
            {
                offer.goldReward = Random.Range(1, 4) + (int)room.rarity * 2;
                if (Random.value < 0.3f + (int)room.rarity * 0.1f)
                    offer.gemReward = Random.Range(1, 3);
                if (Random.value < 0.15f)
                    offer.keyReward = 1;
            }
            else if (room.roomType == RoomType.Treasure)
            {
                offer.goldReward = Random.Range(3, 8) + (int)room.rarity * 3;
                offer.gemReward = Random.Range(1, 3);
                if (Random.value < 0.3f)
                    offer.keyReward = 1;
            }
            else
            {
                if (Random.value < 0.25f)
                    offer.goldReward = Random.Range(1, 3);
                if (Random.value < 0.1f)
                    offer.gemReward = 1;
            }

            return offer;
        }

        private static string GetRewardLabel(RoomOffer offer)
        {
            var sb = new System.Text.StringBuilder();
            if (offer.goldReward > 0) sb.Append($"+{offer.goldReward}G ");
            if (offer.gemReward > 0) sb.Append($"+{offer.gemReward}Gem ");
            if (offer.keyReward > 0) sb.Append("+1Key ");
            return sb.ToString().TrimEnd();
        }

        public static void GetDamageRange(RoomData room, out int min, out int max)
        {
            bool hasCombat = room.roomType == RoomType.Combat
                          || room.roomType == RoomType.Elite
                          || room.roomType == RoomType.Trap
                          || room.roomType == RoomType.Boss;

            if (!hasCombat)
            {
                min = 0;
                max = 0;
                return;
            }

            int rarityBase = room.rarity switch
            {
                RoomRarity.Common => 0,
                RoomRarity.Rare => 1,
                RoomRarity.Epic => 2,
                RoomRarity.Legendary => 3,
                _ => 0
            };

            int typeBase = room.roomType switch
            {
                RoomType.Combat => 0,
                RoomType.Trap => 1,
                RoomType.Elite => 2,
                RoomType.Boss => 3,
                _ => 0
            };

            min = 1 + rarityBase + typeBase;
            max = min + 2 + rarityBase;
            if (max > 10) max = 10;
        }

        public static int RollDamage(RoomData room)
        {
            GetDamageRange(room, out int min, out int max);
            if (max <= 0) return 0;
            return Random.Range(min, max + 1);
        }

        private Sprite GetRoomSprite(RoomData room)
        {
            if (room.roomSprite != null)
                return room.roomSprite;

            if (spriteMapper != null && !string.IsNullOrEmpty(room.roomId))
            {
                var idStr = room.roomId.Replace("room_", "");
                if (int.TryParse(idStr, out int id))
                    return spriteMapper.GetSprite(id);
            }

            return room.cardSprite;
        }

        private string GetDoorsLabel(RoomData room)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var d in room.doors)
            {
                sb.Append(d switch
                {
                    Direction.North => "N ",
                    Direction.South => "S ",
                    Direction.East => "E ",
                    Direction.West => "W ",
                    _ => ""
                });
            }
            return sb.ToString().TrimEnd();
        }

        private Color GetTypeColor(RoomType type)
        {
            return type switch
            {
                RoomType.Combat => new Color(0.4f, 0.15f, 0.2f, 0.9f),
                RoomType.Elite => new Color(0.5f, 0.1f, 0.15f, 0.9f),
                RoomType.Treasure => new Color(0.4f, 0.35f, 0.05f, 0.9f),
                RoomType.Shop => new Color(0.1f, 0.35f, 0.1f, 0.9f),
                RoomType.Rest => new Color(0.1f, 0.35f, 0.2f, 0.9f),
                RoomType.Event => new Color(0.2f, 0.2f, 0.4f, 0.9f),
                RoomType.Trap => new Color(0.35f, 0.05f, 0.05f, 0.9f),
                RoomType.Puzzle => new Color(0.2f, 0.2f, 0.45f, 0.9f),
                RoomType.Shrine => new Color(0.4f, 0.4f, 0.1f, 0.9f),
                RoomType.Stair => new Color(0.3f, 0.15f, 0.4f, 0.9f),
                RoomType.Boss => new Color(0.5f, 0.05f, 0.05f, 0.9f),
                _ => new Color(0.2f, 0.2f, 0.2f, 0.9f)
            };
        }
    }
}
