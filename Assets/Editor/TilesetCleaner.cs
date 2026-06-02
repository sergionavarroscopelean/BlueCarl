using UnityEngine;
using UnityEditor;

namespace DungeonArchitect.EditorTools
{
    public class TilesetCleaner : EditorWindow
    {
        [MenuItem("Dungeon Architect/Clean Tileset Arrows")]
        public static void CleanArrows()
        {
            string path = "Assets/Resources/Sprites/RoomTileset.png";
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null)
            {
                Debug.LogError("Tileset not found at " + path);
                return;
            }

            string fullPath = System.IO.Path.Combine(Application.dataPath, "Resources/Sprites/RoomTileset.png");
            byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
            var editTex = new Texture2D(2, 2);
            editTex.LoadImage(bytes);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            var sheet = importer.spritesheet;
            foreach (var sprite in sheet)
            {
                CleanArrowsInSprite(editTex, sprite.rect);
            }

            editTex.Apply();
            bytes = editTex.EncodeToPNG();
            System.IO.File.WriteAllBytes(fullPath, bytes);
            Object.DestroyImmediate(editTex);

            AssetDatabase.Refresh();
            Debug.Log($"Cleaned arrow markers from {sheet.Length} sprites in tileset.");
        }

        private static void CleanArrowsInSprite(Texture2D tex, Rect spriteRect)
        {
            int sx = (int)spriteRect.x;
            int sy = (int)spriteRect.y;
            int sw = (int)spriteRect.width;
            int sh = (int)spriteRect.height;

            int arrowSize = Mathf.Max(sw, sh) / 6;

            PatchArea(tex, sx + sw / 2 - arrowSize / 2, sy + sh - arrowSize - 2, arrowSize, arrowSize, sx, sy, sw, sh);
            PatchArea(tex, sx + sw / 2 - arrowSize / 2, sy + 2, arrowSize, arrowSize, sx, sy, sw, sh);
            PatchArea(tex, sx + 2, sy + sh / 2 - arrowSize / 2, arrowSize, arrowSize, sx, sy, sw, sh);
            PatchArea(tex, sx + sw - arrowSize - 2, sy + sh / 2 - arrowSize / 2, arrowSize, arrowSize, sx, sy, sw, sh);
        }

        private static void PatchArea(Texture2D tex, int px, int py, int pw, int ph, int sx, int sy, int sw, int sh)
        {
            Color avgColor = SampleSurrounding(tex, px, py, pw, ph, sx, sy, sw, sh);

            for (int y = py; y < py + ph && y < tex.height; y++)
            {
                for (int x = px; x < px + pw && x < tex.width; x++)
                {
                    if (x < 0 || y < 0) continue;
                    Color pixel = tex.GetPixel(x, y);
                    if (pixel.grayscale > 0.7f && pixel.a > 0.5f)
                    {
                        tex.SetPixel(x, y, avgColor);
                    }
                }
            }
        }

        private static Color SampleSurrounding(Texture2D tex, int px, int py, int pw, int ph, int sx, int sy, int sw, int sh)
        {
            float r = 0, g = 0, b = 0;
            int count = 0;
            int margin = 3;

            for (int y = py - margin; y < py + ph + margin; y++)
            {
                for (int x = px - margin; x < px + pw + margin; x++)
                {
                    if (x < sx || x >= sx + sw || y < sy || y >= sy + sh) continue;
                    if (x >= px && x < px + pw && y >= py && y < py + ph) continue;

                    Color c = tex.GetPixel(x, y);
                    if (c.a < 0.5f) continue;
                    if (c.grayscale > 0.7f) continue;

                    r += c.r; g += c.g; b += c.b;
                    count++;
                }
            }

            if (count == 0) return new Color(0.15f, 0.13f, 0.1f);
            return new Color(r / count, g / count, b / count);
        }
    }
}
