using UnityEngine;
using System.Collections.Generic;
using DungeonArchitect.Data;

namespace DungeonArchitect.Utils
{
    public static class RoomFactory
    {
        public static List<Direction> GetDoorsForShape(RoomShape shape)
        {
            return shape switch
            {
                RoomShape.Corridor => new List<Direction> { Direction.East, Direction.West },
                RoomShape.Corner => new List<Direction> { Direction.North, Direction.East },
                RoomShape.TJunction => new List<Direction> { Direction.North, Direction.East, Direction.West },
                RoomShape.Crossroad => new List<Direction> { Direction.North, Direction.South, Direction.East, Direction.West },
                RoomShape.DeadEnd => new List<Direction> { Direction.North },
                RoomShape.Hall => new List<Direction> { Direction.North, Direction.South },
                _ => new List<Direction> { Direction.North, Direction.South }
            };
        }

        public static RoomRarity RollRarity()
        {
            float roll = Random.value * 100f;
            if (roll <= 2f) return RoomRarity.Legendary;
            if (roll <= 10f) return RoomRarity.Epic;
            if (roll <= 30f) return RoomRarity.Rare;
            return RoomRarity.Common;
        }

        public static float GetRarityWeight(RoomRarity rarity)
        {
            return rarity switch
            {
                RoomRarity.Common => 0.70f,
                RoomRarity.Rare => 0.20f,
                RoomRarity.Epic => 0.08f,
                RoomRarity.Legendary => 0.02f,
                _ => 0.70f
            };
        }
    }
}
