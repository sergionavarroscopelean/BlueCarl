using UnityEngine;
using System.Collections.Generic;

namespace DungeonArchitect.Data
{
    [CreateAssetMenu(fileName = "NewDeck", menuName = "Dungeon Architect/Deck Data")]
    public class DeckData : ScriptableObject
    {
        public string deckName;
        public List<RoomData> rooms = new List<RoomData>();
        public const int MaxDeckSize = 30;

        [TextArea(2, 4)]
        public string description;

        public bool IsFull => rooms.Count >= MaxDeckSize;

        public void AddRoom(RoomData room)
        {
            if (!IsFull)
                rooms.Add(room);
        }

        public void RemoveRoom(RoomData room)
        {
            rooms.Remove(room);
        }

        public int GetRoomTypeCount(RoomType type)
        {
            int count = 0;
            foreach (var room in rooms)
            {
                if (room.roomType == type)
                    count++;
            }
            return count;
        }
    }
}
