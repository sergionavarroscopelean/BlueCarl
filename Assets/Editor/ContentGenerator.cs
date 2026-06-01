using UnityEngine;
using UnityEditor;
using System.IO;
using DungeonArchitect.Data;
using System.Collections.Generic;

namespace DungeonArchitect.EditorTools
{
    public class ContentGenerator : EditorWindow
    {
        [MenuItem("Dungeon Architect/Generate Starter Content")]
        public static void GenerateAll()
        {
            CreateDirectories();
            GenerateRooms();
            GenerateEnemies();
            GenerateItems();
            GenerateRelics();
            GenerateClasses();
            GenerateStarterDeck();
            AssetDatabase.Refresh();
            Debug.Log("Dungeon Architect: All starter content generated!");
        }

        private static void CreateDirectories()
        {
            string[] dirs = {
                "Assets/ScriptableObjects/Rooms",
                "Assets/ScriptableObjects/Enemies",
                "Assets/ScriptableObjects/Items",
                "Assets/ScriptableObjects/Relics",
                "Assets/ScriptableObjects/Classes",
                "Assets/ScriptableObjects/Decks"
            };
            foreach (var dir in dirs)
            {
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    var parts = dir.Split('/');
                    string current = parts[0];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string next = current + "/" + parts[i];
                        if (!AssetDatabase.IsValidFolder(next))
                            AssetDatabase.CreateFolder(current, parts[i]);
                        current = next;
                    }
                }
            }
        }

        private static void GenerateRooms()
        {
            var rooms = new (string name, RoomType type, RoomRarity rarity, RoomShape shape, int gold, int xp, int keys, int gems, int time)[]
            {
                ("Goblin Nest", RoomType.Combat, RoomRarity.Common, RoomShape.Corridor, 20, 10, 0, 0, 2),
                ("Skeleton Crypt", RoomType.Combat, RoomRarity.Common, RoomShape.Hall, 25, 15, 0, 0, 2),
                ("Cultist Lair", RoomType.Combat, RoomRarity.Common, RoomShape.Corner, 30, 15, 0, 0, 3),
                ("Ogre Den", RoomType.Combat, RoomRarity.Rare, RoomShape.DeadEnd, 50, 25, 0, 0, 4),
                ("Bandit Camp", RoomType.Combat, RoomRarity.Common, RoomShape.TJunction, 35, 20, 0, 0, 3),
                ("Spider Cavern", RoomType.Combat, RoomRarity.Common, RoomShape.Corridor, 20, 10, 0, 0, 2),
                ("Undead Pit", RoomType.Combat, RoomRarity.Rare, RoomShape.Crossroad, 40, 20, 0, 0, 3),
                ("Dragon Wyrmling", RoomType.Combat, RoomRarity.Epic, RoomShape.DeadEnd, 80, 50, 0, 1, 5),
                ("Dark Knight", RoomType.Elite, RoomRarity.Rare, RoomShape.Hall, 60, 40, 0, 1, 4),
                ("Lich Chamber", RoomType.Elite, RoomRarity.Epic, RoomShape.DeadEnd, 100, 60, 1, 2, 5),
                ("Gold Vault", RoomType.Treasure, RoomRarity.Common, RoomShape.Corridor, 50, 0, 0, 0, 1),
                ("Gem Cache", RoomType.Treasure, RoomRarity.Rare, RoomShape.DeadEnd, 30, 0, 0, 2, 1),
                ("Ancient Treasury", RoomType.Treasure, RoomRarity.Epic, RoomShape.Corner, 100, 0, 1, 1, 1),
                ("Key Chamber", RoomType.Treasure, RoomRarity.Common, RoomShape.Hall, 10, 0, 1, 0, 1),
                ("Wandering Merchant", RoomType.Shop, RoomRarity.Common, RoomShape.TJunction, 0, 0, 0, 0, 1),
                ("Black Market", RoomType.Shop, RoomRarity.Rare, RoomShape.Corner, 0, 0, 0, 0, 1),
                ("Campfire", RoomType.Rest, RoomRarity.Common, RoomShape.Corridor, 0, 5, 0, 0, 2),
                ("Hot Springs", RoomType.Rest, RoomRarity.Rare, RoomShape.DeadEnd, 0, 10, 0, 0, 1),
                ("Mysterious Altar", RoomType.Event, RoomRarity.Common, RoomShape.TJunction, 0, 0, 0, 0, 1),
                ("Fortune Teller", RoomType.Event, RoomRarity.Rare, RoomShape.Corner, 0, 0, 0, 0, 1),
                ("Spike Trap", RoomType.Trap, RoomRarity.Common, RoomShape.Hall, 20, 0, 0, 0, 1),
                ("Poison Gas", RoomType.Trap, RoomRarity.Common, RoomShape.Corridor, 15, 0, 1, 0, 1),
                ("Ancient Puzzle", RoomType.Puzzle, RoomRarity.Common, RoomShape.Corner, 40, 20, 1, 0, 2),
                ("Runic Lock", RoomType.Puzzle, RoomRarity.Rare, RoomShape.DeadEnd, 60, 30, 0, 1, 3),
                ("Shrine of Power", RoomType.Shrine, RoomRarity.Rare, RoomShape.DeadEnd, 0, 0, 0, 0, 1),
                ("Shrine of Time", RoomType.Shrine, RoomRarity.Epic, RoomShape.DeadEnd, 0, 0, 0, 0, 0),
            };

            for (int i = 0; i < rooms.Length; i++)
            {
                var r = rooms[i];
                var room = ScriptableObject.CreateInstance<RoomData>();
                room.roomId = $"room_{i:D3}";
                room.roomName = r.name;
                room.roomType = r.type;
                room.rarity = r.rarity;
                room.shape = r.shape;
                room.goldReward = r.gold;
                room.xpReward = r.xp;
                room.keyReward = r.keys;
                room.gemReward = r.gems;
                room.timeCost = r.time;
                room.gemCost = r.rarity == RoomRarity.Legendary ? 1 : 0;
                room.doors = GetDoorsForShape(r.shape);

                string safeName = r.name.Replace(" ", "_");
                AssetDatabase.CreateAsset(room, $"Assets/ScriptableObjects/Rooms/{safeName}.asset");
            }
        }

        private static void GenerateEnemies()
        {
            var enemies = new (string name, EnemyType type, int hp, int dmg, int armor, int xp, int gold)[]
            {
                ("Goblin Scout", EnemyType.Goblin, 15, 5, 0, 10, 10),
                ("Goblin Warrior", EnemyType.Goblin, 25, 8, 1, 15, 15),
                ("Skeleton Soldier", EnemyType.Skeleton, 30, 7, 3, 15, 12),
                ("Skeleton Archer", EnemyType.Skeleton, 20, 10, 1, 12, 10),
                ("Dark Cultist", EnemyType.Cultist, 25, 12, 0, 20, 20),
                ("Cultist Priest", EnemyType.Cultist, 35, 8, 2, 25, 25),
                ("Young Ogre", EnemyType.Ogre, 50, 15, 2, 30, 30),
                ("Ogre Brute", EnemyType.Ogre, 70, 20, 4, 40, 40),
                ("Goblin Chief", EnemyType.Goblin, 40, 12, 2, 25, 25),
                ("Bone Lord", EnemyType.Skeleton, 60, 14, 5, 35, 35),
            };

            for (int i = 0; i < enemies.Length; i++)
            {
                var e = enemies[i];
                var enemy = ScriptableObject.CreateInstance<EnemyData>();
                enemy.enemyId = $"enemy_{i:D3}";
                enemy.enemyName = e.name;
                enemy.enemyType = e.type;
                enemy.maxHP = e.hp;
                enemy.damage = e.dmg;
                enemy.armor = e.armor;
                enemy.xpReward = e.xp;
                enemy.goldReward = e.gold;

                string safeName = e.name.Replace(" ", "_");
                AssetDatabase.CreateAsset(enemy, $"Assets/ScriptableObjects/Enemies/{safeName}.asset");
            }
        }

        private static void GenerateItems()
        {
            var items = new (string name, ItemType type, int hp, int time, int keys, int cost)[]
            {
                ("Minor Health Potion", ItemType.HealthPotion, 20, 0, 0, 15),
                ("Health Potion", ItemType.HealthPotion, 40, 0, 0, 30),
                ("Greater Health Potion", ItemType.HealthPotion, 70, 0, 0, 50),
                ("Small Hourglass", ItemType.Hourglass, 0, 5, 0, 20),
                ("Hourglass", ItemType.Hourglass, 0, 10, 0, 35),
                ("Grand Hourglass", ItemType.Hourglass, 0, 20, 0, 60),
                ("Bomb", ItemType.Bomb, 0, 0, 0, 25),
                ("Mega Bomb", ItemType.Bomb, 0, 0, 0, 45),
                ("Skeleton Key", ItemType.SkeletonKey, 0, 0, 1, 40),
                ("Master Key", ItemType.SkeletonKey, 0, 0, 3, 100),
            };

            for (int i = 0; i < items.Length; i++)
            {
                var it = items[i];
                var item = ScriptableObject.CreateInstance<ItemData>();
                item.itemId = $"item_{i:D3}";
                item.itemName = it.name;
                item.itemType = it.type;
                item.hpRestore = it.hp;
                item.timeRestore = it.time;
                item.keysGiven = it.keys;
                item.goldCost = it.cost;

                string safeName = it.name.Replace(" ", "_");
                AssetDatabase.CreateAsset(item, $"Assets/ScriptableObjects/Items/{safeName}.asset");
            }
        }

        private static void GenerateRelics()
        {
            var relics = new (string name, RoomRarity rarity, float goldMult, float stairBonus, int timebonus, int hpBonus, float combatMult, string desc)[]
            {
                ("Lucky Coin", RoomRarity.Common, 1.2f, 0f, 0, 0, 1f, "+20% Gold from all sources"),
                ("Explorer's Compass", RoomRarity.Rare, 1f, 0.03f, 0, 0, 1f, "Increases stair spawn chance"),
                ("Torch of Eternity", RoomRarity.Epic, 1f, 0f, 25, 0, 1f, "+25 Maximum Time"),
                ("Iron Heart", RoomRarity.Common, 1f, 0f, 0, 20, 1f, "+20 Maximum HP"),
                ("Warrior's Gauntlet", RoomRarity.Rare, 1f, 0f, 0, 0, 1.3f, "+30% Combat Damage"),
                ("Crown of Greed", RoomRarity.Epic, 1.5f, 0f, 0, 0, 1f, "+50% Gold from all sources"),
                ("Thief's Lockpick", RoomRarity.Common, 1f, 0f, 0, 0, 1f, "Start each floor with +1 Key"),
                ("Ancient Map", RoomRarity.Rare, 1f, 0.05f, 0, 0, 1f, "Greatly increases stair spawn chance"),
                ("Blood Chalice", RoomRarity.Epic, 1f, 0f, 0, -10, 1.5f, "-10 Max HP but +50% damage"),
                ("Hourglass of Ages", RoomRarity.Legendary, 1f, 0f, 50, 0, 1f, "+50 Maximum Time"),
                ("Dragon Scale", RoomRarity.Rare, 1f, 0f, 0, 30, 1f, "+30 Maximum HP"),
                ("Midas Touch", RoomRarity.Legendary, 2f, 0f, 0, 0, 1f, "Double all Gold earned"),
                ("Pathfinder's Boots", RoomRarity.Common, 1f, 0f, 5, 0, 1f, "+5 Maximum Time"),
                ("Cursed Blade", RoomRarity.Rare, 1f, 0f, 0, 0, 1.8f, "+80% damage but rooms cost +1 Time"),
                ("Phoenix Feather", RoomRarity.Legendary, 1f, 0f, 0, 0, 1f, "Revive once per run with 50% HP"),
            };

            for (int i = 0; i < relics.Length; i++)
            {
                var r = relics[i];
                var relic = ScriptableObject.CreateInstance<RelicData>();
                relic.relicId = $"relic_{i:D3}";
                relic.relicName = r.name;
                relic.rarity = r.rarity;
                relic.goldMultiplier = r.goldMult;
                relic.stairChanceBonus = r.stairBonus;
                relic.maxTimeBonus = r.timebonus;
                relic.maxHPBonus = r.hpBonus;
                relic.combatDamageMultiplier = r.combatMult;
                relic.description = r.desc;

                string safeName = r.name.Replace(" ", "_").Replace("'", "");
                AssetDatabase.CreateAsset(relic, $"Assets/ScriptableObjects/Relics/{safeName}.asset");
            }
        }

        private static void GenerateClasses()
        {
            var classes = new (string name, CharacterClass cls, int hp, int time, int gold, int keys, int gems, float dmg, float goldBonus, string desc)[]
            {
                ("Warrior", CharacterClass.Warrior, 120, 45, 0, 0, 0, 1.2f, 1f, "High HP, deals more damage in combat"),
                ("Rogue", CharacterClass.Rogue, 80, 50, 20, 2, 0, 1f, 1.2f, "Extra keys and gold bonus"),
                ("Mage", CharacterClass.Mage, 70, 50, 0, 0, 3, 1f, 1f, "Starts with gems, sees more draft options"),
                ("Explorer", CharacterClass.Explorer, 90, 65, 10, 1, 0, 0.9f, 1f, "Much more time to explore the dungeon"),
            };

            for (int i = 0; i < classes.Length; i++)
            {
                var c = classes[i];
                var classData = ScriptableObject.CreateInstance<ClassData>();
                classData.classId = $"class_{i:D3}";
                classData.className = c.name;
                classData.characterClass = c.cls;
                classData.startingHP = c.hp;
                classData.startingTime = c.time;
                classData.startingGold = c.gold;
                classData.startingKeys = c.keys;
                classData.startingGems = c.gems;
                classData.combatDamageBonus = c.dmg;
                classData.goldBonus = c.goldBonus;
                classData.description = c.desc;

                AssetDatabase.CreateAsset(classData, $"Assets/ScriptableObjects/Classes/{c.name}.asset");
            }
        }

        private static void GenerateStarterDeck()
        {
            var deck = ScriptableObject.CreateInstance<DeckData>();
            deck.deckName = "Starter Deck";
            deck.description = "A balanced starter deck with 30 rooms.";
            AssetDatabase.CreateAsset(deck, "Assets/ScriptableObjects/Decks/StarterDeck.asset");
        }

        private static List<Direction> GetDoorsForShape(RoomShape shape)
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
    }
}
