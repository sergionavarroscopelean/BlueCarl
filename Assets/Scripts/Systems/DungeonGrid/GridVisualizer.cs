using UnityEngine;
using DungeonArchitect.Data;

namespace DungeonArchitect.Systems
{
    public class GridVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DungeonGridManager gridManager;

        [Header("Background")]
        [SerializeField] private Color backgroundColor = new Color(0.05f, 0.08f, 0.18f, 1f);
        [SerializeField] private Color lineColor = new Color(0.2f, 0.35f, 0.55f, 0.9f);
        [SerializeField] private float lineWidth = 0.02f;

        private GameObject backgroundQuad;
        private GameObject linesParent;
        private bool created;

        private void Start()
        {
            if (gridManager == null)
                gridManager = Object.FindObjectOfType<DungeonGridManager>();

            Hide();

            if (gridManager != null)
                gridManager.OnRoomPlaced += OnFirstRoomPlaced;
        }

        private void OnDestroy()
        {
            if (gridManager != null)
                gridManager.OnRoomPlaced -= OnFirstRoomPlaced;
        }

        private void OnFirstRoomPlaced(Vector2Int pos, RoomInstance room)
        {
            if (created) return;
            Show();
        }

        public void Show()
        {
            if (!created)
            {
                if (gridManager == null) return;
                CreateBackground();
                CreateGridLines();
                created = true;
            }
            if (backgroundQuad != null) backgroundQuad.SetActive(true);
            if (linesParent != null) linesParent.SetActive(true);
        }

        public void Hide()
        {
            if (backgroundQuad != null) backgroundQuad.SetActive(false);
            if (linesParent != null) linesParent.SetActive(false);
        }

        private void CreateBackground()
        {
            float cw = gridManager.CellWidth;
            float ch = gridManager.CellHeight;
            int gw = gridManager.GridWidth;
            int gh = gridManager.GridHeight;

            var center = gridManager.GridToWorld(new Vector2Int(gw / 2, gh / 2));
            float totalW = gw * cw;
            float totalH = gh * ch;

            backgroundQuad = new GameObject("GridBackground");
            backgroundQuad.transform.SetParent(transform, false);
            backgroundQuad.transform.position = new Vector3(center.x, center.y, 0f);

            var sr = backgroundQuad.AddComponent<SpriteRenderer>();
            sr.sprite = CreateWhiteSprite();
            sr.color = backgroundColor;
            sr.sortingOrder = -10;
            backgroundQuad.transform.localScale = new Vector3(totalW, totalH, 1f);
        }

        private void CreateGridLines()
        {
            linesParent = new GameObject("GridLines");
            linesParent.transform.SetParent(transform, false);

            float cw = gridManager.CellWidth;
            float ch = gridManager.CellHeight;
            int gw = gridManager.GridWidth;
            int gh = gridManager.GridHeight;

            var center = gridManager.GridToWorld(new Vector2Int(gw / 2, gh / 2));
            float leftX = center.x - cw / 2f - (gw / 2) * cw;
            float botY = center.y - ch / 2f - (gh / 2) * ch;
            float rightX = leftX + gw * cw;
            float topY = botY + gh * ch;

            for (int x = 0; x <= gw; x++)
            {
                float posX = leftX + x * cw;
                CreateLine(new Vector3(posX, botY, 0f), new Vector3(posX, topY, 0f));
            }

            for (int y = 0; y <= gh; y++)
            {
                float posY = botY + y * ch;
                CreateLine(new Vector3(leftX, posY, 0f), new Vector3(rightX, posY, 0f));
            }
        }

        private void CreateLine(Vector3 start, Vector3 end)
        {
            var go = new GameObject("Line");
            go.transform.SetParent(linesParent.transform, false);

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = lineColor;
            lr.endColor = lineColor;
            lr.sortingOrder = -9;
        }

        private static Sprite whiteSprite;
        private static Sprite CreateWhiteSprite()
        {
            if (whiteSprite != null) return whiteSprite;

            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    tex.SetPixel(x, y, Color.white);
            tex.Apply();
            whiteSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
            return whiteSprite;
        }

        public Color GetRoomColor(RoomType type)
        {
            return type switch
            {
                RoomType.Combat => new Color(0.8f, 0.2f, 0.2f),
                RoomType.Elite => new Color(0.8f, 0f, 0f),
                RoomType.Treasure => new Color(1f, 0.84f, 0f),
                RoomType.Shop => new Color(0.2f, 0.6f, 0.2f),
                RoomType.Rest => new Color(0.2f, 0.8f, 0.4f),
                RoomType.Event => new Color(0.5f, 0.5f, 0.8f),
                RoomType.Trap => new Color(0.6f, 0f, 0.6f),
                RoomType.Shrine => new Color(0.9f, 0.9f, 0.2f),
                RoomType.Stair => new Color(1f, 1f, 1f),
                RoomType.Boss => new Color(1f, 0f, 0f),
                _ => Color.gray
            };
        }
    }
}
