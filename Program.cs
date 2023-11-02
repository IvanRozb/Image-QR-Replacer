class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: Image-QR-Replacer.exe <rtfFilePath> <qrCodeText>");
            return;
        }

        string rtfFilePath = args[0];
        string qrCodeText = args[1];

        string rtfContent = FileHandler.ReadFileContent(rtfFilePath);

        string imageRtf = RtfContentExtractor.ExtractImageFromRtfContent(rtfContent);
        int width = RtfContentExtractor.GetPropertyFromRtfContent("picwgoal", imageRtf);
        int height = RtfContentExtractor.GetPropertyFromRtfContent("pichgoal", imageRtf);
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

        int fullWidth = (int)(width * scaleX);
        int fullHeight = (int)(height * scaleY);

        Bitmap qrCodeBitmap = QRCodeGenerator.GenerateQRCode(qrCodeText, fullWidth, fullHeight);
        string qrCodeRtf = qrCodeBitmap.ToRtfString();

        string result = rtfContent.Replace(imageRtf, qrCodeRtf);
        FileHandler.WriteToFile(rtfFilePath, result);
    }
}
