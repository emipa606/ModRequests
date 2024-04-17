using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StrayBullets
{
    [StaticConstructorOnStartup]
    public class Compatibility
    {
        static bool _coilguns = false;

        public static bool VWE_Coilguns => _coilguns;

        static Compatibility()
        {
            _coilguns = ModLister.HasActiveModWithName("Vanilla Weapons Expanded - Coilguns");
            if (_coilguns)
                Main.LogMessage($"Found Vanilla Weapons Expanded - Coilguns", true);
        }
    }
}
