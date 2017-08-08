using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

#if WINDOWS
namespace HexBoxLib
{
    internal class GDI32
    {
        [DllImport("gdi32.dll")]
        internal static extern int SetBkMode(IntPtr hdc, int mode);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiObj);

        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        internal static extern int SelectClipRgn(IntPtr hdc, IntPtr hrgn);

        [DllImport("gdi32.dll", EntryPoint = "TextOutW")]
        internal static extern bool TextOut(IntPtr hdc, int x, int y, [MarshalAs(UnmanagedType.LPWStr)] string str, int len);

        [DllImport("gdi32.dll", EntryPoint = "ExtTextOutW")]
        internal static extern bool ExtTextOut(IntPtr hdc, int x, int y, int fuOptions, IntPtr lprc, [MarshalAs(UnmanagedType.LPWStr)] string str, int len, IntPtr lpDx);

        [DllImport("gdi32.dll")]
        internal static extern int SetTextColor(IntPtr hdc, int color);

        [DllImport("gdi32.dll", EntryPoint = "GetTextExtentPoint32W")]
        private static extern int GetTextExtentPoint32(IntPtr hdc, [MarshalAs(UnmanagedType.LPWStr)] string str, int len, ref Size size);      
    }
}
#endif