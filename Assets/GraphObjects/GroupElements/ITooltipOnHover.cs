public interface ITooltipOnHover {
    TooltipContent GetTooltip();
    void OnClick(Kamera activeKamera);
}
public struct TooltipContent {
    public string text;
    public string url;
}
