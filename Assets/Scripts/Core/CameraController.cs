using UnityEngine;
using DungeonArchitect.Systems;

namespace DungeonArchitect.Core
{
    public class CameraController : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private DungeonGridManager gridManager;
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float zoomSpeed = 2f;

        [Header("Zoom")]
        [SerializeField] private float minZoom = 3f;
        [SerializeField] private float maxZoom = 12f;
        [SerializeField] private float defaultZoom = 7f;

        [Header("Pan")]
        [SerializeField] private bool allowPan = true;
        [SerializeField] private float panSpeed = 10f;

        private Camera cam;
        private Vector3 targetPosition;
        private float targetZoom;
        private Vector3 dragOrigin;
        private bool isDragging;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            targetZoom = defaultZoom;
        }

        private void Start()
        {
            if (gridManager != null)
                gridManager.OnPlayerMoved += OnPlayerMoved;
        }

        public void SnapTo(Vector3 worldPos)
        {
            worldPos.z = transform.position.z;
            transform.position = worldPos;
            targetPosition = worldPos;
        }

        private void Update()
        {
            HandleZoom();
            HandlePan();
            ApplyCamera();
        }

        private void OnPlayerMoved(Vector2Int gridPos)
        {
            targetPosition = gridManager.GridToWorld(gridPos);
            targetPosition.z = transform.position.z;
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetZoom -= scroll * zoomSpeed;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }
        }

        private void HandlePan()
        {
            if (!allowPan) return;

            if (Input.GetMouseButtonDown(2))
            {
                isDragging = true;
                dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            }

            if (Input.GetMouseButton(2) && isDragging)
            {
                var diff = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
                targetPosition += diff;
            }

            if (Input.GetMouseButtonUp(2))
                isDragging = false;
        }

        private void ApplyCamera()
        {
            var pos = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            pos.z = transform.position.z;
            transform.position = pos;

            if (cam.orthographic)
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomSpeed * Time.deltaTime);
        }

        public void CenterOnGrid()
        {
            targetPosition = Vector3.zero;
            targetPosition.z = transform.position.z;
            targetZoom = defaultZoom;
        }
    }
}
