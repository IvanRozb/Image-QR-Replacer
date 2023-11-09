internal static class RtfConstants
{
    internal const string ImageMarker = "{\\pict";
    internal const string ImageEndMarker = "}\\par";
    internal const string ImageFallbackEndMarker = "\\par";

    internal class Formats
    {
        internal const string WindowsMetafile = "wmetafile8";
        internal const string Png = "pngblip";
        internal const string Jpeg = "jpegblip";
    }

    internal class Properties
    {
        internal const string Width = "\\picw";
        internal const string Height = "\\pich";
        internal const string ScaleX = "\\picscalex";
        internal const string ScaleY = "\\picscaley";
    }
}