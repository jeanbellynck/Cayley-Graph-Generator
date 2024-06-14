using System.Collections.Generic;
using Unity.VisualScripting;
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

    [SerializeField] SplitSlider splitSlider;

    [SerializeField] float leftViewport => LeftInset / Screen.width;
    [SerializeField] float rightViewport => 1f - RightInset / Screen.width;

    // referenced from UI
    float leftInset = 0f;
    public float LeftInset {
        get => leftInset;
        set {
            leftInset = value;
            splitSlider.LeftInset = value;
            UpdateViewports();
        }
    }

    // referenced from UI
    float rightInset = 0f;
    public float RightInset {
        get => rightInset;
        set {
            rightInset = value;
            splitSlider.RightInset = value;
            UpdateViewports();
        }
    }

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
    public void UpdateViewports(float splitPercentage) {
        if (splitPercentage < leftViewport)
            splitPercentage = leftViewport;
        if (splitPercentage > rightViewport)
            splitPercentage = rightViewport;
        mainKamera.Cam.rect = new(leftViewport, 0, splitPercentage- leftViewport, 1);
        secondaryKamera.Cam.rect = new(splitPercentage, 0, rightViewport - splitPercentage, 1);
    }

    void UpdateViewports() => UpdateViewports(splitSlider.Value);

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
            case CameraLockState.MainToSecondary or CameraLockState.SecondaryToMain: // default
                UnlockCameras();
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
    }

    // referenced from UI
    public void UnlockCameras()
    {
        mainKamera.LockTo(null);
        secondaryKamera.LockTo(null);
        cameraLockState = CameraLockState.Unlocked;
    }
}
