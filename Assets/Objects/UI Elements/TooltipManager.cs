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

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start() {
        Cursor.visible = true;
        tooltipPanel.gameObject.SetActive(false);
    }
    
    void Update() {
        var mousePosition = Input.mousePosition;
        if (!uiActivated && cameraManager.TryGetCamera(mousePosition, out var camera)) { // UI elements have priority

            Ray ray = camera.ScreenPointToRay(mousePosition);
            layerMask = camera.cullingMask; // LayerMask.GetMask("TooltipObjects");


            Debug.DrawRay(ray.origin, ray.direction.normalized * 2000, Color.yellow,0.1f);
            if (Physics.Raycast(ray, out var hit, maxDistance: 2000, layerMask)) { 
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

        if (!isActive && Activated && Time.time >= timer + hoverTime) {
            ActuallyShowTooltip();
            isActive = true;
        }

        if (!isActive) return;

        if (objectActivated && Input.GetMouseButtonUp(0) && cameraManager.TryGetKamera(mousePosition, out var kamera))
            kamera.center = lastHoverObject;

        if (Input.GetMouseButtonUp(1) && !string.IsNullOrWhiteSpace(content.url)) 
            Application.OpenURL(content.url);

        tooltipPanel.position = new(mousePosition.x + 10, mousePosition.y - 10);
    }

    public void OnHover(TooltipContent content, bool fromUI = true) {
        timer = Time.time;
        this.content = content;
        uiActivated = fromUI;
        objectActivated = !fromUI;
    }

    void ActuallyShowTooltip()
    {
        tooltipPanel.gameObject.SetActive(true);
        tooltipText.text = content.text ?? "";
        if (!string.IsNullOrWhiteSpace(content.url)) tooltipText.text += "\n<b>Right click for more info.</b>";

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel);
        tooltipPanel.SetAsLastSibling();
    }



    public void HideTooltip() {
        uiActivated = false;
        objectActivated = false;
        isActive = false;
        tooltipPanel.gameObject.SetActive(false);
    }
}