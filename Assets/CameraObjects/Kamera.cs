using UnityEngine;
using System;
using System.Linq;
using DanielLochner.Assets.SimpleSideMenu;

public class Kamera : MonoBehaviour
{
    [SerializeField] float wheelSensitivity = 1; 
    [SerializeField] float pinchSensitivity = 0.05f; 
    [SerializeField] float rotationSpeed = 1;

    [field: SerializeField] public Camera Cam { get; private set; }

    [SerializeField] Kamera otherKamera;

    public CenterPointer centerPointer;
    // Camera movement should only be possible when the sideMenu states are closed
    [SerializeField] SimpleSideMenu[] sideMenues = {};

    bool pinching = false;

    void Awake() {
        if (Cam == null && TryGetComponent<Camera>(out var cam))
            Cam = cam;
    }

    void Update()
    {
        if (otherKamera != null) {
            transform.position = otherKamera.transform.position;
            transform.rotation = otherKamera.transform.rotation;
            Cam.orthographicSize = otherKamera.Cam.orthographicSize;
            return;
        }

        Vector3 mousePosition = Input.touchCount > 0 ? Input.touches.First().position : Input.mousePosition;
        if (centerPointer?.center != null) transform.position = centerPointer.center.position;
        // Camera movement should only be possible when the sideMenu states are closed
        if (!IsMouseInViewport(mousePosition) ||
            sideMenues.Any(sideMenu => sideMenu.TargetState == State.Open))
            return;

        // Zoom by mouse wheel
        Cam.orthographicSize = Math.Max(1, Cam.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * wheelSensitivity);

        // Zoom by finger pinch is broken on WebGL. Zoom will be deactivated on mobile devices.
        if (Input.touchCount == 2)
        {
            pinching = true;
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);
            
            if (touchZero.phase == TouchPhase.Moved && touchOne.phase == TouchPhase.Moved) {
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
                
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                deltaMagnitudeDiff = Mathf.Clamp(deltaMagnitudeDiff, -5, 5);
                Cam.orthographicSize -= deltaMagnitudeDiff * pinchSensitivity;
                Cam.orthographicSize = Math.Max(1, Cam.orthographicSize);
            }
            
        }
        
        if (Input.touchCount == 0 && pinching) {
            pinching = false;
        }

        // If mouse or finger is down, rotate the camera
        if (Input.GetMouseButton(0) &&
            Input.touchCount != 2 &&
            !pinching
            ) {
            float h = Input.GetAxis("Mouse X") * rotationSpeed * 5;
            float v = Input.GetAxis("Mouse Y") * rotationSpeed * 5;

            // Create a rotation for each axis and multiply them together
            Quaternion rotation = Quaternion.Euler(-v, h, 0);
            transform.rotation *= rotation;
        }
        if (Input.touchCount == 1 && !pinching) {
            Touch touchZero = Input.GetTouch(0);
            if (touchZero.phase == TouchPhase.Moved) {
                float h = touchZero.deltaPosition.x * rotationSpeed;
                float v = touchZero.deltaPosition.y * rotationSpeed;

                // Create a rotation for each axis and multiply them together
                Quaternion rotation = Quaternion.Euler(-v, h, 0);
                transform.rotation *= rotation;
            }
        }
    }

    public virtual bool IsMouseInViewport(Vector3 mousePosition)
    {
        return Cam.ScreenToViewportPoint(mousePosition) is { x: <= 1 and >= 0, y: >= 0 and <= 1 };
    }

    public void zoomIn() => Cam.orthographicSize = Math.Max(1, Cam.orthographicSize - 3);

    public void zoomOut() => Cam.orthographicSize = Math.Max(1, Cam.orthographicSize + 3);

    public virtual Ray ScreenPointToRay(Vector3 mousePosition) => Cam.ScreenPointToRay(mousePosition);
    public int cullingMask => Cam.cullingMask;

    // referenced from Dropdown
    public void SetMask(int mode) {
        Cam.cullingMask = mode switch {
            0 => // Group & Subgroup
                LayerMask.GetMask("Default", "Subgroup", "SubgroupOnly"),
            1 => // Group only
                LayerMask.GetMask("Default", "Subgroup"),
            2 => // Subgroup only
                LayerMask.GetMask("Subgroup", "SubgroupOnly"),
            _ => Cam.cullingMask
        };
    }

    public void LockTo(Kamera other) {
        otherKamera = other;
    }
}

[Serializable]
public class CenterPointer {
    public Transform center;
}
