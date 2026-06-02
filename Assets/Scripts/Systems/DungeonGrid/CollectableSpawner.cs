using UnityEngine;
using System.Collections.Generic;

namespace DungeonArchitect.Systems
{
    public static class CollectableSpawner
    {
        public static void SpawnCollectables(RoomOffer offer, GameObject roomVisual)
        {
            if (roomVisual == null) return;

            var sr = roomVisual.GetComponent<SpriteRenderer>();
            float halfX = 0.3f, halfY = 0.3f;
            if (sr != null && sr.sprite != null)
            {
                halfX = sr.sprite.bounds.extents.x * 0.6f;
                halfY = sr.sprite.bounds.extents.y * 0.6f;
            }
            var center = sr != null && sr.sprite != null ? (Vector3)sr.sprite.bounds.center : Vector3.zero;

            var positions = GeneratePositions(offer, halfX, halfY);
            int idx = 0;

            for (int i = 0; i < offer.goldReward; i++)
            {
                var pos = positions[idx % positions.Count];
                SpawnOne(CollectableType.Gold, 1, roomVisual.transform, center + pos, idx);
                idx++;
            }

            for (int i = 0; i < offer.gemReward; i++)
            {
                var pos = positions[idx % positions.Count];
                SpawnOne(CollectableType.Gem, 1, roomVisual.transform, center + pos, idx);
                idx++;
            }

            for (int i = 0; i < offer.keyReward; i++)
            {
                var pos = positions[idx % positions.Count];
                SpawnOne(CollectableType.Key, 1, roomVisual.transform, center + pos, idx);
                idx++;
            }
        }

        private static void SpawnOne(CollectableType type, int amount, Transform parent, Vector3 localPos, int index)
        {
            var go = new GameObject($"Collectable_{type}");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos + Vector3.back * 0.08f;
            go.transform.localScale = Vector3.one * 0.22f;

            var collectable = go.AddComponent<RoomCollectable>();
            collectable.Initialize(type, amount);
            collectable.StartAutoCollect(1f + index * 0.3f);
        }

        private static List<Vector3> GeneratePositions(RoomOffer offer, float halfX, float halfY)
        {
            int total = offer.goldReward + offer.gemReward + offer.keyReward;
            var positions = new List<Vector3>();

            if (total <= 0)
                return positions;

            if (total == 1)
            {
                positions.Add(Vector3.zero);
                return positions;
            }

            float angleStep = 360f / total;
            float radius = Mathf.Min(halfX, halfY) * 0.5f;

            for (int i = 0; i < total; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius + Random.Range(-0.05f, 0.05f);
                float y = Mathf.Sin(angle) * radius + Random.Range(-0.05f, 0.05f);
                positions.Add(new Vector3(x, y, 0));
            }

            return positions;
        }
    }
}
