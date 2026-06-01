using UnityEngine;
using System.Collections.Generic;
using DungeonArchitect.Data;

namespace DungeonArchitect.Utils
{
    [CreateAssetMenu(fileName = "RoomSpriteMapper", menuName = "Dungeon Architect/Room Sprite Mapper")]
    public class RoomSpriteMapper : ScriptableObject
    {
        [System.Serializable]
        public class RoomSpriteEntry
        {
            public int roomId;
            public string roomName;
            public Sprite sprite;
        }

        public List<RoomSpriteEntry> entries = new List<RoomSpriteEntry>();

        private Dictionary<int, Sprite> spriteMap;

        public void Initialize()
        {
            spriteMap = new Dictionary<int, Sprite>();
            foreach (var entry in entries)
            {
                if (entry.sprite != null)
                    spriteMap[entry.roomId] = entry.sprite;
            }
        }

        public Sprite GetSprite(int roomId)
        {
            if (spriteMap == null) Initialize();
            spriteMap.TryGetValue(roomId, out var sprite);
            return sprite;
        }

        public Sprite GetSpriteByName(string roomName)
        {
            foreach (var entry in entries)
            {
                if (entry.roomName == roomName)
                    return entry.sprite;
            }
            return null;
        }
    }
}
