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
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SAM.UI
{
    public static class Theme
    {
        public static readonly Color Background = Color.FromArgb(20, 22, 26);
        public static readonly Color Surface = Color.FromArgb(28, 31, 37);
        public static readonly Color SurfaceRaised = Color.FromArgb(38, 42, 50);
        public static readonly Color SurfaceHover = Color.FromArgb(48, 53, 63);
        public static readonly Color Border = Color.FromArgb(58, 63, 74);
        public static readonly Color Text = Color.FromArgb(233, 235, 238);
        public static readonly Color TextMuted = Color.FromArgb(162, 168, 179);
        public static readonly Color TextDim = Color.FromArgb(112, 118, 130);
        public static readonly Color Accent = Color.FromArgb(227, 176, 75);
        public static readonly Color AccentHover = Color.FromArgb(238, 194, 106);
        public static readonly Color OnAccent = Color.FromArgb(27, 21, 6);
        public static readonly Color Danger = Color.FromArgb(232, 119, 108);

        public static readonly Font Body = new("Segoe UI", 9.75f);
        public static readonly Font BodyStrong = new("Segoe UI Semibold", 9.75f);
        public static readonly Font Small = new("Segoe UI", 8.75f);
        public static readonly Font Title = new("Segoe UI Semibold", 16f);

        public const int CornerRadius = 6;

        private static readonly string _IconFamily = FindIconFamily();
        private static readonly Dictionary<float, Font> _IconFonts = new();

        private static string FindIconFamily()
        {
            using (var fonts = new InstalledFontCollection())
            {
                return fonts.Families.Any(family => family.Name == "Segoe Fluent Icons") == true
                    ? "Segoe Fluent Icons"
                    : "Segoe MDL2 Assets";
            }
        }

        public static Font Icon(float size)
        {
            if (_IconFonts.TryGetValue(size, out var font) == false)
            {
                font = new(_IconFamily, size);
                _IconFonts[size] = font;
            }
            return font;
        }

        // Control.DeviceDpi stays at 96 in a system aware .NET Framework process.
        private static readonly float _ScaleFactor = ReadScaleFactor();

        private static float ReadScaleFactor()
        {
            using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                return graphics.DpiX / 96f;
            }
        }

        public static int Scale(int value)
        {
            return (int)Math.Round(value * _ScaleFactor);
        }

        public static Color Blend(Color from, Color to, float amount)
        {
            int Mix(int a, int b) => (int)Math.Round(a + ((b - a) * amount));
            return Color.FromArgb(Mix(from.R, to.R), Mix(from.G, to.G), Mix(from.B, to.B));
        }

        public static GraphicsPath Rounded(RectangleF bounds, float radius)
        {
            var diameter = radius * 2;
            GraphicsPath path = new();
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void Apply(Form form)
        {
            form.BackColor = Background;
            form.ForeColor = Text;
            form.Font = Body;

            if (form.IsHandleCreated == true)
            {
                ApplyTitleBar(form);
            }
            form.HandleCreated += (sender, e) => ApplyTitleBar((Form)sender);
        }

        private static void ApplyTitleBar(Form form)
        {
            // These attributes need Windows 10 20H1 and Windows 11, older versions ignore them.
            int enabled = 1;
            Native.DwmSetWindowAttribute(form.Handle, Native.UseImmersiveDarkMode, ref enabled, sizeof(int));

            int caption = Surface.R | (Surface.G << 8) | (Surface.B << 16);
            Native.DwmSetWindowAttribute(form.Handle, Native.CaptionColor, ref caption, sizeof(int));
        }

        public static void ApplyDarkScrollbars(Control control)
        {
            foreach (var scrollBar in control.Controls.OfType<ScrollBar>())
            {
                Native.SetWindowTheme(scrollBar.Handle, "DarkMode_Explorer", null);
            }
        }

        private static class Native
        {
            public const int UseImmersiveDarkMode = 20;
            public const int CaptionColor = 35;

            [DllImport("dwmapi.dll")]
            public static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);

            [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
            public static extern int SetWindowTheme(IntPtr window, string subApp, string subIdList);
        }
    }

    public static class Glyphs
    {
        public const string Add = "";
        public const string Check = "";
        public const string Download = "";
        public const string Error = "";
        public const string Info = "";
        public const string Lock = "";
        public const string Question = "";
        public const string Refresh = "";
        public const string Search = "";
        public const string Warning = "";
    }
}
