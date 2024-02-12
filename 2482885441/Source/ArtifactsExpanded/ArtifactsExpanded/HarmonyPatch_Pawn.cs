using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace ArtifactsExpanded
{
    [StaticConstructorOnStartup]
    public class HarmonyPatch_Pawn
    {
        //the static constructor
        static HarmonyPatch_Pawn()
        {
            //creates a new Harmony instance and assigns it a unique ID
            var harmonyInstance = new Harmony("RimWorld.Carnagion.ArtifactsExpanded.HarmonyPatch_Pawn");

            //assigns original method, prefix method, and postfix method to variables
            var originalMethod = AccessTools.Method(typeof(Pawn), "Kill");
            var prefixMethod = AccessTools.Method(typeof(HarmonyPatch_Pawn), "Prefix_Kill");
            var postfixMethod = AccessTools.Method(typeof(HarmonyPatch_Pawn), "Postfix_Kill");

            //calls the patch method
            harmonyInstance.Patch(originalMethod, prefix: new HarmonyMethod(prefixMethod), postfix: new HarmonyMethod(postfixMethod));
        }

        static bool Prefix_Kill(Pawn __instance, out List<object> __state)
        {
            __state = new List<object>();
            //proceeds if pawn is wearing archotech sacrificer belt
            if (PawnIsWearingSacrificerBelt(__instance) != -1)
            {
                __state.Add(__instance);
                __state.Add(__instance.Map);
                return true;
            }
            //lets original method run by default
            return true;
        }

        static void Postfix_Kill(List<object> __state)
        {
            //proceeds if pawn was wearing archotech sacrificer belt (see prefix)
            if (!__state.NullOrEmpty())
            {
                Pawn pawnToResurrect = (Pawn)__state[0];
                Map pawnMap = (Map)__state[1];
                //creates a list of pawns that can be randomly chosen to kill
                List<Pawn> possiblePawns = new List<Pawn>();
                List<Pawn> allPawns = pawnMap.mapPawns.SpawnedPawnsInFaction(pawnToResurrect.Faction);
                foreach (Pawn currentPawn in allPawns)
                {
                    //adds pawn to list only if pawn is not wearing an archotech sacrificer belt
                    if (PawnIsWearingSacrificerBelt(currentPawn) == -1)
                    {
                        if (currentPawn.IsPrisoner && ArtifactsExpandedMod.modSettings.sacrificerBeltCountPrisoners)
                        {
                            possiblePawns.Add(currentPawn);
                            continue;
                        }
                        if (currentPawn.RaceProps.Animal && ArtifactsExpandedMod.modSettings.sacrificerBeltCountAnimals)
                        {
                            possiblePawns.Add(currentPawn);
                            continue;
                        }
                        possiblePawns.Add(currentPawn);
                    }
                }
                //proceeds if there is at least one random pawn to be chosen
                if (possiblePawns.NullOrEmpty() || possiblePawns.Count == 0)
                {
                    return;
                }
                //chooses a random pawn to kill
                int randomIndex = Rand.Range(0, possiblePawns.Count);
                Pawn pawnToSacrifice = possiblePawns.ElementAt(randomIndex);
                //resurrects the dead pawn
                ResurrectionUtility.Resurrect(pawnToResurrect);
                //locks the archotech sacrificer belt if pawn is still wearing it
                int sacrificerBeltIndex = PawnIsWearingSacrificerBelt(pawnToResurrect);
                if (sacrificerBeltIndex != -1)
                {
                    pawnToResurrect.apparel.Lock(pawnToResurrect.apparel.WornApparel[sacrificerBeltIndex]);
                }
                //gives thought to resurrected colonists if enabled in settings
                if (ArtifactsExpandedMod.modSettings.sacrificerBeltGivesThought && pawnToResurrect.IsColonist)
                {
                    Pawn_NeedsTracker pawnNeeds = pawnToResurrect.needs;
                    if (pawnNeeds != null)
                    {
                        Need_Mood pawnMood = pawnNeeds.mood;
                        if (pawnMood != null)
                        {
                            MemoryThoughtHandler pawnMemories = pawnMood.thoughts.memories;
                            if (pawnToSacrifice.IsColonist)
                            {
                                pawnMemories.TryGainMemory(ThoughtDefOf.KnowColonistSacrificed, pawnToSacrifice);
                            }
                            else if (pawnToSacrifice.IsPrisoner)
                            {
                                pawnMemories.TryGainMemory(ThoughtDefOf.KnowPrisonerSacrificed, pawnToSacrifice);
                            }
                            else
                            {
                                pawnMemories.TryGainMemory(ThoughtDefOf.KnowAnimalSacrificed, pawnToSacrifice);
                            }
                        }
                    }
                }
                //sends a notification if necessary
                if (PawnUtility.ShouldSendNotificationAbout(pawnToResurrect) || PawnUtility.ShouldSendNotificationAbout(pawnToSacrifice))
                {
                    Messages.Message("PawnResurrectedBySacrifice".Translate(pawnToResurrect.LabelShort, pawnToResurrect, pawnToSacrifice.LabelShort, pawnToSacrifice), pawnToResurrect, MessageTypeDefOf.NegativeEvent, true);
                }
                //kills the randomly chosen pawn
                pawnToSacrifice.Kill(null, null);
                return;
            }
        }

        //method to check if pawn is wearing a sacrificer belt
        public static int PawnIsWearingSacrificerBelt(Pawn pawn)
        {
            if (pawn.apparel == null)
            {
                return -1;
            }
            for (int i = 0; i < pawn.apparel.WornApparelCount; i++)
            {
                if (pawn.apparel.WornApparel[i].def == ThingDefOf.Apparel_ArchotechSacrificerBelt)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}