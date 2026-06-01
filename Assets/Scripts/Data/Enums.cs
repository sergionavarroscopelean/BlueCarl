namespace DungeonArchitect.Data
{
    public enum RoomType
    {
        Combat,
        Elite,
        Treasure,
        Shop,
        Rest,
        Event,
        Trap,
        Puzzle,
        Shrine,
        Stair,
        Boss
    }

    public enum RoomRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    public enum RoomShape
    {
        Corridor,
        Corner,
        TJunction,
        Crossroad,
        DeadEnd,
        Hall
    }

    public enum Direction
    {
        North,
        South,
        East,
        West
    }

    public enum CharacterClass
    {
        Warrior,
        Rogue,
        Mage,
        Explorer
    }

    public enum EnemyType
    {
        Goblin,
        Skeleton,
        Cultist,
        Ogre
    }

    public enum ItemType
    {
        HealthPotion,
        Hourglass,
        Bomb,
        SkeletonKey
    }

    public enum GameState
    {
        MainMenu,
        DeckBuilding,
        Exploring,
        RoomDraft,
        RoomPlacement,
        ResolvingRoom,
        Combat,
        Shop,
        Event,
        BossEncounter,
        FloorComplete,
        GameOver,
        Victory
    }
}
