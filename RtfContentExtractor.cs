using System.Text.RegularExpressions;

public static class RtfContentExtractor
{
    public static IEnumerable<RtfImage> ExtractImageFromRtfContent(string rtfContent)
    {
        var startIndex = 0;
        var imageSubstrings = new List<RtfImage>();
        while (startIndex < rtfContent.Length)
        {
            startIndex = rtfContent.IndexOf(RtfConstants.ImageMarker, startIndex);

            if (startIndex == -1)
            {
                break;
            }

            int endIndex = rtfContent.IndexOf(RtfConstants.ImageEndMarker, startIndex);

            if (endIndex == -1)
            {
                endIndex = rtfContent.IndexOf(RtfConstants.ImageFallbackEndMarker, startIndex);
            }

            if (endIndex != -1)
            {
                int count = endIndex - startIndex + RtfConstants.ImageEndMarker.Length;

                imageSubstrings.Add(GetRtfImage(rtfContent.Substring(startIndex, count)));
                startIndex = endIndex + 1;
            }
            else
            {
                startIndex++;
            }
        }

        return imageSubstrings;
    }

    public static RtfImage GetRtfImage(string rtfImage)
    {
        var imageInfo = new RtfImage
        {
            ImageContent = rtfImage,
            Width = 1,
            Height = 1,
        };

        var matches = Regex.Matches(rtfImage, $@"{Regex.Escape(RtfConstants.Properties.Width)}(\d+)|{Regex.Escape(RtfConstants.Properties.Height)}(\d+)|{Regex.Escape(RtfConstants.Properties.ScaleX)}(\d+)|{Regex.Escape(RtfConstants.Properties.ScaleY)}(\d+)");

        foreach (Match match in matches)
        {
            string property = match.Value;
            int value;

            if (property.StartsWith(RtfConstants.Properties.Width) && int.TryParse(property.Substring(RtfConstants.Properties.Width.Length), out value))
            {
                imageInfo.Width = value;
            }
            else if (property.StartsWith(RtfConstants.Properties.Height) && int.TryParse(property.Substring(RtfConstants.Properties.Height.Length), out value))
            {
                imageInfo.Height = value;
            }
            else if (property.StartsWith(RtfConstants.Properties.ScaleX) && int.TryParse(property.Substring(RtfConstants.Properties.ScaleX.Length), out value))
            {
                imageInfo.ScaleX = value / 100.0f;
            }
            else if (property.StartsWith(RtfConstants.Properties.ScaleY) && int.TryParse(property.Substring(RtfConstants.Properties.ScaleY.Length), out value))
            {
                imageInfo.ScaleY = value / 100.0f;
            }
        }

        return imageInfo;
    }
}
