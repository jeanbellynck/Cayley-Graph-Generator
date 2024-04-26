using UnityEngine;
using UnityEngine.EventSystems;


public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ITooltipOnHover {
    public TooltipContent content = new();

    public void OnPointerEnter(PointerEventData eventData) {
        TooltipManager.Instance?.OnHoverBegin(this);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Instance?.OnHoverEnd(this);
    }

    public TooltipContent GetTooltip() => content;

    public virtual void OnClick(Kamera activeKamera) {}

    public void OnHover(Kamera activeKamera) { }
    public void OnHoverEnd() { }
}