using RimWorld;
using System.Collections.Generic;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace BillDoorsFramework
{
    public class CompEquippedGizmo : ThingComp
    {
        public virtual IEnumerable<Gizmo> CompGetGizmosEquipped()
        {
            yield return null;
        }
    }

    public class CompEnemyEquippedAlert : ThingComp
    {
        public override void Notify_Equipped(Pawn pawn)
        {
            CheckForHolder();
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            UnCheck();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            UnCheck();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            UnCheck();
        }

        public void CheckForHolder()
        {
            if (parent.ParentHolder is Pawn_EquipmentTracker tracker && (tracker.pawn.Faction?.HostileTo(Faction.OfPlayer) ?? false))
            {
                Alert_HighThreatWeapon.AddThing(parent, tracker.pawn);
                Find.Alerts.AlertsReadoutUpdate();
            }
        }

        public void UnCheck()
        {
            Alert_HighThreatWeapon.RemoveThing(parent);
            Find.Alerts.AlertsReadoutUpdate();
        }
    }
}
