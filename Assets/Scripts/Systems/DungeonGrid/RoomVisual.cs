using UnityEngine;
using DungeonArchitect.Data;
using DungeonArchitect.Utils;

namespace DungeonArchitect.Systems
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class RoomVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteRenderer doorIndicatorN;
        [SerializeField] private SpriteRenderer doorIndicatorS;
        [SerializeField] private SpriteRenderer doorIndicatorE;
        [SerializeField] private SpriteRenderer doorIndicatorW;
        [SerializeField] private GameObject highlightBorder;
        [SerializeField] private GameObject fogOverlay;

        private RoomInstance roomInstance;
        private bool isRevealed;

        public RoomInstance RoomInstance => roomInstance;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(RoomInstance instance, RoomSpriteMapper spriteMapper)
        {
            roomInstance = instance;

            if (spriteMapper != null)
            {
                int roomId = int.Parse(instance.Data.roomId.Replace("room_", ""));
                var sprite = spriteMapper.GetSprite(roomId);
                if (sprite != null)
                    spriteRenderer.sprite = sprite;
            }

            spriteRenderer.color = GetTypeColor(instance.Data.roomType);
            SetupDoorIndicators(instance.Data);
            SetRevealed(false);
        }

        public void SetRevealed(bool revealed)
        {
            isRevealed = revealed;
            if (fogOverlay != null)
                fogOverlay.SetActive(!revealed);
            spriteRenderer.color = revealed
                ? GetTypeColor(roomInstance.Data.roomType)
                : new Color(0.3f, 0.3f, 0.3f, 0.8f);
        }

        public void SetHighlighted(bool highlighted)
        {
            if (highlightBorder != null)
                highlightBorder.SetActive(highlighted);
        }

        public void SetAsCurrentRoom(bool isCurrent)
        {
            SetRevealed(true);
            SetHighlighted(isCurrent);
        }

        private void SetupDoorIndicators(RoomData data)
        {
            if (doorIndicatorN != null) doorIndicatorN.gameObject.SetActive(data.HasDoor(Direction.North));
            if (doorIndicatorS != null) doorIndicatorS.gameObject.SetActive(data.HasDoor(Direction.South));
            if (doorIndicatorE != null) doorIndicatorE.gameObject.SetActive(data.HasDoor(Direction.East));
            if (doorIndicatorW != null) doorIndicatorW.gameObject.SetActive(data.HasDoor(Direction.West));
        }

        private Color GetTypeColor(RoomType type)
        {
            return type switch
            {
                RoomType.Combat => new Color(0.9f, 0.4f, 0.6f),
                RoomType.Elite => new Color(0.9f, 0.3f, 0.5f),
                RoomType.Treasure => new Color(1f, 0.7f, 0.1f),
                RoomType.Shop => new Color(0.3f, 0.8f, 0.3f),
                RoomType.Rest => new Color(0.3f, 0.9f, 0.5f),
                RoomType.Event => new Color(0.4f, 0.4f, 0.9f),
                RoomType.Trap => new Color(0.8f, 0.2f, 0.2f),
                RoomType.Puzzle => new Color(0.5f, 0.5f, 0.9f),
                RoomType.Shrine => new Color(0.9f, 0.9f, 0.3f),
                RoomType.Stair => new Color(0.7f, 0.3f, 0.9f),
                RoomType.Boss => new Color(1f, 0.1f, 0.1f),
                _ => Color.gray
            };
        }
    }
}
