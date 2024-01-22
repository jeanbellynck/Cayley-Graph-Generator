using UnityEngine;
using UnityEngine.EventSystems;
using System;
using DanielLochner.Assets.SimpleSideMenu;

public class Kamera : MonoBehaviour
{
    public float wheelSensitivity = 1; 
    public float pinchSensitivity = 0.05f; 
    
    public float rotationSpeed = 1;
    public Transform target;

    public Vector2 turn;

    // Camera movement should only be possible when the sideMenu states are closed
    public GameObject[] sideMenues;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Get the camera object
        Camera cam = Camera.main;
        if(target != null) transform.parent.position = target.position;

        // Camera movement should only be possible when the sideMenu states are closed
        foreach (GameObject sideMenu in sideMenues)
        {
            if (sideMenu.GetComponent<SimpleSideMenu>().TargetState == State.Open)
            {
                return;
            }
        }

        // Zoom by mouse wheel
        cam.orthographicSize = Math.Max(1, cam.orthographicSize - Input.GetAxis("Mouse ScrollWheel")*wheelSensitivity);

        // Zoom by finger pinch
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);
            
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
            
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
            
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
            
            cam.orthographicSize += deltaMagnitudeDiff * pinchSensitivity;
            cam.orthographicSize = Math.Max(1, cam.orthographicSize);
        }
        
        // If mouse or finger is down, rotate the camera
        if (Input.GetMouseButton(0)) {
            float h = Input.GetAxis("Mouse X") * rotationSpeed * 5;
            float v = Input.GetAxis("Mouse Y") * rotationSpeed * 5;

            // Create a rotation for each axis and multiply them together
            Quaternion rotation = Quaternion.Euler(-v, h, 0);
            transform.parent.rotation = transform.parent.rotation * rotation;
        }
        if (Input.touchCount == 1) {
            Touch touchZero = Input.GetTouch(0);
            float h = touchZero.deltaPosition.x * rotationSpeed;
            float v = touchZero.deltaPosition.y * rotationSpeed;

            // Create a rotation for each axis and multiply them together
            Quaternion rotation = Quaternion.Euler(-v, h, 0);
            transform.parent.rotation = transform.parent.rotation * rotation;
        }
        
    }
}
