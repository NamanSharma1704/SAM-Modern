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
using System.Windows.Forms;

namespace SAM.UI
{
    public sealed class ThemedDialog : Form
    {
        public static DialogResult Show(string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return Show(null, text, title, buttons, icon);
        }

        public static DialogResult Show(
            IWin32Window owner,
            string text,
            string title,
            MessageBoxButtons buttons,
            MessageBoxIcon icon)
        {
            using (ThemedDialog dialog = new(owner != null, text, title, buttons, icon))
            {
                return dialog.ShowDialog(owner);
            }
        }

        private ThemedDialog(
            bool hasOwner,
            string text,
            string title,
            MessageBoxButtons buttons,
            MessageBoxIcon icon)
        {
            Theme.Apply(this);
            this.BackColor = Theme.Surface;
            this.Text = title;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = hasOwner == false;
            this.StartPosition = hasOwner == true ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.None;

            int margin = this.Scaled(24);
            int iconWidth = this.Scaled(36);
            int textWidth = this.Scaled(400);
            int buttonHeight = this.Scaled(34);

            var (glyph, color) = GetIcon(icon);
            Label iconLabel = new()
            {
                Text = glyph,
                Font = Theme.Icon(18f),
                ForeColor = color,
                Bounds = new(margin, margin, iconWidth, this.Scaled(32)),
                TextAlign = ContentAlignment.TopLeft,
                AccessibleRole = AccessibleRole.Graphic,
                AccessibleName = icon.ToString(),
            };

            var flags = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix;
            var measured = TextRenderer.MeasureText(text, Theme.Body, new(textWidth, 0), flags);
            Label messageLabel = new()
            {
                Text = text,
                UseMnemonic = false,
                ForeColor = Theme.Text,
                Bounds = new(margin + iconWidth, margin, textWidth, measured.Height + this.Scaled(4)),
            };

            int top = messageLabel.Bottom + margin;
            int width = margin + iconWidth + textWidth + margin;
            int right = width - margin;
            this.ClientSize = new(width, top + buttonHeight + margin);
            this.Controls.Add(iconLabel);
            this.Controls.Add(messageLabel);

            if (buttons == MessageBoxButtons.YesNo)
            {
                // A warning asks about something risky, so Enter has to land on the safe answer.
                var isRisky = icon != MessageBoxIcon.Question;
                var noKind = isRisky == true ? ThemedButtonKind.Primary : ThemedButtonKind.Secondary;
                var yesKind = isRisky == true ? ThemedButtonKind.Secondary : ThemedButtonKind.Primary;
                var no = this.AddButton("No", DialogResult.No, noKind, ref right, top, buttonHeight);
                var yes = this.AddButton("Yes", DialogResult.Yes, yesKind, ref right, top, buttonHeight);
                this.AcceptButton = isRisky == true ? no : yes;
                this.CancelButton = no;
                this.ActiveControl = isRisky == true ? no : yes;
            }
            else
            {
                var ok = this.AddButton("OK", DialogResult.OK, ThemedButtonKind.Primary, ref right, top, buttonHeight);
                this.AcceptButton = ok;
                this.CancelButton = ok;
            }
        }

        private int Scaled(int value)
        {
            return Theme.Scale(value);
        }

        private ThemedButton AddButton(
            string text,
            DialogResult result,
            ThemedButtonKind kind,
            ref int right,
            int top,
            int height)
        {
            ThemedButton button = new()
            {
                Text = text,
                Kind = kind,
                DialogResult = result,
            };
            this.Controls.Add(button);

            int width = Math.Max(this.Scaled(84), button.FitWidth());
            button.SetBounds(right - width, top, width, height);
            right -= width + this.Scaled(8);
            return button;
        }

        private static (string, Color) GetIcon(MessageBoxIcon icon)
        {
            return icon switch
            {
                MessageBoxIcon.Error => (Glyphs.Error, Theme.Danger),
                MessageBoxIcon.Warning => (Glyphs.Warning, Theme.Accent),
                MessageBoxIcon.Question => (Glyphs.Question, Theme.Accent),
                _ => (Glyphs.Info, Theme.TextMuted),
            };
        }
    }
}
