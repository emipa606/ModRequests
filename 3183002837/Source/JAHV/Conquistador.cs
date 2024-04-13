using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using Verse;
using Verse.Sound;

namespace JAHV
{
    public class Conquistador : VehiclePawn
    {
        public bool activeBoost;

        public SoundDef abilityAudio = DefDatabase<SoundDef>.GetNamed("JAHV_ConquistadorCombatCommand");

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (!base.CanMoveWithOperators)
            {
                yield break;
            }
            yield return new Command_Action
            {
                defaultLabel = "Combat Boost",
                icon = TexButton.Plus,
                action = delegate
                {
                    if (activeBoost)
                    {
                        SoundDefOf.ClickReject.PlayOneShotOnCamera();
                    }
                    else if (GetComp<Comp_CombatBoost>().cooldowntick > Find.TickManager.TicksGame)
                    {
                        SoundDefOf.ClickReject.PlayOneShotOnCamera();
                        Messages.Message("Combat boost is on cooldown", MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else
                    {
                        activeBoost = true;
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        abilityAudio.PlayOneShot(SoundInfo.InMap(this));
                    }
                }
            };
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInspectString());
            Comp_CombatBoost combatBoost = GetComp<Comp_CombatBoost>();
            if (!activeBoost && Find.TickManager.TicksGame < combatBoost.cooldowntick)
            {
                sb.Append("\nCombat boost is on cooldown for {0} hours".Formatted(((float)(combatBoost.cooldowntick - Find.TickManager.TicksGame) / 2500f).ToString("F1")));
            }
            return sb.ToString();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref activeBoost, "activeBoost", defaultValue: false);
        }
    }
}
