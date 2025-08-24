using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore
{
    [StaticConstructorOnStartup]
    internal static class SCLoader
    {

        static SCLoader()
        {
            if (ModLister.HasActiveModWithName("Vanilla Factions Expanded - Pirates"))
            {

            }
        }

        public static List<TabRecord> LoadTabSettings()
        {

            return new List<TabRecord>();
        }
    }
}
