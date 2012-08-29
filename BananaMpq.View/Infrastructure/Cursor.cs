using System.Runtime.InteropServices;
using System.Windows;

namespace BananaMpq.View.Infrastructure
{
    public static class Cursor
    {
        public static Point Position
        {
            get
            {
                POINT p; 
                GetCursorPos(out p);
                return new Point(p.X, p.Y);
            }
            set 
            { 
                SetCursorPos((int)value.X, (int)value.Y);
            }
        }

        public static bool Show
        {
            set
            {
                while (ShowCursor(value) < 0 == value)
                {
                }
            }
        }

        [DllImport("User32", CallingConvention = CallingConvention.StdCall)]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("User32", CallingConvention = CallingConvention.StdCall)]
        private static extern void GetCursorPos(out POINT p);

        [DllImport("User32", CallingConvention = CallingConvention.StdCall)]
        private static extern int ShowCursor(bool show);

        private struct POINT
        {
            public int X, Y;
        }
    }
}