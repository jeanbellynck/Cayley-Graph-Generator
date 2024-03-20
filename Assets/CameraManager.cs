using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    [SerializeField] List<Camera> cameras;

    public Camera GetCamera(Vector3 mousePosition) {
        foreach (var kamera in cameras)
            if (kamera.ScreenToViewportPoint(mousePosition) is { x: <= 1 and >= 0, y: >= 0 and <= 1 })
                return kamera;

        return null;
    }
    public Kamera GetKamera(Vector3 mousePosition) {
        foreach (var camera in cameras)
            if (
                camera.TryGetComponent<Kamera>(out var kamera)
                && kamera.IsMouseInViewport(mousePosition)
            )
                return kamera;

        return null;
    }
    public bool TryGetKamera(Vector3 mousePosition, out Kamera kamera) {
        kamera = GetKamera(mousePosition);
        return kamera != null;
    }

    public bool TryGetCamera(Vector3 mousePosition, out Camera kamera) {
        kamera = GetCamera(mousePosition);
        return kamera != null;
    }
    
    void Start() {
        UpdateViewports(1);
    }

    public void UpdateViewports(float screenPercentage) {
        var (mainCam, secondaryCam) = (cameras[0], cameras[1]);
        mainCam.rect = new Rect(0, 0, screenPercentage, 1);
        secondaryCam.rect = new Rect(screenPercentage, 0, 1 - screenPercentage, 1);
    }

}
