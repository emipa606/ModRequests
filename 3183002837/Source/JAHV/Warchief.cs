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
    public class Warchief : VehiclePawn
    {
        public int nextChargetick;

        public SoundDef abilityAudio = DefDatabase<SoundDef>.GetNamed("JAHV_WarchiefActivateShield");

        public bool shield;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Patches.shutDown.SetValue(GetComp<CompProjectileInterceptor>(), !shield);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (base.CanMoveWithOperators && !shield && Find.TickManager.TicksGame >= nextChargetick)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Activate shield",
                    icon = TexCommand.DesirePower,
                    action = delegate
                    {
                        shield = true;
                        Patches.shutDown.SetValue(GetComp<CompProjectileInterceptor>(), false);
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        abilityAudio.PlayOneShot(SoundInfo.InMap(this));
                        nextChargetick = Find.TickManager.TicksGame + 1800;
                    }
                };
            }
        }

        public override void Tick()
        {
            if (shield && Find.TickManager.TicksGame > nextChargetick)
            {
                shield = false;
                Patches.shutDown.SetValue(GetComp<CompProjectileInterceptor>(), true);
                nextChargetick = Find.TickManager.TicksGame + 60000;
            }
            base.Tick();
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInspectString());
            if (!shield && Find.TickManager.TicksGame < nextChargetick)
            {
                sb.Append("\nShield on cooldown for {0} hours".Formatted(((float)(nextChargetick - Find.TickManager.TicksGame) / 2500f).ToString("F1")));
            }
            return sb.ToString();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref nextChargetick, "nextChargetick", 0);
            Scribe_Values.Look(ref shield, "nextChargetick", defaultValue: false);
        }
    }
}
