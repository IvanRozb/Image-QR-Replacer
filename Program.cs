using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: Image-QR-Replacer.exe <rtfFilePath> <qrCodeText>");
            return;
        }

        var rtfFilePath = args[0];
        var qrCodeText = args[1];

        var rtfContent = FileHandler.ReadFileContent(rtfFilePath);

        var imageRtf = RtfContentExtractor.ExtractImageFromRtfContent(rtfContent);

        var width = RtfContentExtractor.GetPropertyFromRtfContent("picwgoal", imageRtf);
        var height = RtfContentExtractor.GetPropertyFromRtfContent("pichgoal", imageRtf);
        float scaleX;
        float scaleY;
        try
        {
            scaleX = (float)RtfContentExtractor.GetPropertyFromRtfContent("picscalex", imageRtf) / 100;
            scaleY = (float)RtfContentExtractor.GetPropertyFromRtfContent("picscaley", imageRtf) / 100;
        }
        catch (Exception)
        {
            scaleX = 1;
            scaleY = 1;
        }

        var fullWidth  = (int)(width  * scaleX);
        var fullHeight = (int)(height * scaleY);

        var qrCode = QRCodeGenerator.GenerateQRCode(qrCodeText, fullWidth, fullHeight);
        var qrCodeRtfString = qrCode.ToRtfString();

        var pattern = "\\b(?:89504e47|ffd8ffe0|01000900)[a-zA-Z0-9\\s]+\\b"; //pattern to find binary data
        var regex = new Regex(pattern);

        var result = regex.Replace(rtfContent, qrCodeRtfString);
        result = result.Replace("pngblip", "wmetafile8"); //pngblip is tag for setting png format of the image in rtf
        result = result.Replace("jpegblip", "wmetafile8");//jpegblip is tag for setting png format of the image in rtf

        FileHandler.WriteToFile(rtfFilePath, result);
    }
}
