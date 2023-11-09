public record RtfImage
{
    public int Width { get; set; }
    public int Height { get; set; }
    public float ScaleX { get; set; }
    public float ScaleY { get; set; }
    public string ImageContent { get; set; } = default!;

    public bool IsSquare()
    {
        float adjustedWidth = Width * ScaleX;
        float adjustedHeight = Height * ScaleY;

        return Math.Abs(adjustedWidth - adjustedHeight) < 0.0001;
    }
}
