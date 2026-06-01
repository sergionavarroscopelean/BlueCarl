using UnityEngine;

namespace DungeonArchitect.Data
{
    [CreateAssetMenu(fileName = "NewClass", menuName = "Dungeon Architect/Class Data")]
    public class ClassData : ScriptableObject
    {
        public string classId;
        public string className;
        public CharacterClass characterClass;
        public Sprite portrait;

        [Header("Starting Stats")]
        public int startingHP = 100;
        public int startingTime = 50;
        public int startingGold;
        public int startingKeys;
        public int startingGems;

        [Header("Bonuses")]
        public float combatDamageBonus = 1f;
        public float goldBonus = 1f;
        public int extraDraftOptions;

        [TextArea(2, 4)]
        public string description;
    }
}
