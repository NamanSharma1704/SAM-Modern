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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using SAM.Game.Stats;
using SAM.UI;

namespace SAM.Game
{
    internal sealed class AchievementList : ScrollHost
    {
        private const int SkeletonRows = 7;

        private const TextFormatFlags LineFlags =
            TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter;

        private const TextFormatFlags BlockFlags =
            TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;

        private readonly Dictionary<Bitmap, Bitmap> _Icons = new();
        private readonly Dictionary<int, float> _HoverAmounts = new();

        private List<AchievementInfo> _Items = new();
        private int _Selected = -1;
        private int _Hovered = -1;

        private int _Margin;
        private int _TopMargin;
        private int _RowHeight;
        private int _RowPitch;
        private int _IconSize;

        public event Action<AchievementInfo> ItemToggled;
        public event Action<AchievementInfo> ProtectedToggled;

        public string EmptyTitle { get; set; } = "";
        public string EmptyHint { get; set; } = "";

        private bool _IsLoading;

        public bool IsLoading
        {
            get => this._IsLoading;
            set
            {
                this._IsLoading = value;
                this.Invalidate();
            }
        }

        public IReadOnlyList<AchievementInfo> Items => this._Items;

        public AchievementList()
        {
            this.AccessibleName = "Achievements";
            this.AccessibleRole = AccessibleRole.List;
        }

        public int SelectedIndex
        {
            get => this._Selected;
            set
            {
                this._Selected = value;
                this.Invalidate();
            }
        }

        public void SetItems(List<AchievementInfo> items)
        {
            this._Items = items;
            this._Selected = items.Count > 0 ? 0 : -1;
            this._Hovered = -1;
            this._HoverAmounts.Clear();
            this.ResetScroll();
            this.MeasureRows();
            this.Invalidate();
        }

        public HashSet<AchievementInfo> GetVisibleItems()
        {
            HashSet<AchievementInfo> visible = new();
            if (this._RowPitch > 0)
            {
                int first = Math.Max(0, ((int)this.Offset - this._TopMargin) / this._RowPitch);
                int last = Math.Min(this._Items.Count - 1, ((int)this.Offset + this.Height) / this._RowPitch);
                for (int index = first; index <= last; index++)
                {
                    visible.Add(this._Items[index]);
                }
            }
            return visible;
        }

        private void MeasureRows()
        {
            this._Margin = Theme.Scale(20);
            this._TopMargin = Theme.Scale(4);
            this._RowHeight = Theme.Scale(80);
            this._RowPitch = this._RowHeight + Theme.Scale(6);
            this._IconSize = Theme.Scale(56);
            this.ScrollStep = this._RowPitch;
            this.ContentHeight = this._TopMargin + this._Margin + (this._Items.Count * this._RowPitch);
        }

        private Rectangle GetRowBounds(int index)
        {
            return new(
                this._Margin,
                this._TopMargin + (index * this._RowPitch) - (int)this.Offset,
                Math.Max(0, this.ClientSize.Width - (this._Margin * 2)),
                this._RowHeight);
        }

        private Rectangle GetToggleBounds(Rectangle row)
        {
            int width = Theme.Scale(42);
            int height = Theme.Scale(24);
            return new(row.Right - width - Theme.Scale(16), row.Y + ((row.Height - height) / 2), width, height);
        }

        private int HitTest(Point point)
        {
            int y = point.Y - this._TopMargin + (int)this.Offset;
            if (y < 0)
            {
                return -1;
            }

            int index = y / this._RowPitch;
            return index < this._Items.Count && this.GetRowBounds(index).Contains(point) ? index : -1;
        }

        private bool IsOverToggle(int index, Point point)
        {
            var toggle = this.GetToggleBounds(this.GetRowBounds(index));
            toggle.Inflate(Theme.Scale(8), Theme.Scale(8));
            return toggle.Contains(point);
        }

        private void Toggle(int index)
        {
            var info = this._Items[index];
            if (info.IsProtected == true)
            {
                this.ProtectedToggled?.Invoke(info);
                return;
            }

            info.IsChecked = !info.IsChecked;
            this.Invalidate();
            this.ItemToggled?.Invoke(info);
        }

        private Bitmap GetIcon(Bitmap source)
        {
            if (this._Icons.TryGetValue(source, out var icon) == true)
            {
                return icon;
            }

            int size = this._IconSize;
            using (Bitmap scaled = new(size, size, PixelFormat.Format32bppPArgb))
            {
                using (var graphics = Graphics.FromImage(scaled))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(source, 0, 0, size, size);
                }

                icon = new(size, size, PixelFormat.Format32bppPArgb);
                using (var graphics = Graphics.FromImage(icon))
                using (TextureBrush brush = new(scaled))
                using (var path = Theme.Rounded(new(0, 0, size, size), Theme.Scale(8)))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.FillPath(brush, path);
                }
            }

            this._Icons[source] = icon;
            return icon;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.Clear(this.BackColor);

            if (this._Items.Count == 0)
            {
                if (this._IsLoading == true)
                {
                    this.DrawSkeleton(graphics);
                }
                else
                {
                    this.DrawEmptyState(graphics);
                }
                return;
            }

            int first = Math.Max(0, ((int)this.Offset - this._TopMargin) / this._RowPitch);
            int last = Math.Min(this._Items.Count - 1, ((int)this.Offset + this.Height) / this._RowPitch);
            for (int index = first; index <= last; index++)
            {
                this.DrawRow(graphics, index);
            }

            this.DrawThumb(graphics);
        }

        private void DrawRow(Graphics graphics, int index)
        {
            var info = this._Items[index];
            var bounds = this.GetRowBounds(index);
            var isSelected = index == this._Selected;
            var isFocused = isSelected == true && this.Focused == true;
            this._HoverAmounts.TryGetValue(index, out var hover);

            var fill = Theme.Blend(Theme.Background, Theme.Surface, Math.Max(0.45f, Math.Max(hover, isSelected == true ? 1 : 0)));
            if (info.IsChanged == true)
            {
                fill = Theme.Blend(fill, Theme.Accent, 0.08f);
            }

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = Theme.Rounded(bounds, Theme.Scale(8)))
            using (SolidBrush brush = new(fill))
            {
                graphics.FillPath(brush, path);
                if (isFocused == true)
                {
                    using (Pen pen = new(Theme.Blend(fill, Theme.Accent, 0.7f)))
                    {
                        graphics.DrawPath(pen, path);
                    }
                }
            }

            if (info.IsChanged == true)
            {
                RectangleF marker = new(bounds.X + Theme.Scale(5), bounds.Y + Theme.Scale(18), Theme.Scale(3), bounds.Height - Theme.Scale(36));
                using (var path = Theme.Rounded(marker, marker.Width / 2))
                using (SolidBrush brush = new(Theme.Accent))
                {
                    graphics.FillPath(brush, path);
                }
            }

            Rectangle icon = new(
                bounds.X + Theme.Scale(16),
                bounds.Y + ((bounds.Height - this._IconSize) / 2),
                this._IconSize,
                this._IconSize);
            if (info.Icon != null)
            {
                graphics.DrawImageUnscaled(this.GetIcon(info.Icon), icon.Location);
            }
            else
            {
                using (var path = Theme.Rounded(icon, Theme.Scale(8)))
                using (SolidBrush brush = new(Theme.SurfaceRaised))
                {
                    graphics.FillPath(brush, path);
                }
            }

            var toggle = this.GetToggleBounds(bounds);
            this.DrawState(graphics, info, toggle);

            int statusWidth = Theme.Scale(170);
            int statusRight = toggle.Left - Theme.Scale(16);
            int textLeft = icon.Right + Theme.Scale(16);
            int textRight = statusRight - statusWidth - Theme.Scale(12);

            this.DrawStatus(graphics, info, new(statusRight - statusWidth, bounds.Y, statusWidth, bounds.Height));

            var nameColor = info.IsChecked == true ? Theme.Text : Theme.Blend(Theme.TextMuted, Theme.Text, 0.5f);
            Rectangle name = new(textLeft, bounds.Y + Theme.Scale(13), Math.Max(0, textRight - textLeft), Theme.Scale(20));
            TextRenderer.DrawText(graphics, info.Name, Theme.BodyStrong, name, nameColor, LineFlags);

            Rectangle description = new(name.X, name.Bottom + Theme.Scale(3), name.Width, bounds.Bottom - name.Bottom - Theme.Scale(10));
            TextRenderer.DrawText(graphics, info.Description, Theme.Body, description, Theme.TextMuted, BlockFlags);
        }

        private void DrawStatus(Graphics graphics, AchievementInfo info, Rectangle area)
        {
            string text;
            Font font;
            Color color;
            if (info.IsProtected == true)
            {
                text = "Protected";
                font = Theme.Small;
                color = Theme.Danger;
            }
            else if (info.IsChanged == true)
            {
                text = info.IsChecked == true ? "Will unlock" : "Will lock";
                font = Theme.BodyStrong;
                color = Theme.AccentHover;
            }
            else
            {
                text = info.UnlockTime.HasValue == true ? info.UnlockTime.Value.ToString() : "";
                font = Theme.Small;
                color = Theme.TextMuted;
            }

            TextRenderer.DrawText(graphics, text, font, area, color, LineFlags | TextFormatFlags.Right);
        }

        private void DrawState(Graphics graphics, AchievementInfo info, Rectangle toggle)
        {
            if (info.IsProtected == true)
            {
                TextRenderer.DrawText(
                    graphics,
                    Glyphs.Lock,
                    Theme.Icon(12f),
                    toggle,
                    Theme.TextDim,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                return;
            }

            var isOn = info.IsChecked == true;
            float radius = toggle.Height / 2f;
            using (var path = Theme.Rounded(toggle, radius))
            using (SolidBrush brush = new(isOn == true ? Theme.Accent : Theme.SurfaceHover))
            {
                graphics.FillPath(brush, path);
            }

            int inset = Theme.Scale(4);
            int knob = toggle.Height - (inset * 2);
            int x = isOn == true ? toggle.Right - knob - inset : toggle.X + inset;
            using (SolidBrush brush = new(isOn == true ? Theme.OnAccent : Theme.TextMuted))
            {
                graphics.FillEllipse(brush, x, toggle.Y + inset, knob, knob);
            }
        }

        private void DrawSkeleton(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush row = new(Theme.Blend(Theme.Background, Theme.Surface, 0.45f)))
            using (SolidBrush block = new(Theme.Surface))
            {
                for (int index = 0; index < SkeletonRows; index++)
                {
                    var bounds = this.GetRowBounds(index);
                    using (var path = Theme.Rounded(bounds, Theme.Scale(8)))
                    {
                        graphics.FillPath(row, path);
                    }

                    Rectangle icon = new(bounds.X + Theme.Scale(16), bounds.Y + Theme.Scale(12), this._IconSize, this._IconSize);
                    using (var path = Theme.Rounded(icon, Theme.Scale(8)))
                    {
                        graphics.FillPath(block, path);
                    }

                    RectangleF name = new(icon.Right + Theme.Scale(16), bounds.Y + Theme.Scale(20), Theme.Scale(180 + (40 * (index % 3))), Theme.Scale(12));
                    using (var path = Theme.Rounded(name, Theme.Scale(4)))
                    {
                        graphics.FillPath(block, path);
                    }

                    RectangleF description = new(name.X, name.Bottom + Theme.Scale(10), name.Width * 1.6f, Theme.Scale(10));
                    using (var path = Theme.Rounded(description, Theme.Scale(4)))
                    {
                        graphics.FillPath(block, path);
                    }
                }
            }
        }

        private void DrawEmptyState(Graphics graphics)
        {
            var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak;
            int top = (this.Height / 2) - Theme.Scale(40);
            Rectangle title = new(0, top, this.Width, Theme.Scale(36));
            Rectangle hint = new(0, title.Bottom, this.Width, Theme.Scale(30));
            TextRenderer.DrawText(graphics, this.EmptyTitle, Theme.Title, title, Theme.Text, flags);
            TextRenderer.DrawText(graphics, this.EmptyHint, Theme.Body, hint, Theme.TextMuted, flags);
        }

        protected override bool Animate()
        {
            var isActive = false;

            if (this._Hovered >= 0 && this._HoverAmounts.ContainsKey(this._Hovered) == false)
            {
                this._HoverAmounts[this._Hovered] = 0;
            }

            foreach (var index in this._HoverAmounts.Keys.ToList())
            {
                var amount = this._HoverAmounts[index];
                if (index == this._Hovered)
                {
                    amount = Math.Min(1, amount + 0.2f);
                    isActive |= amount < 1;
                    this._HoverAmounts[index] = amount;
                }
                else
                {
                    amount -= 0.2f;
                    if (amount <= 0)
                    {
                        this._HoverAmounts.Remove(index);
                    }
                    else
                    {
                        this._HoverAmounts[index] = amount;
                        isActive = true;
                    }
                }
            }

            return isActive;
        }

        protected override void OnResize(EventArgs e)
        {
            this.MeasureRows();
            base.OnResize(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var hovered = this.IsOverThumb == true ? -1 : this.HitTest(e.Location);
            if (hovered != this._Hovered)
            {
                this._Hovered = hovered;
                this.StartAnimation();
            }

            this.Cursor = hovered >= 0 && this.IsOverToggle(hovered, e.Location) == true ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this._Hovered = -1;
            this.Cursor = Cursors.Default;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (this.IsOverThumb == false)
            {
                var index = this.HitTest(e.Location);
                if (index >= 0)
                {
                    this.SelectedIndex = index;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left && this.IsOverThumb == false)
            {
                var index = this.HitTest(e.Location);
                if (index >= 0 && this.IsOverToggle(index, e.Location) == true)
                {
                    this.Toggle(index);
                }
            }
        }

        private void MoveSelection(int target)
        {
            if (this._Items.Count == 0)
            {
                return;
            }

            this.SelectedIndex = Math.Max(0, Math.Min(this._Items.Count - 1, target));
            int top = this._Selected * this._RowPitch;
            this.ScrollIntoView(top, top + this._RowHeight + this._TopMargin + this._Margin);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData switch
            {
                Keys.Up or Keys.Down => true,
                _ => base.IsInputKey(keyData),
            };
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            int page = Math.Max(1, this.Height / Math.Max(1, this._RowPitch));
            switch (e.KeyCode)
            {
                case Keys.Up:
                {
                    this.MoveSelection(this._Selected - 1);
                    break;
                }

                case Keys.Down:
                {
                    this.MoveSelection(this._Selected + 1);
                    break;
                }

                case Keys.PageUp:
                {
                    this.MoveSelection(this._Selected - page);
                    break;
                }

                case Keys.PageDown:
                {
                    this.MoveSelection(this._Selected + page);
                    break;
                }

                case Keys.Home:
                {
                    this.MoveSelection(0);
                    break;
                }

                case Keys.End:
                {
                    this.MoveSelection(this._Items.Count - 1);
                    break;
                }

                case Keys.Space:
                {
                    if (this._Selected >= 0)
                    {
                        this.Toggle(this._Selected);
                    }
                    break;
                }

                default:
                {
                    return;
                }
            }

            e.Handled = true;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new ListAccessibleObject(this);
        }

        private sealed class ListAccessibleObject : ControlAccessibleObject
        {
            private readonly AchievementList _List;

            public ListAccessibleObject(AchievementList list)
                : base(list)
            {
                this._List = list;
            }

            public override string Value
            {
                get
                {
                    var index = this._List._Selected;
                    if (index < 0 || index >= this._List._Items.Count)
                    {
                        return "";
                    }

                    var info = this._List._Items[index];
                    return info.Name + (info.IsChecked == true ? ", unlocked" : ", locked");
                }
                set
                {
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                foreach (var icon in this._Icons.Values)
                {
                    icon.Dispose();
                }
                this._Icons.Clear();
            }
            base.Dispose(disposing);
        }
    }
}
