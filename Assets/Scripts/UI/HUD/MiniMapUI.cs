using UnityEngine;
using UnityEngine.UI;
using DungeonArchitect.Core;
using DungeonArchitect.Systems;
using DungeonArchitect.Data;

namespace DungeonArchitect.UI
{
    public class MiniMapUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform mapContainer;
        [SerializeField] private GameObject miniMapCellPrefab;
        [SerializeField] private float cellSize = 16f;

        [Header("Colors")]
        [SerializeField] private Color exploredColor = new Color(0.4f, 0.4f, 0.4f);
        [SerializeField] private Color currentColor = Color.white;
        [SerializeField] private Color unexploredColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);

        private Image[,] mapCells;
        private DungeonGridManager gridManager;

        private void Start()
        {
            gridManager = GameManager.Instance.Grid;
            gridManager.OnRoomPlaced += OnRoomPlaced;
            gridManager.OnPlayerMoved += OnPlayerMoved;

            InitializeMap();
        }

        private void OnDestroy()
        {
            if (gridManager != null)
            {
                gridManager.OnRoomPlaced -= OnRoomPlaced;
                gridManager.OnPlayerMoved -= OnPlayerMoved;
            }
        }

        private void InitializeMap()
        {
            int width = gridManager.GridWidth;
            int height = gridManager.GridHeight;
            mapCells = new Image[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cellGO = Instantiate(miniMapCellPrefab, mapContainer);
                    var rect = cellGO.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(cellSize, cellSize);
                    rect.anchoredPosition = new Vector2(x * cellSize, y * cellSize);

                    var image = cellGO.GetComponent<Image>();
                    image.color = unexploredColor;
                    mapCells[x, y] = image;
                }
            }
        }

        private void OnRoomPlaced(Vector2Int pos, RoomInstance room)
        {
            if (mapCells == null) return;
            if (pos.x < 0 || pos.x >= gridManager.GridWidth) return;
            if (pos.y < 0 || pos.y >= gridManager.GridHeight) return;

            mapCells[pos.x, pos.y].color = exploredColor;
        }

        private void OnPlayerMoved(Vector2Int pos)
        {
            if (mapCells == null) return;

            for (int x = 0; x < gridManager.GridWidth; x++)
            {
                for (int y = 0; y < gridManager.GridHeight; y++)
                {
                    var room = gridManager.GetRoomAt(new Vector2Int(x, y));
                    if (room != null)
                        mapCells[x, y].color = exploredColor;
                }
            }

            mapCells[pos.x, pos.y].color = currentColor;
        }
    }
}
