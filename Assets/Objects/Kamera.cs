using UnityEngine;
using System;
using System.Linq;
using DanielLochner.Assets.SimpleSideMenu;
using NUnit.Framework.Constraints;

public class Kamera : MonoBehaviour
{
    [SerializeField] float wheelSensitivity = 1; 
    [SerializeField] float pinchSensitivity = 0.05f; 
    
    [SerializeField] float rotationSpeed = 1;

    // Camera movement should only be possible when the sideMenu states are closed
    public SimpleSideMenu[] sideMenues;

    bool pinching = false;
    public Transform center ;
    [SerializeField] Camera cam;
    

    void Update()
    {
        var mousePosition = Input.touchCount > 0 ? Input.touches.First().position : new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        if (center != null) transform.position = center.position;
        // Camera movement should only be possible when the sideMenu states are closed
        if (cam.ScreenToViewportPoint(mousePosition) is not { x: <= 1 and >= 0, y: >= 0 and <= 1 } ||
            sideMenues.Any(sideMenu => sideMenu.TargetState == State.Open))
            return;

        // Zoom by mouse wheel
        cam.orthographicSize = Math.Max(1, cam.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * wheelSensitivity);

        // Zoom by finger pinch is broken on WebGL. Zoom will be deactived on mobile devices.
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
                cam.orthographicSize -= deltaMagnitudeDiff * pinchSensitivity;
                cam.orthographicSize = Math.Max(1, cam.orthographicSize);
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

    public void zoomIn() => cam.orthographicSize = Math.Max(1, cam.orthographicSize - 3);

    public void zoomOut() => cam.orthographicSize = Math.Max(1, cam.orthographicSize + 3);
}
