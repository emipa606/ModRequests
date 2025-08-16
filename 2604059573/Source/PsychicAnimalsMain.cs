using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace PsychicAnimals
{
    public class PsychicAnimalsMain
    {
        [HarmonyPatch(typeof(Pawn), "GetGizmos")]
        class GetGizmosForAnimalsPatch
        {
            public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
            {
                foreach (Gizmo original in __result)
                {
                    yield return original;
                }

                if (__instance.Faction == Faction.OfPlayer && __instance.RaceProps.Animal && __instance.training != null && __instance.training.HasLearned(TrainableDefOf.Release) == AcceptanceReport.WasAccepted)
                {
                    if (__instance.abilities != null)
                    {
                        foreach (Gizmo gizmo in __instance.abilities.GetGizmos())
                        {
                            yield return gizmo;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Hediff_Psylink), "TryGiveAbilityOfLevel")]
        class TryGiveAbilityOfLevelPatch
        {
            public static bool Prefix(Hediff_Psylink __instance, int abilityLevel, bool sendLetter = true)
            {
                Pawn pawn = __instance.pawn;
                if (pawn?.RaceProps?.Animal ?? false)
                {
                    
                    string text = "LetterLabelPsylinkLevelGained".Translate() + ": " + pawn.LabelShortCap;
                    string text2 = null;
                    if (pawn.abilities == null)
                    {
                        pawn.abilities = new Pawn_AbilityTracker(pawn);
                    }
                    if (!pawn.abilities.abilities.Any((Ability a) => a.def.level == abilityLevel))
                    {
                        Dictionary<AbilityDef, bool> dict = PsychicAnimalsMod.Selectpsycasts.AllowedAbilities();
                        List<AbilityDef> allowedabilities = dict.Where(x => x.Value == true).Select(x => x.Key).ToList();
                        AbilityDef val = allowedabilities.Where((AbilityDef a) => a.level == abilityLevel).RandomElement();
                        pawn.abilities.GainAbility(val);
                        text2 = Hediff_Psylink.MakeLetterTextNewPsylinkLevel(pawn, abilityLevel, Gen.YieldSingle(val));
                    }
                    else
                    {
                        text2 = Hediff_Psylink.MakeLetterTextNewPsylinkLevel(pawn, abilityLevel);
                    }
                    if (sendLetter && PawnUtility.ShouldSendNotificationAbout(pawn))
                    {
                        Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.PositiveEvent, pawn);
                    }
                    return false;
                }
                return true;

            }
        }

        [HarmonyPatch(typeof(JobDriver_Meditate), "MeditationTick")]
        class MeditationTickPatch
        {
            public static bool Prefix(JobDriver_Meditate __instance, Mote ___psyfocusMote, Sustainer ___sustainer)
            {
                Pawn pawn = __instance.pawn;
                if (pawn.RaceProps.Animal)
                {
                    if (pawn.IsHashIntervalTick(100))
                    {
                        FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.Meditating);
                    }
                    if (!ModsConfig.RoyaltyActive)
                    {
                        return false;
                    }
                    pawn.psychicEntropy.Notify_Meditated();
                    if (pawn.HasPsylink && pawn.psychicEntropy.PsychicSensitivity > float.Epsilon)
                    {
                        float yOffset = (float)(pawn.Position.x % 2 + pawn.Position.z % 2) / 10f;
                        if (___psyfocusMote == null || ___psyfocusMote.Destroyed)
                        {
                            ___psyfocusMote = MoteMaker.MakeAttachedOverlay(pawn, ThingDefOf.Mote_PsyfocusPulse, Vector3.zero);
                            ___psyfocusMote.yOffset = yOffset;
                        }
                        ___psyfocusMote.Maintain();
                        if (___sustainer == null || ___sustainer.Ended)
                        {
                            ___sustainer = SoundDefOf.MeditationGainPsyfocus.TrySpawnSustainer(SoundInfo.InMap(pawn, MaintenanceType.PerTick));
                        }
                        ___sustainer.Maintain();
                        pawn.psychicEntropy.GainPsyfocus(__instance.Focus.Thing);
                    }
                    return false;
                }
                return true;
            }
        }
        /*

        [HarmonyPatch(typeof(CompUseEffect_InstallImplant), "CanBeUsedBy")]
        public static bool Prefix(ref bool __result, CompUseEffect_InstallImplant __instance, Pawn p, ref string failReason)
        {
            if (!(p.Faction == Faction.OfPlayer && p.RaceProps.Animal && p.training != null && p.training.HasLearned(TrainableDefOf.Release) == AcceptanceReport.WasAccepted))
            {
                return true;
            }
            if (p.RaceProps.body.GetPartsWithDef(__instance.Props.bodyPart).FirstOrFallback() == null)
            {
                failReason = "InstallImplantNoBodyPart".Translate() + ": " + __instance.Props.bodyPart.LabelShort;
                __result = false;
                return false;

            }
            Hediff existingImplant = __instance.GetExistingImplant(p);
            if (existingImplant != null)
            {
                if (!__instance.Props.canUpgrade)
                {
                    failReason = "InstallImplantAlreadyInstalled".Translate();
                    __result = false;
                    return false;
                }
                Hediff_Level hediff_Level = (Hediff_Level)existingImplant;
                if ((float)hediff_Level.level >= hediff_Level.def.maxSeverity)
                {
                    failReason = "InstallImplantAlreadyMaxLevel".Translate();
                    __result = false;
                    return false;
                }
            }
            failReason = null;
            __result = true;
            return false;

        }
        */
    }

}
