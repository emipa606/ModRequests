using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;

namespace ChickenVOID
{
    [StaticConstructorOnStartup]
    static class HarmonyContainer
    {
        static HarmonyContainer()
        {
            Harmony harmony = new Harmony("com.dninemfive.chickenVOID");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker", new Type[] { typeof(IncidentParms) })]
        class VOIDPatch
        {
            [HarmonyPrefix]
            public static bool TryExecuteWorkerPrefix(ref IncidentParms parms)
            {
                Extension ext;
                if((ext = parms.faction?.def.GetModExtension<Extension>()) != null)
                {
                    if(ext.arrivalMode != null) parms.raidArrivalMode = ext.arrivalMode;
                    if (ext.strategy != null) parms.raidStrategy = ext.strategy;
                    IncidentDef inc;
                    if((inc = ext.incidentOnArrival) != null)
                    {
                        inc.Worker.TryExecute(StorytellerUtility.DefaultParmsNow(inc.category, parms.target));
                    }
                }
                return true;
            }
        }
    }
    public class Extension : DefModExtension
    {
#pragma warning disable CS0649
        public PawnsArrivalModeDef arrivalMode;
        public IncidentDef incidentOnArrival;
        public RaidStrategyDef strategy;
#pragma warning restore CS0649
    }
}
