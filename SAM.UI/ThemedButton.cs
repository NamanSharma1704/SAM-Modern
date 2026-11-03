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
    public enum ThemedButtonKind
    {
        Secondary,
        Primary,
        Ghost,
    }

    public sealed class ThemedButton : Button
    {
        private const TextFormatFlags TextFlags =
            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;

        private readonly Fade _Hover;
        private bool _Pressed;

        public ThemedButtonKind Kind { get; set; }
        public string Glyph { get; set; }

        public ThemedButton()
        {
            this._Hover = new(this);
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Font = Theme.BodyStrong;
            this.Cursor = Cursors.Hand;
        }

        public int FitWidth()
        {
            int padding = Theme.Scale(14);
            int width = padding * 2;
            if (string.IsNullOrEmpty(this.Glyph) == false)
            {
                width += TextRenderer.MeasureText(this.Glyph, Theme.Icon(11f), Size.Empty, TextFlags).Width;
                width += Theme.Scale(8);
            }
            return width + TextRenderer.MeasureText(this.Text, this.Font, Size.Empty, TextFlags).Width;
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
            this._Pressed = false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this._Pressed = true;
            this.Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            this._Pressed = false;
            this.Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            this.Cursor = this.Enabled == true ? Cursors.Hand : Cursors.Default;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            var parent = this.Parent?.BackColor ?? Theme.Background;
            graphics.Clear(parent);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Color fill, text, border;
            var hover = this._Hover.Value;
            if (this.Enabled == false)
            {
                fill = this.Kind == ThemedButtonKind.Ghost ? parent : Theme.Surface;
                text = Theme.TextDim;
                border = fill;
            }
            else if (this.Kind == ThemedButtonKind.Primary)
            {
                fill = Theme.Blend(Theme.Accent, Theme.AccentHover, hover);
                text = Theme.OnAccent;
                border = fill;
            }
            else if (this.Kind == ThemedButtonKind.Ghost)
            {
                fill = Theme.Blend(parent, Theme.SurfaceHover, hover);
                text = Theme.Blend(Theme.TextMuted, Theme.Text, hover);
                border = fill;
            }
            else
            {
                fill = Theme.Blend(Theme.SurfaceRaised, Theme.SurfaceHover, hover);
                text = Theme.Text;
                border = Theme.Border;
            }

            if (this._Pressed == true)
            {
                fill = Theme.Blend(fill, Theme.Background, 0.2f);
            }

            RectangleF bounds = new(0.5f, 0.5f, this.Width - 1, this.Height - 1);
            using (var path = Theme.Rounded(bounds, Theme.Scale(Theme.CornerRadius)))
            using (SolidBrush brush = new(fill))
            using (Pen pen = new(this.Focused == true && this.ShowFocusCues == true ? Theme.Accent : border))
            {
                graphics.FillPath(brush, path);
                graphics.DrawPath(pen, path);
            }

            this.DrawContent(graphics, text);
        }

        private void DrawContent(Graphics graphics, Color color)
        {
            var hasGlyph = string.IsNullOrEmpty(this.Glyph) == false;
            var hasText = string.IsNullOrEmpty(this.Text) == false;
            var icon = Theme.Icon(11f);

            var glyphSize = hasGlyph == true ? TextRenderer.MeasureText(graphics, this.Glyph, icon, Size.Empty, TextFlags) : Size.Empty;
            var textSize = hasText == true ? TextRenderer.MeasureText(graphics, this.Text, this.Font, Size.Empty, TextFlags) : Size.Empty;
            int gap = hasGlyph == true && hasText == true ? Theme.Scale(8) : 0;

            int x = (this.Width - glyphSize.Width - gap - textSize.Width) / 2;
            if (hasGlyph == true)
            {
                Point point = new(x, (this.Height - glyphSize.Height) / 2);
                TextRenderer.DrawText(graphics, this.Glyph, icon, point, color, TextFlags);
                x += glyphSize.Width + gap;
            }

            if (hasText == true)
            {
                Point point = new(x, (this.Height - textSize.Height) / 2);
                TextRenderer.DrawText(graphics, this.Text, this.Font, point, color, TextFlags);
            }
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
