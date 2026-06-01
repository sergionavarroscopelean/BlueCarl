using UnityEngine;

namespace DungeonArchitect.Data
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Dungeon Architect/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string itemId;
        public string itemName;
        public ItemType itemType;
        public Sprite sprite;
        public int goldCost;

        [Header("Effects")]
        public int hpRestore;
        public int timeRestore;
        public int keysGiven;

        [TextArea(2, 4)]
        public string description;
    }
}
