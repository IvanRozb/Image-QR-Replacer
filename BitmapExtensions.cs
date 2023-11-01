using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class BitmapExtensions
{
    public static string ToRtfString(this Bitmap bitmap)
    {
        const int MM_ANISOTROPIC = 8;
        Metafile metafile = null;
        float dpiX;
        float dpiY;

        using (Graphics g = Graphics.FromImage(bitmap))
        {
            IntPtr hDC = g.GetHdc();
            metafile = new Metafile(hDC, EmfType.EmfOnly);
            g.ReleaseHdc(hDC);
        }

        using (Graphics g = Graphics.FromImage(metafile))
        {
            g.DrawImage(bitmap, 0, 0);
            dpiX = g.DpiX;
            dpiY = g.DpiY;
        }

        IntPtr _hEmf = metafile.GetHenhmetafile();
        var _bufferSize = GdipEmfToWmfBits(_hEmf, 0, null, MM_ANISOTROPIC, EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);
        var _buffer = new byte[_bufferSize];
        GdipEmfToWmfBits(_hEmf, _bufferSize, _buffer, MM_ANISOTROPIC, EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);
        IntPtr hmf = SetMetaFileBitsEx(_bufferSize, _buffer);
        var tempfile = Path.GetTempFileName();
        CopyMetaFile(hmf, tempfile);
        DeleteMetaFile(hmf);
        DeleteEnhMetaFile(_hEmf);

        var stream = new MemoryStream();
        var data = File.ReadAllBytes(tempfile);

        var count = data.Length;
        stream.Write(data, 0, count);

        var proto = @"{\pict\wmetafile8\picw" + bitmap.Width
                         + @"\pich" + bitmap.Height
                         + @"\picwgoal" + bitmap.Width
                         + @"\pichgoal" + bitmap.Height
                         + " "
                         + BitConverter.ToString(stream.ToArray()).Replace("-", "")
                         + "\n";
        return proto;
    }

    [DllImport("gdiplus.dll")]
    private static extern uint GdipEmfToWmfBits(IntPtr _hEmf, uint _bufferSize, byte[] _buffer, int _mappingMode, EmfToWmfBitsFlags _flags);
    [DllImport("gdi32.dll")]
    private static extern IntPtr SetMetaFileBitsEx(uint _bufferSize, byte[] _buffer);
    [DllImport("gdi32.dll")]
    private static extern IntPtr CopyMetaFile(IntPtr hWmf, string filename);
    [DllImport("gdi32.dll")]
    private static extern bool DeleteMetaFile(IntPtr hWmf);
    [DllImport("gdi32.dll")]
    private static extern bool DeleteEnhMetaFile(IntPtr hEmf);
}
