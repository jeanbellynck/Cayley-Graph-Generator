using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ToggleHelper : MonoBehaviour
{
    Toggle toggle;

    void Awake() => toggle = GetComponent<Toggle>();

    public bool IsOff {
        get => toggle == null || !toggle.isOn;
        set { if (toggle != null) toggle.isOn = !value; }
    }
}
