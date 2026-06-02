using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DungeonArchitect.Data;
using DungeonArchitect.Core;
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
        private Direction entryDirection;

        public void Initialize(IReadOnlyList<RoomData> rooms, Vector3 worldPos, Camera cam, System.Action<RoomOffer> callback, Direction entry = Direction.South)
        {
            onRoomChosen = callback;
            entryDirection = entry;
            spriteMapper = UnityEngine.Resources.FindObjectsOfTypeAll<RoomSpriteMapper>().Length > 0
                ? UnityEngine.Resources.FindObjectsOfTypeAll<RoomSpriteMapper>()[0]
                : null;

            offers.Clear();
            foreach (var room in rooms)
                offers.Add(GenerateOffer(room));

            EnsureAtLeastOneFree();
            EnsureThreeOffers(rooms);

            CreatePopupCanvas(worldPos, cam);
            CreateOfferCards();
        }

        private void EnsureAtLeastOneFree()
        {
            bool hasFree = false;
            foreach (var o in offers)
                if (o.costType == RoomCostType.Free) { hasFree = true; break; }

            if (!hasFree && offers.Count > 0)
            {
                var first = offers[0];
                first.costType = RoomCostType.Free;
                first.costAmount = 0;
                offers[0] = first;
            }
        }

        private void EnsureThreeOffers(IReadOnlyList<RoomData> rooms)
        {
            var usedIds = new HashSet<string>();
            foreach (var o in offers)
                usedIds.Add(o.room.roomId);

            int attempts = 0;
            while (offers.Count < 3 && attempts < 30)
            {
                attempts++;
                var room = rooms[Random.Range(0, rooms.Count)];
                if (usedIds.Contains(room.roomId)) continue;

                usedIds.Add(room.roomId);
                var extra = GenerateOffer(room);
                if (offers.Count == 0)
                {
                    extra.costType = RoomCostType.Free;
                    extra.costAmount = 0;
                }
                offers.Add(extra);
            }
        }

        private void CreatePopupCanvas(Vector3 worldPos, Camera cam)
        {
            var canvasGO = new GameObject("PopupCanvas");
            canvasGO.transform.SetParent(transform);
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0f, 0f);
            bgRT.anchorMax = new Vector2(1f, 0.94f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            bgGO.AddComponent<CanvasRenderer>();
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.02f, 0.02f, 0.05f, 0.7f);
            bgImg.raycastTarget = true;
        }

        private void CreateOfferCards()
        {
            var containerGO = new GameObject("CardContainer");
            containerGO.transform.SetParent(canvas.transform, false);
            var containerRT = containerGO.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.5f, 0.5f);
            containerRT.anchorMax = new Vector2(0.5f, 0.5f);
            containerRT.pivot = new Vector2(0.5f, 0.5f);
            containerRT.anchoredPosition = Vector2.zero;

            float cardWidth = 280f;
            float cardHeight = 420f;
            float spacing = 20f;
            float totalWidth = offers.Count * cardWidth + (offers.Count - 1) * spacing;
            containerRT.sizeDelta = new Vector2(totalWidth, cardHeight);

            float startX = -totalWidth / 2f + cardWidth / 2f;

            for (int i = 0; i < offers.Count; i++)
            {
                var offer = offers[i];
                var cardGO = CreateCard(offer, containerRT);
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

            bool canAfford = CanAffordOffer(offer);
            btn.interactable = canAfford;

            if (!canAfford)
                img.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            var capturedOffer = offer;
            btn.onClick.AddListener(() => OnCardClick(capturedOffer));

            var iconGO = new GameObject("RoomImage");
            iconGO.transform.SetParent(cardGO.transform, false);
            var iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.05f, 0.35f);
            iconRT.anchorMax = new Vector2(0.95f, 0.95f);
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

            float rotAngle = GetRotationAngle(entryDirection);
            iconRT.localRotation = Quaternion.Euler(0, 0, rotAngle);

            AddDoorCorridors(iconGO.transform, room, rotAngle);

            var nameGO = new GameObject("Name");
            nameGO.transform.SetParent(cardGO.transform, false);
            var nameRT = nameGO.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.05f, 0.22f);
            nameRT.anchorMax = new Vector2(0.95f, 0.36f);
            nameRT.offsetMin = nameRT.offsetMax = Vector2.zero;
            nameGO.AddComponent<CanvasRenderer>();
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text = room.roomName;
            nameTMP.fontSize = 18;
            nameTMP.color = Color.white;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.enableWordWrapping = true;

            var typeGO = new GameObject("Type");
            typeGO.transform.SetParent(cardGO.transform, false);
            var typeRT = typeGO.AddComponent<RectTransform>();
            typeRT.anchorMin = new Vector2(0.05f, 0.14f);
            typeRT.anchorMax = new Vector2(0.95f, 0.23f);
            typeRT.offsetMin = typeRT.offsetMax = Vector2.zero;
            typeGO.AddComponent<CanvasRenderer>();
            var typeTMP = typeGO.AddComponent<TextMeshProUGUI>();
            typeTMP.text = room.roomType.ToString();
            typeTMP.fontSize = 14;
            typeTMP.color = new Color(0.8f, 0.8f, 0.8f);
            typeTMP.alignment = TextAlignmentOptions.Center;

            var costGO = new GameObject("Cost");
            costGO.transform.SetParent(cardGO.transform, false);
            var costRT = costGO.AddComponent<RectTransform>();
            costRT.anchorMin = new Vector2(0.05f, 0.08f);
            costRT.anchorMax = new Vector2(0.95f, 0.16f);
            costRT.offsetMin = costRT.offsetMax = Vector2.zero;
            costGO.AddComponent<CanvasRenderer>();
            var costTMP = costGO.AddComponent<TextMeshProUGUI>();
            costTMP.fontSize = 14;
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
            infoTMP.fontSize = 13;
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

        private static bool CanAffordOffer(RoomOffer offer)
        {
            if (offer.costType == RoomCostType.Free) return true;

            var resources = GameManager.Instance.Resources;
            return offer.costType switch
            {
                RoomCostType.Key => resources.Keys >= 1,
                RoomCostType.Gems => resources.Gems >= offer.costAmount,
                _ => true
            };
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

        private void AddDoorCorridors(Transform imageParent, RoomData room, float imageRotation)
        {
            foreach (var door in room.doors)
            {
                var corridorGO = new GameObject($"Corridor_{door}");
                corridorGO.transform.SetParent(imageParent, false);
                var corridorRT = corridorGO.AddComponent<RectTransform>();

                float pos = 0.5f;
                float corridorW = 0.25f;
                float corridorH = 0.12f;

                switch (door)
                {
                    case Direction.North:
                        corridorRT.anchorMin = new Vector2(0.5f - corridorW / 2f, 1f - 0.01f);
                        corridorRT.anchorMax = new Vector2(0.5f + corridorW / 2f, 1f + corridorH);
                        break;
                    case Direction.South:
                        corridorRT.anchorMin = new Vector2(0.5f - corridorW / 2f, -corridorH);
                        corridorRT.anchorMax = new Vector2(0.5f + corridorW / 2f, 0.01f);
                        break;
                    case Direction.East:
                        corridorRT.anchorMin = new Vector2(1f - 0.01f, 0.5f - corridorW / 2f);
                        corridorRT.anchorMax = new Vector2(1f + corridorH, 0.5f + corridorW / 2f);
                        break;
                    case Direction.West:
                        corridorRT.anchorMin = new Vector2(-corridorH, 0.5f - corridorW / 2f);
                        corridorRT.anchorMax = new Vector2(0.01f, 0.5f + corridorW / 2f);
                        break;
                }

                corridorRT.offsetMin = corridorRT.offsetMax = Vector2.zero;
                corridorGO.AddComponent<CanvasRenderer>();
                var cImg = corridorGO.AddComponent<Image>();
                cImg.sprite = GetCorridorUISprite();
                cImg.raycastTarget = false;
            }
        }

        private static Sprite corridorUISprite;
        private static Sprite GetCorridorUISprite()
        {
            if (corridorUISprite != null) return corridorUISprite;

            int size = 16;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color stoneBase = new Color(0.38f, 0.34f, 0.28f);
            Color stoneDark = new Color(0.28f, 0.24f, 0.2f);
            Color mortar = new Color(0.15f, 0.12f, 0.1f);
            Color border = new Color(0.45f, 0.4f, 0.32f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                    {
                        tex.SetPixel(x, y, border);
                    }
                    else if (y % 4 == 0 || (x % 4 == 0 && (y / 4) % 2 == 0) || ((x + 2) % 4 == 0 && (y / 4) % 2 == 1))
                    {
                        tex.SetPixel(x, y, mortar);
                    }
                    else
                    {
                        float n = ((x * 13 + y * 7) % 11) / 11f;
                        tex.SetPixel(x, y, Color.Lerp(stoneDark, stoneBase, 0.4f + n * 0.4f));
                    }
                }
            }

            tex.Apply();
            corridorUISprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return corridorUISprite;
        }

        private static float GetRotationAngle(Direction entry)
        {
            return entry switch
            {
                Direction.South => 0f,
                Direction.West => -90f,
                Direction.North => 180f,
                Direction.East => 90f,
                _ => 0f
            };
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
