using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace HediffApplier
{
    public class GenderRatioUtility
    {
        public List<Pawn> GetListByGender(List<Pawn> pawnlist, Gender g)
        {
            List<Pawn> genderedList = new List<Pawn>();
            foreach (Pawn pawn in pawnlist)
            {
                if (g == Gender.Male)
                { if (pawn.gender == Gender.Male) genderedList.Add(pawn); }
                else if (g == Gender.Female)
                { if (pawn.gender == Gender.Female) genderedList.Add(pawn); }
            }
            return genderedList;
        }
    }

    [HarmonyPatch]
    public static class HarmonyPatches
    {
        public static float gfactor = 0.5f;

        public static MethodInfo myMethodInfo = HarmonyLib.AccessTools.FindIncludingInnerTypes(typeof(IncidentWorker_Raid),
            (type) => HarmonyLib.AccessTools.FirstMethod(type, (method) => method.Name.Contains("TryExecuteWorker") && method.ReturnType == typeof(bool)));

        [HarmonyTargetMethod]
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            MethodInfo m = myMethodInfo;
            yield return m;
            m = (MethodInfo)null;
        }


        public static bool Prefix(IncidentWorker_Raid __instance, IncidentParms parms, ref bool __result)
        {
            Type instancetype = __instance.GetType();
            instancetype.GetMethod("ResolveRaidPoints", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, parameters: new object[] { parms });
            if (!(bool)instancetype.GetMethod("TryResolveRaidFaction", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { parms }))
                __result = false;
            PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;
            __instance.ResolveRaidStrategy(parms, combat);
            __instance.ResolveRaidArriveMode(parms);
            parms.raidStrategy.Worker.TryGenerateThreats(parms);
            if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
                __result = false;
            float points = parms.points;
            parms.points = IncidentWorker_Raid.AdjustedRaidPoints(parms.points, parms.raidArrivalMode, parms.raidStrategy, parms.faction, combat);
            List<Pawn> pawnList = parms.raidStrategy.Worker.SpawnThreats(parms);
            if (pawnList == null)
            {
                pawnList = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms)).ToList<Pawn>();
                if (pawnList.Count == 0)
                {
                    Log.Error("Got no pawns spawning raid from parms " + (object)parms);
                    __result = false;
                    return true;
                }
            }
            if (pawnList.ElementAt(0).RaceProps.hasGenders&&!pawnList.ElementAt(0).def.defName.Contains("Revia"))
            {
                GenderRatioUtility gru = new GenderRatioUtility();
                List<Pawn> supplyList = new List<Pawn>();
                supplyList = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms)).ToList<Pawn>();
                List<Pawn> extraSupply = new List<Pawn>();
                extraSupply = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms)).ToList<Pawn>();
                foreach (Pawn pawn in extraSupply)
                {
                    supplyList.Add(pawn);
                }
                List<Pawn> femaleList = gru.GetListByGender(pawnList, Gender.Female);
                List<Pawn> maleList = gru.GetListByGender(pawnList, Gender.Male);
                List<Pawn> maleSupplyList = gru.GetListByGender(supplyList, Gender.Male);
                LetterDef otherLetter = (LetterDef)instancetype.GetMethod("GetLetterDef", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, parameters: null);
                if (otherLetter.defName == "PositiveEvent")
                    gfactor = 0.93f;
                //
                float desiredMaleCount = (float)Math.Floor((decimal)gfactor * pawnList.Count);
                int femaleCount = femaleList.Count;
                int maleCount = maleList.Count;
                System.Random r = new System.Random();
                //
                if (desiredMaleCount > maleCount)
                {
                    foreach (Pawn p in femaleList.Take((int)desiredMaleCount - maleCount))
                    {
                        pawnList.Remove(p);
                    }
                    foreach (Pawn p in maleSupplyList.Take((int)desiredMaleCount - maleCount))
                    {
                        pawnList.Add(p);
                    }
                }
                if (otherLetter.defName != "PositiveEvent")
                {
                    foreach (Pawn p in pawnList)
                    {
                        if (p.gender == Gender.Female && p.ageTracker.AgeBiologicalYears >= 50)
                        {
                            p.ageTracker.AgeBiologicalTicks = r.Next(18, 49) * 3600000L;
                            if (p.health.hediffSet.hediffs.Count != 0) { p.health.RemoveAllHediffs(); }
                        }
                    }
                }
            }
            parms.raidArrivalMode.Worker.Arrive(pawnList, parms);
            instancetype.GetMethod("GenerateRaidLoot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, parameters: new object[] { parms, points, pawnList });
            TaggedString letterLabel = (string)instancetype.GetMethod("GetLetterLabel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, parameters: new object[] { parms });
            TaggedString letterText = (string)instancetype.GetMethod("GetLetterText", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, parameters: new object[] { parms, pawnList });
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter((IEnumerable<Pawn>)pawnList, ref letterLabel, ref letterText, (string)instancetype.GetMethod("GetRelatedPawnsInfoLetterText", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { parms }), true);
            List<TargetInfo> targetInfoList = new List<TargetInfo>();
            if (parms.pawnGroups != null)
            {
                List<List<Pawn>> source = IncidentParmsUtility.SplitIntoGroups(pawnList, parms.pawnGroups);
                List<Pawn> list = source.MaxBy<List<Pawn>, int>((Func<List<Pawn>, int>)(x => x.Count));
                if (list.Any<Pawn>())
                    targetInfoList.Add((TargetInfo)(Thing)list[0]);
                for (int index = 0; index < source.Count; ++index)
                {
                    if (source[index] != list && source[index].Any<Pawn>())
                        targetInfoList.Add((TargetInfo)(Thing)source[index][0]);
                }
            }
            else if (pawnList.Any<Pawn>())
            {
                foreach (Pawn pawn in pawnList)
                    targetInfoList.Add((TargetInfo)(Thing)pawn);
            }
            LetterDef someletter = (LetterDef)instancetype.GetMethod("GetLetterDef", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, parameters: null);
            typeof(IncidentWorker).GetMethod("SendStandardLetter", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {
                typeof(TaggedString),
                typeof(TaggedString),
                typeof(LetterDef),typeof(IncidentParms),typeof(LookTargets),typeof(NamedArgument[]) }, null).Invoke(__instance, parameters:
                new object[] { letterLabel, letterText,(LetterDef)someletter,
                parms, (LookTargets)targetInfoList, (NamedArgument[])Array.Empty<NamedArgument>()});
            if (parms.controllerPawn == null || parms.controllerPawn.Faction != Faction.OfPlayer)
                parms.raidStrategy.Worker.MakeLords(parms, pawnList);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
            if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.ShieldBelts))
            {
                for (int index = 0; index < pawnList.Count; ++index)
                {
                    if (pawnList[index].apparel.WornApparel.Any<Apparel>((Predicate<Apparel>)(ap => ap is ShieldBelt)))
                    {
                        LessonAutoActivator.TeachOpportunity(ConceptDefOf.ShieldBelts, OpportunityType.Critical);
                        break;
                    }
                }
            }
            __result = true;
            return false;
        }
    }
}
