using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public class AnimalWishInfo : IExposable
    {
        public ThingDef animal;
        public int amount = -1;
        public bool shouldBeBonded = false;
        public List<int> includedStage;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref animal, "animal");
            Scribe_Values.Look(ref amount, "amount", -1);
            Scribe_Values.Look(ref shouldBeBonded, "shouldBeBonded", false);
            Scribe_Collections.Look(ref includedStage, "includedStage");
        }
    }
}
