using UnityEngine;
using System.Collections.Generic;
using DungeonArchitect.Data;
using DungeonArchitect.Core;

namespace DungeonArchitect.Systems
{
    public class RoomDraftManager : MonoBehaviour
    {
        [Header("Draft Settings")]
        [SerializeField] private int draftSize = 3;

        private List<RoomData> currentDraft = new List<RoomData>();
        private RoomData selectedRoom;

        public IReadOnlyList<RoomData> CurrentDraft => currentDraft;
        public RoomData SelectedRoom => selectedRoom;

        public event System.Action<List<RoomData>> OnDraftGenerated;
        public event System.Action<RoomData> OnRoomSelected;
        public event System.Action OnDraftCleared;

        public void GenerateDraft()
        {
            currentDraft.Clear();
            selectedRoom = null;

            var deckManager = GameManager.Instance.Deck;
            var drawnRooms = deckManager.DrawRooms(draftSize);

            foreach (var room in drawnRooms)
            {
                currentDraft.Add(room);
            }

            if (currentDraft.Count == 0)
            {
                deckManager.ReshuffleDeck();
                drawnRooms = deckManager.DrawRooms(draftSize);
                foreach (var room in drawnRooms)
                    currentDraft.Add(room);
            }

            OnDraftGenerated?.Invoke(currentDraft);
        }

        public void SelectRoom(int index)
        {
            if (index < 0 || index >= currentDraft.Count) return;

            selectedRoom = currentDraft[index];
            OnRoomSelected?.Invoke(selectedRoom);
            GameManager.Instance.OnRoomSelected(selectedRoom);
        }

        public void SelectRoom(RoomData room)
        {
            if (!currentDraft.Contains(room)) return;

            selectedRoom = room;
            OnRoomSelected?.Invoke(selectedRoom);
            GameManager.Instance.OnRoomSelected(selectedRoom);
        }

        public void ClearDraft()
        {
            currentDraft.Clear();
            selectedRoom = null;
            OnDraftCleared?.Invoke();
        }

        public bool CanAffordRoom(RoomData room)
        {
            var resources = GameManager.Instance.Resources;
            return resources.Gems >= room.gemCost;
        }

        public void SetDraftSize(int size)
        {
            draftSize = Mathf.Max(2, size);
        }
    }
}
