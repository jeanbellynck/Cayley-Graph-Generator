using UnityEngine;
using TMPro;
using UnityEngine.UI;

/**
    * This class is responsible for managing the tooltip that appears when hovering over a group in the group gallery.
    * Inspired by https://www.youtube.com/watch?v=y2N_J391ptg
    */
public class TooltipManager : MonoBehaviour {

    public static TooltipManager Instance { get; private set; }

    [SerializeField] RectTransform tooltipPanel;
    [SerializeField] TMP_Text tooltipText;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] float hoverTime = 1f;
    
    TooltipContent content;
    float timer;
    bool uiActivated;
    bool objectActivated;
    bool Activated => uiActivated || objectActivated;
    bool isActive;
    int layerMask;
    Transform lastHoverObject;
    ITooltipOnHover lastTooltipThing;

    void Awake() {
        if (Instance == this) 
            return;
        if (Instance == null) {
            Instance = this;
            return;
        }
        Destroy(gameObject);
    }

    [SerializeField] Kamera uiCamera;
    void Start() {
        Cursor.visible = true;
        tooltipPanel.gameObject.SetActive(false);
    }

    void Update() {
        var mousePosition = Input.mousePosition;
        if (!uiActivated && cameraManager.TryGetKamera(mousePosition, out var kamera)) { // UI elements have priority

            Ray ray = kamera.ScreenPointToRay(mousePosition);

            layerMask = kamera.cullingMask; // LayerMask.GetMask("TooltipObjects");

            Debug.DrawRay(ray.origin, ray.direction.normalized * 2000, Color.yellow,0.1f);

            if (Physics.Raycast(ray, out var hit, maxDistance: 2000, layerMask)) { 
                var tooltipObject = hit.transform;
                if (tooltipObject != lastHoverObject) {
                    OnHoverEnd();
                    lastHoverObject = tooltipObject;
                    if (tooltipObject.TryGetComponent<ITooltipOnHover>(out var tooltipThing))
                        OnHoverBegin(tooltipThing, false, kamera);
                }
            }
            else {
                OnHoverEnd();
                lastHoverObject = null;
            }
        }

        if (!isActive && Activated && Time.time >= timer + hoverTime) {
            ShowTooltip();
            isActive = true;
        }

        if (!isActive) return;

        if (Input.GetMouseButtonUp(0)) {
            if (objectActivated) {
                if (cameraManager.TryGetKamera(mousePosition, out kamera)) 
                    lastTooltipThing?.OnClick(kamera);
            } else if (uiActivated) {
                lastTooltipThing?.OnClick(null);
            }
        }

        if (Input.GetMouseButtonUp(1) && !string.IsNullOrWhiteSpace(content.url)) 
            Application.OpenURL(content.url);

        tooltipPanel.position = new(mousePosition.x + 10, mousePosition.y - 10);
    }

    public void OnHoverBegin(ITooltipOnHover tooltipThing, bool fromUI = true, Kamera kamera = null) {
        timer = Time.time;
        lastTooltipThing = tooltipThing;
        content = tooltipThing.GetTooltip();
        uiActivated = fromUI;
        objectActivated = !fromUI;
        tooltipThing.OnHover(kamera);
    }

    void ShowTooltip()
    {
        var text = content.text ?? "";
        if (!string.IsNullOrWhiteSpace(content.url)) 
            text += "\n<b>Right click for more info.</b>";

        if (string.IsNullOrWhiteSpace(text))
            return;
        tooltipPanel.gameObject.SetActive(true);
        tooltipText.text = text;

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel);
        tooltipPanel.SetAsLastSibling();
    }



    public void OnHoverEnd() {
        uiActivated = false;
        objectActivated = false;
        isActive = false;
        tooltipPanel.gameObject.SetActive(false);
        if (lastTooltipThing != null)
            lastTooltipThing?.OnHoverEnd();
        lastTooltipThing = null;
        lastHoverObject = null;
    }
}