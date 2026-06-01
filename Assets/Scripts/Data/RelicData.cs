using UnityEngine;

namespace DungeonArchitect.Data
{
    [CreateAssetMenu(fileName = "NewRelic", menuName = "Dungeon Architect/Relic Data")]
    public class RelicData : ScriptableObject
    {
        public string relicId;
        public string relicName;
        public Sprite sprite;
        public RoomRarity rarity;

        [Header("Passive Bonuses")]
        public float goldMultiplier = 1f;
        public float stairChanceBonus;
        public int maxTimeBonus;
        public int maxHPBonus;
        public float combatDamageMultiplier = 1f;

        [TextArea(2, 4)]
        public string description;
    }
}
