using UnityEngine;
using UnityEditor;
using DungeonArchitect.Utils;

namespace DungeonArchitect.EditorTools
{
    public class TilesetImporter : EditorWindow
    {
        [MenuItem("Dungeon Architect/Setup Room Sprite Mapper")]
        public static void SetupSpriteMapper()
        {
            var mapper = ScriptableObject.CreateInstance<RoomSpriteMapper>();

            string[] roomNames = {
                "Escaleras de Entrada",
                "Escaleras de Descenso",
                "Santuario Antiguo",
                "Campamento Abandonado",
                "Portal Arcano",
                "Sala de Descanso",
                "Biblioteca Silenciosa",
                "Fuente Curativa",
                "Capilla Derruida",
                "Camara de Meditacion",
                "Jardin Subterraneo",
                "Refugio de Exploradores",
                "Cripta Consagrada",
                "Trono del Nigromante",
                "Guarida del Dragon",
                "Arena del Campeon",
                "Camara del Rey Rata",
                "Laboratorio del Liche",
                "Sala del Guardian de Piedra",
                "Camara del Tesoro",
                "Boveda Sellada",
                "Cofre Maldito",
                "Almacen Imperial",
                "Sala de Reliquias",
                "Camara del Botin Perdido",
                "Pasillo de Flechas",
                "Sala de Pinchos",
                "Foso Oculto",
                "Camara de Gas",
                "Corredor Giratorio",
                "Sala Inundada",
                "Camara de Llamas",
                "Laberinto de Espejos",
                "Nido de Murcielagos",
                "Madriguera Goblin",
                "Sala de Esqueletos",
                "Perreras Orcas",
                "Cripta Infestada",
                "Camara de Aranas",
                "Cuartel Derruido",
                "Altar Profano",
                "Sala de Hongos Gigantes",
                "Corral de Bestias",
                "Sala de Guerra",
                "Plaza Subterranea",
                "Tunel de Emboscada",
                "Camara de Cultistas",
                "Fundicion Infernal",
                "Sala de Invocacion",
                "Catacumbas Profundas",
                "Fortaleza Interior",
                "Mercader Misterioso",
                "Estatua Parlante",
                "Prision Antigua",
                "Camara de Acertijos",
                "Pozo de los Deseos",
                "Sala de Sacrificios",
                "Observatorio Arcano",
                "Sala del Oraculo",
                "Encrucijada de Runas",
            };

            for (int i = 0; i < roomNames.Length; i++)
            {
                mapper.entries.Add(new RoomSpriteMapper.RoomSpriteEntry
                {
                    roomId = i + 1,
                    roomName = roomNames[i],
                    sprite = null
                });
            }

            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");

            AssetDatabase.CreateAsset(mapper, "Assets/ScriptableObjects/RoomSpriteMapper.asset");
            AssetDatabase.Refresh();

            Debug.Log("Room Sprite Mapper created! Slice your tileset and assign sprites to each entry.");
            Debug.Log("To slice: Select tileset texture > Inspector > Sprite Mode: Multiple > Sprite Editor > Slice (Grid 10x6)");
            Selection.activeObject = mapper;
        }

        [MenuItem("Dungeon Architect/Auto-Assign Tileset Sprites")]
        public static void AutoAssignTilesetSprites()
        {
            var mapper = AssetDatabase.LoadAssetAtPath<RoomSpriteMapper>("Assets/ScriptableObjects/RoomSpriteMapper.asset");
            if (mapper == null)
            {
                EditorUtility.DisplayDialog("Error", "RoomSpriteMapper.asset not found.\nRun 'Setup Room Sprite Mapper' first.", "OK");
                return;
            }

            var allAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/Sprites/RoomTileset.png");
            var sprites = new System.Collections.Generic.List<Sprite>();
            foreach (var asset in allAssets)
            {
                if (asset is Sprite s)
                    sprites.Add(s);
            }

            if (sprites.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No sprites found in RoomTileset.png.\nMake sure the texture is sliced (Sprite Mode: Multiple).", "OK");
                return;
            }

            // Sort by the numeric suffix in the sprite name (e.g. RoomTileset_0, RoomTileset_12)
            sprites.Sort((a, b) =>
            {
                int IndexOf(string name) => int.TryParse(name.Substring(name.LastIndexOf('_') + 1), out int n) ? n : 0;
                return IndexOf(a.name).CompareTo(IndexOf(b.name));
            });

            // Sort entries by roomId so assignment is deterministic regardless of list order
            var sorted = new System.Collections.Generic.List<RoomSpriteMapper.RoomSpriteEntry>(mapper.entries);
            sorted.Sort((a, b) => a.roomId.CompareTo(b.roomId));

            int assigned = 0;
            for (int i = 0; i < sorted.Count && i < sprites.Count; i++)
            {
                sorted[i].sprite = sprites[i];
                assigned++;
            }

            EditorUtility.SetDirty(mapper);
            AssetDatabase.SaveAssets();

            Debug.Log($"Auto-assigned {assigned} sprites to RoomSpriteMapper.");
            Selection.activeObject = mapper;
        }

        [MenuItem("Dungeon Architect/Help - Tileset Import Instructions")]
        public static void ShowImportHelp()
        {
            EditorUtility.DisplayDialog("Tileset Import Instructions",
                "1. Import your room tileset image into Assets/Resources/Sprites/\n\n" +
                "2. Select the image in Project window\n\n" +
                "3. In Inspector set:\n" +
                "   - Texture Type: Sprite (2D and UI)\n" +
                "   - Sprite Mode: Multiple\n" +
                "   - Pixels Per Unit: 100\n" +
                "   - Filter Mode: Point (for pixel art)\n\n" +
                "4. Open Sprite Editor > Slice:\n" +
                "   - Type: Grid By Cell Count\n" +
                "   - Column: 10, Row: 6\n\n" +
                "5. Apply slicing\n\n" +
                "6. Go to Dungeon Architect > Setup Room Sprite Mapper\n\n" +
                "7. Assign each sliced sprite to its entry in the mapper\n\n" +
                "Rooms are numbered 1-60, left-to-right, top-to-bottom.",
                "Got it!");
        }
    }
}
