using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace CMS
{
    [StaticConstructorOnStartup]
    public static class ConsistentMapStone
    {
        static ConsistentMapStone()
        {
            var harmony = new Harmony("UdderlyEvelyn.ConsistentMapStone");
            harmony.PatchAll();
        }
    }
}
