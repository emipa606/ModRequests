using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public class IngestibleInfo : IExposable
    {
        public ThingDef ingestible;
        public float amount = -1;
        public bool inIngredient = false;
        public void ExposeData()
        {
            Scribe_Defs.Look(ref ingestible, "ingestible");
            Scribe_Values.Look(ref amount, "amount", -1);
            Scribe_Values.Look(ref inIngredient, "inIngredient", false);
        }
    }

}
