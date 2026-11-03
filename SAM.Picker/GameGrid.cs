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
using SAM.UI;

namespace SAM.Picker
{
    internal sealed class GameGrid : ScrollHost
    {
        private const float ArtRatio = 87f / 231f;
        private const int MinTileWidth = 206;
        private const int SkeletonTiles = 12;

        private const TextFormatFlags NameFlags =
            TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter;

        private readonly Dictionary<Bitmap, Bitmap> _Art = new();
        private readonly Dictionary<int, float> _HoverAmounts = new();

        private List<GameInfo> _Items = new();
        private int _Selected = -1;
        private int _Hovered = -1;

        private string _TypeAhead = "";
        private DateTime _TypeAheadTime;

        private int _Columns = 1;
        private int _Margin;
        private int _Gap;
        private int _TileWidth;
        private int _ArtHeight;
        private int _TileHeight;
        private int _RowHeight;

        public event EventHandler ItemActivate;
        public event Action<GameInfo> LogoNeeded;

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

        public GameGrid()
        {
            this.AccessibleName = "Games";
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

        public void SetItems(List<GameInfo> items)
        {
            this._Items = items;
            this._Selected = items.Count > 0 ? 0 : -1;
            this._Hovered = -1;
            this._HoverAmounts.Clear();
            this.ResetScroll();
            this.MeasureTiles();
            this.Invalidate();
        }

        public void EnsureVisible(int index)
        {
            if (index < 0 || index >= this._Items.Count)
            {
                return;
            }

            int top = (index / this._Columns) * this._RowHeight;
            this.ScrollIntoView(top, top + this._TileHeight + (this._Margin * 2));
        }

        public bool IsItemVisible(GameInfo info)
        {
            int index = this._Items.IndexOf(info);
            return index >= 0 && this.GetTileBounds(index).IntersectsWith(this.ClientRectangle);
        }

        private void MeasureTiles()
        {
            this._Margin = Theme.Scale(20);
            this._Gap = Theme.Scale(16);

            int available = Math.Max(1, this.ClientSize.Width - (this._Margin * 2));
            int columns = Math.Max(1, (available + this._Gap) / (Theme.Scale(MinTileWidth) + this._Gap));
            int width = (available - (this._Gap * (columns - 1))) / columns;

            if (width != this._TileWidth)
            {
                this.ClearArt();
            }

            this._Columns = columns;
            this._TileWidth = width;
            this._ArtHeight = (int)Math.Round(width * ArtRatio);
            this._TileHeight = this._ArtHeight + Theme.Scale(30);
            this._RowHeight = this._TileHeight + Theme.Scale(16);
            this.ScrollStep = this._RowHeight;

            int rows = (this._Items.Count + columns - 1) / columns;
            this.ContentHeight = (this._Margin * 2) + (rows * this._RowHeight);
        }

        private Rectangle GetTileBounds(int index)
        {
            return new(
                this._Margin + ((index % this._Columns) * (this._TileWidth + this._Gap)),
                this._Margin + ((index / this._Columns) * this._RowHeight) - (int)this.Offset,
                this._TileWidth,
                this._TileHeight);
        }

        private int HitTest(Point point)
        {
            int x = point.X - this._Margin;
            int y = point.Y - this._Margin + (int)this.Offset;
            if (x < 0 || y < 0)
            {
                return -1;
            }

            int column = x / (this._TileWidth + this._Gap);
            int row = y / this._RowHeight;
            int index = (row * this._Columns) + column;
            if (column >= this._Columns || index >= this._Items.Count)
            {
                return -1;
            }

            return this.GetTileBounds(index).Contains(point) ? index : -1;
        }

        private Bitmap GetArt(Bitmap source)
        {
            if (this._Art.TryGetValue(source, out var art) == true)
            {
                return art;
            }

            int width = this._TileWidth;
            int height = this._ArtHeight;
            using (Bitmap scaled = new(width, height, PixelFormat.Format32bppPArgb))
            {
                using (var graphics = Graphics.FromImage(scaled))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(source, 0, 0, width, height);
                }

                art = new(width, height, PixelFormat.Format32bppPArgb);
                using (var graphics = Graphics.FromImage(art))
                using (TextureBrush brush = new(scaled))
                using (var path = Theme.Rounded(new(0, 0, width, height), Theme.Scale(8)))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.FillPath(brush, path);
                }
            }

            this._Art[source] = art;
            return art;
        }

        private void ClearArt()
        {
            foreach (var art in this._Art.Values)
            {
                art.Dispose();
            }
            this._Art.Clear();
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

            int firstRow = Math.Max(0, ((int)this.Offset - this._Margin) / this._RowHeight);
            int lastRow = ((int)this.Offset + this.Height) / this._RowHeight;
            int first = firstRow * this._Columns;
            int last = Math.Min(this._Items.Count - 1, ((lastRow + 1) * this._Columns) - 1);
            for (int index = first; index <= last; index++)
            {
                this.DrawTile(graphics, index);
            }

            this.DrawThumb(graphics);
        }

        private void DrawTile(Graphics graphics, int index)
        {
            var info = this._Items[index];
            var bounds = this.GetTileBounds(index);
            var isSelected = index == this._Selected;
            this._HoverAmounts.TryGetValue(index, out var hover);

            if (info.Logo == null)
            {
                this.LogoNeeded?.Invoke(info);
            }

            Rectangle art = new(
                bounds.X,
                bounds.Y - (int)Math.Round(Theme.Scale(2) * hover),
                this._TileWidth,
                this._ArtHeight);

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (info.Logo != null)
            {
                graphics.DrawImageUnscaled(this.GetArt(info.Logo), art.Location);
            }
            else
            {
                this.DrawPlaceholder(graphics, art, info);
            }

            if (isSelected == true || hover > 0)
            {
                var color = isSelected == true
                    ? (this.Focused == true ? Theme.Accent : Theme.Blend(Theme.Background, Theme.Accent, 0.6f))
                    : Theme.Blend(Theme.Background, Theme.TextMuted, hover);
                using (Pen pen = new(color, isSelected == true ? Theme.Scale(2) : 1))
                using (var path = Theme.Rounded(RectangleF.Inflate(art, Theme.Scale(3), Theme.Scale(3)), Theme.Scale(10)))
                {
                    graphics.DrawPath(pen, path);
                }
            }

            int nameTop = bounds.Y + this._ArtHeight + Theme.Scale(7);
            int nameHeight = bounds.Bottom - nameTop;
            int nameWidth = bounds.Width;

            if (info.Type != "normal")
            {
                var tag = char.ToUpperInvariant(info.Type[0]) + info.Type.Substring(1);
                var size = TextRenderer.MeasureText(graphics, tag, Theme.Small, Size.Empty, NameFlags);
                Rectangle area = new(bounds.Right - size.Width, nameTop, size.Width, nameHeight);
                TextRenderer.DrawText(graphics, tag, Theme.Small, area, Theme.TextDim, NameFlags);
                nameWidth -= size.Width + Theme.Scale(10);
            }

            TextRenderer.DrawText(
                graphics,
                info.Name,
                isSelected == true ? Theme.BodyStrong : Theme.Body,
                new Rectangle(bounds.X, nameTop, nameWidth, nameHeight),
                isSelected == true ? Theme.Text : Theme.Blend(Theme.TextMuted, Theme.Text, hover),
                NameFlags);
        }

        private void DrawPlaceholder(Graphics graphics, Rectangle art, GameInfo info)
        {
            using (SolidBrush brush = new(Theme.Surface))
            using (var path = Theme.Rounded(art, Theme.Scale(8)))
            {
                graphics.FillPath(brush, path);
            }

            var initial = string.IsNullOrEmpty(info.Name) == true ? "" : info.Name.Substring(0, 1);
            TextRenderer.DrawText(
                graphics,
                initial,
                Theme.Title,
                art,
                Theme.TextDim,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        private void DrawSkeleton(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush brush = new(Theme.Surface))
            {
                for (int index = 0; index < SkeletonTiles; index++)
                {
                    var bounds = this.GetTileBounds(index);
                    using (var art = Theme.Rounded(new(bounds.X, bounds.Y, this._TileWidth, this._ArtHeight), Theme.Scale(8)))
                    {
                        graphics.FillPath(brush, art);
                    }

                    RectangleF name = new(
                        bounds.X,
                        bounds.Y + this._ArtHeight + Theme.Scale(12),
                        this._TileWidth * (0.45f + (0.1f * (index % 4))),
                        Theme.Scale(10));
                    using (var bar = Theme.Rounded(name, Theme.Scale(4)))
                    {
                        graphics.FillPath(brush, bar);
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
            this.MeasureTiles();
            base.OnResize(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var hovered = this.IsOverThumb == true ? -1 : this.HitTest(e.Location);
            if (hovered != this._Hovered)
            {
                this._Hovered = hovered;
                this.Cursor = hovered >= 0 ? Cursors.Hand : Cursors.Default;
                this.StartAnimation();
            }
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

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.Button == MouseButtons.Left && this.HitTest(e.Location) >= 0)
            {
                this.ItemActivate?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData switch
            {
                Keys.Up or Keys.Down or Keys.Left or Keys.Right => true,
                _ => base.IsInputKey(keyData),
            };
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            int pageRows = Math.Max(1, this.Height / Math.Max(1, this._RowHeight));
            int target = e.KeyCode switch
            {
                Keys.Left => this._Selected - 1,
                Keys.Right => this._Selected + 1,
                Keys.Up => this._Selected - this._Columns,
                Keys.Down => this._Selected + this._Columns,
                Keys.PageUp => this._Selected - (pageRows * this._Columns),
                Keys.PageDown => this._Selected + (pageRows * this._Columns),
                Keys.Home => 0,
                Keys.End => this._Items.Count - 1,
                _ => int.MinValue,
            };

            if (e.KeyCode == Keys.Enter && this._Selected >= 0)
            {
                e.Handled = true;
                this.ItemActivate?.Invoke(this, EventArgs.Empty);
            }
            else if (target != int.MinValue && this._Items.Count > 0)
            {
                e.Handled = true;
                this.SelectedIndex = Math.Max(0, Math.Min(this._Items.Count - 1, target));
                this.EnsureVisible(this._Selected);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (char.IsControl(e.KeyChar) == true)
            {
                return;
            }

            if ((DateTime.UtcNow - this._TypeAheadTime).TotalSeconds > 1)
            {
                this._TypeAhead = "";
            }

            this._TypeAheadTime = DateTime.UtcNow;
            this._TypeAhead += e.KeyChar;

            var text = this._TypeAhead;
            var index = this._Items.FindIndex(
                info => info.Name != null &&
                        info.Name.StartsWith(text, StringComparison.CurrentCultureIgnoreCase));
            if (index >= 0)
            {
                this.SelectedIndex = index;
                this.EnsureVisible(index);
            }
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new GridAccessibleObject(this);
        }

        private sealed class GridAccessibleObject : ControlAccessibleObject
        {
            private readonly GameGrid _Grid;

            public GridAccessibleObject(GameGrid grid)
                : base(grid)
            {
                this._Grid = grid;
            }

            public override string Value
            {
                get
                {
                    var index = this._Grid._Selected;
                    return index >= 0 && index < this._Grid._Items.Count ? this._Grid._Items[index].Name : "";
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
                this.ClearArt();
            }
            base.Dispose(disposing);
        }
    }
}
