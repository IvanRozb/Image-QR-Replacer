﻿using System.Text.RegularExpressions;

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

        if (string.IsNullOrEmpty(rtfFilePath) || string.IsNullOrEmpty(qrCodeText))
        {
            Console.WriteLine("Incorrect input");
            Environment.Exit(0);
        }

        var rtfContent = FileHandler.ReadFileContent(rtfFilePath, out var encoding);
        var rtfImages = RtfContentExtractor.ExtractImageFromRtfContent(rtfContent)
            .Where(rtfImage => rtfImage.IsSquare())
            .ToList();
        if (!rtfImages.Any())
        {
            Console.WriteLine("No images found to replace");
            Environment.Exit(0);
        }

        var result = rtfContent;
        foreach (var image in rtfImages)
        {
            result = result.Replace(image.ImageContent, ReplaceImageWithQrCode(image));
        }

        FileHandler.WriteToFile(rtfFilePath, result, encoding);

        string ReplaceImageWithQrCode(RtfImage image)
        {
            var fullWidth = (int)(image.Width * image.ScaleX);
            var fullHeight = (int)(image.Height * image.ScaleY);

            var qrCode = QRCodeGenerator.GenerateQRCode(qrCodeText, fullWidth, fullHeight);
            var qrCodeRtfString = qrCode.ToRtfString();

            var pattern = "\\b(?:89504e47|ffd8ffe0|01000900)[a-zA-Z0-9\\s]+\\b";
            var regex = new Regex(pattern);

            var result = regex.Replace(image.ImageContent, qrCodeRtfString);

            return result
                .Replace(RtfConstants.Formats.Png, RtfConstants.Formats.WindowsMetafile)
                .Replace(RtfConstants.Formats.Jpeg, RtfConstants.Formats.WindowsMetafile);
        }
    }
}
