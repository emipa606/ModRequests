using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{
    public class ItemNeededStat : IExposable
    {
        public StatDef def;
        public float minValue;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "stats");
            Scribe_Values.Look(ref minValue, "minValue");
        }
    }

}
