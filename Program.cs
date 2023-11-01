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

        Bitmap qrCodeBitmap = QRCodeGenerator.GenerateQRCode(qrCodeText, width, height);
        string qrCodeRtf = qrCodeBitmap.ToRtfString();

        string result = rtfContent.Replace(imageRtf, qrCodeRtf);
        FileHandler.WriteToFile(rtfFilePath, result);
    }
}
