using UnityEngine;
using UnityEditor;
using System.IO;
using DungeonArchitect.Data;
using System.Collections.Generic;

namespace DungeonArchitect.EditorTools
{
    public class ContentGenerator : EditorWindow
    {
        [MenuItem("Dungeon Architect/Generate All Content")]
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
            Debug.Log("Dungeon Architect: All content generated (60 rooms, 10 enemies, 10 items, 15 relics, 4 classes)!");
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
            // Delete existing room assets
            string roomPath = "Assets/ScriptableObjects/Rooms";
            if (AssetDatabase.IsValidFolder(roomPath))
            {
                var guids = AssetDatabase.FindAssets("t:RoomData", new[] { roomPath });
                foreach (var guid in guids)
                    AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
            }

            // All 60 rooms matching the tileset
            // Format: (id, name, type, rarity, exits[], timeCost, gold, xp, keys, gems, gemCost, description)
            var rooms = new List<RoomDefinition>
            {
                // === ESPECIAL (Special) ===
                new(1, "Escaleras de Entrada", RoomType.Stair, RoomRarity.Common,
                    new[]{Direction.South, Direction.East}, 0, 0, 0, 0, 0, 0, "Punto de entrada al piso actual"),
                new(2, "Escaleras de Descenso", RoomType.Stair, RoomRarity.Common,
                    new[]{Direction.North, Direction.West}, 0, 0, 0, 0, 0, 0, "Descenso al siguiente piso"),

                // === SEGURO (Safe/Rest) ===
                new(3, "Santuario Antiguo", RoomType.Shrine, RoomRarity.Rare,
                    new[]{Direction.North, Direction.South}, 1, 0, 10, 0, 0, 0, "Un santuario que otorga bendiciones antiguas"),
                new(4, "Campamento Abandonado", RoomType.Rest, RoomRarity.Common,
                    new[]{Direction.North, Direction.South}, 2, 10, 5, 0, 0, 0, "Un lugar seguro para descansar brevemente"),
                new(5, "Portal Arcano", RoomType.Event, RoomRarity.Rare,
                    new[]{Direction.North, Direction.South, Direction.East, Direction.West}, 1, 0, 15, 0, 0, 0, "Un portal misterioso con destino incierto"),
                new(6, "Sala de Descanso", RoomType.Rest, RoomRarity.Common,
                    new[]{Direction.East, Direction.West}, 2, 0, 5, 0, 0, 0, "Recupera energia en esta sala tranquila"),
                new(7, "Biblioteca Silenciosa", RoomType.Shrine, RoomRarity.Common,
                    new[]{Direction.North, Direction.East}, 1, 0, 20, 0, 0, 0, "Tomos antiguos revelan conocimiento oculto"),
                new(8, "Fuente Curativa", RoomType.Rest, RoomRarity.Rare,
                    new[]{Direction.South, Direction.East}, 1, 0, 0, 0, 0, 0, "Aguas magicas que restauran la salud"),
                new(9, "Capilla Derruida", RoomType.Shrine, RoomRarity.Common,
                    new[]{Direction.North, Direction.West}, 1, 0, 10, 0, 0, 0, "Restos de un lugar sagrado"),
                new(10, "Camara de Meditacion", RoomType.Rest, RoomRarity.Common,
                    new[]{Direction.South, Direction.West}, 2, 0, 5, 0, 0, 0, "Meditar aqui restaura la calma"),
                new(11, "Jardin Subterraneo", RoomType.Rest, RoomRarity.Rare,
                    new[]{Direction.North, Direction.South, Direction.East}, 1, 5, 10, 0, 0, 0, "Un jardin imposible bajo la tierra"),
                new(12, "Refugio de Exploradores", RoomType.Rest, RoomRarity.Common,
                    new[]{Direction.East, Direction.West}, 2, 15, 5, 0, 0, 0, "Aventureros anteriores dejaron provisiones"),
                new(13, "Cripta Consagrada", RoomType.Shrine, RoomRarity.Rare,
                    new[]{Direction.North}, 1, 0, 15, 0, 1, 0, "Tierra sagrada que repele el mal"),

                // === JEFE (Boss) ===
                new(14, "Trono del Nigromante", RoomType.Boss, RoomRarity.Epic,
                    new[]{Direction.South}, 5, 100, 80, 1, 3, 0, "El nigromante invoca a sus siervos no-muertos"),
                new(15, "Guarida del Dragon", RoomType.Boss, RoomRarity.Legendary,
                    new[]{Direction.West}, 6, 150, 100, 0, 5, 0, "Un dragon ancestral protege su tesoro"),
                new(16, "Arena del Campeon", RoomType.Boss, RoomRarity.Epic,
                    new[]{Direction.North, Direction.South}, 5, 120, 90, 1, 3, 0, "Un campeon invicto aguarda su proximo rival"),
                new(17, "Camara del Rey Rata", RoomType.Boss, RoomRarity.Epic,
                    new[]{Direction.East}, 4, 80, 70, 2, 2, 0, "El rey de las ratas domina las cloacas"),
                new(18, "Laboratorio del Liche", RoomType.Boss, RoomRarity.Legendary,
                    new[]{Direction.North}, 6, 130, 100, 0, 4, 0, "Experimentos prohibidos cobran vida"),
                new(19, "Sala del Guardian de Piedra", RoomType.Boss, RoomRarity.Epic,
                    new[]{Direction.South, Direction.East}, 5, 100, 80, 1, 3, 0, "Un golem ancestral protege el paso"),

                // === TESORO (Treasure) ===
                new(20, "Camara del Tesoro", RoomType.Treasure, RoomRarity.Rare,
                    new[]{Direction.West}, 1, 80, 0, 0, 1, 0, "Una camara repleta de riquezas"),
                new(21, "Boveda Sellada", RoomType.Treasure, RoomRarity.Epic,
                    new[]{Direction.North}, 1, 120, 0, 0, 2, 1, "Requiere una llave para acceder"),
                new(22, "Cofre Maldito", RoomType.Treasure, RoomRarity.Rare,
                    new[]{Direction.East, Direction.West}, 2, 60, 0, 1, 0, 0, "El cofre promete riquezas... a un precio"),
                new(23, "Almacen Imperial", RoomType.Treasure, RoomRarity.Common,
                    new[]{Direction.North, Direction.South}, 1, 50, 0, 0, 0, 0, "Provisiones olvidadas del antiguo imperio"),
                new(24, "Sala de Reliquias", RoomType.Treasure, RoomRarity.Epic,
                    new[]{Direction.South}, 1, 40, 20, 0, 2, 1, "Reliquias de poder incalculable"),
                new(25, "Camara del Botin Perdido", RoomType.Treasure, RoomRarity.Rare,
                    new[]{Direction.North, Direction.East}, 1, 70, 0, 1, 1, 0, "Tesoros de aventureros caidos"),

                // === TRAMPA (Trap) ===
                new(26, "Pasillo de Flechas", RoomType.Trap, RoomRarity.Common,
                    new[]{Direction.East, Direction.West}, 1, 15, 5, 0, 0, 0, "Flechas disparan desde las paredes"),
                new(27, "Sala de Pinchos", RoomType.Trap, RoomRarity.Common,
                    new[]{Direction.North, Direction.South}, 1, 20, 5, 0, 0, 0, "El suelo esta cubierto de pinchos"),
                new(28, "Foso Oculto", RoomType.Trap, RoomRarity.Common,
                    new[]{Direction.North, Direction.East}, 2, 10, 5, 0, 0, 0, "El suelo se abre bajo tus pies"),
                new(29, "Camara de Gas", RoomType.Trap, RoomRarity.Rare,
                    new[]{Direction.South, Direction.West}, 2, 25, 10, 0, 0, 0, "Gas venenoso llena la habitacion"),
                new(30, "Corredor Giratorio", RoomType.Trap, RoomRarity.Rare,
                    new[]{Direction.East, Direction.West}, 2, 30, 10, 1, 0, 0, "Las paredes rotan aplastando a los incautos"),
                new(31, "Sala Inundada", RoomType.Trap, RoomRarity.Common,
                    new[]{Direction.North, Direction.South}, 2, 15, 5, 0, 0, 0, "Aguas heladas cubren el suelo"),
                new(32, "Camara de Llamas", RoomType.Trap, RoomRarity.Rare,
                    new[]{Direction.North, Direction.East, Direction.West}, 2, 35, 15, 0, 0, 0, "Chorros de fuego cruzan la sala"),
                new(33, "Laberinto de Espejos", RoomType.Trap, RoomRarity.Epic,
                    new[]{Direction.South, Direction.East, Direction.West}, 3, 40, 20, 0, 1, 0, "Los reflejos confunden y desorientan"),

                // === COMBATE PEQUEÑO (Small Combat) ===
                new(34, "Nido de Murcielagos", RoomType.Combat, RoomRarity.Common,
                    new[]{Direction.North, Direction.South}, 2, 15, 10, 0, 0, 0, "Una colonia de murcielagos agresivos"),
                new(35, "Madriguera Goblin", RoomType.Combat, RoomRarity.Common,
                    new[]{Direction.East, Direction.West}, 2, 20, 10, 0, 0, 0, "Goblins protegen su territorio"),
                new(36, "Sala de Esqueletos", RoomType.Combat, RoomRarity.Common,
                    new[]{Direction.North, Direction.East}, 2, 20, 12, 0, 0, 0, "Esqueletos se levantan de sus tumbas"),
                new(37, "Perreras Orcas", RoomType.Combat, RoomRarity.Common,
                    new[]{Direction.South, Direction.West}, 2, 25, 12, 0, 0, 0, "Wargs encadenados rugen al verte"),
                new(38, "Cripta Infestada", RoomType.Combat, RoomRarity.Common,
                    new[]{Direction.North, Direction.South}, 3, 25, 15, 0, 0, 0, "No-muertos acechan en la oscuridad"),
                new(39, "Camara de Aranas", RoomType.Combat, RoomRarity.Common,
                    new[]{Direction.East, Direction.West}, 2, 20, 10, 0, 0, 0, "Telaranas enormes cubren el techo"),
                new(40, "Cuartel Derruido", RoomType.Combat, RoomRarity.Common,
                    new[]{Direction.North, Direction.East, Direction.West}, 2, 25, 12, 0, 0, 0, "Soldados no-muertos aun montan guardia"),
                new(41, "Altar Profano", RoomType.Combat, RoomRarity.Rare,
                    new[]{Direction.South}, 3, 35, 20, 0, 0, 0, "Cultistas realizan un ritual oscuro"),
                new(42, "Sala de Hongos Gigantes", RoomType.Combat, RoomRarity.Rare,
                    new[]{Direction.North, Direction.South, Direction.East}, 2, 30, 15, 0, 0, 0, "Hongos toxicos cobran vida"),
                new(43, "Corral de Bestias", RoomType.Combat, RoomRarity.Rare,
                    new[]{Direction.East, Direction.West}, 3, 35, 18, 0, 0, 0, "Bestias salvajes enjauladas... ya no mas"),

                // === COMBATE GRANDE (Large Combat / Elite) ===
                new(44, "Sala de Guerra", RoomType.Elite, RoomRarity.Rare,
                    new[]{Direction.North, Direction.South, Direction.East}, 4, 60, 35, 0, 1, 0, "Un ejercito de no-muertos se prepara"),
                new(45, "Plaza Subterranea", RoomType.Elite, RoomRarity.Rare,
                    new[]{Direction.North, Direction.South, Direction.East, Direction.West}, 4, 55, 30, 0, 1, 0, "Enemigos llegan de todas direcciones"),
                new(46, "Tunel de Emboscada", RoomType.Elite, RoomRarity.Rare,
                    new[]{Direction.East, Direction.West}, 3, 50, 30, 1, 0, 0, "Una trampa perfecta para los incautos"),
                new(47, "Camara de Cultistas", RoomType.Elite, RoomRarity.Epic,
                    new[]{Direction.North, Direction.South}, 4, 65, 40, 0, 1, 0, "Un aquelarre completo te rodea"),
                new(48, "Fundicion Infernal", RoomType.Elite, RoomRarity.Epic,
                    new[]{Direction.South, Direction.East}, 4, 70, 40, 0, 1, 0, "Demonios forjan armas en lava"),
                new(49, "Sala de Invocacion", RoomType.Elite, RoomRarity.Epic,
                    new[]{Direction.North, Direction.West}, 4, 65, 35, 0, 2, 0, "Un portal se abre trayendo horrores"),
                new(50, "Catacumbas Profundas", RoomType.Elite, RoomRarity.Rare,
                    new[]{Direction.North, Direction.South, Direction.East}, 4, 55, 30, 1, 1, 0, "Legiones de no-muertos despiertan"),
                new(51, "Fortaleza Interior", RoomType.Elite, RoomRarity.Epic,
                    new[]{Direction.South, Direction.West}, 5, 75, 45, 0, 2, 0, "Los guardias de elite no dejan pasar a nadie"),

                // === EVENTO (Event) ===
                new(52, "Mercader Misterioso", RoomType.Shop, RoomRarity.Common,
                    new[]{Direction.North, Direction.South}, 1, 0, 0, 0, 0, 0, "Un comerciante aparece de las sombras"),
                new(53, "Estatua Parlante", RoomType.Event, RoomRarity.Common,
                    new[]{Direction.East, Direction.West}, 1, 0, 10, 0, 0, 0, "La estatua ofrece un trato enigmatico"),
                new(54, "Prision Antigua", RoomType.Event, RoomRarity.Common,
                    new[]{Direction.North, Direction.South}, 1, 20, 10, 1, 0, 0, "Prisioneros olvidados piden ayuda"),
                new(55, "Camara de Acertijos", RoomType.Puzzle, RoomRarity.Rare,
                    new[]{Direction.East, Direction.West}, 2, 50, 25, 1, 0, 0, "Resuelve el puzzle para obtener la recompensa"),
                new(56, "Pozo de los Deseos", RoomType.Event, RoomRarity.Common,
                    new[]{Direction.North, Direction.East}, 1, 0, 0, 0, 0, 0, "Lanza una moneda y pide un deseo"),
                new(57, "Sala de Sacrificios", RoomType.Event, RoomRarity.Rare,
                    new[]{Direction.South, Direction.West}, 2, 0, 20, 0, 1, 0, "Un altar exige un sacrificio"),
                new(58, "Observatorio Arcano", RoomType.Event, RoomRarity.Rare,
                    new[]{Direction.North, Direction.South, Direction.East}, 1, 0, 25, 0, 0, 0, "Las estrellas revelan el camino"),
                new(59, "Sala del Oraculo", RoomType.Event, RoomRarity.Epic,
                    new[]{Direction.North}, 1, 0, 30, 0, 1, 0, "El oraculo conoce tus secretos"),
                new(60, "Encrucijada de Runas", RoomType.Event, RoomRarity.Common,
                    new[]{Direction.North, Direction.South, Direction.East, Direction.West}, 1, 15, 15, 0, 0, 0, "Runas brillantes marcan el camino"),
            };

            foreach (var def in rooms)
            {
                var room = ScriptableObject.CreateInstance<RoomData>();
                room.roomId = $"room_{def.id:D3}";
                room.roomName = def.name;
                room.roomType = def.type;
                room.rarity = def.rarity;
                room.shape = InferShape(def.exits);
                room.doors = new List<Direction>(def.exits);
                room.timeCost = def.timeCost;
                room.goldReward = def.gold;
                room.xpReward = def.xp;
                room.keyReward = def.keys;
                room.gemReward = def.gems;
                room.gemCost = def.gemCost;
                room.description = def.description;

                string safeName = $"{def.id:D2}_{def.name.Replace(" ", "_")}";
                AssetDatabase.CreateAsset(room, $"Assets/ScriptableObjects/Rooms/{safeName}.asset");
            }

            Debug.Log($"Generated {rooms.Count} rooms");
        }

        private static RoomShape InferShape(Direction[] exits)
        {
            int count = exits.Length;
            if (count == 0) return RoomShape.DeadEnd;
            if (count == 1) return RoomShape.DeadEnd;
            if (count == 4) return RoomShape.Crossroad;
            if (count == 3) return RoomShape.TJunction;

            // 2 exits
            bool hasN = System.Array.Exists(exits, d => d == Direction.North);
            bool hasS = System.Array.Exists(exits, d => d == Direction.South);
            bool hasE = System.Array.Exists(exits, d => d == Direction.East);
            bool hasW = System.Array.Exists(exits, d => d == Direction.West);

            if ((hasN && hasS) || (hasE && hasW))
                return RoomShape.Corridor;
            return RoomShape.Corner;
        }

        private static void GenerateEnemies()
        {
            string path = "Assets/ScriptableObjects/Enemies";
            var guids = AssetDatabase.FindAssets("t:EnemyData", new[] { path });
            foreach (var guid in guids)
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));

            var enemies = new (string name, EnemyType type, int hp, int dmg, int armor, int xp, int gold)[]
            {
                ("Goblin Explorador", EnemyType.Goblin, 15, 5, 0, 10, 10),
                ("Goblin Guerrero", EnemyType.Goblin, 25, 8, 1, 15, 15),
                ("Esqueleto Soldado", EnemyType.Skeleton, 30, 7, 3, 15, 12),
                ("Esqueleto Arquero", EnemyType.Skeleton, 20, 10, 1, 12, 10),
                ("Cultista Oscuro", EnemyType.Cultist, 25, 12, 0, 20, 20),
                ("Sacerdote Cultista", EnemyType.Cultist, 35, 8, 2, 25, 25),
                ("Ogro Joven", EnemyType.Ogre, 50, 15, 2, 30, 30),
                ("Ogro Bruto", EnemyType.Ogre, 70, 20, 4, 40, 40),
                ("Jefe Goblin", EnemyType.Goblin, 40, 12, 2, 25, 25),
                ("Senor de los Huesos", EnemyType.Skeleton, 60, 14, 5, 35, 35),
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
            string path = "Assets/ScriptableObjects/Items";
            var guids = AssetDatabase.FindAssets("t:ItemData", new[] { path });
            foreach (var guid in guids)
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));

            var items = new (string name, ItemType type, int hp, int time, int keys, int cost, string desc)[]
            {
                ("Pocion de Salud Menor", ItemType.HealthPotion, 20, 0, 0, 15, "Restaura 20 HP"),
                ("Pocion de Salud", ItemType.HealthPotion, 40, 0, 0, 30, "Restaura 40 HP"),
                ("Pocion de Salud Mayor", ItemType.HealthPotion, 70, 0, 0, 50, "Restaura 70 HP"),
                ("Reloj de Arena Pequeno", ItemType.Hourglass, 0, 5, 0, 20, "Restaura 5 de Tiempo"),
                ("Reloj de Arena", ItemType.Hourglass, 0, 10, 0, 35, "Restaura 10 de Tiempo"),
                ("Gran Reloj de Arena", ItemType.Hourglass, 0, 20, 0, 60, "Restaura 20 de Tiempo"),
                ("Bomba", ItemType.Bomb, 0, 0, 0, 25, "Destruye una sala adyacente"),
                ("Mega Bomba", ItemType.Bomb, 0, 0, 0, 45, "Destruye multiples salas adyacentes"),
                ("Llave Esqueleto", ItemType.SkeletonKey, 0, 0, 1, 40, "Abre cualquier puerta cerrada"),
                ("Llave Maestra", ItemType.SkeletonKey, 0, 0, 3, 100, "Abre todas las puertas de un piso"),
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
                item.description = it.desc;

                string safeName = it.name.Replace(" ", "_");
                AssetDatabase.CreateAsset(item, $"Assets/ScriptableObjects/Items/{safeName}.asset");
            }
        }

        private static void GenerateRelics()
        {
            string path = "Assets/ScriptableObjects/Relics";
            var guids = AssetDatabase.FindAssets("t:RelicData", new[] { path });
            foreach (var guid in guids)
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));

            var relics = new (string name, RoomRarity rarity, float goldMult, float stairBonus, int timeBonus, int hpBonus, float combatMult, string desc)[]
            {
                ("Moneda de la Suerte", RoomRarity.Common, 1.2f, 0f, 0, 0, 1f, "+20% Oro de todas las fuentes"),
                ("Brujula del Explorador", RoomRarity.Rare, 1f, 0.03f, 0, 0, 1f, "Aumenta la probabilidad de encontrar escaleras"),
                ("Antorcha Eterna", RoomRarity.Epic, 1f, 0f, 25, 0, 1f, "+25 Tiempo Maximo"),
                ("Corazon de Hierro", RoomRarity.Common, 1f, 0f, 0, 20, 1f, "+20 HP Maximo"),
                ("Guantelete del Guerrero", RoomRarity.Rare, 1f, 0f, 0, 0, 1.3f, "+30% Dano en Combate"),
                ("Corona de la Codicia", RoomRarity.Epic, 1.5f, 0f, 0, 0, 1f, "+50% Oro de todas las fuentes"),
                ("Ganzua del Ladron", RoomRarity.Common, 1f, 0f, 0, 0, 1f, "+1 Llave al inicio de cada piso"),
                ("Mapa Antiguo", RoomRarity.Rare, 1f, 0.05f, 0, 0, 1f, "Gran aumento de probabilidad de escaleras"),
                ("Caliz de Sangre", RoomRarity.Epic, 1f, 0f, 0, -10, 1.5f, "-10 HP Max pero +50% dano"),
                ("Reloj de las Eras", RoomRarity.Legendary, 1f, 0f, 50, 0, 1f, "+50 Tiempo Maximo"),
                ("Escama de Dragon", RoomRarity.Rare, 1f, 0f, 0, 30, 1f, "+30 HP Maximo"),
                ("Toque de Midas", RoomRarity.Legendary, 2f, 0f, 0, 0, 1f, "Duplica todo el Oro obtenido"),
                ("Botas del Caminante", RoomRarity.Common, 1f, 0f, 5, 0, 1f, "+5 Tiempo Maximo"),
                ("Espada Maldita", RoomRarity.Rare, 1f, 0f, 0, 0, 1.8f, "+80% dano pero salas cuestan +1 Tiempo"),
                ("Pluma del Fenix", RoomRarity.Legendary, 1f, 0f, 0, 0, 1f, "Revive una vez por partida con 50% HP"),
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
                relic.maxTimeBonus = r.timeBonus;
                relic.maxHPBonus = r.hpBonus;
                relic.combatDamageMultiplier = r.combatMult;
                relic.description = r.desc;

                string safeName = r.name.Replace(" ", "_").Replace("'", "");
                AssetDatabase.CreateAsset(relic, $"Assets/ScriptableObjects/Relics/{safeName}.asset");
            }
        }

        private static void GenerateClasses()
        {
            string path = "Assets/ScriptableObjects/Classes";
            var guids = AssetDatabase.FindAssets("t:ClassData", new[] { path });
            foreach (var guid in guids)
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));

            var classes = new (string name, CharacterClass cls, int hp, int time, int gold, int keys, int gems, float dmg, float goldBonus, string desc)[]
            {
                ("Guerrero", CharacterClass.Warrior, 120, 45, 0, 0, 0, 1.2f, 1f, "Alto HP y mas dano en combate"),
                ("Picaro", CharacterClass.Rogue, 80, 50, 20, 2, 0, 1f, 1.2f, "Llaves extra y bonus de oro"),
                ("Mago", CharacterClass.Mage, 70, 50, 0, 0, 3, 1f, 1f, "Empieza con gemas, ve mas opciones en el draft"),
                ("Explorador", CharacterClass.Explorer, 90, 65, 10, 1, 0, 0.9f, 1f, "Mucho mas tiempo para explorar"),
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
            string path = "Assets/ScriptableObjects/Decks";
            var guids = AssetDatabase.FindAssets("t:DeckData", new[] { path });
            foreach (var guid in guids)
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));

            var deck = ScriptableObject.CreateInstance<DeckData>();
            deck.deckName = "Mazo Inicial";
            deck.description = "Un mazo equilibrado de 30 salas para empezar.";
            AssetDatabase.CreateAsset(deck, "Assets/ScriptableObjects/Decks/Mazo_Inicial.asset");
        }

        private class RoomDefinition
        {
            public int id;
            public string name;
            public RoomType type;
            public RoomRarity rarity;
            public Direction[] exits;
            public int timeCost;
            public int gold;
            public int xp;
            public int keys;
            public int gems;
            public int gemCost;
            public string description;

            public RoomDefinition(int id, string name, RoomType type, RoomRarity rarity,
                Direction[] exits, int timeCost, int gold, int xp, int keys, int gems, int gemCost, string description)
            {
                this.id = id;
                this.name = name;
                this.type = type;
                this.rarity = rarity;
                this.exits = exits;
                this.timeCost = timeCost;
                this.gold = gold;
                this.xp = xp;
                this.keys = keys;
                this.gems = gems;
                this.gemCost = gemCost;
                this.description = description;
            }
        }
    }
}
