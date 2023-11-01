using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ZXing;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;

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

        string rtfContent = File.ReadAllText(rtfFilePath);

        QrCodeEncodingOptions options = new()
        {
            DisableECI = true,
            CharacterSet = "UTF-8",
            Width = 500,
            Height = 500
        };

        BarcodeWriter writer = new()
        {
            Format = BarcodeFormat.QR_CODE,
            Options = options
        };

        Bitmap qrCodeBitmap = writer.Write(qrCodeText);

        int startIndex = rtfContent.IndexOf("{\\pict");
        int endIndex = rtfContent.IndexOf("}\\par");

        string stringToReplace = rtfContent[startIndex..endIndex];
        string qrStr = GetEmbedImageString(qrCodeBitmap) + "\n";
        string res = rtfContent.Replace(stringToReplace, qrStr);

        File.WriteAllText(rtfFilePath, res);
    }

    public static string ImageToBase64(string imagePath)
    {
        using System.Drawing.Image image = System.Drawing.Image.FromFile(imagePath);
        using MemoryStream stream = new();
        image.Save(stream, ImageFormat.Png); // Save as PNG to preserve image quality
        byte[] imageBytes = stream.ToArray();
        return Convert.ToBase64String(imageBytes);
    }

    [Flags]
    enum EmfToWmfBitsFlags
    {
        EmfToWmfBitsFlagsDefault = 0x00000000,
        EmfToWmfBitsFlagsEmbedEmf = 0x00000001,
        EmfToWmfBitsFlagsIncludePlaceable = 0x00000002,
        EmfToWmfBitsFlagsNoXORClip = 0x00000004
    }

    const int MM_ISOTROPIC = 7;
    const int MM_ANISOTROPIC = 8;

    [DllImport("gdiplus.dll")]
    private static extern uint GdipEmfToWmfBits(IntPtr _hEmf, uint _bufferSize,
        byte[] _buffer, int _mappingMode, EmfToWmfBitsFlags _flags);
    [DllImport("gdi32.dll")]
    private static extern IntPtr SetMetaFileBitsEx(uint _bufferSize,
        byte[] _buffer);
    [DllImport("gdi32.dll")]
    private static extern IntPtr CopyMetaFile(IntPtr hWmf,
        string filename);
    [DllImport("gdi32.dll")]
    private static extern bool DeleteMetaFile(IntPtr hWmf);
    [DllImport("gdi32.dll")]
    private static extern bool DeleteEnhMetaFile(IntPtr hEmf);

    public static string GetEmbedImageString(Bitmap image)
    {
        Metafile metafile = null;
        float dpiX; float dpiY;

        using (Graphics g = Graphics.FromImage(image))
        {
            IntPtr hDC = g.GetHdc();
            metafile = new Metafile(hDC, EmfType.EmfOnly);
            g.ReleaseHdc(hDC);
        }

        using (Graphics g = Graphics.FromImage(metafile))
        {
            g.DrawImage(image, 0, 0);
            dpiX = g.DpiX;
            dpiY = g.DpiY;
        }

        IntPtr _hEmf = metafile.GetHenhmetafile();
        uint _bufferSize = GdipEmfToWmfBits(_hEmf, 0, null, MM_ANISOTROPIC,
        EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);
        byte[] _buffer = new byte[_bufferSize];
        GdipEmfToWmfBits(_hEmf, _bufferSize, _buffer, MM_ANISOTROPIC,
                                    EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);
        IntPtr hmf = SetMetaFileBitsEx(_bufferSize, _buffer);
        string tempfile = Path.GetTempFileName();
        CopyMetaFile(hmf, tempfile);
        DeleteMetaFile(hmf);
        DeleteEnhMetaFile(_hEmf);

        var stream = new MemoryStream();
        byte[] data = File.ReadAllBytes(tempfile);
        //File.Delete (tempfile);
        int count = data.Length;
        stream.Write(data, 0, count);

        string proto = @"{\pict\wmetafile8\picw" + (int)(((float)image.Width / dpiX) * 2540)
                          + @"\pich" + (int)(((float)image.Height / dpiY) * 2540)
                          + @"\picwgoal" + (int)(((float)image.Width / dpiX) * 1440)
                          + @"\pichgoal" + (int)(((float)image.Height / dpiY) * 1440)
                          + " "
              + BitConverter.ToString(stream.ToArray()).Replace("-", "")
                          + "";
        return proto;
    }
}