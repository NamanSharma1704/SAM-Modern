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
    public abstract class ScrollHost : Control
    {
        private const int GutterWidth = 14;
        private const float Easing = 0.28f;

        private readonly Timer _Animation = new() { Interval = 15 };

        private float _Offset;
        private float _OffsetTarget;
        private int _ContentHeight;

        private bool _IsThumbHot;
        private bool _IsDraggingThumb;
        private int _DragStartY;
        private float _DragStartOffset;

        protected ScrollHost()
        {
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable,
                true);
            this.TabStop = true;
            this.BackColor = Theme.Background;
            this._Animation.Tick += this.OnAnimationTick;
        }

        protected float Offset => this._Offset;
        protected int ScrollStep { get; set; }
        protected bool IsOverThumb => this._IsThumbHot == true || this._IsDraggingThumb == true;

        protected int ContentHeight
        {
            get => this._ContentHeight;
            set
            {
                this._ContentHeight = value;
                this.ClampOffset();
            }
        }

        private float MaxOffset => Math.Max(0, this._ContentHeight - this.Height);

        protected void ResetScroll()
        {
            this._Offset = 0;
            this._OffsetTarget = 0;
        }

        private void ClampOffset()
        {
            this._OffsetTarget = Math.Max(0, Math.Min(this.MaxOffset, this._OffsetTarget));
            this._Offset = Math.Max(0, Math.Min(this.MaxOffset, this._Offset));
        }

        protected void ScrollIntoView(int top, int bottom)
        {
            if (top < this._OffsetTarget)
            {
                this._OffsetTarget = top;
            }
            else if (bottom > this._OffsetTarget + this.Height)
            {
                this._OffsetTarget = bottom - this.Height;
            }

            this.ClampOffset();
            this.StartAnimation();
        }

        protected void StartAnimation()
        {
            this._Animation.Enabled = true;
        }

        // Returns true while the derived control still has something to animate.
        protected virtual bool Animate()
        {
            return false;
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            var isActive = false;

            var distance = this._OffsetTarget - this._Offset;
            if (Math.Abs(distance) < 0.5f)
            {
                this._Offset = this._OffsetTarget;
            }
            else
            {
                this._Offset += distance * Easing;
                isActive = true;
            }

            isActive |= this.Animate();
            this._Animation.Enabled = isActive;
            this.Invalidate();
        }

        private Rectangle GetThumbBounds()
        {
            if (this._ContentHeight <= this.Height)
            {
                return Rectangle.Empty;
            }

            int height = Math.Max(Theme.Scale(44), this.Height * this.Height / this._ContentHeight);
            int y = (int)((this.Height - height) * (this._Offset / this.MaxOffset));
            int width = Theme.Scale(this.IsOverThumb == true ? 8 : 5);
            return new(this.Width - width - Theme.Scale(3), y, width, height);
        }

        protected void DrawThumb(Graphics graphics)
        {
            var thumb = this.GetThumbBounds();
            if (thumb.IsEmpty == true)
            {
                return;
            }

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = Theme.Rounded(thumb, thumb.Width / 2f))
            using (SolidBrush brush = new(this.IsOverThumb == true ? Theme.TextDim : Theme.Border))
            {
                graphics.FillPath(brush, path);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.ClampOffset();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (this._IsDraggingThumb == true)
            {
                var travel = Math.Max(1, this.Height - this.GetThumbBounds().Height);
                var offset = this._DragStartOffset + ((e.Y - this._DragStartY) * this.MaxOffset / travel);
                this._Offset = this._OffsetTarget = Math.Max(0, Math.Min(this.MaxOffset, offset));
                this.Invalidate();
                return;
            }

            var isThumbHot = e.X >= this.Width - Theme.Scale(GutterWidth) && this._ContentHeight > this.Height;
            if (isThumbHot != this._IsThumbHot)
            {
                this._IsThumbHot = isThumbHot;
                this.Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this._IsThumbHot = false;
            this.StartAnimation();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();

            if (e.Button == MouseButtons.Left && this._IsThumbHot == true)
            {
                this._IsDraggingThumb = true;
                this._DragStartY = e.Y;
                this._DragStartOffset = this._Offset;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            this._IsDraggingThumb = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            this._OffsetTarget -= e.Delta * this.ScrollStep / 120f;
            this.ClampOffset();
            this.StartAnimation();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                this._Animation.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
