using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using Verse;
using Reload.Components;

namespace Reload.Jobs
{
    public class WorkGiver_Scanner_Reload : WorkGiver_Scanner
    {
        CompReload comp;
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            try
            {
                List<Thing> weapons = ReloadUtils.GetAllReloadableEquipmentsFromPawn(pawn);

                if (weapons.NullOrEmpty())
                    return true;
                foreach (Thing weapon in weapons)
                {
                    comp = weapon.TryGetComp<CompReload>();
                    if (!comp.CanLoad())
                        continue;
                    if (Setting.EnableAmmo)
                        return pawn.inventory.Count(comp.Props.ammoDef) == 0;
                    return false;
                }
            }
            catch
            {
                Main.LogMessage("Error in WorkGiver_Scanner_Reload.ShouldSkip");
            }
            return true;
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerThings.AllThings.Where(x => x is Pawn);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (comp == null)
                return null;
            Job job = comp.MakeReloadJob();
            job.playerForced = forced;
            return job;
        }
    }
}
