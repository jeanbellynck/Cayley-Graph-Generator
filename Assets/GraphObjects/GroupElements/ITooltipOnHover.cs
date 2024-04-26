using System;

public interface ITooltipOnHover {
    TooltipContent GetTooltip();
    void OnClick(Kamera activeKamera);
    void OnHover(Kamera activeKamera);
    void OnHoverEnd();
}

[Serializable]
public struct TooltipContent {
    public string text;
    public string url;
}
