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
using System.Windows.Forms;

namespace SAM.UI
{
    public sealed class Fade : IDisposable
    {
        private const float Step = 0.2f;

        private readonly Control _Owner;
        private readonly Timer _Timer = new() { Interval = 15 };
        private float _Target;

        public float Value { get; private set; }

        public Fade(Control owner)
        {
            this._Owner = owner;
            this._Timer.Tick += this.OnTick;
        }

        public float Target
        {
            set
            {
                this._Target = value;
                this._Timer.Enabled = this.Value != value;
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            this.Value = this.Value < this._Target
                ? Math.Min(this._Target, this.Value + Step)
                : Math.Max(this._Target, this.Value - Step);

            if (this.Value == this._Target)
            {
                this._Timer.Enabled = false;
            }

            this._Owner.Invalidate();
        }

        public void Dispose()
        {
            this._Timer.Dispose();
        }
    }
}
