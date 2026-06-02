using UnityEngine;
using DungeonArchitect.Data;
using DungeonArchitect.Core;

namespace DungeonArchitect.Systems
{
    public class GridInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DungeonGridManager gridManager;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject placementHighlightPrefab;

        [Header("Settings")]
        [SerializeField] private LayerMask gridLayerMask;

        private RoomData roomToPlace;
        private bool isPlacing;
        private GameObject currentHighlight;

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.RoomPlacement) return;
            if (!isPlacing) return;

            HandlePlacementInput();
        }

        public void BeginPlacement(RoomData room)
        {
            roomToPlace = room;
            isPlacing = true;
            ShowValidPlacements();
        }

        public void CancelPlacement()
        {
            roomToPlace = null;
            isPlacing = false;
            HideHighlights();
        }

        private void HandlePlacementInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var gridPos = ScreenToGridPosition(Input.mousePosition);
                if (gridPos.HasValue && gridManager.CanPlaceRoom(roomToPlace, gridPos.Value))
                {
                    PlaceRoom(gridPos.Value);
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
                GameManager.Instance.ChangeState(GameState.RoomDraft);
            }
        }

        private void PlaceRoom(Vector2Int position)
        {
            var resources = GameManager.Instance.Resources;
            if (roomToPlace.gemCost > 0 && !resources.SpendGems(roomToPlace.gemCost))
                return;

            gridManager.PlaceRoom(roomToPlace, position);
            gridManager.MovePlayerTo(position);
            GameManager.Instance.OnRoomPlaced();

            isPlacing = false;
            roomToPlace = null;
            HideHighlights();

            var roomInstance = gridManager.GetRoomAt(position);
            if (roomInstance != null)
            {
                var resolver = GetComponent<RoomResolver>();
                if (resolver != null)
                    resolver.ResolveRoom(roomInstance);
            }
        }

        private void ShowValidPlacements()
        {
            if (roomToPlace == null) return;
            var valid = gridManager.GetValidPlacements(roomToPlace);
            foreach (var pos in valid)
            {
                var worldPos = gridManager.GridToWorld(pos);
                if (placementHighlightPrefab != null)
                    Instantiate(placementHighlightPrefab, worldPos, Quaternion.identity, transform);
            }
        }

        private void HideHighlights()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
        }

        private Vector2Int? ScreenToGridPosition(Vector3 screenPos)
        {
            if (mainCamera == null) mainCamera = Camera.main;

            var worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            float cellSize = gridManager.CellSize;
            float offsetX = -(gridManager.GridWidth * cellSize) / 2f;
            float offsetY = -(gridManager.GridHeight * cellSize) / 2f;

            int x = Mathf.RoundToInt((worldPos.x - offsetX) / cellSize);
            int y = Mathf.RoundToInt((worldPos.y - offsetY) / cellSize);

            var gridPos = new Vector2Int(x, y);
            if (x >= 0 && x < gridManager.GridWidth && y >= 0 && y < gridManager.GridHeight)
                return gridPos;

            return null;
        }
    }
}
