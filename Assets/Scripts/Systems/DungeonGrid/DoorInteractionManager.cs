using UnityEngine;
using System.Collections.Generic;
using DungeonArchitect.Data;
using DungeonArchitect.Core;

namespace DungeonArchitect.Systems
{
    public class DoorInteractionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DungeonGridManager gridManager;
        [SerializeField] private RoomDraftManager draftManager;
        [SerializeField] private Camera mainCamera;

        [Header("Door Icon Settings")]
        [SerializeField] private Sprite doorIconSprite;
        [SerializeField] private Color doorIconColor = new Color(0.2f, 0.9f, 0.4f, 1f);
        [SerializeField] private float doorIconSize = 0.2f;

        [Header("Popup")]
        [SerializeField] private GameObject draftPopupPrefab;

        private Dictionary<Vector2Int, List<GameObject>> roomDoorIcons = new Dictionary<Vector2Int, List<GameObject>>();
        private Direction selectedDoor;
        private Vector2Int selectedRoomPos;
        private GameObject currentPopup;

        private static Sprite cachedCircleSprite;

        private void Start()
        {
            if (gridManager != null)
                gridManager.OnRoomPlaced += OnRoomPlaced;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
                if (GameManager.Instance.CurrentState == GameState.Exploring)
                    RefreshAllDoorIcons();
            }
        }

        private void Update()
        {
            if (currentPopup != null && Input.GetMouseButtonDown(1))
            {
                DismissPopup();
                GameManager.Instance.ChangeState(GameState.Exploring);
            }
        }

        private void OnDestroy()
        {
            if (gridManager != null)
                gridManager.OnRoomPlaced -= OnRoomPlaced;

            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Exploring)
                RefreshAllDoorIcons();
        }

        private void OnRoomPlaced(Vector2Int pos, RoomInstance room)
        {
            CreateDoorIconsForRoom(pos, room);
            RemoveBlockedDoorIcons();
        }

        public void RefreshAllDoorIcons()
        {
            ClearAllDoorIcons();

            for (int x = 0; x < gridManager.GridWidth; x++)
            {
                for (int y = 0; y < gridManager.GridHeight; y++)
                {
                    var pos = new Vector2Int(x, y);
                    var room = gridManager.GetRoomAt(pos);
                    if (room != null && room.IsExplored)
                        CreateDoorIconsForRoom(pos, room);
                }
            }

            if (GetTotalAvailableDoors() == 0
                && GameManager.Instance.CurrentState == GameState.Exploring
                && GameManager.Instance.RoomsPlacedThisFloor > 0)
                ShowGameOverPopup("No quedan puertas disponibles");
        }

        private int GetTotalAvailableDoors()
        {
            int count = 0;
            foreach (var kvp in roomDoorIcons)
                count += kvp.Value.Count;
            return count;
        }

        private void CreateDoorIconsForRoom(Vector2Int roomPos, RoomInstance room)
        {
            if (roomDoorIcons.ContainsKey(roomPos))
                ClearDoorIconsForRoom(roomPos);

            var icons = new List<GameObject>();
            var directions = new[] { Direction.North, Direction.South, Direction.East, Direction.West };

            foreach (var dir in directions)
            {
                if (!room.Data.HasDoor(dir)) continue;

                var adjacentPos = GetAdjacentPosition(roomPos, dir);
                if (!IsInBounds(adjacentPos)) continue;
                if (gridManager.GetRoomAt(adjacentPos) != null) continue;

                var icon = SpawnDoorIcon(roomPos, room, dir);
                if (icon != null)
                    icons.Add(icon);
            }

            roomDoorIcons[roomPos] = icons;
        }

        private GameObject SpawnDoorIcon(Vector2Int roomPos, RoomInstance room, Direction dir)
        {
            if (room.Visual == null) return null;

            var parentTransform = room.Visual.transform;
            var parentSR = room.Visual.GetComponent<SpriteRenderer>();

            Vector3 spriteCenter = Vector3.zero;
            float halfX = 0.5f;
            float halfY = 0.5f;

            if (parentSR != null && parentSR.sprite != null)
            {
                spriteCenter = (Vector3)parentSR.sprite.bounds.center;
                halfX = parentSR.sprite.bounds.extents.x;
                halfY = parentSR.sprite.bounds.extents.y;
            }

            float offsetX = dir == Direction.East ? halfX : dir == Direction.West ? -halfX : 0f;
            float offsetY = dir == Direction.North ? halfY : dir == Direction.South ? -halfY : 0f;

            var go = new GameObject($"DoorIcon_{dir}");
            go.transform.SetParent(parentTransform, false);
            go.transform.localPosition = spriteCenter + new Vector3(offsetX, offsetY, -0.05f);
            go.transform.localScale = Vector3.one * doorIconSize;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = doorIconSprite != null ? doorIconSprite : GetCircleSprite();
            sr.color = doorIconColor;
            sr.sortingOrder = 10;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.2f, 1.2f);

            var clickable = go.AddComponent<DoorIconClickable>();
            clickable.Initialize(dir, this, roomPos);

            return go;
        }

        public void OnDoorClicked(Direction dir, Vector2Int fromRoomPos)
        {
            selectedDoor = dir;
            selectedRoomPos = fromRoomPos;
            var adjacentPos = GetAdjacentPosition(fromRoomPos, dir);

            GameManager.Instance.SetPendingPlacementSlot(adjacentPos);
            GameManager.Instance.RequestRoomDraft();

            var validRooms = new List<RoomData>();
            foreach (var room in draftManager.CurrentDraft)
            {
                if (gridManager.CanPlaceRoom(room, adjacentPos))
                    validRooms.Add(room);
            }

            if (validRooms.Count == 0)
                validRooms = new List<RoomData>(draftManager.CurrentDraft);

            var stairManager = GameManager.Instance.Stairs;
            if (stairManager.StairSpawned && stairManager.StairRoomData != null)
            {
                if (!validRooms.Contains(stairManager.StairRoomData))
                    validRooms.Add(stairManager.StairRoomData);
            }

            ShowDraftPopup(fromRoomPos, dir, validRooms);
        }

        private void ShowDraftPopup(Vector2Int fromRoomPos, Direction dir, List<RoomData> rooms)
        {
            DismissPopup();
            SetAllDoorIconsVisible(false);
            SetCurrentRoomBorderVisible(false);
            SetAllCollectablesVisible(false);

            var worldPos = gridManager.GridToWorld(fromRoomPos);
            var roomVisual = gridManager.GetRoomAt(fromRoomPos)?.Visual;
            float spriteHalf = GetSpriteHalfSize(roomVisual);
            var offset = GetDirectionOffset(dir) * (spriteHalf + 1.5f);
            var popupWorldPos = worldPos + (Vector3)offset;

            if (draftPopupPrefab != null)
            {
                currentPopup = Instantiate(draftPopupPrefab);
                var popup = currentPopup.GetComponent<DraftPopupUI>();
                if (popup != null)
                    popup.Initialize(rooms, popupWorldPos, mainCamera, OnOfferChosen);
            }
            else
            {
                var go = new GameObject("DraftPopup");
                var popup = go.AddComponent<DraftPopupUI>();
                popup.Initialize(rooms, popupWorldPos, mainCamera, OnOfferChosen);
                currentPopup = go;
            }
        }

        private void OnOfferChosen(RoomOffer offer)
        {
            DismissPopup();

            var room = offer.room;
            var resources = GameManager.Instance.Resources;

            switch (offer.costType)
            {
                case RoomCostType.Key:
                    if (!resources.SpendKey())
                        { GameManager.Instance.ChangeState(GameState.Exploring); return; }
                    break;
                case RoomCostType.Gems:
                    if (!resources.SpendGems(offer.costAmount))
                        { GameManager.Instance.ChangeState(GameState.Exploring); return; }
                    break;
            }

            var draftCopy = new List<RoomData>(draftManager.CurrentDraft);
            var adjacentPos = GetAdjacentPosition(selectedRoomPos, selectedDoor);

            var playerPos = gridManager.PlayerPosition;
            int distance = gridManager.GetPathDistance(playerPos, selectedRoomPos);
            int timeCost = (distance >= 0 ? distance : 0) + 1;
            resources.SpendTime(timeCost);

            if (resources.CurrentTime <= 0)
            {
                ShowGameOverPopup("Te has quedado sin pasos");
                return;
            }

            int combatDamage = DraftPopupUI.RollDamage(room);
            if (combatDamage > 0)
                resources.TakeDamage(combatDamage);

            if (resources.CurrentHP <= 0)
            {
                ShowGameOverPopup("Has muerto en combate");
                return;
            }

            ClearCurrentRoomHighlight();

            gridManager.PlaceRoom(room, adjacentPos);
            gridManager.MovePlayerTo(adjacentPos);

            var instance = gridManager.GetRoomAt(adjacentPos);
            if (instance != null)
            {
                instance.Explore();
                var visual = instance.Visual?.GetComponent<RoomVisual>();
                if (visual != null)
                    visual.SetAsCurrentRoom(true);

                if (instance.Visual != null)
                    CollectableSpawner.SpawnCollectables(offer, instance.Visual);

                if (room.roomType == RoomType.Rest)
                    SpawnHealEffect(instance.Visual, 5);
            }

            var deckManager = GameManager.Instance.Deck;
            foreach (var r in draftCopy)
            {
                if (r != room)
                    deckManager.DiscardRoom(r);
            }
            draftManager.ClearDraft();

            GameManager.Instance.OnRoomPlaced();

            if (room.roomType == RoomType.Stair)
            {
                ShowFloorCompletePopup();
                return;
            }

            GameManager.Instance.ChangeState(GameState.Exploring);
        }

        private void ShowFloorCompletePopup()
        {
            ClearAllDoorIcons();
            GameManager.Instance.ChangeState(GameState.FloorComplete);

            var cam = mainCamera != null ? mainCamera : Camera.main;
            var popupWorldPos = cam.transform.position + Vector3.forward * 11f;

            var go = new GameObject("FloorCompletePopup");
            var popup = go.AddComponent<FloorCompletePopupUI>();
            popup.Initialize(
                GameManager.Instance.CurrentFloor,
                GameManager.Instance.RoomsPlacedThisFloor,
                popupWorldPos,
                cam,
                OnFloorCompleteConfirmed
            );
            currentPopup = go;
        }

        private void OnFloorCompleteConfirmed()
        {
            DismissPopup();
            GameManager.Instance.OnStairFound();
        }

        private void SpawnHealEffect(GameObject roomVisual, int amount)
        {
            GameManager.Instance.Resources.RestoreHP(amount);

            for (int i = 0; i < 3; i++)
            {
                var go = new GameObject($"HealHeart_{i}");
                go.transform.SetParent(roomVisual.transform, false);

                var sr = roomVisual.GetComponent<SpriteRenderer>();
                Vector3 center = sr != null && sr.sprite != null ? (Vector3)sr.sprite.bounds.center : Vector3.zero;
                float rx = Random.Range(-0.3f, 0.3f);
                float ry = Random.Range(-0.3f, 0.3f);
                go.transform.localPosition = center + new Vector3(rx, ry, -0.1f);
                go.transform.localScale = Vector3.one * 0.15f;

                var heartSR = go.AddComponent<SpriteRenderer>();
                heartSR.sprite = GetHeartSprite();
                heartSR.color = new Color(1f, 0.3f, 0.4f);
                heartSR.sortingOrder = 12;

                var flyer = go.AddComponent<HealHeartFlyer>();
                flyer.Initialize(i * 0.15f);
            }
        }

        private static Sprite heartSprite;
        private static Sprite GetHeartSprite()
        {
            if (heartSprite != null) return heartSprite;

            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, Color.clear);

            int cx = size / 2;
            int r = size / 5;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int lx = cx - r, rx2 = cx + r;
                    int top = size * 3 / 4;
                    bool inLeftCircle = (x - lx) * (x - lx) + (y - top) * (y - top) <= r * r;
                    bool inRightCircle = (x - rx2) * (x - rx2) + (y - top) * (y - top) <= r * r;
                    bool inTriangle = y < top && y >= size / 4 &&
                        x >= cx - (top - y) * (cx) / (top - size / 4) &&
                        x <= cx + (top - y) * (cx) / (top - size / 4);
                    if (inLeftCircle || inRightCircle || inTriangle)
                        tex.SetPixel(x, y, Color.white);
                }
            }
            tex.Apply();
            heartSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return heartSprite;
        }

        private void ShowGameOverPopup(string reason)
        {
            ClearAllDoorIcons();
            GameManager.Instance.ChangeState(GameState.GameOver);

            var cam = mainCamera != null ? mainCamera : Camera.main;
            var popupWorldPos = cam.transform.position + Vector3.forward * 11f;

            var go = new GameObject("GameOverPopup");
            var popup = go.AddComponent<GameOverPopupUI>();
            popup.Initialize(reason, GameManager.Instance.CurrentFloor, GameManager.Instance.RoomsPlacedThisFloor, popupWorldPos, cam);
            currentPopup = go;
        }

        public void DismissPopup()
        {
            if (currentPopup != null)
            {
                Destroy(currentPopup);
                currentPopup = null;
                SetAllDoorIconsVisible(true);
                SetCurrentRoomBorderVisible(true);
                SetAllCollectablesVisible(true);
            }
        }

        private void SetAllDoorIconsVisible(bool visible)
        {
            foreach (var kvp in roomDoorIcons)
            {
                foreach (var icon in kvp.Value)
                {
                    if (icon != null)
                        icon.SetActive(visible);
                }
            }
        }

        private void SetAllCollectablesVisible(bool visible)
        {
            var collectables = Object.FindObjectsOfType<RoomCollectable>(true);
            foreach (var c in collectables)
                c.gameObject.SetActive(visible);
        }

        private void SetCurrentRoomBorderVisible(bool visible)
        {
            for (int x = 0; x < gridManager.GridWidth; x++)
            {
                for (int y = 0; y < gridManager.GridHeight; y++)
                {
                    var room = gridManager.GetRoomAt(new Vector2Int(x, y));
                    if (room?.Visual == null) continue;
                    var visual = room.Visual.GetComponent<RoomVisual>();
                    if (visual != null)
                        visual.SetBorderVisible(visible);
                }
            }
        }

        private void ClearCurrentRoomHighlight()
        {
            for (int x = 0; x < gridManager.GridWidth; x++)
            {
                for (int y = 0; y < gridManager.GridHeight; y++)
                {
                    var room = gridManager.GetRoomAt(new Vector2Int(x, y));
                    if (room?.Visual == null) continue;
                    var visual = room.Visual.GetComponent<RoomVisual>();
                    if (visual != null)
                        visual.SetAsCurrentRoom(false);
                }
            }
        }

        private void RemoveBlockedDoorIcons()
        {
            var toUpdate = new List<Vector2Int>();

            foreach (var kvp in roomDoorIcons)
            {
                for (int i = kvp.Value.Count - 1; i >= 0; i--)
                {
                    var icon = kvp.Value[i];
                    if (icon == null)
                    {
                        kvp.Value.RemoveAt(i);
                        continue;
                    }

                    var clickable = icon.GetComponent<DoorIconClickable>();
                    if (clickable == null) continue;

                    var adjPos = GetAdjacentPosition(kvp.Key, clickable.Direction);
                    if (gridManager.GetRoomAt(adjPos) != null)
                    {
                        Destroy(icon);
                        kvp.Value.RemoveAt(i);
                    }
                }
            }
        }

        private void ClearDoorIconsForRoom(Vector2Int roomPos)
        {
            if (!roomDoorIcons.ContainsKey(roomPos)) return;

            foreach (var icon in roomDoorIcons[roomPos])
            {
                if (icon != null) Destroy(icon);
            }
            roomDoorIcons[roomPos].Clear();
        }

        private void ClearAllDoorIcons()
        {
            foreach (var kvp in roomDoorIcons)
            {
                foreach (var icon in kvp.Value)
                {
                    if (icon != null) Destroy(icon);
                }
            }
            roomDoorIcons.Clear();
        }

        private Vector2Int GetAdjacentPosition(Vector2Int pos, Direction dir)
        {
            return dir switch
            {
                Direction.North => pos + Vector2Int.up,
                Direction.South => pos + Vector2Int.down,
                Direction.East => pos + Vector2Int.right,
                Direction.West => pos + Vector2Int.left,
                _ => pos
            };
        }

        private Vector2 GetDirectionOffset(Direction dir)
        {
            return dir switch
            {
                Direction.North => Vector2.up,
                Direction.South => Vector2.down,
                Direction.East => Vector2.right,
                Direction.West => Vector2.left,
                _ => Vector2.zero
            };
        }

        private bool IsInBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < gridManager.GridWidth &&
                   pos.y >= 0 && pos.y < gridManager.GridHeight;
        }

        private float GetSpriteHalfSize(GameObject roomVisual)
        {
            if (roomVisual != null)
            {
                var sr = roomVisual.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                    return sr.sprite.bounds.extents.x * roomVisual.transform.lossyScale.x;
            }
            return 0.5f;
        }

        private Sprite GetCircleSprite()
        {
            if (cachedCircleSprite != null) return cachedCircleSprite;

            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = size / 2f;
            float radius = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= radius)
                        tex.SetPixel(x, y, Color.white);
                    else if (dist <= radius + 1f)
                        tex.SetPixel(x, y, new Color(1, 1, 1, radius + 1f - dist));
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();

            cachedCircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return cachedCircleSprite;
        }
    }
}
