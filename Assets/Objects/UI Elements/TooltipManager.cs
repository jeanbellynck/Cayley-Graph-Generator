using UnityEngine;
using TMPro;
using System.Collections;

/**
    * This class is responsible for managing the tooltip that appears when hovering over a group in the group gallery.
    * Copied from https://www.youtube.com/watch?v=y2N_J391ptg
    */
public class TooltipManager : MonoBehaviour {
    public static TooltipManager _instance;
    public GameObject tooltip;
    public TMP_Text tooltipText;
    private float timer;
    private bool isPointerOver = false;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        } else {
            _instance = this;
        }
    }

    void Start() {
        Cursor.visible = true;
        tooltip.SetActive(false);
    }

    void Update() {
        if (isPointerOver)
        {
            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                tooltip.SetActive(true);
            }
        }
        Vector3 mousePos = Input.mousePosition;
        transform.position = new Vector3(mousePos.x + 10, mousePos.y - 10, 0);
    }

    

    public void ShowTooltip(string tooltipString) {
        tooltipText.text = tooltipString;
        isPointerOver = true;
    }

    
    public void HideTooltip() {
        isPointerOver = false;
        timer = 0;
        tooltip.SetActive(false);
    }
}