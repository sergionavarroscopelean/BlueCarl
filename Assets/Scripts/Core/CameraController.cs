using UnityEngine;
using DungeonArchitect.Systems;

namespace DungeonArchitect.Core
{
    public class CameraController : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private DungeonGridManager gridManager;
        [SerializeField] private float followSpeed = 3f;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minZoom = 1.5f;
        [SerializeField] private float maxZoom = 12f;
        [SerializeField] private float defaultZoom = 3f;
        [SerializeField] private float roomFocusZoom = 2f;

        [Header("Pan")]
        [SerializeField] private float panSpeed = 1f;

        private Camera cam;
        private Vector3 targetPosition;
        private float targetZoom;
        private bool isFollowing;
        private float followTimer;

        private Vector3 dragStart;
        private bool isDragging;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            targetZoom = defaultZoom;
            targetPosition = transform.position;
        }

        private void Start()
        {
            if (gridManager != null)
                gridManager.OnPlayerMoved += OnPlayerMoved;
        }

        private void OnDestroy()
        {
            if (gridManager != null)
                gridManager.OnPlayerMoved -= OnPlayerMoved;
        }

        public void FocusOnPosition(Vector3 worldPos)
        {
            targetPosition = worldPos;
            targetPosition.z = transform.position.z;
            targetZoom = roomFocusZoom;
            isFollowing = true;
            followTimer = 1.5f;
        }

        private void OnPlayerMoved(Vector2Int gridPos)
        {
            var worldPos = gridManager.GridToWorld(gridPos);
            FocusOnPosition(worldPos);
        }

        private void Update()
        {
            HandleZoom();
            HandlePan();
            ApplyCamera();

            if (isFollowing)
            {
                followTimer -= Time.deltaTime;
                if (followTimer <= 0f)
                    isFollowing = false;
            }
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetZoom -= scroll * zoomSpeed;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
                isFollowing = false;
            }
        }

        private void HandlePan()
        {
            if (Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(1))
            {
                isDragging = true;
                dragStart = cam.ScreenToWorldPoint(Input.mousePosition);
                isFollowing = false;
            }

            if ((Input.GetMouseButton(2) || Input.GetMouseButton(1)) && isDragging)
            {
                var current = cam.ScreenToWorldPoint(Input.mousePosition);
                var diff = dragStart - current;
                targetPosition += diff;
                transform.position += diff;
                dragStart = cam.ScreenToWorldPoint(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(2) || Input.GetMouseButtonUp(1))
                isDragging = false;
        }

        private void ApplyCamera()
        {
            if (!isDragging && isFollowing)
            {
                var pos = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
                pos.z = transform.position.z;
                transform.position = pos;
            }

            if (cam.orthographic)
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomSpeed * Time.deltaTime);
        }

        public void CenterOnGrid()
        {
            targetPosition = Vector3.zero;
            targetPosition.z = transform.position.z;
            targetZoom = defaultZoom;
            isFollowing = true;
            followTimer = 2f;
        }
    }
}
