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
        private bool isCurrent;
        private GameObject borderHighlight;

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

            spriteRenderer.color = Color.white;
            SetupDoorIndicators(instance.Data);
            SetRevealed(false);
        }

        public void SetRevealed(bool revealed)
        {
            isRevealed = revealed;
            if (fogOverlay != null)
                fogOverlay.SetActive(!revealed);
            UpdateColor();
        }

        public void SetAsCurrentRoom(bool current)
        {
            isCurrent = current;
            SetRevealed(true);

            if (current)
                ShowBorder();
            else
                HideBorder();
        }

        private void UpdateColor()
        {
            if (!isRevealed)
            {
                spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
                return;
            }
            spriteRenderer.color = Color.white;
        }

        private void ShowBorder()
        {
            if (borderHighlight == null)
            {
                borderHighlight = new GameObject("CurrentBorder");
                borderHighlight.transform.SetParent(transform, false);

                var bounds = spriteRenderer.sprite != null ? spriteRenderer.sprite.bounds : new Bounds(Vector3.zero, Vector3.one);
                borderHighlight.transform.localPosition = (Vector3)bounds.center + Vector3.back * 0.01f;

                var lr = borderHighlight.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.loop = true;
                lr.startWidth = 0.04f;
                lr.endWidth = 0.04f;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = Color.white;
                lr.endColor = Color.white;
                lr.sortingOrder = 9;

                float hx = bounds.extents.x;
                float hy = bounds.extents.y;
                lr.positionCount = 4;
                lr.SetPositions(new Vector3[]
                {
                    new Vector3(-hx, -hy, 0),
                    new Vector3(hx, -hy, 0),
                    new Vector3(hx, hy, 0),
                    new Vector3(-hx, hy, 0)
                });
            }

            borderHighlight.SetActive(true);
        }

        private void HideBorder()
        {
            if (borderHighlight != null)
                borderHighlight.SetActive(false);
        }

        public void SetBorderVisible(bool visible)
        {
            if (borderHighlight != null && isCurrent)
                borderHighlight.SetActive(visible);
        }

        private void SetupDoorIndicators(RoomData data)
        {
            if (doorIndicatorN != null) doorIndicatorN.gameObject.SetActive(false);
            if (doorIndicatorS != null) doorIndicatorS.gameObject.SetActive(false);
            if (doorIndicatorE != null) doorIndicatorE.gameObject.SetActive(false);
            if (doorIndicatorW != null) doorIndicatorW.gameObject.SetActive(false);
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
