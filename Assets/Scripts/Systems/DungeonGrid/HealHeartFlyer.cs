using UnityEngine;

namespace DungeonArchitect.Systems
{
    public class HealHeartFlyer : MonoBehaviour
    {
        private float delay;
        private float elapsed;
        private float duration = 0.7f;
        private Vector3 startPos;
        private bool started;
        private SpriteRenderer sr;

        public void Initialize(float startDelay)
        {
            delay = startDelay;
            sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            elapsed += Time.deltaTime;

            if (elapsed < delay)
                return;

            if (!started)
            {
                started = true;
                startPos = transform.position;
            }

            float t = (elapsed - delay) / duration;
            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            float smooth = t * t * (3f - 2f * t);

            var cam = Camera.main;
            Vector3 targetScreen = new Vector3(Screen.width * 0.05f, Screen.height - 25f, cam.nearClipPlane + 1f);
            Vector3 targetWorld = cam.ScreenToWorldPoint(targetScreen);

            transform.position = Vector3.Lerp(startPos, targetWorld, smooth);
            transform.localScale = Vector3.Lerp(Vector3.one * 0.15f, Vector3.one * 0.06f, smooth);

            if (sr != null)
                sr.color = new Color(1f, 0.3f, 0.4f, 1f - smooth * 0.4f);
        }
    }
}
