using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeKingdom.Presentation.CampaignMap
{
    /// <summary>
    /// Camera rig for the campaign map.
    /// Supports: drag-to-pan, scroll-to-zoom, edge-scroll, keyboard pan,
    /// and programmatic focus (CentreOnMap, FocusTerritory).
    ///
    /// Attach to: CampaignMapScene/CameraRig (parent of Main Camera).
    /// Camera should be a child, looking straight down (orthographic) or
    /// at a fixed angle (perspective with URP).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CampaignMapCameraController : MonoBehaviour
    {
        [Header("Pan")]
        [SerializeField] private float _panSpeed        = 20f;
        [SerializeField] private float _edgeScrollZone  = 40f;   // pixels
        [SerializeField] private bool  _enableEdgeScroll = true;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed     = 5f;
        [SerializeField] private float _minZoom       = 3f;
        [SerializeField] private float _maxZoom       = 25f;

        [Header("Bounds")]
        [SerializeField] private Bounds _mapBounds;

        [Header("Smoothing")]
        [SerializeField] private float _panSmoothing  = 8f;
        [SerializeField] private float _zoomSmoothing = 8f;

        // ── State ──────────────────────────────────────────────────────────
        private Camera    _cam;
        private Vector3   _targetPosition;
        private float     _targetZoom;
        private bool      _isDragging;
        private Vector3   _dragOriginWorld;

        // Input
        private Vector2   _moveInput;   // from keyboard (WASD / arrows)

        // ── Lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            _cam         = GetComponent<Camera>();
            _targetPosition = transform.position;
            _targetZoom     = _cam.orthographicSize;
        }

        private void Update()
        {
            HandleDragPan();
            HandleEdgeScroll();
            HandleKeyboardPan();
            HandleZoom();
            ApplySmoothing();
            ClampToBounds();
        }

        // ── Public API ─────────────────────────────────────────────────────
        public void CentreOnMap(Bounds mapBounds)
        {
            _mapBounds = mapBounds;
            _targetPosition = new Vector3(mapBounds.center.x, mapBounds.center.y, transform.position.z);
            _targetZoom     = Mathf.Max(mapBounds.extents.x, mapBounds.extents.y);
        }

        public void FocusTerritory(Vector3 worldPosition)
        {
            _targetPosition = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
        }

        // ── Input handlers ─────────────────────────────────────────────────
        private void HandleDragPan()
        {
            if (Mouse.current == null) return;

            if (Mouse.current.middleButton.wasPressedThisFrame ||
                (Mouse.current.rightButton.wasPressedThisFrame))
            {
                _isDragging      = true;
                _dragOriginWorld = ScreenToWorld(Mouse.current.position.ReadValue());
            }

            if (_isDragging && (Mouse.current.middleButton.isPressed || Mouse.current.rightButton.isPressed))
            {
                var currentWorld = ScreenToWorld(Mouse.current.position.ReadValue());
                var delta        = _dragOriginWorld - currentWorld;
                _targetPosition += delta;
                _dragOriginWorld = currentWorld; // re-anchor each frame
            }

            if (Mouse.current.middleButton.wasReleasedThisFrame ||
                Mouse.current.rightButton.wasReleasedThisFrame)
            {
                _isDragging = false;
            }
        }

        private void HandleEdgeScroll()
        {
            if (!_enableEdgeScroll) return;
            if (Mouse.current == null) return;

            var mouse  = Mouse.current.position.ReadValue();
            var dir    = Vector3.zero;
            var screen = new Vector2(Screen.width, Screen.height);

            if (mouse.x < _edgeScrollZone)               dir.x = -1;
            else if (mouse.x > screen.x - _edgeScrollZone) dir.x =  1;
            if (mouse.y < _edgeScrollZone)               dir.y = -1;
            else if (mouse.y > screen.y - _edgeScrollZone) dir.y =  1;

            _targetPosition += dir * (_panSpeed * Time.deltaTime);
        }

        private void HandleKeyboardPan()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            var dir = Vector3.zero;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    dir.y =  1;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  dir.y = -1;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  dir.x = -1;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) dir.x =  1;

            _targetPosition += dir * (_panSpeed * Time.deltaTime);
        }

        private void HandleZoom()
        {
            if (Mouse.current == null) return;

            var scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) < 0.01f) return;

            _targetZoom -= scroll * _zoomSpeed * Time.deltaTime;
            _targetZoom  = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);
        }

        private void ApplySmoothing()
        {
            transform.position      = Vector3.Lerp(transform.position, _targetPosition, _panSmoothing * Time.deltaTime);
            _cam.orthographicSize   = Mathf.Lerp(_cam.orthographicSize, _targetZoom, _zoomSmoothing * Time.deltaTime);
        }

        private void ClampToBounds()
        {
            if (_mapBounds.size == Vector3.zero) return;

            var pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, _mapBounds.min.x, _mapBounds.max.x);
            pos.y = Mathf.Clamp(pos.y, _mapBounds.min.y, _mapBounds.max.y);
            transform.position = pos;
        }

        // ── Util ───────────────────────────────────────────────────────────
        private Vector3 ScreenToWorld(Vector2 screenPos)
        {
            return _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _cam.nearClipPlane));
        }
    }
}
