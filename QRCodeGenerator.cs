using ZXing.QrCode;
using ZXing;

public static class QRCodeGenerator
{
    public static Bitmap GenerateQRCode(string qrCodeText, int width, int height)
    {
        var canvas = new Bitmap(width, height);

        Graphics g = Graphics.FromImage(canvas);

        g.FillRectangle(Brushes.White, 0, 0, width, height);

        var size = Math.Min(width, height);
        var xOffset = (width - size) / 2;
        var yOffset = (height - size) / 2;

        var options = new QrCodeEncodingOptions
        {
            DisableECI = true,
            CharacterSet = "UTF-8",
            Width = size,
            Height = size
        };

        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = options
        };

        var bitmap = writer.Write(qrCodeText);

        g.DrawImage(bitmap, xOffset, yOffset);

        return canvas;
    }
}