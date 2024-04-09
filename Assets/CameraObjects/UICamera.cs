using System.Collections;
using DanielLochner.Assets.SimpleSideMenu;
using UnityEngine;
using UnityEngine.UI;

public class UICamera : Kamera {
    [SerializeField] RectTransform renderRectTransform;
    Rect renderRect => renderRectTransform.rect;
    [SerializeField] RawImage renderTarget;
    [SerializeField] RenderTexture renderTexture;
    [SerializeField] RectTransform canvas;
    [SerializeField] Vector2 screenOffset;
    [SerializeField] Vector2 canvasOffset;
    [SerializeField] Vector2 canvasToScreenScale = new(1,1);
    [SerializeField] Vector2 screenToCameraOutputScale = new(1,1);
    float stopCheckingTime;

    void Start() => Deactivate();

    public void SideMenuStateChanged(State a, State b) {
        if (b == State.Open) Activate();
        else Deactivate();
        stopCheckingTime = Time.time + 0.5f; // coroutine just needed bc the side menu is still slightly shifted when the event is called
    }

    public void SideMenuStateChanging(State _, State __) {
        StartCoroutine(checkPosition());
        stopCheckingTime = float.MaxValue;
        return;

        IEnumerator checkPosition() {
            while (Time.time <= stopCheckingTime) {
                yield return null; 
                FixScale();
            }
        }
    }

    void Activate() {
        //cam.targetTexture.Release();
        //cam.targetTexture = null;
        FixScale();
        cam.enabled = true;
        //cam.rect = normalizeRectToCanvasViewport(renderRectTransform);
    }

    //Rect normalizeRectToCanvasViewport(RectTransform rect) { // ZUM VERZWEIFELN
    //    var screenRect = canvas.GetComponent<RectTransform>().rect;
    //    return new Rect(
    //        rect.position.x / screenRect.width,   
    //        1 - rect.position.y / screenRect.height,
    //        rect.rect.width / screenRect.width,
    //        rect.rect.height / screenRect.height
    //    );
    //}



    void Deactivate() {
        FixScale();
        cam.targetTexture.Create();
        cam.enabled = false;
    }

    public override bool IsMouseInViewport(Vector3 mousePosition) {
        return renderRect.Contains(renderRectTransform.InverseTransformPoint(mousePosition));
    }

    
    public override Ray ScreenPointToRay(Vector3 mousePosition) {
        if (Input.GetKeyUp(KeyCode.Alpha1)) FixScale();
        // transform.position is in pixels on the current screen, from the bottom
        // canvasOffset is in pixels on the canvas (1920�1080)
        // screenOffset is in pixels on the current screen
        // mousePosition is in pixels on the current screen, from the bottom
        // pos is in pixels on the canvas, from the bottom
        Ray res = new();
        Vector3 pos = new(
            (mousePosition.x - screenOffset.x) * screenToCameraOutputScale.x - canvasOffset.x,
            (mousePosition.y - screenOffset.y) * screenToCameraOutputScale.y - canvasOffset.y);
        
        if (float.IsFinite(pos.x) && float.IsFinite(pos.y))
            res = cam.ScreenPointToRay(pos);

        if (Input.GetKeyUp(KeyCode.Space))
            Debug.Log("mouse pos:" + mousePosition +
                      ", scaled offset pos: " + pos +
                      ", rect:" + renderRectTransform.rect +
                      ", at" + renderRectTransform.position + 
                      ", scaled Rect:" + new Vector2(renderRect.height * canvasToScreenScale.x, renderRect.width * canvasToScreenScale.y) +
                      ", ray:" + res);

        return res;
    }

    void FixScale()
    {
        canvasToScreenScale = new(
           Screen.width / canvas.rect.width,   
           Screen.height / canvas.rect.height 
        );
        screenOffset = new(
            renderRectTransform.position.x,
            renderRectTransform.position.y
        );
        if (FixRenderTexture())
            screenToCameraOutputScale = new(
                renderRect.width * canvasToScreenScale.x / renderTexture.width,
                renderRect.height * canvasToScreenScale.y / renderTexture.height
            );
    }


    bool FixRenderTexture() {
        var width = Mathf.RoundToInt(Mathf.Max(100, renderRect.width * canvasToScreenScale.x));
        var height = Mathf.RoundToInt(Mathf.Max(100, renderRect.height * canvasToScreenScale.y));
        if (renderTexture != null && !renderTexture.ToString().Equals("null") && renderTexture.height == height && renderTexture.width == width)
            return false;
        
        
        if (renderTexture != null) Destroy(renderTexture);
        renderTexture = new(width, height, 24) {
            antiAliasing = 4
        };
        renderTarget.texture = cam.targetTexture = renderTexture;
        return true;
    }
}
