using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using Verse.Sound;
using ThumperDef;
using ThumperModSettings;
using System.Diagnostics.Eventing.Reader;
using Verse.Noise;
using UnityEngine;
using static HarmonyLib.Code;

namespace Init
{

    [StaticConstructorOnStartup]
    public static class Starter
    {
        static Starter()
        {
            new Harmony("VIC.Thumper").PatchAll();
            VFEIPatch.Patch();
        }
    }

    [HarmonyPatch(typeof(InfestationCellFinder), "GetScoreAt")]
    public static class VanillaPatch
    {
        static void Postfix(ref float __result, IntVec3 cell, Map map)
        {
            if (__result != 0f)
            {
                //Check if current cell being checked is close to Thumper building.

                Predicate<Thing> validator = (Thing t) => ((t as Building).PowerComp as CompPowerTrader).PowerOn;
                Thing thing = GenClosest.ClosestThing_Global(
                  cell,
                  map.listerThings.ThingsOfDef(ThumperDefOf.VIC_Thumper),
                  ThumperMod.settings.ThumperRadius,
                  validator);

                Predicate<Thing> validator2 = (Thing t) => ((t as Building).PowerComp as CompPowerTrader).PowerOn;
                Thing thing2 = GenClosest.ClosestThing_Global(
                  cell,
                  map.listerThings.ThingsOfDef(ThumperDefOf.VIC_ThumperXL),
                  ThumperMod.settings.ThumperXLRadius,
                  validator2);

                if (thing != null || thing2 != null)
                {
                    __result = 0f;
                }
            }
        }
    }
    public static class VFEIPatch
    {
        public static void Patch()
        {
            var harmony = new Harmony("VIC.LargeInfestationPatch");

            if (ModsConfig.IsActive("OskarPotocki.VFE.Insectoid"))
            {
                harmony.Patch(
                    original: AccessTools.Method(AccessTools.TypeByName("VFEI.Events.IncidentWorker_GiantInfestation"), "TryFindCell"),
                    postfix: new HarmonyMethod(typeof(VFEIPatch), nameof(MyPostfix))
                ); 
            }
        }

        static void MyPostfix(ref bool __result, ref IntVec3 cell, Map map)
        {
            if (__result)
            {
                Predicate<Thing> validator = (Thing t) => ((t as Building).PowerComp as CompPowerTrader).PowerOn;
                Thing thing = GenClosest.ClosestThing_Global(
                    cell,
                    map.listerThings.ThingsOfDef(ThumperDefOf.VIC_Thumper),
                    ThumperMod.settings.ThumperRadius,
                    validator);

                Predicate<Thing> validator2 = (Thing t) => ((t as Building).PowerComp as CompPowerTrader).PowerOn;
                Thing thing2 = GenClosest.ClosestThing_Global(
                    cell,
                    map.listerThings.ThingsOfDef(ThumperDefOf.VIC_ThumperXL),
                    ThumperMod.settings.ThumperXLRadius,
                    validator2);

                if (thing != null || thing2 != null)
                {
                    cell = new IntVec3(-100000, -100000, -100000);
                    __result = false;
                }
            }
        }
    }
}

namespace Thumper
{
    public class Thought_ThumperAnnoyance : ThoughtWorker
    {

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
            {
                return false;
            }
            bool tempreturn = false;

            List<Thing> thumper_list = p.Map.listerThings.ThingsOfDef(ThumperDefOf.VIC_Thumper);
            for (int i = 0; i < thumper_list.Count; i++)
            {
                CompPowerTrader compPowerTrader = thumper_list[i].TryGetComp<CompPowerTrader>();
                if ((compPowerTrader == null || compPowerTrader.PowerOn) && p.Position.InHorDistOf(thumper_list[i].Position, ThumperMod.settings.ThumperRadius))
                {
                    tempreturn = true;
                }
            }

            List<Thing> thumper_listXL = p.Map.listerThings.ThingsOfDef(ThumperDefOf.VIC_ThumperXL);
            for (int i = 0; i < thumper_listXL.Count; i++)
            {
                CompPowerTrader compPowerTrader = thumper_listXL[i].TryGetComp<CompPowerTrader>();
                if ((compPowerTrader == null || compPowerTrader.PowerOn) && p.Position.InHorDistOf(thumper_listXL[i].Position, ThumperMod.settings.ThumperXLRadius))
                {
                    tempreturn = true;
                }
            }

            return tempreturn;
        }
    }

    public class Building_Thumper : Building
    {
        private CompPowerTrader powerComp;
        private void Thumper()
        {
            powerComp = GetComp<CompPowerTrader>();
            //SoundDef sound = ThumperDefOf.VIC_Thumper_hit;

            if (powerComp.PowerOn == true && ThumperMod.settings.enableSound == true)
            {
                //Play Sound
                ThumperDefOf.VIC_Thumper_hit.PlayOneShot(new TargetInfo(this.Position, Map));
            }
        }

        public override void Tick()
        {
            if (this.IsHashIntervalTick(960))
            {
                Thumper();
            }
        }
    }

    public class Building_ThumperXL : Building
    {
        private CompPowerTrader powerComp;
        private void Thumper()
        {

            powerComp = GetComp<CompPowerTrader>();
            //SoundDef sound = ThumperDefOf.VIC_Thumper_hit;

            if (powerComp.PowerOn == true && ThumperMod.settings.enableSound == true)
            {
                //Play Sound
                ThumperDefOf.VIC_Thumper_hit_deep.PlayOneShot(new TargetInfo(this.Position, Map));
            }
        }

        public override void Tick()
        {
            if (this.IsHashIntervalTick(1920))
            {
                Thumper();
            }
        }

    }

    public class PlaceWorker_ThumperRadius : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            GenDraw.DrawRadiusRing(center, ThumperMod.settings.ThumperRadius);
        }
    }

    public class PlaceWorker_ThumperXLRadius : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            GenDraw.DrawRadiusRing(center, ThumperMod.settings.ThumperXLRadius);
        }
    }
}
