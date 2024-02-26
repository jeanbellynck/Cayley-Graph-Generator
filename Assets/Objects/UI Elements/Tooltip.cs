using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public string text = "";
    public string url = "";

    public void OnPointerEnter(PointerEventData eventData) {
        TooltipManager._instance.ShowTooltip(text, url);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager._instance.HideTooltip();
    }

}