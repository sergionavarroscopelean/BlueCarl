using UnityEngine;

namespace DungeonArchitect.Systems
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class EmptySlotIndicator : MonoBehaviour
    {
        private Vector2Int gridPos;
        private ValidSlotsManager manager;
        private SpriteRenderer sr;

        private static Sprite cachedSprite;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateSprite();
            sr.color = new Color(0.5f, 0.7f, 1f, 0.35f);
            sr.sortingOrder = -1;
        }

        public void Init(Vector2Int pos, ValidSlotsManager mgr)
        {
            gridPos = pos;
            manager = mgr;
        }

        private void OnMouseEnter() => sr.color = new Color(0.5f, 0.7f, 1f, 0.65f);
        private void OnMouseExit()  => sr.color = new Color(0.5f, 0.7f, 1f, 0.35f);

        private void OnMouseDown()
        {
            manager?.OnSlotClicked(gridPos);
        }

        // Generates a bordered square sprite at runtime so no texture asset is needed.
        private static Sprite GetOrCreateSprite()
        {
            if (cachedSprite != null) return cachedSprite;

            int size = 64;
            int border = 4;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool onBorder = x < border || x >= size - border || y < border || y >= size - border;
                    tex.SetPixel(x, y, onBorder ? Color.white : Color.clear);
                }
            }
            tex.Apply();

            cachedSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return cachedSprite;
        }
    }
}
