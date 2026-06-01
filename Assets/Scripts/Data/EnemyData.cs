using UnityEngine;

namespace DungeonArchitect.Data
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Dungeon Architect/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string enemyId;
        public string enemyName;
        public EnemyType enemyType;
        public int maxHP;
        public int damage;
        public int armor;
        public int xpReward;
        public int goldReward;
        public Sprite sprite;

        [TextArea(2, 4)]
        public string description;
    }
}
