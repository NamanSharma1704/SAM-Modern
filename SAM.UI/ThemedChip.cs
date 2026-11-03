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
    public sealed class ThemedChip : CheckBox
    {
        private const TextFormatFlags TextFlags =
            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;

        private readonly Fade _Hover;

        public ThemedChip()
        {
            this._Hover = new(this);
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            this.FlatStyle = FlatStyle.Flat;
            this.AutoSize = false;
            this.Font = Theme.Body;
            this.Cursor = Cursors.Hand;
        }

        public int FitWidth()
        {
            return (Theme.Scale(12) * 2) +
                   TextRenderer.MeasureText(this.Text, Theme.BodyStrong, Size.Empty, TextFlags).Width;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this._Hover.Target = 1;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this._Hover.Target = 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            var parent = this.Parent?.BackColor ?? Theme.Background;
            graphics.Clear(parent);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var hover = this._Hover.Value;
            var isOn = this.Checked == true;
            var fill = isOn == true
                ? Theme.Blend(Theme.Surface, Theme.Accent, 0.16f + (0.06f * hover))
                : Theme.Blend(parent, Theme.SurfaceHover, hover);
            var border = isOn == true ? Theme.Blend(Theme.Border, Theme.Accent, 0.6f) : Theme.Border;
            if (this.Focused == true && this.ShowFocusCues == true)
            {
                border = Theme.Accent;
            }

            RectangleF bounds = new(0.5f, 0.5f, this.Width - 1, this.Height - 1);
            using (var path = Theme.Rounded(bounds, Theme.Scale(Theme.CornerRadius)))
            using (SolidBrush brush = new(fill))
            using (Pen pen = new(border))
            {
                graphics.FillPath(brush, path);
                graphics.DrawPath(pen, path);
            }

            var font = isOn == true ? Theme.BodyStrong : this.Font;
            var color = isOn == true ? Theme.AccentHover : Theme.Blend(Theme.TextMuted, Theme.Text, hover);
            var size = TextRenderer.MeasureText(graphics, this.Text, font, Size.Empty, TextFlags);
            Point point = new((this.Width - size.Width) / 2, (this.Height - size.Height) / 2);
            TextRenderer.DrawText(graphics, this.Text, font, point, color, TextFlags);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                this._Hover.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
