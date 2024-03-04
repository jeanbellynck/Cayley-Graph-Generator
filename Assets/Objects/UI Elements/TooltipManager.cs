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

    public static TooltipManager Instance {
        get {
            if (_instance == null) _instance = new GameObject().AddComponent<TooltipManager>();
            return _instance;
        }
    }
    public GameObject tooltip;
    public string rightClickURL = "";
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
            if (timer >= 1f && tooltipText.text != "")
            {
                tooltip.SetActive(true);
                ///tooltipText.ForceMeshUpdate();
                //LayoutRebuilder.ForceRebuildLayoutImmediate(tooltip.transform.GetChild(0).GetComponent<RectTransform>());
                LayoutRebuilder.ForceRebuildLayoutImmediate(tooltip.GetComponent<RectTransform>());
                // On right click do a RickRoll
                if (Input.GetMouseButtonDown(1) && rightClickURL != "")
                {
                    Application.OpenURL(rightClickURL);
                    //Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
                }
            }
        }
        Vector3 mousePos = Input.mousePosition;
        transform.position = new Vector3(mousePos.x + 10, mousePos.y - 10, 0);
    }

    
    public void ShowTooltip(string tooltipString, string rightClickURL = "") {
        if(tooltipString == "") {
            isPointerOver = false;
            return;
        }
        
        tooltipText.text = tooltipString;
        this.rightClickURL = rightClickURL;
        if(rightClickURL != "") {
            tooltipText.text += "\n<b>Right click for more info.</b>";
        }
        isPointerOver = true;
    }

    public void OnMouseDown() {
    }
 
    
    public void HideTooltip() {
        isPointerOver = false;
        timer = 0;
        tooltip.SetActive(false);
    }
}