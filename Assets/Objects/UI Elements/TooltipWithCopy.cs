using UnityEngine;

public class TooltipWithCopy : Tooltip {
    [SerializeField] RelatorMenu relatorMenu;
    void Start() {
        content = new() { text = "Click to copy!" };
    }
    public override void OnClick(Kamera activeKamera) {
        var copyText = string.Join(", ", relatorMenu.GetRelatorStrings());
        GUIUtility.systemCopyBuffer = copyText;
    }
}