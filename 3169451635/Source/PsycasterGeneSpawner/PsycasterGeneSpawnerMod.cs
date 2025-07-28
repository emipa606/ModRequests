using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using VanillaPsycastsExpanded;
using Verse;

namespace PsycasterGeneSpawner
{
    [StaticConstructorOnStartup]
    public static class PsycasterGeneSpawnerMod
    {
        static PsycasterGeneSpawnerMod() {
            var harmony = new Harmony("PsycasterGeneSpawnerMod");

            harmony.PatchAll();
        }      
    }
}
