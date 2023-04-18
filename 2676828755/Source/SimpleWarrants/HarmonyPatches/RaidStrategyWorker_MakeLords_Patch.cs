using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace SimpleWarrants
{
    [HarmonyPatch(typeof(RaidStrategyWorker), "MakeLords")]
    public static class RaidStrategyWorker_MakeLords_Patch
    {
        public static Warrant_Pawn warrantToHunt;

        public static void Postfix(IncidentParms parms, List<Pawn> pawns)
        {
            if (warrantToHunt != null)
            {
                var lords = pawns.Select(x => x.GetLord()).Distinct();
                foreach (var lord in lords)
                {
                    if (lord != null)
                    {
                        if (WarrantsManager.Instance.raidLords.ContainsKey(warrantToHunt))
                        {
                            WarrantsManager.Instance.raidLords[warrantToHunt].lords.Add(lord);
                        }
                        else
                        {
                            WarrantsManager.Instance.raidLords[warrantToHunt] = new LordCollection
                            {
                                lords = new List<Lord> { lord }
                            };
                        }
                    }
                }
                warrantToHunt = null;
            }
        }
    }
}
