using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace MoreArchotechGarbage
{
    [HotSwappable]
    public class ArchotechGeneRipper : Building_GeneExtractor
    {
        public List<GeneDef> selectedGenes = new List<GeneDef>(); 
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref selectedGenes, "selectedGenes", LookMode.Def);
        }
        public override Vector3 PawnDrawOffset
        {
            get
            {
                var drawOffset = IntVec3.West.RotatedBy(base.Rotation).ToVector3() / def.size.x;
                drawOffset += new Vector3(0.35f, 0, 0).RotatedBy(base.Rotation);
                return drawOffset;
            }
        }


        public override void Draw()
        {
            base.Draw();
            if (base.Working && selectedPawn != null && ContainedPawn == selectedPawn)
            {
                ContainedPawn.Drawer.renderer.RenderPawnAt(DrawPos + PawnDrawOffset, null, neverAimWeapon: true);
            }
        }
        public override void SelectPawn(Pawn pawn)
        {
            Find.WindowStack.Add(new Dialog_SelectGenes(pawn, this, delegate
            {
                selectedPawn = pawn;
                if (!pawn.IsPrisonerOfColony && !pawn.Downed && !pawn.Dead)
                {
                    pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.EnterBuilding, this), JobTag.Misc);
                }
            }));
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (MoreArchotechGarbageSettings.archotechGeneRipperAllowFreshDeadBodies)
            {

                Command_Action command_Action4 = new Command_Action();
                command_Action4.defaultLabel = "MAG.InsertCorpse".Translate() + "...";
                command_Action4.defaultDesc = "InsertPersonGeneExtractorDesc".Translate();
                command_Action4.icon = InsertPawnTex;
                command_Action4.action = delegate
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (Corpse item in base.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse))
                    {
                        if (item.GetComp<CompRottable>().Stage == RotStage.Fresh)
                        {
                            Pawn pawn = item.InnerPawn;
                            if (pawn.genes != null)
                            {
                                AcceptanceReport acceptanceReport = CanAcceptPawn(pawn);
                                string text = pawn.LabelShortCap + ", " + pawn.genes.XenotypeLabelCap;
                                if (!acceptanceReport.Accepted)
                                {
                                    if (!acceptanceReport.Reason.NullOrEmpty())
                                    {
                                        list.Add(new FloatMenuOption(text + ": " + acceptanceReport.Reason, null, pawn, Color.white));
                                    }
                                }
                                else
                                {
                                    Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.XenogermReplicating);
                                    if (firstHediffOfDef != null)
                                    {
                                        text = text + " (" + firstHediffOfDef.LabelBase + ", " + firstHediffOfDef.TryGetComp<HediffComp_Disappears>().ticksToDisappear.ToStringTicksToPeriod(allowSeconds: true, shortForm: true).Colorize(ColoredText.SubtleGrayColor) + ")";
                                    }
                                    list.Add(new FloatMenuOption(text, delegate
                                    {
                                        SelectPawn(pawn);
                                    }, pawn, Color.white));
                                }
                            }
                        }
                    }
                    if (!list.Any())
                    {
                        list.Add(new FloatMenuOption("NoExtractablePawns".Translate(), null));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                };
                if (!PowerOn)
                {
                    command_Action4.Disable("NoPower".Translate().CapitalizeFirst());
                }
                yield return command_Action4;
            }
        }

        [HarmonyPatch(typeof(Building_GeneExtractor), "TickEffects")]
        public static class Building_GeneExtractor_TickEffects_Patch
        {
            public static bool Prefix(Building_GeneExtractor __instance)
            {
                if (__instance is ArchotechGeneRipper ArchotechGeneRipper)
                {
                    ArchotechGeneRipper.TickEffectsOverride();
                    return false;
                }
                return true;
            }
        }
    
        public void TickEffectsOverride()
        {
            if (sustainerWorking == null || sustainerWorking.Ended)
            {
                sustainerWorking = SoundDefOf.GeneExtractor_Working.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
            }
            else
            {
                sustainerWorking.Maintain();
            }

            progressBar ??= EffecterDefOf.ProgressBarAlwaysVisible.Spawn();

            progressBar.EffectTick(new TargetInfo(Position + IntVec3.North.RotatedBy(Rotation), Map), TargetInfo.Invalid);
            var mote = ((SubEffecter_ProgressBar)progressBar.children[0]).mote;
            if (mote != null)
            {
                mote.progress = 1f - Mathf.Clamp01((float)ticksRemaining / (float)MoreArchotechGarbageSettings.archotechGeneRipperDuration);
                mote.offsetZ = -0.8f;
            }
        }
    
        public override void TryAcceptPawn(Pawn pawn)
        {
            if ((bool)CanAcceptPawn(pawn))
            {
                selectedPawn = pawn;
                Thing corpseOrPawn = pawn.Dead ? pawn.Corpse : pawn;
                bool num = corpseOrPawn.DeSpawnOrDeselect();
                if (innerContainer.TryAddOrTransfer(corpseOrPawn))
                {
                    startTick = Find.TickManager.TicksGame;
                    ticksRemaining = MoreArchotechGarbageSettings.archotechGeneRipperDuration;
                }
                if (num)
                {
                    Find.Selector.Select(corpseOrPawn, playSound: false, forceDesignatorDeselect: false);
                }
            }
        }

        [HarmonyPatch(typeof(Building_GeneExtractor), "HeldPawnBodyAngle", MethodType.Getter)]
        public static class Building_GeneExtractor_HeldPawnBodyAngle_Patch
        {
            public static bool Prefix(Building_GeneExtractor __instance, ref float __result)
            {
                if (__instance is ArchotechGeneRipper archotechGeneRipper)
                {
                    __result = archotechGeneRipper.Rotation.AsAngle;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Building_GeneExtractor), "HeldPawnPosture", MethodType.Getter)]
        public static class Building_GeneExtractor_HeldPawnPosture_Patch
        {
            public static bool Prefix(Building_GeneExtractor __instance, ref PawnPosture __result)
            {
                if (__instance is ArchotechGeneRipper archotechGeneRipper)
                {
                    __result = PawnPosture.LayingOnGroundNormal;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Building_GeneExtractor), "ContainedPawn", MethodType.Getter)]
        public static class Building_GeneExtractor_ContainedPawn_Patch
        {
            public static bool Prefix(Building_GeneExtractor __instance, ref Pawn __result)
            {
                if (__instance is ArchotechGeneRipper archotechGeneRipper)
                {
                    var thing = archotechGeneRipper.innerContainer.FirstOrDefault();
                    if (thing is Pawn pawn)
                    {
                        __result = pawn;
                    }
                    else if (thing is Corpse corpse)
                    {
                        __result = corpse.InnerPawn;
                    }
                    return false;
                }
                return true;
            }
        }

        public override AcceptanceReport CanAcceptPawn(Pawn pawn)
        {
            if (!pawn.IsColonist && !pawn.IsSlaveOfColony && !pawn.IsPrisonerOfColony)
            {
                return false;
            }
            if (selectedPawn != null && selectedPawn != pawn)
            {
                return false;
            }
            if (!pawn.RaceProps.Humanlike || pawn.IsQuestLodger())
            {
                return false;
            }
            if (!PowerOn)
            {
                return "NoPower".Translate().CapitalizeFirst();
            }
            if (innerContainer.Count > 0)
            {
                return "Occupied".Translate();
            }
            if (pawn.health.hediffSet.HasHediff(HediffDefOf.XenogerminationComa))
            {
                return "InXenogerminationComa".Translate();
            }
            return true;
        }

        [HarmonyPatch(typeof(Building_GeneExtractor), "Finish")]
        public static class Building_GeneExtractor_Finish_Patch
        {
            public static bool Prefix(Building_GeneExtractor __instance)
            {
                if (__instance is ArchotechGeneRipper ArchotechGeneRipper)
                {
                    ArchotechGeneRipper.FinishOverride();
                    return false;
                }
                return true;
            }
        }
    
        public void FinishOverride()
        {
            startTick = -1;
            selectedPawn = null;
            sustainerWorking = null;
            powerCutTicks = 0;
            if (ContainedPawn == null)
            {
                return;
            }
            Pawn containedPawn = ContainedPawn;
            Genepack genepack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);
            selectedGenes = selectedGenes.InRandomOrder().Take(MoreArchotechGarbageSettings.archotechGeneRipperRandomAmountOfExtractedGenesRange.RandomInRange).ToList();
            genepack.Initialize(selectedGenes);
            GeneUtility.ExtractXenogerm(containedPawn, Mathf.RoundToInt(60000f * GeneTuning.GeneExtractorRegrowingDurationDaysRange.RandomInRange));
            IntVec3 intVec = (def.hasInteractionCell ? InteractionCell : base.Position);
            innerContainer.TryDropAll(intVec, base.Map, ThingPlaceMode.Near);

            if (!containedPawn.Dead)
            {
                var dInfo = new DamageInfo(DamageDefOf.ExecutionCut, 9999f, 999f,
                    hitPart: containedPawn.health.hediffSet.GetBrain());
                dInfo.SetAllowDamagePropagation(false);
                containedPawn.forceNoDeathNotification = true;
                containedPawn.Kill(dInfo);
                containedPawn.forceNoDeathNotification = false;
                if (containedPawn.Dead)
                {
                    ThoughtUtility.GiveThoughtsForPawnExecuted(containedPawn, null, PawnExecutionKind.GenericBrutal);
                }
            }

            if (selectedGenes.Any())
            {
                GenPlace.TryPlaceThing(genepack, intVec, base.Map, ThingPlaceMode.Near);
            }
            Messages.Message("GeneExtractionComplete".Translate(containedPawn.Named("PAWN")) + ": " 
                + selectedGenes.Select((GeneDef x) => x.label).ToCommaList().CapitalizeFirst(), 
                new LookTargets(containedPawn, genepack), MessageTypeDefOf.PositiveEvent);
        }
    }
}