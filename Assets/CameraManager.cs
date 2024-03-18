using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    [SerializeField] List<Camera> cameras;
    [SerializeField] Slider slider;

    public Camera GetCamera(Vector3 mousePosition) {
        foreach (var kamera in cameras)
            if (kamera.ScreenToViewportPoint(mousePosition) is { x: <= 1 and >= 0, y: >= 0 and <= 1 })
                return kamera;

        return null;
    }
    public Kamera GetKamera(Vector3 mousePosition) {
        foreach (var kamera in cameras)
            // kamera.rect.Contains(mousePosition) (rect is normalized, i.e. 0-1)
            if (
                kamera.ScreenToViewportPoint(mousePosition) is { x: <= 1 and >= 0, y: >= 0 and <= 1 } &&
                kamera.TryGetComponent<Kamera>(out var result)
            )
                return result;

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
        slider.onValueChanged.AddListener(UpdateViewports);
        UpdateViewports(1);
    }

    void UpdateViewports(float screenPercentage) {
        var (mainCam, secondaryCam) = (cameras[0], cameras[1]);
        mainCam.rect = new Rect(0, 0, screenPercentage, 1);
        secondaryCam.rect = new Rect(screenPercentage, 0, 1 - screenPercentage, 1);
    }

}
