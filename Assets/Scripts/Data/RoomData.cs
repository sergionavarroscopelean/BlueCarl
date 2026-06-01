using UnityEngine;
using System.Collections.Generic;

namespace DungeonArchitect.Data
{
    [CreateAssetMenu(fileName = "NewRoom", menuName = "Dungeon Architect/Room Data")]
    public class RoomData : ScriptableObject
    {
        public string roomId;
        public string roomName;
        public RoomRarity rarity;
        public RoomType roomType;
        public RoomShape shape;
        public int gemCost;
        public int timeCost;
        public List<Direction> doors = new List<Direction>();

        [Header("Rewards")]
        public int goldReward;
        public int xpReward;
        public int keyReward;
        public int gemReward;

        [Header("Combat")]
        public EnemyEncounter enemyEncounter;

        [Header("Visuals")]
        public Sprite roomSprite;
        public Sprite cardSprite;
        public Color roomColor = Color.white;

        [TextArea(2, 4)]
        public string description;

        public bool HasDoor(Direction direction)
        {
            return doors.Contains(direction);
        }

        public static Direction GetOppositeDirection(Direction dir)
        {
            return dir switch
            {
                Direction.North => Direction.South,
                Direction.South => Direction.North,
                Direction.East => Direction.West,
                Direction.West => Direction.East,
                _ => Direction.North
            };
        }
    }

    [System.Serializable]
    public class EnemyEncounter
    {
        public EnemyData[] enemies;
        public int difficulty;
    }
}
