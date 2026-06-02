using UnityEngine;
using DungeonArchitect.Core;

namespace DungeonArchitect.Systems
{
    public enum CollectableType { Gold, Gem, Key }

    public class RoomCollectable : MonoBehaviour
    {
        private CollectableType type;
        private int amount;
        private SpriteRenderer spriteRenderer;
        private bool collected;
        private float bobTimer;
        private float autoCollectTimer;
        private bool autoCollectStarted;

        private static Sprite goldSprite;
        private static Sprite gemSprite;
        private static Sprite keySprite;

        public void Initialize(CollectableType collectType, int collectAmount)
        {
            type = collectType;
            amount = collectAmount;

            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetSprite(type);
            spriteRenderer.color = GetColor(type);
            spriteRenderer.sortingOrder = 8;

            var col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;

            bobTimer = Random.Range(0f, Mathf.PI * 2f);
        }

        public void StartAutoCollect(float delay)
        {
            autoCollectTimer = delay;
            autoCollectStarted = true;
        }

        private void Update()
        {
            if (collected) return;

            if (autoCollectStarted)
            {
                autoCollectTimer -= Time.deltaTime;
                if (autoCollectTimer <= 0f)
                {
                    collected = true;
                    Collect();
                    return;
                }
            }

            bobTimer += Time.deltaTime * 2.5f;
            float bob = Mathf.Sin(bobTimer) * 0.03f;
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                transform.localPosition.y + bob * Time.deltaTime,
                transform.localPosition.z
            );
        }

        private void OnMouseDown()
        {
            if (collected) return;
            collected = true;
            Collect();
        }

        private void Collect()
        {
            var resources = GameManager.Instance.Resources;
            switch (type)
            {
                case CollectableType.Gold: resources.AddGold(amount); break;
                case CollectableType.Gem: resources.AddGems(amount); break;
                case CollectableType.Key: resources.AddKeys(amount); break;
            }

            StartCoroutine(FlyToHUD());
        }

        private System.Collections.IEnumerator FlyToHUD()
        {
            var cam = Camera.main;
            Vector3 targetScreen = GetHUDTargetScreen();
            Vector3 startPos = transform.position;

            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t * (3f - 2f * t);

                Vector3 targetWorld = cam.ScreenToWorldPoint(new Vector3(targetScreen.x, targetScreen.y, cam.nearClipPlane + 1f));
                transform.position = Vector3.Lerp(startPos, targetWorld, t);
                transform.localScale = Vector3.Lerp(Vector3.one * 0.12f, Vector3.one * 0.05f, t);

                float alpha = 1f - t * 0.5f;
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);

                yield return null;
            }

            Destroy(gameObject);
        }

        private Vector3 GetHUDTargetScreen()
        {
            float x = type switch
            {
                CollectableType.Gold => Screen.width * 0.22f,
                CollectableType.Key => Screen.width * 0.30f,
                CollectableType.Gem => Screen.width * 0.38f,
                _ => Screen.width * 0.5f
            };
            return new Vector3(x, Screen.height - 25f, 0f);
        }

        private static Sprite GetSprite(CollectableType type)
        {
            return type switch
            {
                CollectableType.Gold => GetOrCreateSprite(ref goldSprite, CollectableType.Gold),
                CollectableType.Gem => GetOrCreateSprite(ref gemSprite, CollectableType.Gem),
                CollectableType.Key => GetOrCreateSprite(ref keySprite, CollectableType.Key),
                _ => null
            };
        }

        private static Sprite GetOrCreateSprite(ref Sprite cache, CollectableType type)
        {
            if (cache != null) return cache;

            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, Color.clear);

            switch (type)
            {
                case CollectableType.Gold:
                    DrawCircle(tex, size / 2, size / 2, size / 2 - 2, Color.white);
                    break;
                case CollectableType.Gem:
                    DrawDiamond(tex, size, Color.white);
                    break;
                case CollectableType.Key:
                    DrawKey(tex, size, Color.white);
                    break;
            }

            tex.Apply();
            cache = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return cache;
        }

        private static void DrawCircle(Texture2D tex, int cx, int cy, int radius, Color color)
        {
            for (int y = 0; y < tex.height; y++)
                for (int x = 0; x < tex.width; x++)
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius)
                        tex.SetPixel(x, y, color);
        }

        private static void DrawDiamond(Texture2D tex, int size, Color color)
        {
            int cx = size / 2, cy = size / 2;
            int r = size / 2 - 2;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    if (Mathf.Abs(x - cx) + Mathf.Abs(y - cy) <= r)
                        tex.SetPixel(x, y, color);
        }

        private static void DrawKey(Texture2D tex, int size, Color color)
        {
            int cx = size / 2;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    int dx = x - cx, dy = y - size / 4;
                    if (dx * dx + dy * dy <= (size / 5) * (size / 5))
                        tex.SetPixel(x, y, color);
                    if (x >= cx - 1 && x <= cx + 1 && y >= size / 4 && y <= size - 3)
                        tex.SetPixel(x, y, color);
                    if (y >= size * 3 / 4 - 1 && y <= size * 3 / 4 + 1 && x >= cx && x <= cx + size / 5)
                        tex.SetPixel(x, y, color);
                }
        }

        private static Color GetColor(CollectableType type)
        {
            return type switch
            {
                CollectableType.Gold => new Color(1f, 0.85f, 0.2f),
                CollectableType.Gem => new Color(0.6f, 0.3f, 0.9f),
                CollectableType.Key => new Color(0.8f, 0.8f, 0.8f),
                _ => Color.white
            };
        }
    }
}
