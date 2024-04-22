using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplitSlider : MonoBehaviour
{
    public Slider slider;
    public GameObject handle;

    bool active = false;
    
    public void SetActive(bool active)
    {
        this.active = active;
        handle.SetActive(active);
        if(active) {
            slider.value = 0.5f;
        } else {
            slider.value = 1.0f;
        }
    }
}
