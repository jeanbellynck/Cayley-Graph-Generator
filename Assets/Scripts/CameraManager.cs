using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] List<Camera> cameras;
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera secondaryCamera;

    public Camera GetCamera(Vector3 mousePosition) {
        foreach (var camera in cameras) {
            if (camera.TryGetComponent<Kamera>(out var kamera)) {
                if (kamera.IsMouseInViewport(mousePosition))
                    return camera;
            }
            else {
                if (camera.ScreenToViewportPoint(mousePosition) is { x: <= 1 and >= 0, y: >= 0 and <= 1 })
                    return camera;
            }
        }

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

    // referenced from UI
    public void UpdateViewports(float screenPercentage) {
        mainCamera.rect = new(0, 0, screenPercentage, 1);
        secondaryCamera.rect = new(screenPercentage, 0, 1 - screenPercentage, 1);
    }

}
