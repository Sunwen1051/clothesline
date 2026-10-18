namespace Clothesline;

/// <summary>晾衣绳滚轮：夹子上缩放这一张，空白处横向滚动。基准缩略图 184×216。</summary>
internal static class PegZoom
{
    public const double BaseWidth = 184;
    public const double BaseHeight = 216;
    public const double Min = 0.5;
    public const double Max = 2;
    public const double Default = 1;
    public const double Step = 1.1;
    public const double DefaultLineHeight = (BaseHeight + CardExtra) * Default + WindowChrome;

    /// <summary>卡片上下留白：边框外边距 14 加内边距 10。</summary>
    public const double CardExtra = 24;

    /// <summary>绳子、阴影和横向滚动条占用的高度。</summary>
    public const double WindowChrome = 56;

    public enum Gesture
    {
        ZoomPeg,
        ScrollLine,
    }

    public static Gesture Decide(bool pointerOverPeg) =>
        pointerOverPeg ? Gesture.ZoomPeg : Gesture.ScrollLine;

    public static double Next(double current, int wheelDelta)
    {
        double value = double.IsFinite(current) ? current : Default;
        if (wheelDelta > 0) value *= Step;
        else if (wheelDelta < 0) value /= Step;
        return Math.Clamp(value, Min, Max);
    }

    public static double LineHeight(double maxZoom) =>
        (BaseHeight + CardExtra) * Next(maxZoom, 0) + WindowChrome;
}
