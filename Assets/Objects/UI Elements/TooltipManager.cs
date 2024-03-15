using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

/**
    * This class is responsible for managing the tooltip that appears when hovering over a group in the group gallery.
    * Copied from https://www.youtube.com/watch?v=y2N_J391ptg
    */
public class TooltipManager : MonoBehaviour {
    static TooltipManager _instance;

    public static TooltipManager Instance =>_instance;
    
    [SerializeField] RectTransform tooltipPanel;
    [SerializeField] TMP_Text tooltipText;
    [SerializeField] float hoverTime = 1f;
    
    TooltipContent content;
    float timer;
    bool uiActivated;
    bool objectActivated;
    bool Activated => uiActivated || objectActivated;
    bool isActive;
    int tooltipLayer;
    Transform lastHoverObject;

    void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        } else {
            _instance = this;
        }
        tooltipLayer = LayerMask.NameToLayer("TooltipObjects");
    }

    void Start() {
        Cursor.visible = true;
        tooltipPanel.gameObject.SetActive(false);
    }
    
    void Update() {
        if (!uiActivated) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, tooltipLayer)) {
                var tooltipObject = hit.transform;
                if (tooltipObject != lastHoverObject) {
                    lastHoverObject = tooltipObject;
                    if (tooltipObject.TryGetComponent<ITooltipOnHover>(out var tooltipThing))
                        OnHover(tooltipThing.GetTooltip(), false);
                    else
                        HideTooltip();
                }
            }
            else {
                lastHoverObject = null;
                HideTooltip();
            }
        }

        if (Activated) {
            if (Time.time >= timer + hoverTime && !isActive) {
                ActuallyShowTooltip();
                isActive = true;
            }
        }

        if (!isActive) return;

        if (Input.GetMouseButtonDown(1) && content.url != "") {
            Application.OpenURL(content.url);
            // On right click do a RickRoll
            //Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        }

        Vector3 mousePos = Input.mousePosition;
        tooltipPanel.position = new(mousePos.x + 10, mousePos.y - 10);
    }

    void ActuallyShowTooltip()
    {
        tooltipPanel.gameObject.SetActive(true);
        tooltipText.text = content.text;
        if (content.url != "") tooltipText.text += "\n<b>Right click for more info.</b>";

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel);
        transform.SetAsLastSibling();
    }


    public void OnHover(TooltipContent content, bool fromUI = true) {
        //timer = Time.time;
        this.content = content;
        if (fromUI) {
            uiActivated = true;
            objectActivated = false;
        }
        else {
            objectActivated = true;
            uiActivated = false;
        }
    }


    public void HideTooltip() {
        uiActivated = false;
        objectActivated = false;
        tooltipPanel.gameObject.SetActive(false);
    }
}