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
    public sealed class ThemedProgress : Control
    {
        private float _Value;

        public ThemedProgress()
        {
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            this.AccessibleRole = AccessibleRole.ProgressBar;
        }

        public float Value
        {
            get => this._Value;
            set
            {
                this._Value = Math.Max(0, Math.Min(1, value));
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.Clear(this.Parent?.BackColor ?? Theme.Background);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            float radius = this.Height / 2f;
            using (var path = Theme.Rounded(new(0, 0, this.Width, this.Height), radius))
            using (SolidBrush brush = new(Theme.SurfaceRaised))
            {
                graphics.FillPath(brush, path);
            }

            if (this._Value > 0)
            {
                float width = Math.Max(this.Height, this.Width * this._Value);
                using (var path = Theme.Rounded(new(0, 0, width, this.Height), radius))
                using (SolidBrush brush = new(Theme.Accent))
                {
                    graphics.FillPath(brush, path);
                }
            }
        }
    }
}
