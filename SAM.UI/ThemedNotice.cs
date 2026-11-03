/* Copyright (c) 2026 Naman
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SAM.UI
{
    public sealed class ThemedNotice : Control
    {
        private const TextFormatFlags TitleFlags =
            TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;

        private const TextFormatFlags BodyFlags =
            TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;

        private string _Title = "";
        private string _Body = "";

        public string Glyph { get; set; } = Glyphs.Lock;

        public ThemedNotice()
        {
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            this.AccessibleRole = AccessibleRole.StaticText;
        }

        public string Title
        {
            get => this._Title;
            set
            {
                this._Title = value ?? "";
                this.Refit();
            }
        }

        public string Body
        {
            get => this._Body;
            set
            {
                this._Body = value ?? "";
                this.Refit();
            }
        }

        private Rectangle Box => new(
            Theme.Scale(20),
            0,
            Math.Max(0, this.Width - Theme.Scale(40)),
            Math.Max(0, this.Height - Theme.Scale(10)));

        private int TextLeft => Theme.Scale(16 + 24 + 12);

        private int TextWidth => Math.Max(Theme.Scale(100), this.Box.Width - this.TextLeft - Theme.Scale(16));

        public int FitHeight()
        {
            var body = TextRenderer.MeasureText(this._Body, Theme.Body, new(this.TextWidth, 0), BodyFlags);
            return Theme.Scale(14 + 20 + 4) + body.Height + Theme.Scale(14 + 10);
        }

        private void Refit()
        {
            this.AccessibleName = this._Title + ". " + this._Body;
            var height = this.FitHeight();
            if (height != this.Height)
            {
                this.Height = height;
            }
            this.Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Refit();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.Clear(this.Parent?.BackColor ?? Theme.Background);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var box = this.Box;
            RectangleF outline = new(box.X + 0.5f, box.Y + 0.5f, box.Width - 1, box.Height - 1);
            using (var path = Theme.Rounded(outline, Theme.Scale(8)))
            using (SolidBrush brush = new(Theme.Surface))
            using (Pen pen = new(Theme.Border))
            {
                graphics.FillPath(brush, path);
                graphics.DrawPath(pen, path);
            }

            int top = box.Y + Theme.Scale(14);
            Rectangle icon = new(box.X + Theme.Scale(16), top, Theme.Scale(24), Theme.Scale(20));
            TextRenderer.DrawText(
                graphics,
                this.Glyph,
                Theme.Icon(12f),
                icon,
                Theme.TextMuted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            Rectangle title = new(box.X + this.TextLeft, top, this.TextWidth, Theme.Scale(20));
            TextRenderer.DrawText(graphics, this._Title, Theme.BodyStrong, title, Theme.Text, TitleFlags | TextFormatFlags.VerticalCenter);

            Rectangle body = new(title.X, title.Bottom + Theme.Scale(4), this.TextWidth, box.Bottom - title.Bottom);
            TextRenderer.DrawText(graphics, this._Body, Theme.Body, body, Theme.TextMuted, BodyFlags);
        }
    }
}
