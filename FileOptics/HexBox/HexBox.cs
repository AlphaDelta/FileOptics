using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HexBoxLib
{
    internal class HexBoxScroll : VScrollBar, IDisposable
    {
        new public int SmallChange
        {
            get { return base.SmallChange; }
            set
            {
                base.SmallChange = value;
                MWChange = value * 3;
            }
        }

        int MWChange = 0;

        public void PerformMouseWheel(MouseEventArgs e)
        {
            int small = this.SmallChange;
            this.SmallChange = MWChange;
            this.OnMouseWheel(e);
            this.SmallChange = small;
        }
    }

    public class HexBox : Control, IDisposable
    {
        const int MINIMUM_BUFFER_SIZE_IN_ROWS = 128;
        HexBoxScroll vscrollbar;
        public HexBox()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            ////this.AutoScroll = true;

            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            //I dare you to find an easier way to do this
            vscrollbar = new HexBoxScroll();
            vscrollbar.Minimum = 0;
            vscrollbar.Maximum = 0;
            vscrollbar.Visible = false;
            vscrollbar.Dock = DockStyle.Right;

            vscrollbar.Scroll += OnScroll;

            this.Controls.Add(vscrollbar);

            SetTheme(HexBoxTheme.HxD);

            //this.AutoScrollMinSize = new Size(620, 0);
        }

        public List<Highlight> Highlights = new List<Highlight>();

        //[DefaultValue(typeof(Size), "0, 0")]
        //new public Size AutoScrollMinSize
        //{
        //    get { return base.AutoScrollMinSize; }
        //    set { base.AutoScrollMinSize = value; }
        //}

        public void ScrollTo(int row)
        {
            vscrollbar.Value = row * (charheight + _RowSpacing);
            ScrollEventArgs e = new ScrollEventArgs(ScrollEventType.EndScroll, vscrollbar.Value);
            OnScroll(vscrollbar, e);
        }

        public void SetTheme(HexBoxTheme theme)
        {
            if (theme == HexBoxTheme.HxD)
            {
                _XOffset = 10 + 7;
                _YOffset = 8;
                _RowSpacing = 0;// 1;
                _HeaderMargin = 4;
                _OffsetMargin = _TableMargin = 8 + 8;
                _HighlightPaddingX = 2;
                _HighlightPaddingY = 0;
                //_HeaderMargin = _OffsetMargin = _TableMargin = 5;

                BackColor = Color.White;

                HeaderColor = Color.FromArgb(0x00, 0x00, 0xBF);
                OffsetColor = HeaderColor;

                HexTableColor = Color.FromArgb(0x11, 0x11, 0x11);
                ASCIITableColor = HexTableColor;

                _Font = new Font("Courier New", 9.75f);

                //if (!this.DesignMode) this.AutoScrollMinSize = new Size(620, 0);

                CalculatePositions();
            }
            else if (theme == HexBoxTheme.EBlack)
            {
                _XOffset = 10;
                _YOffset = 8;
                _RowSpacing = 2;
                _HeaderMargin = 4;
                _OffsetMargin = _TableMargin = 8;
                _HighlightPaddingX = 4;
                _HighlightPaddingY = 1;
                //_HeaderMargin = _OffsetMargin = _TableMargin = 5;

                BackColor = Color.FromArgb(0x1E, 0x1E, 0x1E);

                HeaderColor = Color.FromArgb(0x00, 0xEE, 0x00);
                OffsetColor = HeaderColor;

                HexTableColor = Color.FromArgb(0xF5, 0xF5, 0xF5);
                ASCIITableColor = Color.FromArgb(0xC5, 0xF5, 0xC5);

                _Font = new Font("Consolas", 8.75f);

                //if (!this.DesignMode) this.AutoScrollMinSize = new Size(620, 0);

                CalculatePositions();
            }

        }

        public int
        _XOffset, _YOffset,
        _RowSpacing, //Spacing between rows
        _HeaderMargin, //Margin between header and hex table / offset
        _OffsetMargin, //Margin between left offset and hex table / header
        _TableMargin,
        _HighlightPaddingX,
        _HighlightPaddingY; //Margin between hex table and ASCII table

        int charwidth = 0, charheight = 0;
        int RowHeight = 0;

        Color
        _HeaderColor,
        _OffsetColor,
        _HexTableColor,
        _ASCIITableColor;

        public Color HeaderColor { get { return _HeaderColor; } set { _HeaderColor = value; _iHeaderColor = (value.B << 0x10) + (value.G << 0x08) + value.R; } }
        public Color OffsetColor { get { return _OffsetColor; } set { _OffsetColor = value; _iOffsetColor = (value.B << 0x10) + (value.G << 0x08) + value.R; } }
        public Color HexTableColor { get { return _HexTableColor; } set { _HexTableColor = value; _iHexTableColor = (value.B << 0x10) + (value.G << 0x08) + value.R; } }
        public Color ASCIITableColor { get { return _ASCIITableColor; } set { _ASCIITableColor = value; _iASCIITableColor = (value.B << 0x10) + (value.G << 0x08) + value.R; } }

        int _iHexTableColor, _iASCIITableColor, _iOffsetColor, _iHeaderColor;

        //public new Font Font { get { return _Font; } set { _Font = value; GDI32.DeleteObject(ptrfont); ptrfont = _Font.ToHfont(); CalculatePositions(); } }
        Font _Font = new Font("Courier New", 9.75f);
        //IntPtr ptrfont;

        Point
        ptmoffset, ptHeader, ptOffset, ptHexTable, ptASCIITable;
        public void CalculatePositions()
        {
            recalcsizes = true;
        }

        public void ReadFile(string path)
        {
            ReadStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public void ReadBytes(byte[] bytes)
        {
            ReadStream(new MemoryStream(bytes));
        }

        Stream stream = null;
        int rows = 1;
        byte[] bufferdata = null;
        int bufferdatalength = 0, bufferblocksize, bufferbegin, bufferend, bufferoffset;
        char[] //ASCIIbuffer,
            HexBuffer = null;
        public void ReadStream(Stream s)
        {
            if (!s.CanRead)
                throw new ArgumentException("Stream must be readable");
            if (!s.CanSeek)
                throw new ArgumentException("Stream must be seekable");

            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
            }
            stream = s;

            this.Highlights.Clear();

            //rows = (int)Math.Ceiling((float)stream.Length / 0x10f);
            //if (rows < 1) rows = 1;
            //MessageBox.Show(((int)s.Length >> 4).ToString());
            rows = (int)s.Length >> 4;
            if (((int)s.Length & 0x0F) != 0) rows++;

            //int bytes = MINIMUM_BUFFER_SIZE_IN_ROWS * 0x10;
            //bufferdata = new byte[bytes];
            //HexBuffer = new char[bytes * 3 - 1];
            //ASCIIbuffer = new char[bytes];

            //bufferdatalength = s.Read(bufferdata, 0, bytes);
            bufferoffset = 0;
            //if (rows < MINIMUM_BUFFER_SIZE_IN_ROWS) bufferblocksize = MINIMUM_BUFFER_SIZE_IN_ROWS;
            CalculateBuffers();

            CalculateScroll();

            vscrollbar.Value = 0;
            OnScroll(null, new ScrollEventArgs(ScrollEventType.EndScroll, 0));
            this.Invalidate();
        }

        public static Encoding ANSI = Encoding.GetEncoding("Windows-1252");
        void CalculateBuffers() //Set buffer offset before calling
        {
            if (stream == null) return;

            bufferblocksize = (visiblerows < MINIMUM_BUFFER_SIZE_IN_ROWS ? MINIMUM_BUFFER_SIZE_IN_ROWS : visiblerows);
            int bytes = (bufferblocksize * 3) << 4;
            //if (bytes < 1) System.Diagnostics.Debugger.Break();

            bufferdata = new byte[bytes];
            stream.Seek(bufferoffset, SeekOrigin.Begin);
            bufferdatalength = stream.Read(bufferdata, 0, bytes);
            //if (bufferdatalength < 1) System.Diagnostics.Debugger.Break();

            bufferbegin = bufferoffset >> 4;
            bufferend = bufferbegin + bufferblocksize * 3;

            HexBuffer = ByteArrayToHexViaLookup32(bufferdata, bufferdatalength);

            for (int i = 0; i < bufferdatalength; i++)
                if (bufferdata[i] <= 0x1F ||
                    bufferdata[i] == 0x7F ||
                    bufferdata[i] == 0x81 ||
                    bufferdata[i] == 0x8D ||
                    bufferdata[i] == 0x8F ||
                    bufferdata[i] == 0x90 ||
                    bufferdata[i] == 0x98 ||
                    bufferdata[i] == 0x9D ||
                    bufferdata[i] == 0xAD) bufferdata[i] = 0x2E;
        }

        private static readonly uint[] _lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            uint[] result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        private static char[] ByteArrayToHexViaLookup32(byte[] bytes, int length)
        {
            char[] result = new char[length * 3 - 1];
            for (int i = 0, pos = 0; i < length; i++, pos++)
            {
                uint val = _lookup32[bytes[i]];
                result[pos] = (char)val;
                result[++pos] = (char)(val >> 16);
                if (pos < result.Length - 1)
                    result[++pos] = ' ';
            }
            return result;// new string(result);
        }

        //int pev = 0;
        bool recalcsizes = true;
        Size shextable, soffset, sascii, schar;
        protected override void OnPaint(PaintEventArgs e)
        {
            if (recalcsizes)
            {
                //Size charsize = TextRenderer.MeasureText(e.Graphics, "0", _Font, ClientSize, TextFormatFlags.NoPadding);
                //charwidth = charsize.Width;

                Size s = new System.Drawing.Size(2048, 1024);
                shextable = TextRenderer.MeasureText(e.Graphics, "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00", _Font, s, TextFormatFlags.NoPadding);
                soffset = TextRenderer.MeasureText(e.Graphics, "00000000", _Font, s, TextFormatFlags.NoPadding);
                sascii = TextRenderer.MeasureText(e.Graphics, "................", _Font, s, TextFormatFlags.NoPadding);
                schar = TextRenderer.MeasureText(e.Graphics, "0", _Font, s, TextFormatFlags.NoPadding);
                charwidth = schar.Width;
                charheight = schar.Height;

                RowHeight = shextable.Height;

                ptmoffset = new Point(_XOffset, _YOffset);

                ptHeader = new Point(_XOffset + soffset.Width + _OffsetMargin, _YOffset);
                ptOffset = new Point(_XOffset, _YOffset + shextable.Height + _HeaderMargin);
                ptHexTable = new Point(ptHeader.X, ptOffset.Y);

                ptASCIITable = new Point(ptHexTable.X + shextable.Width + _TableMargin, ptHexTable.Y);

                CalculateVisible();

                vscrollbar.LargeChange = visiblerows * (RowHeight + _RowSpacing);
                vscrollbar.SmallChange = RowHeight + _RowSpacing;

                recalcsizes = false;
            }

            //base.OnPaint(e);
            e.Graphics.Clear(this.BackColor);

            if (this.DesignMode)
            {
                TextRenderer.DrawText(e.Graphics, "00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F", _Font, ptHeader, HeaderColor, TextFormatFlags.NoPadding);
                TextRenderer.DrawText(e.Graphics, "00000000", _Font, ptOffset, OffsetColor, TextFormatFlags.NoPadding);
                TextRenderer.DrawText(e.Graphics, "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00", _Font, ptHexTable, HexTableColor, TextFormatFlags.NoPadding);
                TextRenderer.DrawText(e.Graphics, "................", _Font, ptASCIITable, ASCIITableColor, TextFormatFlags.NoPadding);

                return;
            }


#if WINDOWS
            IntPtr clip = e.Graphics.Clip.GetHrgn(e.Graphics);
            IntPtr hdc = e.Graphics.GetHdc();
            IntPtr ptrfont = _Font.ToHfont(), ptrfontold = IntPtr.Zero;
            try
            {
                GDI32.SelectClipRgn(hdc, clip);
                GDI32.DeleteObject(clip);

                GDI32.SetBkMode(hdc, 1); //Transparent
                ptrfontold = GDI32.SelectObject(hdc, ptrfont);

                GDI32.SetTextColor(hdc, _iHeaderColor);
                GDI32.ExtTextOut(hdc, ptHeader.X, ptHeader.Y, 0x00, IntPtr.Zero, "00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F", 47, IntPtr.Zero);

                if (rows < 1 || stream == null)
                {
                    GDI32.SetTextColor(hdc, _iOffsetColor);
                    GDI32.ExtTextOut(hdc, ptOffset.X, ptOffset.Y, 0x00, IntPtr.Zero, "00000000", 8, IntPtr.Zero);
                    return;
                }
#else
                TextRenderer.DrawText(e.Graphics, "00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F", _Font, ptHeader, HeaderColor, TextFormatFlags.NoPadding);
                
                if (rows < 1 || stream == null)
                {
                    TextRenderer.DrawText(e.Graphics, "00000000", _Font, ptOffset, OffsetColor, TextFormatFlags.NoPadding);
                    return;
                }
#endif

                Point pptoffset, ppthexoffset, pptasciioffset;
                int bufferloc, hexloc, hexleft, hexlength, asciileft, asciilength;

                if ((rowoffset - bufferbegin) < 1) bufferloc = 0;
                else bufferloc = (rowoffset - bufferbegin) << 4;

                for (int i = rowoffset, j = 0, y = ptOffset.Y; i < rows && j < visiblerows; i++, j++, bufferloc += 0x10, y += RowHeight + _RowSpacing)
                {
                    //y = ptOffset.Y + (RowHeight + _RowSpacing) * j;

                    pptoffset = new Point(ptOffset.X, y);
                    ppthexoffset = new Point(ptHexTable.X, y);
                    pptasciioffset = new Point(ptASCIITable.X, y);

                    hexloc = bufferloc * 3;
                    hexleft = HexBuffer.Length - hexloc;
                    hexlength = (hexleft < 0x2F ? hexleft : 0x2F);
                    asciileft = bufferdatalength - bufferloc;
                    asciilength = (asciileft < 0x10 ? asciileft : 0x10);

                    //Array.Copy(HexBuffer, bufferloc, hexb, 0, hexlength);

                    if (hexlength < 0 || asciilength < 0) return;

#if WINDOWS
                    GDI32.SetTextColor(hdc, _iOffsetColor);
                    //GDI32.TextOut(hdc, pptoffset.X, pptoffset.Y, i.ToString("X7") + "0", 8);
                    GDI32.ExtTextOut(hdc, pptoffset.X, pptoffset.Y, 0x00, IntPtr.Zero, i.ToString("X7") + "0", 8, IntPtr.Zero);

                    GDI32.SetTextColor(hdc, _iHexTableColor);
                    //GDI32.TextOut(hdc, ppthexoffset.X, ppthexoffset.Y, new string(HexBuffer, hexloc, hexlength), hexlength);
                    GDI32.ExtTextOut(hdc, ppthexoffset.X, ppthexoffset.Y, 0x00, IntPtr.Zero, new string(HexBuffer, hexloc, hexlength), hexlength, IntPtr.Zero);

                    GDI32.SetTextColor(hdc, _iASCIITableColor);
                    //GDI32.TextOut(hdc, pptasciioffset.X, pptasciioffset.Y, ANSI.GetString(bufferdata, bufferloc, asciilength), asciilength);
                    GDI32.ExtTextOut(hdc, pptasciioffset.X, pptasciioffset.Y, 0x00, IntPtr.Zero, ANSI.GetString(bufferdata, bufferloc, asciilength), asciilength, IntPtr.Zero);
                    //TextRenderer.DrawText(e.Graphics, new string(HexBuffer, hexloc, hexlength), _Font, ppthexoffset, HexTableColor);
                    //TextRenderer.DrawText(e.Graphics, ANSI.GetString(bufferdata, bufferloc, asciilength), _Font, pptasciioffset, ASCIITableColor);
#else
                    TextRenderer.DrawText(e.Graphics, i.ToString("X7") + "0", _Font, pptoffset, OffsetColor, TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(e.Graphics, new string(HexBuffer, hexloc, hexlength), _Font, ppthexoffset, HexTableColor, TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(e.Graphics, ANSI.GetString(bufferdata, bufferloc, asciilength), _Font, pptasciioffset, ASCIITableColor, TextFormatFlags.NoPadding);
#endif

                }

#if WINDOWS
            }
            finally
            {
                if (hdc != IntPtr.Zero)
                {
                    GDI32.SelectClipRgn(hdc, IntPtr.Zero);
                    GDI32.SelectObject(hdc, ptrfontold);
                    e.Graphics.ReleaseHdc(hdc);
                    GDI32.DeleteObject(ptrfont);
                }
            }
#endif

            #region Highlight 2
            int threechar = (charwidth * 3);
            if (!this.DesignMode)
            {
                int lastrow = rowoffset + visiblerows;
                foreach (Highlight h in Highlights)
                {
                    if (h.startrow > lastrow || h.endrow < rowoffset)
                        continue; //Outside of visible range

                    Brush back = new SolidBrush(h.colorback);
                    Pen border = new Pen(h.color);

                    int hrows = h.endrow - h.startrow;
                    int length = (h.end + 1) - h.start;

                    if (hrows <= 0) //Highlight begins and ends on the same row
                    {
                        Rectangle rect = new Rectangle(
                            ptHexTable.X + (h.start & 0x0F) * threechar,
                            ptHexTable.Y + (h.startrow - rowoffset) * (charheight + _RowSpacing),
                            length * threechar - charwidth,
                            charheight
                            );

                        e.Graphics.FillRectangle(back, rect);
                        e.Graphics.DrawRectangle(border, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

                        //ASCII
                        rect = new Rectangle(
                            ptASCIITable.X + (h.start & 0x0F) * charwidth,
                            ptASCIITable.Y + (h.startrow - rowoffset) * (charheight + _RowSpacing),
                            length * charwidth,
                            charheight
                            );

                        e.Graphics.FillRectangle(back, rect);
                        e.Graphics.DrawRectangle(border, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                    }
                    else if (hrows == 1) //Highlight begins on one row, and ends on the row directly under it
                    {
                        if ((h.start & 0x0F) <= (h.end & 0x0F) && h.startrow >= rowoffset && h.endrow < lastrow) //Rows will face eachother eventually
                        {
                            int startx = ptHexTable.X + (h.start & 0x0F) * threechar;
                            int endx = ptHexTable.X + ((h.end & 0x0F) + 1) * threechar - charwidth - 1;

                            Point[] pts = new Point[10];

                            pts[1] = new Point(startx, ptHexTable.Y + (h.startrow - rowoffset) * (charheight + _RowSpacing));
                            pts[0] = new Point(pts[1].X, pts[1].Y + charheight - 1);
                            pts[2] = new Point(ptHexTable.X + shextable.Width - 1, pts[1].Y);
                            pts[3] = new Point(pts[2].X, pts[0].Y);
                            pts[4] = new Point(endx, pts[3].Y);
                            pts[5] = new Point(pts[4].X, pts[4].Y +_RowSpacing + 1);
                            pts[6] = new Point(pts[5].X, pts[5].Y + charheight - 1);
                            pts[7] = new Point(ptHexTable.X, pts[6].Y);
                            pts[8] = new Point(ptHexTable.X, pts[5].Y);
                            pts[9] = new Point(pts[0].X, pts[5].Y);

                            e.Graphics.FillPolygon(back, pts);
                            e.Graphics.DrawPolygon(border, pts);

                            //ASCII
                            startx = ptASCIITable.X + (h.start & 0x0F) * charwidth;
                            endx = ptASCIITable.X + ((h.end & 0x0F) + 1) * charwidth - 1;

                            //pts = new Point[10];

                            pts[1] = new Point(startx, ptASCIITable.Y + (h.startrow - rowoffset) * (charheight + _RowSpacing));
                            pts[0] = new Point(pts[1].X, pts[1].Y + charheight - 1);
                            pts[2] = new Point(ptASCIITable.X + sascii.Width - 1, pts[1].Y);
                            pts[3] = new Point(pts[2].X, pts[0].Y);
                            pts[4] = new Point(endx, pts[3].Y);
                            pts[5] = new Point(pts[4].X, pts[4].Y + _RowSpacing + 1);
                            pts[6] = new Point(pts[5].X, pts[5].Y + charheight - 1);
                            pts[7] = new Point(ptASCIITable.X, pts[6].Y);
                            pts[8] = new Point(ptASCIITable.X, pts[5].Y);
                            pts[9] = new Point(pts[0].X, pts[5].Y);

                            e.Graphics.FillPolygon(back, pts);
                            e.Graphics.DrawPolygon(border, pts);
                        }
                        else
                        {
                            if (h.startrow >= rowoffset)
                            {
                                Rectangle rect = new Rectangle(
                                    ptHexTable.X + (h.start & 0x0F) * threechar,
                                    ptHexTable.Y + (h.startrow - rowoffset) * (charheight + _RowSpacing),
                                    (0x10 - (h.start & 0x0F)) * threechar - charwidth,
                                    charheight
                                    );

                                e.Graphics.FillRectangle(back, rect);
                                e.Graphics.DrawRectangle(border, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

                                //ASCII
                                rect = new Rectangle(
                                    ptASCIITable.X + (h.start & 0x0F) * charwidth,
                                    ptASCIITable.Y + (h.startrow - rowoffset) * (charheight + _RowSpacing),
                                    (0x10 - (h.start & 0x0F)) * charwidth,
                                    charheight
                                    );

                                e.Graphics.FillRectangle(back, rect);
                                e.Graphics.DrawRectangle(border, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                            }
                            if (h.endrow < lastrow)
                            {
                                Rectangle rect = new Rectangle(
                                    ptHexTable.X,
                                    ptHexTable.Y + (h.endrow - rowoffset) * (charheight + _RowSpacing),
                                    ((h.end & 0x0F) + 1) * threechar - charwidth,
                                    charheight
                                    );

                                e.Graphics.FillRectangle(back, rect);
                                e.Graphics.DrawRectangle(border, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

                                //ASCII
                                rect = new Rectangle(
                                    ptASCIITable.X,
                                    ptASCIITable.Y + (h.endrow - rowoffset) * (charheight + _RowSpacing),
                                    ((h.end & 0x0F) + 1) * charwidth,
                                    charheight
                                    );

                                e.Graphics.FillRectangle(back, rect);
                                e.Graphics.DrawRectangle(border, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                            }
                        }
                    }
                    else //Highlight has a center area that will span the full length of a row at least once
                    {
                        int startx = ptHexTable.X + (h.start & 0x0F) * threechar;
                        int endx = ptHexTable.X + ((h.end & 0x0F) + 1) * threechar - charwidth - 1;
                        int startxa = ptASCIITable.X + (h.start & 0x0F) * charwidth;
                        int endxa = ptASCIITable.X + ((h.end & 0x0F) + 1) * charwidth - 1;

                        if (h.startrow >= rowoffset && h.endrow < lastrow) //Both start and end are visible
                        {
                            Point[] pts = new Point[10];

                            int between = (hrows - 1) * (charheight + _RowSpacing);

                            pts[1] = new Point(startx, ptHexTable.Y + (h.startrow - rowoffset) * (charheight + _RowSpacing));
                            pts[0] = new Point(pts[1].X, pts[1].Y + charheight - 1);
                            //pts[2] = new Point(pts[1].X + (0x10 - (h.start & 0x0F)) * threechar - charwidth - 1, pts[1].Y);
                            pts[2] = new Point(ptHexTable.X + shextable.Width - 1, pts[1].Y);
                            pts[3] = new Point(pts[2].X, pts[0].Y + between);
                            pts[4] = new Point(endx, pts[3].Y);
                            pts[5] = new Point(pts[4].X, pts[4].Y + _RowSpacing + 1);
                            pts[6] = new Point(pts[5].X, pts[5].Y + charheight - 1);
                            pts[7] = new Point(ptHexTable.X, pts[6].Y);
                            pts[8] = new Point(ptHexTable.X, pts[5].Y - between);
                            pts[9] = new Point(pts[0].X, pts[8].Y);

                            e.Graphics.FillPolygon(back, pts);
                            e.Graphics.DrawPolygon(border, pts);

                            //ASCII
                            pts[1] = new Point(startxa, ptASCIITable.Y + (h.startrow - rowoffset) * (charheight + _RowSpacing));
                            pts[0] = new Point(pts[1].X, pts[1].Y + charheight - 1);
                            //pts[2] = new Point(pts[1].X + (0x10 - (h.start & 0x0F)) * threechar - charwidth - 1, pts[1].Y);
                            pts[2] = new Point(ptASCIITable.X + sascii.Width - 1, pts[1].Y);
                            pts[3] = new Point(pts[2].X, pts[0].Y + between);
                            pts[4] = new Point(endxa, pts[3].Y);
                            pts[5] = new Point(pts[4].X, pts[4].Y + _RowSpacing + 1);
                            pts[6] = new Point(pts[5].X, pts[5].Y + charheight - 1);
                            pts[7] = new Point(ptASCIITable.X, pts[6].Y);
                            pts[8] = new Point(ptASCIITable.X, pts[5].Y - between);
                            pts[9] = new Point(pts[0].X, pts[8].Y);

                            e.Graphics.FillPolygon(back, pts);
                            e.Graphics.DrawPolygon(border, pts);
                        }
                        else if (h.startrow >= rowoffset && h.endrow >= lastrow) //End is off-screen
                        {
                            Point[] pts = new Point[6], ptsborder = new Point[6];

                            pts[0] = new Point(ptHexTable.X, ptHexTable.Y + visiblerows * (charheight + _RowSpacing));
                            pts[3] = new Point(startx, ptHexTable.Y + (h.startrow - rowoffset) * (charheight + _RowSpacing));
                            pts[2] = new Point(startx, pts[3].Y + charheight + _RowSpacing);
                            pts[1] = new Point(pts[0].X, pts[2].Y);
                            pts[4] = new Point(ptHexTable.X + shextable.Width - 1, pts[3].Y);
                            pts[5] = new Point(pts[4].X, pts[0].Y);

                            ptsborder[0] = new Point(pts[0].X, pts[0].Y - 1);
                            ptsborder[1] = pts[1];
                            ptsborder[2] = pts[2];
                            ptsborder[3] = pts[3];
                            ptsborder[4] = pts[4];
                            ptsborder[5] = new Point(pts[5].X, pts[5].Y - 1);

                            e.Graphics.FillPolygon(back, pts);
                            e.Graphics.DrawLines(border, ptsborder);

                            //ASCII
                            pts[0] = new Point(ptASCIITable.X, ptASCIITable.Y + visiblerows * (charheight + _RowSpacing));
                            pts[3] = new Point(startxa, ptASCIITable.Y + (h.startrow - rowoffset) * (charheight + _RowSpacing));
                            pts[2] = new Point(startxa, pts[3].Y + charheight + _RowSpacing);
                            pts[1] = new Point(pts[0].X, pts[2].Y);
                            pts[4] = new Point(ptASCIITable.X + sascii.Width - 1, pts[3].Y);
                            pts[5] = new Point(pts[4].X, pts[0].Y);

                            ptsborder[0] = new Point(pts[0].X, pts[0].Y - 1);
                            ptsborder[1] = pts[1];
                            ptsborder[2] = pts[2];
                            ptsborder[3] = pts[3];
                            ptsborder[4] = pts[4];
                            ptsborder[5] = new Point(pts[5].X, pts[5].Y - 1);

                            e.Graphics.FillPolygon(back, pts);
                            e.Graphics.DrawLines(border, ptsborder);
                        }
                        else if (h.startrow < rowoffset && h.endrow < lastrow) //Start is off-screen
                        {
                            Point[] pts;

                            if (h.endrow == rowoffset)
                            {
                                pts = new Point[4];
                                //Point[] ptsborder = new Point[4];

                                pts[0] = new Point(ptHexTable.X, ptHexTable.Y + 1);
                                pts[1] = new Point(pts[0].X, pts[0].Y + charheight - 2);
                                pts[2] = new Point(endx, pts[1].Y);
                                pts[3] = new Point(pts[2].X, pts[0].Y);

                                e.Graphics.FillPolygon(back, pts);
                                e.Graphics.DrawLines(border, pts);

                                //ASCII

                                pts[0] = new Point(ptASCIITable.X, ptASCIITable.Y + 1);
                                pts[1] = new Point(pts[0].X, pts[0].Y + charheight - 2);
                                pts[2] = new Point(endxa, pts[1].Y);
                                pts[3] = new Point(pts[2].X, pts[0].Y);

                                e.Graphics.FillPolygon(back, pts);
                                e.Graphics.DrawLines(border, pts);
                                continue;
                            }

                            pts = new Point[6];

                            pts[0] = new Point(ptHexTable.X, ptHexTable.Y + 1);
                            pts[3] = new Point(endx, ptHexTable.Y + (h.endrow - rowoffset) * (charheight + _RowSpacing) - _RowSpacing - 1);
                            pts[1] = new Point(pts[0].X, pts[3].Y + charheight + _RowSpacing);
                            pts[2] = new Point(pts[3].X, pts[1].Y);
                            pts[4] = new Point(ptHexTable.X + shextable.Width - 1, pts[3].Y);
                            pts[5] = new Point(pts[4].X, pts[0].Y);

                            e.Graphics.FillPolygon(back, pts);
                            e.Graphics.DrawLines(border, pts);

                            //ASCII
                            pts[0] = new Point(ptASCIITable.X, ptASCIITable.Y + 1);
                            pts[3] = new Point(endxa, ptASCIITable.Y + (h.endrow - rowoffset) * (charheight + _RowSpacing) - _RowSpacing - 1);
                            pts[1] = new Point(pts[0].X, pts[3].Y + charheight + _RowSpacing);
                            pts[2] = new Point(pts[3].X, pts[1].Y);
                            pts[4] = new Point(ptASCIITable.X + sascii.Width - 1, pts[3].Y);
                            pts[5] = new Point(pts[4].X, pts[0].Y);

                            e.Graphics.FillPolygon(back, pts);
                            e.Graphics.DrawLines(border, pts);
                        }
                        else //Both are off-screen
                        {
                            Rectangle rect = new Rectangle(ptHexTable.X, ptHexTable.Y + 1, shextable.Width - 1, (charheight + _RowSpacing) * visiblerows - 1);

                            e.Graphics.FillRectangle(back, rect);
                            e.Graphics.DrawLine(border, new Point(rect.Left, rect.Top), new Point(rect.Left, rect.Bottom - 1));
                            e.Graphics.DrawLine(border, new Point(rect.Right, rect.Top), new Point(rect.Right, rect.Bottom - 1));

                            //ASCII
                            rect = new Rectangle(ptASCIITable.X, ptASCIITable.Y + 1, sascii.Width - 1, (charheight + _RowSpacing) * visiblerows - 1);

                            e.Graphics.FillRectangle(back, rect);
                            e.Graphics.DrawLine(border, new Point(rect.Left, rect.Top), new Point(rect.Left, rect.Bottom - 1));
                            e.Graphics.DrawLine(border, new Point(rect.Right, rect.Top), new Point(rect.Right, rect.Bottom - 1));
                        }
                    }
                }
            }
            #endregion
            //TextRenderer.DrawText(e.Graphics, "Visible rows: " + visiblerows, _Font, ptHexTable, HexTableColor);
        }

        void CalculateScroll()
        {
            //AutoScrollMinSize = new Size(AutoScrollMinSize.Width, (RowHeight + _RowSpacing) * rows);

            vscrollbar.Maximum = this.Height + (RowHeight + _RowSpacing) * (rows - 1) - 1;
            vscrollbar.Visible = rows > 1;
        }

        int visiblerows = 0;
        void CalculateVisible()
        {
            int visiblepixels = this.Height - ptHexTable.Y;
            if (visiblepixels < 1)
            {
                visiblerows = 1;
                return;
            }
            int temp = (int)Math.Ceiling((float)visiblepixels / (float)(RowHeight + _RowSpacing));

            if (temp == visiblerows) return;

            visiblerows = temp;

            vscrollbar.LargeChange = visiblerows * (RowHeight + _RowSpacing);

            CalculateBuffers();
        }

        int rowoffset;
        internal void OnScroll(object sender, ScrollEventArgs se)
        {
            //base.OnScroll(se);

            int tempoffset = (int)Math.Floor((float)(se.NewValue) / (float)(RowHeight + _RowSpacing));
            bool diff = (tempoffset != rowoffset);
            rowoffset = (tempoffset < 0 ? 0 : tempoffset);
            if (rowoffset > rows - 1) rowoffset = rows - 1;
            if (!diff) return;


            //if (rowoffset > 1 && rowoffset <= bufferbegin)
            //{
            //    bufferoffset -= bufferblocksize << 4;
            //    CalculateBuffers();
            //}
            //else if (rowoffset + visiblerows < rows && rowoffset + visiblerows >= bufferend - 1)
            //{
            //    bufferoffset += bufferblocksize << 4;
            //    CalculateBuffers();
            //}

            if (rowoffset < 1)
            {
                bufferoffset = 0;
                CalculateBuffers();
            }
            else if (rowoffset <= bufferbegin || rowoffset + visiblerows >= bufferend - 1)
            {
                //bufferoffset -= bufferblocksize << 4;
                bufferoffset = ((int)Math.Floor((float)rowoffset / (float)bufferblocksize) * bufferblocksize) << 4;
                CalculateBuffers();
            }

            this.Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            //vscrollbar.LargeChange = this.Height;

            base.OnResize(e);

            CalculateVisible();
            CalculateScroll();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int p = vscrollbar.Value;
            vscrollbar.PerformMouseWheel(e);
            base.OnMouseWheel(e);
            if (p != vscrollbar.Value) this.Invalidate();
        }

        //bool nopaint = false;
        //const int
        //    WM_HSCROLL = 0x0114,
        //    WM_VSCROLL = 0x0115,
        //    WM_PAINT   = 0x000F;
        //protected override void WndProc(ref Message m)
        //{
        //    switch (m.Msg)
        //    {
        //        case WM_PAINT:
        //            if(!nopaint)
        //                base.WndProc(ref m);
        //            break;
        //        case WM_VSCROLL:
        //        case WM_HSCROLL:
        //            this.Invalidate();
        //            nopaint = true;
        //            base.WndProc(ref m);
        //            nopaint = false;
        //            break;
        //        default:
        //            base.WndProc(ref m);
        //            break;
        //    }
        //}

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        var cp = base.CreateParams;
        //        cp.ExStyle |= 0x02000000;    // Turn on WS_EX_COMPOSITED
        //        return cp;
        //    }
        //}

        protected override void Dispose(bool disposing)
        {
            if (stream != null)
                stream.Dispose();

            base.Dispose(disposing);
        }
    }
}
