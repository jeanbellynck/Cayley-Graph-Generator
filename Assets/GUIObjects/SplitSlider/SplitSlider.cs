using UnityEngine;
using UnityEngine.UI;

public class SplitSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] GameObject handle;
    RectTransform sliderRectTransform;
    public float Value => slider.value;

    bool active = false;
    float rightInset = 0f;
    float leftInset = 0f;

    void Awake() => sliderRectTransform = slider.GetComponent<RectTransform>();

    public bool Active {
        get => active;
        set {
            active = value;
            handle.SetActive(active);
            slider.interactable = active;
            if (active) {
                slider.value = 0.7f;
            } else {
                slider.value = 1.0f;
            } 
        }
    }

    public float LeftInset {
        get => leftInset;
        set { 
            leftInset = value;
            sliderRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, leftInset, Screen.width - leftInset - rightInset);
            float newMinValue = leftInset / Screen.width;
            slider.minValue = newMinValue;
            if (slider.value < newMinValue) slider.value = newMinValue;
        } 
    }

    public float RightInset {
        get => rightInset;
        set { 
            this.rightInset = value;
            sliderRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, rightInset, Screen.width - leftInset - rightInset);
            float newMaxValue = 1 - rightInset / Screen.width;
            slider.maxValue = newMaxValue;
            if (slider.value > newMaxValue) slider.value = newMaxValue;
        } 
    }
}
