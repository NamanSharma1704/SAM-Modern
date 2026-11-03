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
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SAM.UI
{
    public sealed class RoundedPicture : Control
    {
        private Bitmap _Source;
        private Bitmap _Scaled;

        public RoundedPicture()
        {
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            this.AccessibleRole = AccessibleRole.Graphic;
        }

        public Bitmap Image
        {
            get => this._Source;
            set
            {
                this._Source = value;
                this.ClearScaled();
                this.Invalidate();
            }
        }

        private void ClearScaled()
        {
            this._Scaled?.Dispose();
            this._Scaled = null;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.ClearScaled();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.Clear(this.Parent?.BackColor ?? Theme.Background);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF bounds = new(0, 0, this.Width, this.Height);
            int radius = Theme.Scale(8);

            if (this._Source == null)
            {
                using (var path = Theme.Rounded(bounds, radius))
                using (SolidBrush brush = new(Theme.SurfaceRaised))
                {
                    graphics.FillPath(brush, path);
                }
                return;
            }

            if (this._Scaled == null)
            {
                this._Scaled = new(this.Width, this.Height, PixelFormat.Format32bppPArgb);
                using (var scaled = Graphics.FromImage(this._Scaled))
                {
                    scaled.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    scaled.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    scaled.DrawImage(this._Source, 0, 0, this.Width, this.Height);
                }
            }

            using (TextureBrush brush = new(this._Scaled))
            using (var path = Theme.Rounded(bounds, radius))
            {
                graphics.FillPath(brush, path);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                this.ClearScaled();
            }
            base.Dispose(disposing);
        }
    }
}
