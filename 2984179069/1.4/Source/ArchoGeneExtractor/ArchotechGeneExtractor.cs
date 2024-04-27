using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MoreArchotechGarbage
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
    [HotSwappable]
    public class ArchotechGeneExtractor : Building_GeneExtractor
    {
        public override Vector3 PawnDrawOffset
        {
            get 
            {
                var drawOffset = IntVec3.West.RotatedBy(base.Rotation).ToVector3() / def.size.x;
                drawOffset += new Vector3(0.25f, 0, 0).RotatedBy(base.Rotation);
                return drawOffset;
            } 
        }

        [HarmonyPatch(typeof(Building_GeneExtractor), "TickEffects")]
        public static class Building_GeneExtractor_TickEffects_Patch
        {
            public static bool Prefix(Building_GeneExtractor __instance)
            {
                if (__instance is ArchotechGeneExtractor archotechGeneExtractor)
                {
                    archotechGeneExtractor.TickEffectsOverride();
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
            if (progressBar == null)
            {
                progressBar = EffecterDefOf.ProgressBarAlwaysVisible.Spawn();
            }
            progressBar.EffectTick(new TargetInfo(base.Position + IntVec3.North.RotatedBy(base.Rotation), base.Map), TargetInfo.Invalid);
            MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBar.children[0]).mote;
            if (mote != null)
            {
                mote.progress = 1f - Mathf.Clamp01((float)ticksRemaining / (float)MoreArchotechGarbageSettings.archotechGeneExtractorDuration);
                mote.offsetZ = ((base.Rotation == Rot4.North) ? 0.5f : (-0.5f));
            }
        }

        public override void TryAcceptPawn(Pawn pawn)
        {
            if ((bool)CanAcceptPawn(pawn))
            {
                selectedPawn = pawn;
                bool num = pawn.DeSpawnOrDeselect();
                if (innerContainer.TryAddOrTransfer(pawn))
                {
                    startTick = Find.TickManager.TicksGame;
                    ticksRemaining = MoreArchotechGarbageSettings.archotechGeneExtractorDuration;
                }
                if (num)
                {
                    Find.Selector.Select(pawn, playSound: false, forceDesignatorDeselect: false);
                }
            }
        }

        [HarmonyPatch(typeof(Building_GeneExtractor), "Finish")]
        public static class Building_GeneExtractor_Finish_Patch
        {
            public static bool Prefix(Building_GeneExtractor __instance)
            {
                if (__instance is ArchotechGeneExtractor archotechGeneExtractor)
                {
                    archotechGeneExtractor.FinishOverride();
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
            List<GeneDef> genesToAdd = new List<GeneDef>();
            Genepack genepack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);
            int num = Mathf.Min((int)GeneCountChanceCurve.RandomElementByWeight((CurvePoint p) => p.y).x, containedPawn.genes.GenesListForReading.Count());
            for (int i = 0; i < num; i++)
            {
                if (!containedPawn.genes.GenesListForReading.TryRandomElementByWeight(SelectionWeight, out var result))
                {
                    break;
                }
                genesToAdd.Add(result.def);
            }
            if (genesToAdd.Any())
            {
                genepack.Initialize(genesToAdd);
            }
            GeneUtility.ExtractXenogerm(containedPawn, Mathf.RoundToInt(60000f * GeneTuning.GeneExtractorRegrowingDurationDaysRange.RandomInRange));
            IntVec3 intVec = (def.hasInteractionCell ? InteractionCell : base.Position);
            innerContainer.TryDropAll(intVec, base.Map, ThingPlaceMode.Near);
            if (!containedPawn.Dead && (containedPawn.IsPrisonerOfColony || containedPawn.IsSlaveOfColony))
            {
                containedPawn.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.XenogermHarvested_Prisoner);
            }
            if (genesToAdd.Any())
            {
                GenPlace.TryPlaceThing(genepack, intVec, base.Map, ThingPlaceMode.Near);
            }
            Messages.Message("GeneExtractionComplete".Translate(containedPawn.Named("PAWN")) + ": " + genesToAdd.Select((GeneDef x) => x.label).ToCommaList().CapitalizeFirst(), new LookTargets(containedPawn, genepack), MessageTypeDefOf.PositiveEvent);
            float SelectionWeight(Gene g)
            {
                if (genesToAdd.Contains(g.def))
                {
                    return 0f;
                }
                if (g.def.endogeneCategory == EndogeneCategory.Melanin)
                {
                    return 0f;
                }
                if (!GeneTuning.BiostatRange.Includes(g.def.biostatMet + genesToAdd.Sum((GeneDef x) => x.biostatMet)))
                {
                    return 0f;
                }
                if (g.def.biostatCpx > 0)
                {
                    return 3f;
                }
                return 1f;
            }
        }
    }
}
