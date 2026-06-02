using UnityEngine;
using DungeonArchitect.Data;

namespace DungeonArchitect.Systems
{
    [RequireComponent(typeof(Collider2D))]
    public class DoorIconClickable : MonoBehaviour
    {
        private Direction direction;
        private DoorInteractionManager manager;
        private Vector2Int roomPosition;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private float pulseTimer;

        public Direction Direction => direction;

        public void Initialize(Direction dir, DoorInteractionManager mgr, Vector2Int roomPos)
        {
            direction = dir;
            manager = mgr;
            roomPosition = roomPos;
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        private void Update()
        {
            pulseTimer += Time.deltaTime * 3f;
            if (spriteRenderer != null)
            {
                float pulse = Mathf.Lerp(0.7f, 1f, (Mathf.Sin(pulseTimer) + 1f) / 2f);
                spriteRenderer.color = originalColor * pulse;
            }
        }

        private void OnMouseDown()
        {
            if (manager != null)
                manager.OnDoorClicked(direction, roomPosition);
        }

        private void OnMouseEnter()
        {
            transform.localScale *= 1.3f;
        }

        private void OnMouseExit()
        {
            transform.localScale /= 1.3f;
        }
    }
}
