using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace FreeRDC.Services.Host
{
    
    // Taken from http://www.developerfusion.com/code/4630/capture-a-screen-shot/
    public class ScreenCapture
    {

        public Image Capture()
        {
            IntPtr hWnd = User32.GetDesktopWindow();
            IntPtr hdcSrc = User32.GetWindowDC(hWnd);

            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(hWnd, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            GDI32.SelectObject(hdcDest, hOld);
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(hWnd, hdcSrc);
            Image img = Image.FromHbitmap(hBitmap);
            GDI32.DeleteObject(hBitmap);
            return img;
        }

        /*
        public Bitmap Capture2()
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics g = Graphics.FromImage(bmp))
                g.CopyFromScreen(
                    Screen.PrimaryScreen.Bounds.X,
                    Screen.PrimaryScreen.Bounds.Y,
                    0, 0,
                    bmp.Size,
                    CopyPixelOperation.SourceCopy
                );
            return bmp;
        }
        */

        public MemoryStream Capture3()
        {
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 10L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            ImageCodecInfo encoder = ImageCodecInfo.GetImageEncoders().Where(x => x.FormatID == ImageFormat.Jpeg.Guid).Single();

            // Don't use using!!!
            MemoryStream ms = new MemoryStream();

            Image img = Capture();
            //Image img = ConvertTo8bpp(Capture());
            img.Save(ms, encoder, myEncoderParameters);
            ms.Position = 0;

            return ms;
        }

        public Bitmap ConvertTo8bpp(Image img)
        {
            var ms = new MemoryStream();   // Don't use using!!!
            img.Save(ms, ImageFormat.Gif);
            ms.Position = 0;
            return new Bitmap(ms);
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private class GDI32
        {

            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

            [StructLayout(LayoutKind.Sequential)]
            public struct CURSORINFO
            {
                public Int32 cbSize;
                public Int32 flags;
                public IntPtr hCursor;
                public POINTAPI ptScreenPos;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct POINTAPI
            {
                public int x;
                public int y;
            }
            public const Int32 CURSOR_SHOWING = 0x0001;
            public const Int32 DI_NORMAL = 0x0003;
            [DllImport("user32.dll")]
            public static extern bool GetCursorInfo(out CURSORINFO pci);
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool DrawIconEx(IntPtr hdc, int xLeft, int yTop, IntPtr hIcon, int cxWidth, int cyHeight, int istepIfAniCur, IntPtr hbrFlickerFreeDraw, int diFlags);
        }
    }

}

/*

                Image desktop = Capture();
            int cx = 0;
            int cy = 0;
            Image cursor = User32.CaptureCursor(ref cx, ref cy);
            Graphics g;
            if (cursor != null)
            {
                g = Graphics.FromImage(desktop);
                Rectangle r = new Rectangle(cx, cy, cursor.Width, cursor.Height);
                g.DrawImage(cursor, r);
                g.Flush();
            }
            
            desktop.Save(ms, encoder, myEncoderParameters);
            ms.Position = 0;

    */