using DanielLochner.Assets.SimpleSideMenu;
using UnityEngine;
using UnityEngine.UI;
using Canvas = UnityEngine.Canvas;

public class UICamera : Kamera {
    [SerializeField] RectTransform renderRectTransform;
    Rect renderRect => renderRectTransform.rect;
    [SerializeField] RawImage renderTarget;
    [SerializeField] RenderTexture renderTexture;
    [SerializeField] Canvas canvas;

    void Start() => Deactivate();

    public void SideMenuStateChanged(State a, State b) {
        if (b == State.Open) Activate();
        else Deactivate();
    }

    void Activate() {
        //cam.targetTexture.Release();
        //cam.targetTexture = null;
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
        if (renderTexture == null)
            renderTexture = new(Mathf.RoundToInt(renderRect.width), Mathf.RoundToInt(renderRect.height), 24);
        renderTarget.texture = cam.targetTexture = renderTexture;
        
        cam.targetTexture.Create();
        cam.enabled = false;

    }

    public override bool IsMouseInViewport(Vector3 mousePosition) {
        return renderRect.Contains(renderRectTransform.InverseTransformPoint(mousePosition));
    }
}
