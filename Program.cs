using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
        int startIndex = rtfContent.IndexOf("{\\pict");
        int endIndex = rtfContent.IndexOf("}\\par");

        string stringToReplace = rtfContent[startIndex..endIndex];
        string width = GetNumberAfterRtfProps("picwgoal", stringToReplace);
        string height = GetNumberAfterRtfProps("pichgoal", stringToReplace);

        if (int.TryParse(width, out int imageWidth) && int.TryParse(height, out int imageHeight))
        {
            var qrCodeBitmap = CreateResultBitmapFromRtf(stringToReplace, qrCodeText, imageWidth, imageHeight);
            string qrStr = GetEmbedImageString(qrCodeBitmap, width, height) + "\n";
            string res = rtfContent.Replace(stringToReplace, qrStr);

            File.WriteAllText(rtfFilePath, res);
        }
        else
        {
            Console.WriteLine("Failed to parse image dimensions.");
        }
    }

    static Bitmap CreateResultBitmapFromRtf(string rtfImageString, string qrCodeText, int imageWidth, int imageHeight)
    {
        Bitmap canvas = new(imageWidth, imageHeight);

        using (Graphics g = Graphics.FromImage(canvas))
        {
            g.FillRectangle(Brushes.White, 0, 0, imageWidth, imageHeight);
        }

        int qrCodeSize = Math.Min(imageWidth, imageHeight);
        int xOffset = (imageWidth - qrCodeSize) / 2;
        int yOffset = (imageHeight - qrCodeSize) / 2;

        var options = new QrCodeEncodingOptions
        {
            DisableECI = true,
            CharacterSet = "UTF-8",
            Width = qrCodeSize,
            Height = qrCodeSize
        };

        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = options
        };

        var qrCodeBitmap = writer.Write(qrCodeText);

        using (Graphics g = Graphics.FromImage(canvas))
        {
            g.DrawImage(qrCodeBitmap, xOffset, yOffset);
        }

        return canvas;
    }

    static string GetNumberAfterRtfProps(string prop, string str)
    {
        string pattern = prop + @"(\d+)";
        Match match = Regex.Match(str, pattern);
        return match.Groups[1].Value;
    }


    [Flags]
    enum EmfToWmfBitsFlags
    {
        EmfToWmfBitsFlagsDefault = 0x00000000,
        EmfToWmfBitsFlagsEmbedEmf = 0x00000001,
        EmfToWmfBitsFlagsIncludePlaceable = 0x00000002,
        EmfToWmfBitsFlagsNoXORClip = 0x00000004
    }

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

    public static string GetEmbedImageString(Bitmap image, string width, string height)
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

        string proto = @"{\pict\wmetafile8\picw" + (int)(image.Width / dpiX * 10000)
                          + @"\pich" + (int)(image.Height / dpiY * 10000)
                          + @"\picwgoal" + width
                          + @"\pichgoal" + height
                          + " "
              + BitConverter.ToString(stream.ToArray()).Replace("-", "")
                          + "";
        return proto;
    }
}