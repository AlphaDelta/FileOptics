using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace HexBoxLib
{
    public class Highlight
    {
        internal int start, end, startrow, endrow;
        internal Color color, colorback;

        public Highlight(int start, int end, Color color)
        {
            this.start = start;
            this.end = end;
            this.startrow = start >> 4;
            this.endrow = end >> 4;
            this.color = color;
            this.colorback = Color.FromArgb(0x39, color.R, color.G, color.B);
        }
        public Highlight(int startX, int startY, int endX, int endY, Color color)
        {
            this.start = (startY << 4) + (startX & 0x0F);
            this.end = (endY << 4) + (endX & 0x0F);
            this.startrow = startY;
            this.endrow = endY;
            this.color = color;
            this.colorback = Color.FromArgb(0x39, color.R, color.G, color.B);
        }
    }
}
