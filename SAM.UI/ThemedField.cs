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
    public sealed class ThemedField : Control
    {
        private readonly CueTextBox _Box;

        public string Glyph { get; set; }
        public bool NumbersOnly { get; set; }

        public event EventHandler Submitted;

        public ThemedField()
        {
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            this.TabStop = false;

            this._Box = new()
            {
                BorderStyle = BorderStyle.None,
                BackColor = Theme.SurfaceRaised,
                ForeColor = Theme.Text,
                Font = Theme.Body,
            };
            this._Box.KeyDown += this.OnBoxKeyDown;
            this._Box.KeyPress += this.OnBoxKeyPress;
            this._Box.TextChanged += (sender, e) => this.OnTextChanged(e);
            this._Box.GotFocus += (sender, e) => this.Invalidate();
            this._Box.LostFocus += (sender, e) => this.Invalidate();
            this.Controls.Add(this._Box);
        }

        public override string Text
        {
            get => this._Box.Text;
            set => this._Box.Text = value;
        }

        public string Cue
        {
            get => this._Box.Cue;
            set
            {
                this._Box.Cue = value;
                this._Box.AccessibleName = value;
            }
        }

        private void OnBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                this.Submitted?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (this.NumbersOnly == true &&
                char.IsDigit(e.KeyChar) == false &&
                char.IsControl(e.KeyChar) == false)
            {
                e.Handled = true;
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this._Box.Focus();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            int left = Theme.Scale(string.IsNullOrEmpty(this.Glyph) == true ? 12 : 36);
            int right = Theme.Scale(10);
            this._Box.SetBounds(
                left,
                (this.Height - this._Box.Height) / 2,
                Math.Max(0, this.Width - left - right),
                this._Box.Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.Clear(this.Parent?.BackColor ?? Theme.Background);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF bounds = new(0.5f, 0.5f, this.Width - 1, this.Height - 1);
            using (var path = Theme.Rounded(bounds, Theme.Scale(Theme.CornerRadius)))
            using (SolidBrush brush = new(Theme.SurfaceRaised))
            using (Pen pen = new(this._Box.Focused == true ? Theme.Accent : Theme.Border))
            {
                graphics.FillPath(brush, path);
                graphics.DrawPath(pen, path);
            }

            if (string.IsNullOrEmpty(this.Glyph) == false)
            {
                Rectangle area = new(Theme.Scale(12), 0, Theme.Scale(16), this.Height);
                TextRenderer.DrawText(
                    graphics,
                    this.Glyph,
                    Theme.Icon(11f),
                    area,
                    Theme.TextMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }

        // The native cue banner ignores the theme and is too faint on a dark field.
        private sealed class CueTextBox : TextBox
        {
            private const int PaintMessage = 0x000F;

            public string Cue { get; set; }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == PaintMessage &&
                    this.TextLength == 0 &&
                    string.IsNullOrEmpty(this.Cue) == false)
                {
                    using (var graphics = Graphics.FromHwnd(this.Handle))
                    {
                        TextRenderer.DrawText(
                            graphics,
                            this.Cue,
                            this.Font,
                            this.ClientRectangle,
                            Theme.TextMuted,
                            this.BackColor,
                            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    }
                }
            }
        }
    }
}
