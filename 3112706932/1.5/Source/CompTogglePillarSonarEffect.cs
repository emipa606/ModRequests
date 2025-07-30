using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using VFED;

namespace ImperialFunctionality
{
    public class CompTogglePillarSonarEffect : ThingComp
    {
        public bool active = true;
        public CompMotionDetector compMotionDetector;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compMotionDetector = parent.GetComp<CompMotionDetector>();
        }

        [HarmonyPatch(typeof(CompMotionDetector), "ThrowMote")]
        public static class CompMotionDetector_ThrowMote_Patch
        {
            public static bool Prefix(CompMotionDetector __instance, ref Mote __result)
            {
                var comp = __instance.parent.GetComp<CompTogglePillarSonarEffect>();
                if (comp != null && comp.active is false)
                {
                    __result = MoteMaker.MakeStaticMote(comp.parent.DrawPos, comp.parent.Map, IF_DefOf.IF_MoteInvisible);
                    return false;
                }
                return true;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction == Faction.OfPlayer)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "IF.ToggleVisuals".Translate(),
                    icon = parent.def.uiIcon,
                    isActive = () => active,
                    toggleAction = () => 
                    { 
                        active = !active;
                        compMotionDetector.moteGlow = null;
                        compMotionDetector.moteScan = null;
                    },
                };
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref active, "active", true);
        }
    }
}
