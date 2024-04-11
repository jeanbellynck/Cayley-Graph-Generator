using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class RightClickButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] UnityEvent<int> OnClick = new();

    public virtual void OnPointerClick(PointerEventData eventData) {
        if (isActiveAndEnabled)
            OnClick.Invoke((int)eventData.button);
    }
}
