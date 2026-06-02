using UnityEngine;
using System.Collections.Generic;
using DungeonArchitect.Data;
using DungeonArchitect.Utils;

namespace DungeonArchitect.Systems
{
    public class DungeonGridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 9;
        [SerializeField] private int gridHeight = 9;
        [SerializeField] private float cellWidth = 1.35f;
        [SerializeField] private float cellHeight = 1.35f;

        [Header("Visuals")]
        [SerializeField] private Transform gridParent;
        [SerializeField] private GameObject roomPrefab;
        [SerializeField] private GameObject validPlacementIndicator;
        [SerializeField] private RoomSpriteMapper spriteMapper;

        [Header("Corridors")]
        [SerializeField] private float corridorWidth = 0.35f;

        [Header("Debug")]
        [SerializeField] private bool autoStartOnPlay = false;
        [SerializeField] private RoomData debugStartRoom;

        private RoomInstance[,] grid;
        private Vector2Int playerPosition;
        private List<Vector2Int> validPlacements = new List<Vector2Int>();

        public Vector2Int PlayerPosition => playerPosition;
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float CellWidth => cellWidth;
        public float CellHeight => cellHeight;

        public IReadOnlyList<Vector2Int> ValidPlacements => validPlacements;

        public event System.Action<Vector2Int, RoomInstance> OnRoomPlaced;
        public event System.Action<Vector2Int> OnPlayerMoved;
        public event System.Action<IReadOnlyList<Vector2Int>> OnValidPlacementsChanged;

        private void Start()
        {
            if (autoStartOnPlay && debugStartRoom != null)
            {
                InitializeFloor();
                PlaceStartingRoom(debugStartRoom);
            }
        }

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

            var instance = grid[startPos.x, startPos.y];
            if (instance != null)
            {
                instance.Explore();
                var visual = instance.Visual?.GetComponent<RoomVisual>();
                if (visual != null)
                    visual.SetAsCurrentRoom(true);
            }

            OnPlayerMoved?.Invoke(startPos);
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
            PlaceRoom(roomData, position, null);
        }

        public void PlaceRoom(RoomData roomData, Vector2Int position, Direction? entryDirection)
        {
            var instance = new RoomInstance(roomData, position, entryDirection);
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

        public int GetPathDistance(Vector2Int from, Vector2Int to)
        {
            if (from == to) return 0;

            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<(Vector2Int pos, int dist)>();
            queue.Enqueue((from, 0));
            visited.Add(from);

            var dirMap = new (Vector2Int offset, Direction fromDir, Direction toDir)[]
            {
                (Vector2Int.up, Direction.North, Direction.South),
                (Vector2Int.down, Direction.South, Direction.North),
                (Vector2Int.right, Direction.East, Direction.West),
                (Vector2Int.left, Direction.West, Direction.East)
            };

            while (queue.Count > 0)
            {
                var (current, dist) = queue.Dequeue();
                var currentRoom = grid[current.x, current.y];
                if (currentRoom == null) continue;

                foreach (var (offset, fromDir, toDir) in dirMap)
                {
                    var neighbor = current + offset;
                    if (!IsInBounds(neighbor)) continue;
                    if (visited.Contains(neighbor)) continue;

                    var neighborRoom = grid[neighbor.x, neighbor.y];
                    if (neighborRoom == null) continue;

                    bool currentHasDoor = HasWorldDoor(currentRoom, fromDir);
                    bool neighborHasDoor = HasWorldDoor(neighborRoom, toDir);

                    if (!currentHasDoor || !neighborHasDoor) continue;

                    if (neighbor == to)
                        return dist + 1;

                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, dist + 1));
                }
            }

            return -1;
        }

        public bool HasWorldDoor(RoomInstance room, Direction worldDir)
        {
            var directions = new[] { Direction.North, Direction.South, Direction.East, Direction.West };
            foreach (var dataDir in directions)
            {
                if (!room.Data.HasDoor(dataDir)) continue;
                if (room.RotateToWorld(dataDir) == worldDir) return true;
            }
            return false;
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float offsetX = -(gridWidth * cellWidth) / 2f;
            float offsetY = -(gridHeight * cellHeight) / 2f;
            return new Vector3(
                gridPos.x * cellWidth + offsetX,
                gridPos.y * cellHeight + offsetY,
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
            return true;
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

            OnValidPlacementsChanged?.Invoke(validPlacements);
        }

        private void SpawnRoomVisual(RoomInstance instance)
        {
            if (roomPrefab == null || gridParent == null) return;

            var worldPos = GridToWorld(instance.GridPosition);
            var rotation = GetEntryRotation(instance.EntryDirection);
            var go = Instantiate(roomPrefab, worldPos, rotation, gridParent);
            go.name = $"Room_{instance.Data.roomName}_{instance.GridPosition}";
            instance.SetVisual(go);

            var visual = go.GetComponent<RoomVisual>();
            if (visual != null)
                visual.Initialize(instance, spriteMapper);
        }

        private static Quaternion GetEntryRotation(Direction? entryDirection)
        {
            if (!entryDirection.HasValue) return Quaternion.identity;
            float angle = entryDirection.Value switch
            {
                Direction.North => 0f,
                Direction.South => 180f,
                Direction.East => -90f,
                Direction.West => 90f,
                _ => 0f
            };
            return Quaternion.Euler(0f, 0f, angle);
        }

        public void SpawnCorridor(Vector2Int fromPos, Direction worldDir)
        {
            if (gridParent == null) return;

            var toPos = fromPos + DirectionToOffset(worldDir);
            var fromWorld = GridToWorld(fromPos);
            var toWorld = GridToWorld(toPos);
            var midpoint = (fromWorld + toWorld) / 2f;

            bool horizontal = worldDir == Direction.East || worldDir == Direction.West;

            var go = new GameObject($"Corridor_{fromPos}_{worldDir}");
            go.transform.SetParent(gridParent, false);
            go.transform.position = new Vector3(midpoint.x, midpoint.y, 0.05f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetCorridorSprite();
            sr.sortingOrder = 1;

            float halfSprite = 0.675f;
            float gap = (horizontal ? cellWidth : cellHeight) - halfSprite * 2f;
            if (gap < 0.05f) gap = 0.05f;
            float length = gap + halfSprite * 0.3f;
            float width = corridorWidth;
            float scaleX = horizontal ? length : width;
            float scaleY = horizontal ? width : length;
            go.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        private static Vector2Int DirectionToOffset(Direction dir)
        {
            return dir switch
            {
                Direction.North => Vector2Int.up,
                Direction.South => Vector2Int.down,
                Direction.East => Vector2Int.right,
                Direction.West => Vector2Int.left,
                _ => Vector2Int.zero
            };
        }

        private static Sprite cachedCorridorSprite;

        private static Sprite GetCorridorSprite()
        {
            if (cachedCorridorSprite != null) return cachedCorridorSprite;

            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color stoneBase = new Color(0.38f, 0.34f, 0.28f);
            Color stoneDark = new Color(0.28f, 0.24f, 0.2f);
            Color mortarColor = new Color(0.15f, 0.12f, 0.1f);
            Color borderColor = new Color(0.45f, 0.4f, 0.32f);

            int border = 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (x < border || x >= size - border || y < border || y >= size - border)
                    {
                        tex.SetPixel(x, y, borderColor);
                        continue;
                    }

                    bool isMortarH = (y % 8 == 0);
                    bool isMortarV = (x % 6 == 0) && ((y / 8) % 2 == 0);
                    bool isMortarV2 = ((x + 3) % 6 == 0) && ((y / 8) % 2 == 1);

                    if (isMortarH || isMortarV || isMortarV2)
                    {
                        tex.SetPixel(x, y, mortarColor);
                    }
                    else
                    {
                        float noise = ((x * 13 + y * 7) % 11) / 11f;
                        Color col = Color.Lerp(stoneDark, stoneBase, 0.4f + noise * 0.4f);
                        tex.SetPixel(x, y, col);
                    }
                }
            }

            tex.Apply();
            cachedCorridorSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return cachedCorridorSprite;
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
        public Direction? EntryDirection { get; private set; }

        public RoomInstance(RoomData data, Vector2Int position, Direction? entryDirection = null)
        {
            Data = data;
            GridPosition = position;
            IsExplored = false;
            IsRevealed = false;
            EntryDirection = entryDirection;
        }

        public Direction RotateToWorld(Direction dataDir)
        {
            if (!EntryDirection.HasValue) return dataDir;
            int rotationSteps = EntryDirection.Value switch
            {
                Direction.North => 0,
                Direction.East => 1,
                Direction.South => 2,
                Direction.West => 3,
                _ => 0
            };
            int dirIndex = (int)dataDir;
            for (int i = 0; i < rotationSteps; i++)
            {
                dirIndex = dirIndex switch
                {
                    0 => 2, // North -> East
                    1 => 3, // South -> West
                    2 => 1, // East -> South
                    3 => 0, // West -> North
                    _ => dirIndex
                };
            }
            return (Direction)dirIndex;
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
