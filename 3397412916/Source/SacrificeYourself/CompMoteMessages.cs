using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace SacrificeYourself
{
    public class CompMoteMessages : ThingComp
    {
        private int nextMessageTick;
        private int intervalTicks = 140; // Approx. 5 seconds
        private int remainingMessages = 10;
        private IntVec3 workbenchPosition;
        private Map map;
        private bool isMessageLoopStarted = false;
        private bool firstVisitLogged = false;
        private int firstVisitTick;

        private static readonly List<string> Messages = new List<string>
        {
            "They said this wouldn't hurt!",
            "They said this would be painless!",
            "I've cut so deep—I'm losing so much blood!",
            "What have I done? I've lost so much blood!",
            "This wasn’t supposed to happen! They lied to me—they all lied!",
            "How could they convince me to do this?",
            "They’re monsters for convincing me to do this!",
            "I’m falling apart! My flesh is falling off the bone!",
            "They said there wouldn't be so much blood!",
            "They’re all cowards—standing there while I bleed!",
            "This is what they wanted, isn’t it?!"
        };

        public void Initialize(IntVec3 workbenchPosition, Map map)
        {
            this.workbenchPosition = workbenchPosition;
            this.map = map;
        }

        public override void CompTick()
        {
            base.CompTick();

            if (isMessageLoopStarted)
            {
                // Display messages at regular intervals
                if (remainingMessages > 0 && Find.TickManager.TicksGame >= nextMessageTick)
                {
                    if (parent is Pawn pawn && pawn.Spawned)
                    {
                        string randomMessage = Messages.RandomElement();
                        MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, randomMessage, Color.red);
                    }

                    nextMessageTick += intervalTicks;
                    remainingMessages--;
                }

                // Stop loop if the job ends
                if (parent is Pawn pawnJob && (pawnJob.CurJob == null || !pawnJob.CurJob.def.defName.Equals("DoBill")))
                {
                    StopMessageLoop();
                }
            }
            else if (parent is Pawn pawn && pawn.Spawned && pawn.CurJob != null && pawn.CurJob.def.defName.Equals("DoBill"))
            {
                // Check distance to the workbench
                if (pawn.Position == workbenchPosition)
                {
                    if (!firstVisitLogged)
                    {
                        firstVisitLogged = true;
                        firstVisitTick = Find.TickManager.TicksGame;
                    }
                    else if (Find.TickManager.TicksGame >= firstVisitTick + 30) // Short delay after first visit
                    {
                        StartMessageLoop();
                    }
                }
            }
        }

        private void StartMessageLoop()
        {
            nextMessageTick = Find.TickManager.TicksGame + intervalTicks;
            remainingMessages = 10;
            isMessageLoopStarted = true;
        }

        private void StopMessageLoop()
        {
            isMessageLoopStarted = false;
            remainingMessages = 0;
        }
    }

    [HarmonyPatch(typeof(JobDriver_DoBill), "MakeNewToils")]
    public static class JobDriver_DoBill_MakeNewToils_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(JobDriver_DoBill __instance, ref IEnumerable<Toil> __result)
        {
            Building billGiver = __instance.job.GetTarget(TargetIndex.A).Thing as Building;
            if (billGiver == null || billGiver.def.defName != "NeuroTrainerWorkbench")
            {
                return;
            }

            List<Toil> toilsList = new List<Toil>(__result);

            if (toilsList.Count > 0)
            {
                Toil arrivalMoteToil = new Toil
                {
                    initAction = () =>
                    {
                        Pawn pawn = __instance.pawn;
                        if (pawn != null)
                        {
                            var comp = pawn.TryGetComp<CompMoteMessages>() ?? AddMoteMessagesComp(pawn);
                            comp.Initialize(billGiver.Position, billGiver.Map);
                        }
                    },
                    defaultCompleteMode = ToilCompleteMode.Instant
                };

                toilsList.Insert(1, arrivalMoteToil);
            }

            __result = toilsList;
        }

        private static CompMoteMessages AddMoteMessagesComp(Pawn pawn)
        {
            var comp = new CompMoteMessages();
            comp.parent = pawn;
            pawn.AllComps.Add(comp);
            return comp;
        }
    }
}
