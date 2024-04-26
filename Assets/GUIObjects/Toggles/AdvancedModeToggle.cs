using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AdvancedModeToggle : MonoBehaviour
{
    [SerializeField] bool _advancedMode;
    [SerializeField] UnityEvent<bool> advancedModeChanged = new();
    public bool AdvancedMode {
        get => _advancedMode;
        set {
            _advancedMode = value;
            advancedModeChanged?.Invoke(value);
        }
    }
    public void ToggleAdvancedMode() => AdvancedMode = !AdvancedMode;
    public void SetAdvancedMode(bool advancedMode) => AdvancedMode = advancedMode;

    void Start() => AdvancedMode = false;
}
