using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AndroidsUpgradesTweaks
{
    class HediffComp_EnergyGain : HediffWithComps
    {
        List<Need> energy_needs;

        bool initialized = false;

        public override void Tick()
        {
            if (!initialized)
            {
                energy_needs = new List<Need>();
                List<Need> needs = pawn.needs.AllNeeds;
                for (int i = 0; i < needs.Count; i++)
                {
                    if (needs[i].def.defName == "ChJEnergy")
                    {
                        energy_needs.Add(needs[i]);
                    }
                }
                initialized = true;
            }

            for (int i = 0; i < energy_needs.Count; i++)
            {
                energy_needs[i].CurLevel = energy_needs[i].MaxLevel;
            }
        }
    }
}
