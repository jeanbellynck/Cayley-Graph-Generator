using System;

public interface ITooltipOnHover {
    TooltipContent GetTooltip();
    void OnClick(Kamera activeKamera);
}

[Serializable]
public struct TooltipContent {
    public string text;
    public string url;
}
