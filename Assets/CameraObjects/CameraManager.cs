using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraManager : MonoBehaviour
{
    [SerializeField] List<Camera> cameras;
    [SerializeField] Kamera mainKamera;
    [SerializeField] Kamera secondaryKamera;
    [SerializeField] UnityEvent<bool> onMainToSecondaryLock = new();
    [SerializeField] UnityEvent<bool> onSecondaryToMainLock = new();

    [SerializeField] CameraLockState _cameraLockState;
    CameraLockState cameraLockState {
        get => _cameraLockState;
        set {
            _cameraLockState = value;
            switch (value) {
                case CameraLockState.MainToSecondary:
                    onMainToSecondaryLock.Invoke(true);
                    onSecondaryToMainLock.Invoke(false);
                    break;
                case CameraLockState.SecondaryToMain:
                    onMainToSecondaryLock.Invoke(false);
                    onSecondaryToMainLock.Invoke(true);
                    break;
                case CameraLockState.Unlocked:
                    onMainToSecondaryLock.Invoke(false);
                    onSecondaryToMainLock.Invoke(false);
                    break;
            }
        }
    }
    enum CameraLockState {
        Unlocked,
        SecondaryToMain,
        MainToSecondary
    }

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
    
    //void Awake() {
    //    UpdateViewports(1);
    //}

    // referenced from UI
    public void UpdateViewports(float screenPercentage) {
        mainKamera.Cam.rect = new(0, 0, screenPercentage, 1);
        secondaryKamera.Cam.rect = new(screenPercentage, 0, 1 - screenPercentage, 1);
    }

    // referenced from UI
    public void LockCameras(bool secondaryToMain) {

        switch (cameraLockState) {
            case CameraLockState.Unlocked when secondaryToMain:
            case CameraLockState.MainToSecondary when secondaryToMain:
                LockSecondaryToMain();
                break;
            case CameraLockState.Unlocked when !secondaryToMain:
            case CameraLockState.SecondaryToMain when !secondaryToMain:
                LockMainToSecondary();
                break;
            case CameraLockState.MainToSecondary or CameraLockState.SecondaryToMain:
                Unlock();
                break;
        }
        return;

        void LockSecondaryToMain() {
            secondaryKamera.LockTo(mainKamera);
            cameraLockState = CameraLockState.SecondaryToMain;
            mainKamera.LockTo(null);
        }
        void LockMainToSecondary() {
            mainKamera.LockTo(secondaryKamera);
            cameraLockState = CameraLockState.MainToSecondary;
            secondaryKamera.LockTo(null);
        }

        void Unlock() {
            mainKamera.LockTo(null);
            secondaryKamera.LockTo(null);
            cameraLockState = CameraLockState.Unlocked;
        }

    }

}
