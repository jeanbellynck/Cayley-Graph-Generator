using UnityEngine;

public class TooltipWithCopy : Tooltip {
    [SerializeField] RelatorMenu relatorMenu;
    void Start() {
        content = new() { text = "Click to copy!" };
    }
    public override void OnClick(Kamera activeKamera) {
        GUIUtility.systemCopyBuffer = relatorMenu.CopyableString();
        // TODO: fix this for mobile or WebGL
    }
}