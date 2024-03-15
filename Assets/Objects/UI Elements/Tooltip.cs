using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public struct TooltipContent {
    public string text;
    public string url;
}

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public TooltipContent content { get; set; } = new();
    public string text {
        get => content.text;
        set => content = new() { text = value, url = url };
    }

    public string url {
        get => content.url;
        set => content = new() { text = text, url = value };
    }

    public void OnPointerEnter(PointerEventData eventData) {
        TooltipManager.Instance?.OnHover(content);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Instance?.HideTooltip();
    }

}