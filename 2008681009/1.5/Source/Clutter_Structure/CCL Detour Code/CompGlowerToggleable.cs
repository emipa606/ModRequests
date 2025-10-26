using System;
using Verse;

namespace CCLGlower
{
    public class CompGlowerToggleable : CompGlower
    {
        private bool lit = true;

        public bool Lit
        {
            get
            {
                return this.lit;
            }
            set
            {
                this.lit = value;
                base.UpdateLit();
            }
        }
    }
}
