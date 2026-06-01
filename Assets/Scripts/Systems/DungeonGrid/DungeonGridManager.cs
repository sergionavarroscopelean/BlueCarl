using UnityEngine;
using System.Collections.Generic;
using DungeonArchitect.Data;

namespace DungeonArchitect.Systems
{
    public class DungeonGridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 9;
        [SerializeField] private int gridHeight = 9;
        [SerializeField] private float cellSize = 2f;

        [Header("Visuals")]
        [SerializeField] private Transform gridParent;
        [SerializeField] private GameObject roomPrefab;
        [SerializeField] private GameObject validPlacementIndicator;

        private RoomInstance[,] grid;
        private Vector2Int playerPosition;
        private List<Vector2Int> validPlacements = new List<Vector2Int>();

        public Vector2Int PlayerPosition => playerPosition;
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;

        public event System.Action<Vector2Int, RoomInstance> OnRoomPlaced;
        public event System.Action<Vector2Int> OnPlayerMoved;

        public void InitializeFloor()
        {
            ClearGrid();
            grid = new RoomInstance[gridWidth, gridHeight];
            playerPosition = new Vector2Int(gridWidth / 2, gridHeight / 2);
        }

        public void PlaceStartingRoom(RoomData startRoomData)
        {
            var startPos = new Vector2Int(gridWidth / 2, gridHeight / 2);
            PlaceRoom(startRoomData, startPos);
            playerPosition = startPos;
            UpdateValidPlacements();
        }

        public bool CanPlaceRoom(RoomData room, Vector2Int position)
        {
            if (!IsInBounds(position)) return false;
            if (grid[position.x, position.y] != null) return false;
            if (!HasAdjacentRoom(position)) return false;
            if (!DoorsAlign(room, position)) return false;
            return true;
        }

        public void PlaceRoom(RoomData roomData, Vector2Int position)
        {
            var instance = new RoomInstance(roomData, position);
            grid[position.x, position.y] = instance;

            SpawnRoomVisual(instance);
            UpdateValidPlacements();

            OnRoomPlaced?.Invoke(position, instance);
        }

        public void MovePlayerTo(Vector2Int position)
        {
            if (grid[position.x, position.y] == null) return;
            playerPosition = position;
            OnPlayerMoved?.Invoke(position);
        }

        public List<Vector2Int> GetValidPlacements(RoomData room)
        {
            var placements = new List<Vector2Int>();
            foreach (var pos in validPlacements)
            {
                if (CanPlaceRoom(room, pos))
                    placements.Add(pos);
            }
            return placements;
        }

        public RoomInstance GetRoomAt(Vector2Int position)
        {
            if (!IsInBounds(position)) return null;
            return grid[position.x, position.y];
        }

        public List<Vector2Int> GetAdjacentExploredRooms(Vector2Int position)
        {
            var adjacent = new List<Vector2Int>();
            var directions = new Vector2Int[]
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };

            foreach (var dir in directions)
            {
                var checkPos = position + dir;
                if (IsInBounds(checkPos) && grid[checkPos.x, checkPos.y] != null)
                    adjacent.Add(checkPos);
            }
            return adjacent;
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float offsetX = -(gridWidth * cellSize) / 2f;
            float offsetY = -(gridHeight * cellSize) / 2f;
            return new Vector3(
                gridPos.x * cellSize + offsetX,
                gridPos.y * cellSize + offsetY,
                0f
            );
        }

        private bool IsInBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
        }

        private bool HasAdjacentRoom(Vector2Int position)
        {
            return GetAdjacentExploredRooms(position).Count > 0;
        }

        private bool DoorsAlign(RoomData room, Vector2Int position)
        {
            var directions = new (Vector2Int offset, Direction fromDir, Direction toDir)[]
            {
                (Vector2Int.up, Direction.North, Direction.South),
                (Vector2Int.down, Direction.South, Direction.North),
                (Vector2Int.right, Direction.East, Direction.West),
                (Vector2Int.left, Direction.West, Direction.East)
            };

            bool hasAtLeastOneConnection = false;

            foreach (var (offset, fromDir, toDir) in directions)
            {
                var neighborPos = position + offset;
                if (!IsInBounds(neighborPos)) continue;

                var neighbor = grid[neighborPos.x, neighborPos.y];
                if (neighbor == null) continue;

                bool roomHasDoor = room.HasDoor(fromDir);
                bool neighborHasDoor = neighbor.Data.HasDoor(toDir);

                if (roomHasDoor && neighborHasDoor)
                    hasAtLeastOneConnection = true;

                if (roomHasDoor != neighborHasDoor)
                    return false;
            }

            return hasAtLeastOneConnection;
        }

        private void UpdateValidPlacements()
        {
            validPlacements.Clear();

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (grid[x, y] == null && HasAdjacentRoom(pos))
                        validPlacements.Add(pos);
                }
            }
        }

        private void SpawnRoomVisual(RoomInstance instance)
        {
            if (roomPrefab == null || gridParent == null) return;

            var worldPos = GridToWorld(instance.GridPosition);
            var go = Instantiate(roomPrefab, worldPos, Quaternion.identity, gridParent);
            go.name = $"Room_{instance.Data.roomName}_{instance.GridPosition}";
            instance.SetVisual(go);
        }

        private void ClearGrid()
        {
            if (gridParent == null) return;
            foreach (Transform child in gridParent)
                Destroy(child.gameObject);
        }
    }

    public class RoomInstance
    {
        public RoomData Data { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        public bool IsExplored { get; private set; }
        public bool IsRevealed { get; private set; }
        public GameObject Visual { get; private set; }

        public RoomInstance(RoomData data, Vector2Int position)
        {
            Data = data;
            GridPosition = position;
            IsExplored = false;
            IsRevealed = false;
        }

        public void Explore()
        {
            IsExplored = true;
            IsRevealed = true;
        }

        public void SetVisual(GameObject visual)
        {
            Visual = visual;
        }
    }
}
