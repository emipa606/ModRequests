using RimTrust.Trade;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;


namespace RimTrust.Core.Interactive
{
    public class Building_NeuralChair : Building
    {
        private CompPowerTrader powerComp;

        public bool CanUseNeuralChairNow
        {
            get
            {
                if (!base.Spawned || !base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare))
                {
                    return powerComp.PowerOn;
                }
                return false;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
           
            if (Methods.LegacySkills.Count.Equals(0))
            {
                LoadTrustFunds.LoadLegacySkills();
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some))
            {
                FloatMenuOption item = new FloatMenuOption("CannotUseNoPath".Translate(), null);
                return new List<FloatMenuOption>
            {
                item
            };
            }
            if (base.Spawned && base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare))
            {
                FloatMenuOption item2 = new FloatMenuOption("CannotUseSolarFlare".Translate(), null);
                return new List<FloatMenuOption>
            {
                item2
            };
            }
            if (!powerComp.PowerOn)
            {
                FloatMenuOption item3 = new FloatMenuOption("CannotUseNoPower".Translate(), null);
                return new List<FloatMenuOption>
            {
                item3
            };
            }
            if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                FloatMenuOption item4 = new FloatMenuOption("CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Sight.label)), null);
                return new List<FloatMenuOption>
            {
                item4
            };
            }
            if (selPawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef_Neural.NeuralOverdose) != null)
            {
                FloatMenuOption item5 = new FloatMenuOption("CannotUseReason".Translate("IncapableOfNIC".Translate()), null);
                return new List<FloatMenuOption>
            {
                item5
            };
            }
            if (selPawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef_Neural.NeuralFatigue) != null)
            {
                FloatMenuOption item6 = new FloatMenuOption("CannotUseReason".Translate("IncapableOfNIC2".Translate()), null);
                return new List<FloatMenuOption>
            {
                item6
            };
            }
            return FloatMenuManagerNeuralChair.RequestBuild(this, selPawn);
        }
    }
}