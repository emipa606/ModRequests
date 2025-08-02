using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SacrificeYourself
{
    [StaticConstructorOnStartup]
    public static class ModStartup
    {
        static ModStartup()
        {
            var harmony = new Harmony("com.sacrificeyourself.rimworld");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Bill_Production), "Notify_IterationCompleted")]
    public static class BillProduction_Notify_IterationCompleted_Patch
    {
        public static void Postfix(Bill_Production __instance, Pawn billDoer)
        {
            if (__instance.recipe.defName == "CreateNeurotrainer" && billDoer != null)
            {
                // Interrupt the current job and mark it as finished
                if (billDoer.jobs?.curJob != null)
                {
                    billDoer.jobs.EndCurrentJob(JobCondition.Succeeded);
                }

                // Apply stab damage before finalizing the bill
                ApplyStabDamage(billDoer);

                var highestSkill = billDoer.skills?.skills
                    .OrderByDescending(s => s.Level)
                    .FirstOrDefault();

                if (highestSkill != null)
                {
                    int originalSkillLevel = highestSkill.Level;
                    highestSkill.Level = 0;

                    Log.Message($"Pawn {billDoer.Name}'s {highestSkill.def.defName} skill level reset from {originalSkillLevel} to 0.");

                    string neurotrainerDefName = $"Neurotrainer_{highestSkill.def.defName}";
                    ThingDef neurotrainerDef = DefDatabase<ThingDef>.GetNamedSilentFail(neurotrainerDefName);

                    if (neurotrainerDef != null)
                    {
                        var workbench = __instance.billStack.billGiver as Thing;
                        if (workbench != null)
                        {
                            Thing neurotrainer = ThingMaker.MakeThing(neurotrainerDef);
                            GenPlace.TryPlaceThing(neurotrainer, workbench.Position, workbench.Map, ThingPlaceMode.Near);
                            Log.Message($"Neurotrainer {neurotrainerDefName} created at {workbench.Position}.");
                        }
                        else
                        {
                            Log.Warning("Could not find the workbench for the completed bill.");
                        }
                    }
                    else
                    {
                        Log.Warning($"No Neurotrainer ThingDef found for skill {highestSkill.def.defName}. Make sure a ThingDef exists for {neurotrainerDefName}.");
                    }

                    DisplayMoteMessage(billDoer);

                    // Move the pawn 1 cell south before killing
                    IntVec3 southPosition = billDoer.Position + new IntVec3(0, 0, -1);
                    if (southPosition.InBounds(billDoer.Map) && IsCellEmpty(southPosition, billDoer.Map))
                    {
                        billDoer.Position = southPosition;
                        Log.Message($"Moved pawn {billDoer.Name} to {southPosition} before execution.");
                    }
                    else
                    {
                        Log.Warning($"Could not move pawn {billDoer.Name} south; position {southPosition} is invalid or occupied.");
                    }

                    // Kill the colonist cleanly
                    DamageInfo damageInfo = new DamageInfo(DamageDefOf.ExecutionCut, 9999, instigator: null, hitPart: null);
                    billDoer.Kill(damageInfo);

                    Log.Message($"Pawn {billDoer.Name} has been sacrificed upon completing the CreateNeurotrainer bill.");
                }
                else
                {
                    Log.Warning("Pawn has no skills. No Neurotrainer spawned, but the pawn was not killed.");
                }
            }
        }

        // Helper method to determine if a cell is empty
        private static bool IsCellEmpty(IntVec3 cell, Map map)
        {
            var things = map.thingGrid.ThingsListAt(cell);
            foreach (var thing in things)
            {
                if (thing.def.passability == Traversability.Impassable || thing is Pawn)
                {
                    return false; // Cell is not empty if there's an impassable object or another pawn
                }
            }
            return true; // Cell is empty
        }

        private static void ApplyStabDamage(Pawn pawn)
        {
            // Stab damage logic
            var damageDef = DefDatabase<DamageDef>.GetNamed("Stab", true);
            if (damageDef == null)
            {
                Log.Warning("Stab DamageDef not found.");
                return;
            }

            // Apply damage to a random body part
            BodyPartRecord targetPart = pawn.health.hediffSet.GetRandomNotMissingPart(damageDef, BodyPartHeight.Middle, BodyPartDepth.Outside);
            if (targetPart != null)
            {
                var damageInfo = new DamageInfo(damageDef, 10, instigator: null, hitPart: targetPart);
                pawn.TakeDamage(damageInfo);

                Log.Message($"Applied stab damage to {pawn.Name}'s {targetPart.Label}.");
            }
        }

        private static void DisplayMoteMessage(Pawn pawn)
        {
            // Define a list of potential messages
            List<string> messages = new List<string>
            {
                "Scatter my ashes somewhere beautiful... somewhere I’d be free.",
                "Please, don’t let this be how they remember me... not like this.",
                "Tell my sister I forgive her... she needs to know that.",
                "Don’t cremate me... please, I want to be buried next to my mother.",
                "Tell my kids I love them... and I’m so, so sorry."

            };

            // Choose a random message from the list
            string randomMessage = messages.RandomElement();

            // Display the message as a mote near the colonist
            MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, randomMessage, Color.green);

            Log.Message($"Displayed mote message: {randomMessage}");
        }
    }
}