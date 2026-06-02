using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DungeonArchitect.Data;
using DungeonArchitect.Utils;

namespace DungeonArchitect.Systems
{
    public class DraftPopupUI : MonoBehaviour
    {
        private Canvas canvas;
        private System.Action<RoomData> onRoomChosen;
        private List<GameObject> cardObjects = new List<GameObject>();
        private RoomSpriteMapper spriteMapper;

        public void Initialize(IReadOnlyList<RoomData> rooms, Vector3 worldPos, Camera cam, System.Action<RoomData> callback)
        {
            onRoomChosen = callback;
            spriteMapper = UnityEngine.Resources.FindObjectsOfTypeAll<RoomSpriteMapper>().Length > 0
                ? UnityEngine.Resources.FindObjectsOfTypeAll<RoomSpriteMapper>()[0]
                : null;
            CreatePopupCanvas(worldPos, cam);
            CreateRoomCards(rooms);
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

        private void CreateRoomCards(IReadOnlyList<RoomData> rooms)
        {
            var canvasRT = canvas.GetComponent<RectTransform>();
            float cardWidth = 180f;
            float cardHeight = 260f;
            float spacing = 15f;
            float totalWidth = rooms.Count * cardWidth + (rooms.Count - 1) * spacing;
            float startX = -totalWidth / 2f + cardWidth / 2f;

            for (int i = 0; i < rooms.Count; i++)
            {
                var room = rooms[i];
                var cardGO = CreateCard(room, canvasRT.transform);
                var cardRT = cardGO.GetComponent<RectTransform>();
                cardRT.anchoredPosition = new Vector2(startX + i * (cardWidth + spacing), 0);
                cardRT.sizeDelta = new Vector2(cardWidth, cardHeight);
                cardObjects.Add(cardGO);
            }
        }

        private GameObject CreateCard(RoomData room, Transform parent)
        {
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

            var capturedRoom = room;
            btn.onClick.AddListener(() => OnCardClick(capturedRoom));

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
            nameRT.anchorMin = new Vector2(0.05f, 0.25f);
            nameRT.anchorMax = new Vector2(0.95f, 0.45f);
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
            typeRT.anchorMin = new Vector2(0.05f, 0.13f);
            typeRT.anchorMax = new Vector2(0.95f, 0.26f);
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
            doorsRT.anchorMin = new Vector2(0.05f, 0.02f);
            doorsRT.anchorMax = new Vector2(0.95f, 0.14f);
            doorsRT.offsetMin = doorsRT.offsetMax = Vector2.zero;
            doorsGO.AddComponent<CanvasRenderer>();
            var doorsTMP = doorsGO.AddComponent<TextMeshProUGUI>();
            doorsTMP.text = GetDoorsLabel(room);
            doorsTMP.fontSize = 10;
            doorsTMP.color = new Color(0.9f, 0.9f, 0.5f);
            doorsTMP.alignment = TextAlignmentOptions.Center;

            return cardGO;
        }

        private void OnCardClick(RoomData room)
        {
            onRoomChosen?.Invoke(room);
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
