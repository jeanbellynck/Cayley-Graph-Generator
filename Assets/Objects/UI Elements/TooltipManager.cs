using UnityEngine;
using TMPro;

/**
    * This class is responsible for managing the tooltip that appears when hovering over a group in the group gallery.
    * Copied from https://www.youtube.com/watch?v=y2N_J391ptg
    */
public class TooltipManager : MonoBehaviour {
    public static TooltipManager _instance;
    public TMP_Text tooltipText;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        } else {
            _instance = this;
        }
    }

    void Start() {
        Cursor.visible = true;
        gameObject.SetActive(false);
    }

    void Update() {
        Vector3 mousePos = Input.mousePosition;
        transform.position = new Vector3(mousePos.x + 10, mousePos.y - 10, 0);
    }

    public void ShowTooltip(string text) {
        gameObject.SetActive(true);
        tooltipText.text = text;
    }

    public void HideTooltip() {
        gameObject.SetActive(false);
    }
}