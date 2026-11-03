/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
 * Modified in 2026 by Naman for SAM Modern
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

namespace SAM.Game.Stats
{
    internal class FloatStatInfo : StatInfo
    {
        public float OriginalValue;
        public float FloatValue;
        public float MinValue;
        public float MaxValue;
        public float MaxChange;

        public override object Value
        {
            get => this.FloatValue;
            set
            {
                var f = float.Parse((string)value, System.Globalization.CultureInfo.CurrentCulture);
                if (f.Equals(this.OriginalValue) == false)
                {
                    this.CheckConstraints(f, this.OriginalValue, this.MinValue, this.MaxValue, this.MaxChange);
                }
                this.FloatValue = f;
            }
        }

        public override bool IsModified => this.FloatValue.Equals(this.OriginalValue) == false;
    }
}
