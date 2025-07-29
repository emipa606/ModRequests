using RimWorld;
using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using UnityEngine;
using System.Linq;
using Verse.Sound;
using Verse.AI;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace GS_Core
{
    /// <summary>
    ///     **Fuel system
    /// </summary>

    [StaticConstructorOnStartup]
    public static class FuelUtility
    {
        public static void OffsetFuel(Pawn pawn, float offset)
        {
            if (!ModsConfig.BiotechActive)
            {
                return;
            }
            Pawn_GeneTracker genes = pawn.genes;
            Gene_FuelDrainMod gene_FuelDrainMod = genes?.GetFirstGeneOfType<Gene_FuelDrainMod>();
            if (gene_FuelDrainMod != null)
            {
                OffsetResource(gene_FuelDrainMod, offset);
                return;
            }
            Gene_Fuel gene_Fuel = genes?.GetFirstGeneOfType<Gene_Fuel>();
            if (gene_Fuel != null)
            {
                gene_Fuel.Value += offset;
            }
        }

        public static void OffsetFuelWithModifier(Pawn pawn, float offset)
        {
            offset *= pawn.GetStatValue(GS_StatDefOf.GS_FuelGainFactor);
            OffsetFuel(pawn, offset);
        }

        public static float FuelCost(Ability ab)
        {
            if (ab.comps != null)
            {
                foreach (AbilityComp comp in ab.comps)
                {
                    if (comp is CompAbilityEffect_FuelCost compAbilityEffect_FuelCost)
                    {
                        return compAbilityEffect_FuelCost.Props.FuelCost;
                    }
                }
            }
            return 0f;
        }

        public static void TickResourceDrain(IGeneResourceDrain drain)
        {
            if (drain.CanOffset && drain.Resource != null)
            {
                OffsetResource(drain, (0f - drain.ResourceLossPerDay) / 60000f);
            }
        }

        public static void PostResourceOffset(IGeneResourceDrain drain, float oldValue)
        {
            if (oldValue > 0f && drain.Resource.Value <= 0f)
            {
                Pawn pawn = drain.Pawn;
                if (!pawn.health.hediffSet.HasHediff(GS_HediffDefOf.GS_Powered))
                {
                    pawn.health.AddHediff(GS_HediffDefOf.GS_Powered);
                }
            }
        }

        public static void OffsetResource(IGeneResourceDrain drain, float amnt)
        {
            float value = drain.Resource.Value;
            drain.Resource.Value += amnt;
            PostResourceOffset(drain, value);
        }

        public static IEnumerable<Gizmo> GetResourceDrainGizmos(IGeneResourceDrain drain)
        {
            if (DebugSettings.ShowDevGizmos && drain.Resource != null)
            {
                Gene_Resource resource = drain.Resource;
                yield return new Command_Action
                {
                    defaultLabel = "DEV: " + resource.ResourceLabel + " -10",
                    action = delegate
                    {
                        OffsetResource(drain, -0.1f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: " + resource.ResourceLabel + " +10",
                    action = delegate
                    {
                        OffsetResource(drain, 0.1f);
                    }
                };
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class GS_Genes
    {
        static GS_Genes()
        {
            Log.Message("Greyscythe Industries - Genetics Module");
        }
    }

    [DefOf]
    public static class GS_GeneDefOf
    {
        public static GeneDef GS_Powered;

        static GS_GeneDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(GS_GeneDefOf));
        }
    }

    [DefOf]
    public static class GS_HediffDefOf
    {
        public static HediffDef GS_Powered;

        public static HediffDef GS_ChargeBoostHediff;

        public static HediffDef GS_OverdriveHediff;

        public static HediffDef GS_GuardianHediff;

        static GS_HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(GS_HediffDefOf));
        }
    }

    [DefOf]
    public static class GS_StatDefOf
    {
        public static StatDef GS_FuelGainFactor;

        static GS_StatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(GS_StatDefOf));
        }
    }

    [DefOf]
    public static class GS_ThingDefOf
    {
        public static ThingDef GS_Fuel;

        public static ThingDef GS_ShieldProjector;

        static GS_ThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(GS_ThingDefOf));
        }
    }

    public class CompAbilityEffect_FuelCost : CompAbilityEffect
    {
        public new CompProperties_AbilityFuelCost Props => (CompProperties_AbilityFuelCost)props;

        private bool HasEnoughFuel
        {
            get
            {
                Gene_Fuel gene_Fuel = parent.pawn.genes?.GetFirstGeneOfType<Gene_Fuel>();
                return gene_Fuel != null && gene_Fuel.Value >= Props.FuelCost;
            }
        }

        public override bool CanCast => HasEnoughFuel && base.CanCast;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            FuelUtility.OffsetFuel(parent.pawn, 0f - Props.FuelCost);
        }

        public override bool GizmoDisabled(out string reason)
        {
            Gene_Fuel gene_Fuel = parent.pawn.genes?.GetFirstGeneOfType<Gene_Fuel>();
            if (gene_Fuel == null)
            {
                reason = "Ability Disabled: No Fuel Gene";
                return true;
            }
            if (gene_Fuel.Value < Props.FuelCost)
            {
                reason = "Ability Disabled: No Fuel";
                return true;
            }
            float num = TotalFuelCostOfQueuedAbilities();
            float num2 = Props.FuelCost + num;
            if (Props.FuelCost > float.Epsilon && num2 > gene_Fuel.Value)
            {
                reason = "Ability Disabled: No Fuel";
                return true;
            }
            reason = null;
            return false;
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return HasEnoughFuel;
        }

        private float TotalFuelCostOfQueuedAbilities()
        {
            object obj = parent.pawn.jobs?.curJob?.verbToUse;
            float num = ((obj is Verb_CastAbility { ability: var ability }) ? ((ability != null) ? FuelUtility.FuelCost(ability) : 0f) : 0f);
            float num2 = num;
            if (parent.pawn.jobs != null)
            {
                for (int i = 0; i < parent.pawn.jobs.jobQueue.Count; i++)
                {
                    if (parent.pawn.jobs.jobQueue[i].job.verbToUse is Verb_CastAbility verb_CastAbility2)
                    {
                        float num3 = num2;
                        Ability ability2 = verb_CastAbility2.ability;
                        num2 = num3 + ((ability2 != null) ? FuelUtility.FuelCost(ability2) : 0f);
                    }
                }
            }
            return num2;
        }
    }

    public class CompProperties_AbilityFuelCost : CompProperties_AbilityEffect
    {
        public float FuelCost;

        public CompProperties_AbilityFuelCost()
        {
            compClass = typeof(CompAbilityEffect_FuelCost);
        }

        public override IEnumerable<string> ExtraStatSummary()
        {
            yield return "Fuel cost: " + Mathf.RoundToInt(FuelCost * 100f);
        }
    }

    public class Gene_Fuel : Gene_Resource, IGeneResourceDrain
    {
        public bool FuelAllowed = true;

        public float FuelLevel = 0f;

        private string cachedFuelLabel = null;

        public Gene_Resource Resource => this;

        public override string ResourceLabel
        {
            get
            {
                if (cachedFuelLabel == null)
                {
                    cachedFuelLabel = def.resourceLabel + " (Level: " + FuelLevel + ")";
                }
                return cachedFuelLabel;
            }
        }

        public Pawn Pawn => pawn;

        public bool CanOffset => !pawn.Deathresting;

        public string DisplayLabel => Label + " (Gene)";

        public float ResourceLossPerDay => def.resourceLossPerDay;

        public override float InitialResourceMax => 1f;

        public override float MinLevelForAlert => 0.15f;

        public override float MaxLevelOffset => 0.1f;

        protected override Color BarColor => new ColorInt(2000, 200, 10).ToColor;

        protected override Color BarHighlightColor => new ColorInt(255, 255, 122).ToColor;

        public override void PostAdd()
        {
            base.PostAdd();
            Reset();
        }

        public override void Tick()
        {
            base.Tick();
            FuelUtility.TickResourceDrain(this);
        }

        public override void SetTargetValuePct(float val)
        {
            targetValue = Mathf.Clamp(val * Max, 0f, Max - MaxLevelOffset);
        }

        public bool ShouldConsumeFuelNow()
        {
            return Value < targetValue;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            foreach (Gizmo resourceDrainGizmo in FuelUtility.GetResourceDrainGizmos(this))
            {
                yield return resourceDrainGizmo;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref FuelAllowed, "FuelAllowed", defaultValue: true);
            Scribe_Values.Look(ref FuelLevel, "FuelLevel", 0f);
        }

        public void OffsetFuelLevel(int offset)
        {
            FuelLevel += offset;
            SetMax(1f + FuelLevel / 10f);
            cachedFuelLabel = null;
        }
    }

    public class Gene_FuelDrainMod : Gene, IGeneResourceDrain
    {
        [Unsaved(false)]
        private Gene_Fuel cachedFuelGene;

        private const float MinAgeForDrain = 3f;

        public Gene_Resource Resource
        {
            get
            {
                if (cachedFuelGene == null)
                {
                    cachedFuelGene = pawn.genes.GetFirstGeneOfType<Gene_Fuel>();
                }
                return cachedFuelGene;
            }
        }

        public bool CanOffset => !pawn.Deathresting;

        public float ResourceLossPerDay => def.resourceLossPerDay;

        public Pawn Pawn => pawn;

        public string DisplayLabel => Label + " (" + "Gene".Translate() + ")";

        public override void Tick()
        {
            base.Tick();
            FuelUtility.TickResourceDrain(this);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo resourceDrainGizmo in FuelUtility.GetResourceDrainGizmos(this))
            {
                yield return resourceDrainGizmo;
            }
        }
    }

    [StaticConstructorOnStartup]
    public class GeneGizmo_ResourceFuel : GeneGizmo_Resource
    {
        private static readonly Texture2D FuelCostTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.9f, 0.9f));

        private const float TotalPulsateTime = 0.85f;

        private List<Pair<IGeneResourceDrain, float>> tmpDrainGenes = new List<Pair<IGeneResourceDrain, float>>();

        public GeneGizmo_ResourceFuel(Gene_Resource gene, List<IGeneResourceDrain> drainGenes, Color barColor, Color barhighlightColor)
            : base(gene, drainGenes, barColor, barhighlightColor)
        {
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
            float num = Mathf.Repeat(Time.time, 0.85f);
            float num2 = 1f;
            if (num < 0.1f)
            {
                num2 = num / 0.1f;
            }
            else if (num >= 0.25f)
            {
                num2 = 1f - (num - 0.25f) / 0.6f;
            }
            if (((MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow)?.LastMouseoverGizmo is Command_Ability command_Ability && gene.Max != 0f)
            {
                foreach (CompAbilityEffect effectComp in command_Ability.Ability.EffectComps)
                {
                    if (effectComp is CompAbilityEffect_FuelCost compAbilityEffect_FuelCost && compAbilityEffect_FuelCost.Props.FuelCost > float.Epsilon)
                    {
                        Rect rect = barRect.ContractedBy(3f);
                        float width = rect.width;
                        float num3 = gene.Value / gene.Max;
                        rect.xMax = rect.xMin + width * num3;
                        float num4 = Mathf.Min(compAbilityEffect_FuelCost.Props.FuelCost / gene.Max, 1f);
                        rect.xMin = Mathf.Max(rect.xMin, rect.xMax - width * num4);
                        GUI.color = new Color(1f, 1f, 1f, num2 * 0.7f);
                        GenUI.DrawTextureWithMaterial(rect, FuelCostTex, null);
                        GUI.color = Color.white;
                        break;
                    }
                }
            }
            return result;
        }

        protected override void DrawHeader(Rect labelRect, ref bool mouseOverAnyHighlightableElement)
        {
            if ((gene.pawn.IsColonistPlayerControlled || gene.pawn.IsPrisonerOfColony) && gene is Gene_Fuel gene_Fuel)
            {
                labelRect.xMax -= 24f;
                Rect rect = new Rect(labelRect.xMax, labelRect.y, 24f, 24f);
                Widgets.DefIcon(rect, GS_ThingDefOf.GS_Fuel);
                GUI.DrawTexture(new Rect(rect.center.x, rect.y, rect.width / 2f, rect.height / 2f), gene_Fuel.FuelAllowed ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
                if (Widgets.ButtonInvisible(rect))
                {
                    gene_Fuel.FuelAllowed = !gene_Fuel.FuelAllowed;
                    if (gene_Fuel.FuelAllowed)
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    }
                    else
                    {
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    }
                }
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                    string onOff = (gene_Fuel.FuelAllowed ? "On" : "Off");
                    TooltipHandler.TipRegion(rect, () => "Pawn will consume Fuel material: " + onOff, 828267373);
                    mouseOverAnyHighlightableElement = true;
                }
            }
            base.DrawHeader(labelRect, ref mouseOverAnyHighlightableElement);
        }

        protected override string GetTooltip()
        {
            tmpDrainGenes.Clear();
            string text = $"{gene.ResourceLabel.CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor)}: {gene.ValueForDisplay} / {gene.MaxForDisplay}\n";
            if (gene.pawn.IsColonistPlayerControlled || gene.pawn.IsPrisonerOfColony)
            {
                text = ((!(gene.targetValue <= 0f)) ? (text + "Consume Fuel Below: " + gene.PostProcessValue(gene.targetValue)) : (text + "Never Consume Fuel".ToString()));
            }
            if (!drainGenes.NullOrEmpty())
            {
                float num = 0f;
                foreach (IGeneResourceDrain drainGene in drainGenes)
                {
                    if (drainGene.CanOffset)
                    {
                        tmpDrainGenes.Add(new Pair<IGeneResourceDrain, float>(drainGene, drainGene.ResourceLossPerDay));
                        num += drainGene.ResourceLossPerDay;
                    }
                }
                if (num != 0f)
                {
                    string text2 = ((num < 0f) ? "RegenerationRate".Translate() : "DrainRate".Translate());
                    text = text + "\n\n" + text2 + ": " + "PerDay".Translate(Mathf.Abs(gene.PostProcessValue(num))).Resolve();
                    foreach (Pair<IGeneResourceDrain, float> tmpDrainGene in tmpDrainGenes)
                    {
                        text = text + "\n  - " + tmpDrainGene.First.DisplayLabel.CapitalizeFirst() + ": " + "PerDay".Translate(gene.PostProcessValue(0f - tmpDrainGene.Second).ToStringWithSign()).Resolve();
                    }
                }
            }
            if (!gene.def.resourceDescription.NullOrEmpty())
            {
                text = text + "\n\n" + gene.def.resourceDescription.Formatted(gene.pawn.Named("PAWN")).Resolve();
            }
            return text;
        }
    }

    public class HediffComp_SeverityFromFuel : HediffComp
    {
        private Gene_Fuel cachedFuelGene;

        public HediffCompProperties_SeverityFromFuel Props => (HediffCompProperties_SeverityFromFuel)props;

        public override bool CompShouldRemove => base.Pawn.genes?.GetFirstGeneOfType<Gene_Fuel>() == null;

        private Gene_Fuel Fuel
        {
            get
            {
                if (cachedFuelGene == null)
                {
                    cachedFuelGene = base.Pawn.genes.GetFirstGeneOfType<Gene_Fuel>();
                }
                return cachedFuelGene;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            severityAdjustment += ((Fuel.Value > 0f) ? Props.severityPerHourFuel : Props.severityPerHourEmpty) / 2500f;
        }
    }

    public class HediffCompProperties_SeverityFromFuel : HediffCompProperties
    {
        public float severityPerHourEmpty;

        public float severityPerHourFuel;

        public HediffCompProperties_SeverityFromFuel()
        {
            compClass = typeof(HediffComp_SeverityFromFuel);
        }
    }

    public class IngestionOutcomeDoer_OffsetFuel : IngestionOutcomeDoer
    {
        public float offset;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int amt)
        {
            FuelUtility.OffsetFuelWithModifier(pawn, offset * (float)amt);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            yield return new StatDrawEntry(valueString: ((offset >= 0f) ? "+" : string.Empty) + Mathf.RoundToInt(offset * 100f), category: StatCategoryDefOf.BasicsNonPawnImportant, label: "Fuel", reportText: "Amount of Fuel this restores.", displayPriorityWithinCategory: 1000);
        }
    }

    public class JobGiver_GetFuel : ThinkNode_JobGiver
    {
        private static float? cachedFuelPackFuelGain;

        public static float FuelPackFuelGain
        {
            get
            {
                if (!cachedFuelPackFuelGain.HasValue)
                {
                    if (!ModsConfig.BiotechActive)
                    {
                        cachedFuelPackFuelGain = 0f;
                    }
                    else
                    {
                        object obj = GS_ThingDefOf.GS_Fuel.ingestible?.outcomeDoers?.FirstOrDefault((IngestionOutcomeDoer x) => x is IngestionOutcomeDoer_OffsetFuel);
                        if (!(obj is IngestionOutcomeDoer_OffsetFuel ingestionOutcomeDoer_OffsetFuel))
                        {
                            cachedFuelPackFuelGain = 0f;
                        }
                        else
                        {
                            cachedFuelPackFuelGain = ingestionOutcomeDoer_OffsetFuel.offset;
                        }
                    }
                }
                return cachedFuelPackFuelGain.Value;
            }
        }

        public static void ResetStaticData()
        {
            cachedFuelPackFuelGain = null;
        }

        public override float GetPriority(Pawn pawn)
        {
            if (!ModsConfig.BiotechActive)
            {
                return 0f;
            }
            if (pawn.genes?.GetFirstGeneOfType<Gene_Fuel>() == null)
            {
                return 0f;
            }
            return 9.1f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!ModsConfig.BiotechActive)
            {
                return null;
            }
            Gene_Fuel gene_Fuel = pawn.genes?.GetFirstGeneOfType<Gene_Fuel>();
            if (gene_Fuel == null)
            {
                return null;
            }
            if (!gene_Fuel.ShouldConsumeFuelNow())
            {
                return null;
            }
            if (gene_Fuel.FuelAllowed)
            {
                Thing FuelPack = GetFuelPack(pawn);
                if (FuelPack != null)
                {
                    Job job = JobMaker.MakeJob(JobDefOf.Ingest, FuelPack);
                    job.count = Mathf.Min(FuelPack.stackCount, Mathf.FloorToInt((gene_Fuel.Max - gene_Fuel.Value) / FuelPackFuelGain));
                    return job;
                }
            }
            return null;
        }

        private Thing GetFuelPack(Pawn pawn)
        {
            Thing carriedThing = pawn.carryTracker.CarriedThing;
            if (carriedThing != null && carriedThing.def == GS_ThingDefOf.GS_Fuel)
            {
                return carriedThing;
            }
            for (int i = 0; i < pawn.inventory.innerContainer.Count; i++)
            {
                if (pawn.inventory.innerContainer[i].def == GS_ThingDefOf.GS_Fuel)
                {
                    return pawn.inventory.innerContainer[i];
                }
            }
            return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerThings.ThingsOfDef(GS_ThingDefOf.GS_Fuel), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing t) => pawn.CanReserve(t) && !t.IsForbidden(pawn));
        }
    }

    public class Recipe_FuelInjection : Recipe_Surgery
    {
        public override bool CompletableEver(Pawn surgeryTarget)
        {
            return base.CompletableEver(surgeryTarget);
        }

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return thing.MapHeld != null && thing.MapHeld.listerThings.ThingsOfDef(GS_ThingDefOf.GS_Fuel).Count != 0 && base.AvailableOnNow(thing, part);
        }

        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (!ModsConfig.BiotechActive)
            {
                return;
            }
            float num = 0f;
            for (int i = 0; i < ingredients.Count; i++)
            {
                if (!ingredients[i].def.IsMedicine)
                {
                    num += JobGiver_GetFuel.FuelPackFuelGain * (float)ingredients[i].stackCount;
                }
            }
            if (num > 0f && pawn.genes?.GetFirstGeneOfType<Gene_Fuel>() != null)
            {
                FuelUtility.OffsetFuel(pawn, num);
            }
            for (int j = 0; j < ingredients.Count; j++)
            {
                ingredients[j].Destroy();
            }
        }

        public override float GetIngredientCount(IngredientCount ing, Bill bill)
        {
            return base.GetIngredientCount(ing, bill);
        }
    }

    public class Workgiver_AdministerFuel : WorkGiver_Scanner
    {
        private const float MinLevelForFeedingFuelUnforced = 0.25f;

        private const float FuelPctMax = 0.95f;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Pawn pawn2) || pawn2 == pawn)
            {
                return false;
            }
            Gene_Fuel gene_Fuel = pawn2.genes?.GetFirstGeneOfType<Gene_Fuel>();
            if (gene_Fuel == null)
            {
                return false;
            }
            if (gene_Fuel.ValuePercent >= 0.95f)
            {
                return false;
            }
            if (!forced && gene_Fuel.Value >= 0.25f)
            {
                return false;
            }
            if (!FeedPatientUtility.ShouldBeFed(pawn2))
            {
                return false;
            }
            if (!gene_Fuel.FuelAllowed)
            {
                return false;
            }
            if (!gene_Fuel.ShouldConsumeFuelNow())
            {
                JobFailReason.Is("Not allowed to be refueled");
                return false;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            if (GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(GS_ThingDefOf.GS_Fuel), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing pack) => !pack.IsForbidden(pawn) && pawn.CanReserve(pack)) == null)
            {
                JobFailReason.Is("No Fuel.");
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = (Pawn)t;
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(GS_ThingDefOf.GS_Fuel), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing pack) => !pack.IsForbidden(pawn) && pawn.CanReserve(pack));
            if (thing != null)
            {
                Job job = JobMaker.MakeJob(JobDefOf.FeedPatient, thing, pawn2);
                job.count = 1;
                return job;
            }
            return null;
        }
    }

    public class WorkGiver_Warden_DeliverFuel : WorkGiver_Warden
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!ModsConfig.BiotechActive)
            {
                return null;
            }
            if (!ShouldTakeCareOfPrisoner(pawn, t))
            {
                return null;
            }
            Pawn prisoner = (Pawn)t;
            if (!prisoner.guest.CanBeBroughtFood || !prisoner.Position.IsInPrisonCell(prisoner.Map))
            {
                return null;
            }
            if (WardenFeedUtility.ShouldBeFed(prisoner))
            {
                return null;
            }
            if (!(prisoner.genes?.GetGene(GS_GeneDefOf.GS_Powered) is Gene_Fuel gene_Fuel))
            {
                return null;
            }
            if (!gene_Fuel.FuelAllowed)
            {
                return null;
            }
            if (!gene_Fuel.ShouldConsumeFuelNow())
            {
                return null;
            }
            if (FuelPackAlreadyAvailableFor(prisoner))
            {
                return null;
            }
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(GS_ThingDefOf.GS_Fuel), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing pack) => !pack.IsForbidden(pawn) && pawn.CanReserve(pack) && pack.GetRoom() != prisoner.GetRoom());
            if (thing == null)
            {
                return null;
            }
            Job job = JobMaker.MakeJob(JobDefOf.DeliverFood, thing, prisoner);
            job.count = 1;
            job.targetC = RCellFinder.SpotToChewStandingNear(prisoner, thing);
            return job;
        }

        private bool FuelPackAlreadyAvailableFor(Pawn prisoner)
        {
            if (prisoner.carryTracker.CarriedCount(GS_ThingDefOf.GS_Fuel) > 0)
            {
                return true;
            }
            if (prisoner.inventory.Count(GS_ThingDefOf.GS_Fuel) > 0)
            {
                return true;
            }
            Room room = prisoner.GetRoom();
            if (room != null)
            {
                List<Region> regions = room.Regions;
                for (int i = 0; i < regions.Count; i++)
                {
                    if (regions[i].ListerThings.ThingsOfDef(GS_ThingDefOf.GS_Fuel).Count > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    [StaticConstructorOnStartup]
    public class CommandAbilityToggle : Command_Toggle
    {
        public static readonly Texture2D CooldownTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

        public Ability ability;

        public Pawn pawn;

        public CommandAbilityToggle(Pawn pawn, Ability ability)
        {
            string reason = "";
            this.pawn = pawn;
            this.ability = ability;
            defaultLabel = ability.def.LabelCap;
            defaultDesc = ability.def.description;
            icon = ability.def.uiIcon;
            disabled = ability.GizmoDisabled(out reason);
            disabledReason = reason.Colorize(Color.red);
            Order = 10f;
        }

        protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
        {
            GizmoResult result = base.GizmoOnGUIInt(butRect, parms);
            int min = ability.def.cooldownTicksRange.min;
            if (disabled && min > Find.TickManager.TicksGame)
            {
                GUI.DrawTexture(butRect.RightPartPixels(butRect.width * ((float)ability.CooldownTicksRemaining / (float)min)), CooldownTex);
            }
            return result;
        }
    }

    //
    //  **Charge Boost Ability**
    //
    public class Ability_ChargeBoost : Ability
    {
        public bool active;

        private Gene_Fuel cachedGene;

        public Gene_Fuel Gene
        {
            get
            {
                if (cachedGene == null)
                {
                    cachedGene = pawn.genes.GetFirstGeneOfType<Gene_Fuel>();
                }
                return cachedGene;
            }
        }

        public Ability_ChargeBoost(Pawn pawn)
            : base(pawn)
        {
        }

        public Ability_ChargeBoost(Pawn pawn, AbilityDef def)
            : base(pawn, def)
        {
            base.def = def;
            base.pawn = pawn;
        }

        public override IEnumerable<Command> GetGizmos()
        {
            if (active)
            {
                yield return new CommandAbilityToggle(pawn, this)
                {
                    defaultLabel = "ChargeBoost_Off".Translate(),
                    toggleAction = delegate
                    {
                        active = false;
                        Activate(pawn, LocalTargetInfo.Invalid);
                    },
                    isActive = () => active,
                    icon = ContentFinder<Texture2D>.Get("UI/Abilities/GS_Powered_ChargeBoost")
                };
            }
            else
            {
                yield return new CommandAbilityToggle(pawn, this)
                {
                    defaultLabel = "ChargeBoost_On".Translate(),
                    toggleAction = delegate
                    {
                        active = true;
                        Activate(pawn, LocalTargetInfo.Invalid);
                    },
                    isActive = () => active,
                    icon = ContentFinder<Texture2D>.Get("UI/Abilities/GS_Powered_ChargeBoost")
                };
            }
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            bool result = base.Activate(target, dest);
            ApplyHediff(target, dest);
            if (active)
            {
                StartCooldown(300);
            }
            return result;
        }

        public override void AbilityTick()
        {
            base.AbilityTick();
            if (active)
            {
                if (pawn.Downed)
                {
                    active = false;
                }
                else if (pawn.IsHashIntervalTick(300))
                {
                    Gene.Value += -0.02f;
                }
            }
        }

        public void ApplyHediff(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (!(target.Thing is Pawn pawn))
            {
                return;
            }
            if (active)
            {
                Hediff_ChargeBoost hediff_ChargeBoost = HediffMaker.MakeHediff(GS_HediffDefOf.GS_ChargeBoostHediff, pawn) as Hediff_ChargeBoost;
                hediff_ChargeBoost.ability = this;
                hediff_ChargeBoost.gene = Gene;
                pawn.health.AddHediff(hediff_ChargeBoost);
            }
            else
            {
                Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(GS_HediffDefOf.GS_ChargeBoostHediff);
                if (firstHediffOfDef != null)
                {
                    pawn.health.RemoveHediff(firstHediffOfDef);
                }
            }
        }
    }

    public class Hediff_ChargeBoost : HediffWithComps
    {
        public Ability_ChargeBoost ability;

        private Gene_Fuel cacheGene;

        public override bool ShouldRemove => gene.Value <= 0f || !ability.active;

        public override bool Visible => true;

        public Gene_Fuel gene
        {
            get
            {
                if (cacheGene == null)
                {
                    cacheGene = pawn.genes?.GetFirstGeneOfType<Gene_Fuel>();
                }
                return cacheGene;
            }
            set
            {
                cacheGene = value;
            }
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            if (!dinfo.IgnoreArmor && (dinfo.Def.armorCategory.armorRatingStat == StatDefOf.ArmorRating_Blunt || dinfo.Def.armorCategory.armorRatingStat == StatDefOf.ArmorRating_Sharp))
            {
                float num = Mathf.Clamp(totalDamageDealt / 5f / 100f, 0.005f, 0.03f);
                gene.Value -= num;
            }
        }

        public override void Tick()
        {
            base.Tick();
            Severity = gene.Value;
        }
    }

    //
    //  **Overdrive Ability**
    //
    public class Ability_Overdrive : Ability
    {
        public bool active;

        private Gene_Fuel cachedGene;

        public Gene_Fuel Gene
        {
            get
            {
                if (cachedGene == null)
                {
                    cachedGene = pawn.genes.GetFirstGeneOfType<Gene_Fuel>();
                }
                return cachedGene;
            }
        }

        public Ability_Overdrive(Pawn pawn)
            : base(pawn)
        {
        }

        public Ability_Overdrive(Pawn pawn, AbilityDef def)
            : base(pawn, def)
        {
            base.def = def;
            base.pawn = pawn;
        }

        public override IEnumerable<Command> GetGizmos()
        {
            if (active)
            {
                yield return new CommandAbilityToggle(pawn, this)
                {
                    defaultLabel = "Overdrive_Off".Translate(),
                    toggleAction = delegate
                    {
                        active = false;
                        Activate(pawn, LocalTargetInfo.Invalid);
                    },
                    isActive = () => active,
                    icon = ContentFinder<Texture2D>.Get("UI/Abilities/GS_Powered_Overdrive")
                };
            }
            else
            {
                yield return new CommandAbilityToggle(pawn, this)
                {
                    defaultLabel = "Overdrive_On".Translate(),
                    toggleAction = delegate
                    {
                        active = true;
                        Activate(pawn, LocalTargetInfo.Invalid);
                    },
                    isActive = () => active,
                    icon = ContentFinder<Texture2D>.Get("UI/Abilities/GS_Powered_Overdrive")
                };
            }
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            bool result = base.Activate(target, dest);
            ApplyHediff(target, dest);
            if (active)
            {
                StartCooldown(300);
            }
            return result;
        }

        public override void AbilityTick()
        {
            base.AbilityTick();
            if (active)
            {
                if (pawn.Downed)
                {
                    active = false;
                }
                else if (pawn.IsHashIntervalTick(300))
                {
                    Gene.Value += -0.02f;
                }
            }
        }

        public void ApplyHediff(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (!(target.Thing is Pawn pawn))
            {
                return;
            }
            if (active)
            {
                Hediff_Overdrive hediff_Overdrive = HediffMaker.MakeHediff(GS_HediffDefOf.GS_OverdriveHediff, pawn) as Hediff_Overdrive;
                hediff_Overdrive.ability = this;
                hediff_Overdrive.gene = Gene;
                pawn.health.AddHediff(hediff_Overdrive);
            }
            else
            {
                Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(GS_HediffDefOf.GS_OverdriveHediff);
                if (firstHediffOfDef != null)
                {
                    pawn.health.RemoveHediff(firstHediffOfDef);
                }
            }
        }

        public Boolean isActive()
        { 
            return active; 
        }
    }


    public class Hediff_Overdrive : HediffWithComps
    {
        public Ability_Overdrive ability;

        private Gene_Fuel cacheGene;

        public override bool ShouldRemove => gene.Value <= 0f || !ability.active;

        public override bool Visible => true;

        public Gene_Fuel gene
        {
            get
            {
                if (cacheGene == null)
                {
                    cacheGene = pawn.genes?.GetFirstGeneOfType<Gene_Fuel>();
                }
                return cacheGene;
            }
            set
            {
                cacheGene = value;
            }
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            if (!dinfo.IgnoreArmor && (dinfo.Def.armorCategory.armorRatingStat == StatDefOf.ArmorRating_Blunt || dinfo.Def.armorCategory.armorRatingStat == StatDefOf.ArmorRating_Sharp))
            {
                float num = Mathf.Clamp(totalDamageDealt / 5f / 100f, 0.005f, 0.03f);
                gene.Value -= num;
            }
        }

        public override void Tick()
        {
            base.Tick();
            Severity = gene.Value;
        }
    }

    //
    //  **Guardian Ability**
    //
    public class Ability_Guardian : Ability
    {
        public bool active;

        private Gene_Fuel cachedGene;

        public Gene_Fuel Gene
        {
            get
            {
                if (cachedGene == null)
                {
                    cachedGene = pawn.genes.GetFirstGeneOfType<Gene_Fuel>();
                }
                return cachedGene;
            }
        }

        public Ability_Guardian(Pawn pawn)
            : base(pawn)
        {
        }

        public Ability_Guardian(Pawn pawn, AbilityDef def)
            : base(pawn, def)
        {
            base.def = def;
            base.pawn = pawn;
        }

        public override IEnumerable<Command> GetGizmos()
        {
            if (active)
            {
                yield return new CommandAbilityToggle(pawn, this)
                {
                    defaultLabel = "Guardian_Off".Translate(),
                    toggleAction = delegate
                    {
                        active = false;
                        Activate(pawn, LocalTargetInfo.Invalid);
                    },
                    isActive = () => active,
                    icon = ContentFinder<Texture2D>.Get("UI/Abilities/GS_Powered_Guardian")
                };
            }
            else
            {
                yield return new CommandAbilityToggle(pawn, this)
                {
                    defaultLabel = "Guardian_On".Translate(),
                    toggleAction = delegate
                    {
                        active = true;
                        Activate(pawn, LocalTargetInfo.Invalid);
                    },
                    isActive = () => active,
                    icon = ContentFinder<Texture2D>.Get("UI/Abilities/GS_Powered_Guardian")
                };
            }
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            bool result = base.Activate(target, dest);
            ApplyHediff(target, dest);
            if (active)
            {
                StartCooldown(300);
            }
            return result;
        }

        public override void AbilityTick()
        {
            base.AbilityTick();
            if (active)
            {
                if (pawn.Downed)
                {
                    active = false;
                }
                else if (pawn.IsHashIntervalTick(300))
                {
                    Gene.Value += -0.02f;
                }
            }
        }

        public void ApplyHediff(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (!(target.Thing is Pawn pawn))
            {
                return;
            }
            if (active)
            {
                Hediff_Guardian hediff_Guardian = HediffMaker.MakeHediff(GS_HediffDefOf.GS_GuardianHediff, pawn) as Hediff_Guardian;
                hediff_Guardian.ability = this;
                hediff_Guardian.gene = Gene;
                pawn.health.AddHediff(hediff_Guardian);
            }
            else
            {
                Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(GS_HediffDefOf.GS_GuardianHediff);
                if (firstHediffOfDef != null)
                {
                    pawn.health.RemoveHediff(firstHediffOfDef);
                }
            }
        }

        public Boolean isActive()
        {
            return active;
        }
    }


    public class Hediff_Guardian : HediffWithComps
    {
        public Ability_Guardian ability;

        private Gene_Fuel cacheGene;

        public override bool ShouldRemove => gene.Value <= 0f || !ability.active;

        public override bool Visible => true;

        public Gene_Fuel gene
        {
            get
            {
                if (cacheGene == null)
                {
                    cacheGene = pawn.genes?.GetFirstGeneOfType<Gene_Fuel>();
                }
                return cacheGene;
            }
            set
            {
                cacheGene = value;
            }
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            if (!dinfo.IgnoreArmor && (dinfo.Def.armorCategory.armorRatingStat == StatDefOf.ArmorRating_Blunt || dinfo.Def.armorCategory.armorRatingStat == StatDefOf.ArmorRating_Sharp))
            {
                float num = Mathf.Clamp(totalDamageDealt / 5f / 100f, 0.005f, 0.03f);
                gene.Value -= num;
            }
        }

        public override void Tick()
        {
            base.Tick();
            Severity = gene.Value;
        }
    }

    //
    //  **Plasma vent
    //
    public class CompProperties_AbilitySelfBomb : CompProperties_AbilityEffect
    {
        public float radius = 6f;

        public DamageDef damageType;

        public int damageAmount = -1;

        public float damagePenetration = -1f;

        public bool damageFalloff = false;

        public SoundDef soundCreated = null;

        public ThingDef thingCreated = null;

        public float thingCreatedChance = 0f;

        public GasType? gasType = null;

        public float chanceToStartFire = 0f;

        public bool damageSelf = false;

        public CompProperties_AbilitySelfBomb()
        {
            compClass = typeof(CompAbilityEffect_SelfBomb);
        }
    }

    public class CompAbilityEffect_SelfBomb : CompAbilityEffect
    {
        private new CompProperties_AbilitySelfBomb Props => (CompProperties_AbilitySelfBomb)props;

        private Pawn Pawn => parent.pawn;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            List<Thing> list = new List<Thing>();
            if (!Props.damageSelf)
            {
                list.Add(parent.pawn);
            }

            GenExplosion.DoExplosion(Pawn.Position, Pawn.MapHeld, Props.radius, Props.damageType, Pawn, Props.damageAmount, Props.damagePenetration, Props.soundCreated, null, null, null, Props.thingCreated, Props.thingCreatedChance, 1, Props.gasType, applyDamageToExplosionCellsNeighbors: false, null, 0f, 1, Props.chanceToStartFire, Props.damageFalloff, null, list);
            base.Apply(target, dest);
        }

        /**
         
        public override IEnumerable<PreCastAction> GetPreCastActions()
        {
            yield return new PreCastAction
            {
                action = delegate
                {
                    parent.AddEffecterToMaintain(EffecterDefOf.Fire_Burst.Spawn(parent.pawn.Position, parent.pawn.Map), parent.pawn.Position, 17, parent.pawn.Map);
                },
                ticksAwayFromCast = 17
            };
        }

        **/

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (Pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }
            if (target.HasThing && target.Thing is Pawn pawn)
            {
                return pawn.TargetCurrentlyAimingAt == Pawn;
            }
            return false;
        }
    }

    //
    //  **new evade framework goes here**
    //

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        public const float maxEvadeingChance = 0.95f;

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("feaurie.GS_Core");
            Type typeFromHandle = typeof(Pawn);
            harmony.Patch(typeFromHandle.GetMethod("PreApplyDamage"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Patch_Pawn_PreApplyDamage")));
        }

        public static bool Patch_Pawn_PreApplyDamage(ref Pawn __instance, ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            try
            {
                if (dinfo.Instigator == null)
                {
                    return true;
                }

                Pawn pawn = __instance;
                if (pawn.Downed || pawn.InBed() || pawn.IsBurning() || (pawn.stances?.stunner?.Stunned).GetValueOrDefault() || (!pawn.Drafted && !(pawn.stances?.curStance is Stance_Busy) && !(pawn.stances?.curStance is Stance_Warmup)))
                {
                    return true;
                }

                if (dinfo.WeaponBodyPartGroup == null)
                {
                    ThingDef weapon = dinfo.Weapon;
                    if (weapon == null || !weapon.IsMeleeWeapon)
                    {
                        weapon = dinfo.Weapon;
                        if (weapon == null || !weapon.IsRangedWeapon)
                        {
                            return true;
                        }
                        
                        if (GS_EvadeStatDefOf.GS_Evade_EvadeProjectileChance.Worker.IsDisabledFor(pawn))
                        {
                            return true;
                        }

                        float statValue = pawn.GetStatValue(GS_EvadeStatDefOf.GS_Evade_EvadeProjectileChance);
                        if (!(statValue > 0f))
                        {
                            return true;
                        }

                        // val / num is the chance to hit
                        float numStat = Math.Max(statValue, 1f);
                        float num = 10f;
                        if (dinfo.Instigator is Pawn { skills: not null } pawn2)
                        {
                            num = pawn2.skills.GetSkill(SkillDefOf.Shooting)?.Level ?? 0;
                        }
                        float num2 = Mathf.Clamp((float)numStat / num, GS_EvadeModSettings.Instance.MinEvadeMagnitude, GS_EvadeModSettings.Instance.MaxEvadeMagnitude);
                        float num3 = Math.Min(num2 * 10f, GS_EvadeModSettings.Instance.maximumEvadeChance);
                        if (!Rand.Chance(num3))
                        {
                            return true;
                        }

                        if (pawn.Map != null)
                        {
                            SoundDefOf.BulletImpact_Ground?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                            FleckMaker.Static(pawn.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle).RotatedBy(180f) * 0.5f, scale: Mathf.Min(10f, 2f + dinfo.Amount / 10f), map: pawn.Map, fleckDef: FleckDefOf.AirPuff);
                            if (GS_EvadeModSettings.Instance.verboseEvadeReadout)
                            {
                                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "GS_Evade_TextMote_Evade".Translate($"{num3.ToStringPercent()}={statValue}*{num2}={numStat}M/{num}S "), 3.9f);
                            }
                            else
                            {
                                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "GS_Evade_TextMote_Evade".Translate(num3.ToStringPercent()), 1.9f);
                            }
                        }

                        absorbed = true;
                        return false;
                    }
                }

                if (GS_EvadeStatDefOf.GS_Evade_EvadeMeleeChance.Worker.IsDisabledFor(pawn))
                {
                    return true;
                }

                float statValue2 = pawn.GetStatValue(GS_EvadeStatDefOf.GS_Evade_EvadeMeleeChance);
                if (!(statValue2 > 0f))
                {
                    return true;
                }

                float num4 = Math.Max(statValue2, 1f);
                float num5 = 10f;
                if (dinfo.Instigator is Pawn pawn3)
                {
                    if (pawn3.skills == null)
                    {
                        if (GS_EvadeModSettings.Instance.animalSizeScaling)
                        {
                            float num6 = pawn3.BodySize / pawn.BodySize;
                            num5 = Mathf.Clamp(10f * num6, 10f, 25f);
                        }
                    }
                    else
                    {
                        num5 = pawn3.skills.GetSkill(SkillDefOf.Melee)?.Level ?? 0;
                    }
                }
                float num7 = Mathf.Clamp(num4 / num5, GS_EvadeModSettings.Instance.MinEvadeMagnitude, GS_EvadeModSettings.Instance.MaxEvadeMagnitude);
                float num8 = Math.Min(num4 * 10f, GS_EvadeModSettings.Instance.maximumEvadeChance);
                if (!Rand.Chance(num8))
                {
                    return true;
                }

                if (pawn.Map != null)
                {
                    SoundDefOf.Crunch?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                    FleckMaker.Static(pawn.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle).RotatedBy(180f) * 0.5f, scale: Mathf.Min(10f, 2f + dinfo.Amount / 10f), map: pawn.Map, fleckDef: FleckDefOf.ExplosionFlash);
                    if (GS_EvadeModSettings.Instance.verboseEvadeReadout)
                    {
                        MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "GS_Evade_TextMote_Evade".Translate($"{num8.ToStringPercent()}={statValue2}*{num7}={num4}M/{num5}M "), 3.9f);
                    }
                    else
                    {
                        MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "GS_Evade_TextMote_Evade".Translate(num8.ToStringPercent()), 1.9f);
                    }
                }
                absorbed = true;
                return false;
            }
            catch (Exception arg)
            {
                Log.Error($"Error in Supreme Melee: {arg}");
            }
            return false;
        }
    }

    public static class MathUtils
    {
        public static float TwoWayScale(float first, float second)
        {
            if (second > first)
            {
                return first / second;
            }
            return second / first;
        }
    }

    public class GS_Evade : Mod
    {
        private string maxEvadeChanceBuffer;

        public GS_Evade(ModContentPack content)
            : base(content)
        {
            GS_EvadeModSettings.Instance = GetSettings<GS_EvadeModSettings>();
            if (GS_EvadeModSettings.Instance != null)
            {
                maxEvadeChanceBuffer = GS_EvadeModSettings.Instance.maximumEvadeChance.ToString();
            }
        }

        public override string SettingsCategory()
        {
            return "GS_Evade".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            int num = 0;
            float rowHeight = 48f;
            Rect inRect2 = new Rect(inRect);
            Rect rowRect = UIHelper.GetRowRect(inRect2, rowHeight, num);
            num++;
            Widgets.TextFieldNumericLabeled(UIHelper.GetRowRect(inRect2, rowHeight, num), "GS_Evade_MaxEvadeChance".Translate(), ref GS_EvadeModSettings.Instance.maximumEvadeChance, ref maxEvadeChanceBuffer, 0.01f);
        }
    }

    [DefOf]
    public static class GS_EvadeStatDefOf
    {
        public static StatDef GS_Evade_EvadeProjectileChance;

        public static StatDef GS_Evade_EvadeMeleeChance;
    }


    public class GS_EvadeModSettings : ModSettings
    {
        public static GS_EvadeModSettings Instance;

        public bool animalSizeScaling = true;

        public float maximumEvadeChance = 0.9f;

        public float EvadeMagnitude = 2f;

        public bool verboseEvadeReadout = false;

        public float MinEvadeMagnitude => 1f / EvadeMagnitude;

        public float MaxEvadeMagnitude => EvadeMagnitude;

        public GS_EvadeModSettings()
        {
            Instance = this;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref verboseEvadeReadout, "verboseEvadeReadout", defaultValue: false);
            Scribe_Values.Look(ref maximumEvadeChance, "maximumEvadeChance", 0.9f);
            Scribe_Values.Look(ref animalSizeScaling, "animalSizeScaling", defaultValue: true);
        }
    }

    public static class UIHelper
    {
        public static Rect GetRowRect(Rect inRect, float rowHeight, int row)
        {
            float y = inRect.y + rowHeight * (float)row;
            return new Rect(inRect.x, y, inRect.width, rowHeight);
        }
    }

    //
    // **Visual Mote system**
    //
    public class EffectMote : Mote
    {
        public override float Alpha => 0.4f;

        protected override bool EndOfLife => false;
    }

    public class CompProperties_EffectMote : HediffCompProperties
    {
        public float size = 1f;

        public ThingDef visualMote;

        public CompProperties_EffectMote()
        {
            compClass = typeof(CompEffectMote);
        }
    }

    public class CompEffectMote : HediffComp
    {
        private EffectMote mote;

        public CompProperties_EffectMote Props => (CompProperties_EffectMote)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            CreateOrMoveMote(base.Pawn.TrueCenter());
        }

        public void CreateOrMoveMote(Vector3 pawnPos)
        {
            if (mote == null)
            {
                mote = (EffectMote)ThingMaker.MakeThing(Props.visualMote);
                mote.Scale = 1.9f * Props.size;
                mote.exactPosition = pawnPos;
                GenSpawn.Spawn(mote, pawnPos.ToIntVec3(), base.Pawn.Map);
            }
            else
            {
                mote.exactPosition = pawnPos;
            }
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            ClearMote();
        }

        public override void Notify_PawnKilled()
        {
            ClearMote();
        }

        public override void CompPostPostRemoved()
        {
            ClearMote();
        }

        private void ClearMote()
        {
            if (mote != null && mote.Spawned)
            {
                mote.DeSpawn();
            }
            mote = null;
        }
    }

    //
    // **Shield**
    //
    public class CompProperties_AbilityShield : CompProperties_AbilityEffect
    {
        public float radius = 2f;

        public CompProperties_AbilityShield()
        {
            compClass = typeof(CompAbilityEffect_DeployShield);
        }
    }

    public class CompAbilityEffect_DeployShield : CompAbilityEffect
    {
        private new CompProperties_AbilityShield Props => (CompProperties_AbilityShield)props;

        private Pawn Pawn => parent.pawn;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Map map = Pawn.Map;
            int num = GenRadial.NumCellsInRadius(Props.radius);
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = Pawn.Position + GenRadial.RadialPattern[i];
                if (intVec.IsValid && intVec.InBounds(map))
                {
                    SpawnEffect(GenSpawn.Spawn(GS_ThingDefOf.GS_ShieldProjector, intVec, map));
                }
            }
            Messages.Message("AbilityNotEnoughFreeSpace".Translate(), Pawn, MessageTypeDefOf.RejectInput, historical: false);
        }

        /**
         
        public override IEnumerable<PreCastAction> GetPreCastActions()
        {
            yield return new PreCastAction
            {
                action = delegate
                {
                    parent.AddEffecterToMaintain(EffecterDefOf.Fire_Burst.Spawn(parent.pawn.Position, parent.pawn.Map), parent.pawn.Position, 17, parent.pawn.Map);
                },
                ticksAwayFromCast = 17
            };
        }

        **/
        private static void SpawnEffect(Thing projector)
        {
            FleckMaker.Static(projector.TrueCenter(), projector.Map, FleckDefOf.BroadshieldActivation);
            SoundDefOf.Broadshield_Startup.PlayOneShot(new TargetInfo(projector.Position, projector.Map));
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (Pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }
            if (target.HasThing && target.Thing is Pawn pawn)
            {
                return pawn.TargetCurrentlyAimingAt == Pawn;
            }
            return false;
        }
    }

    /**
     * Implants into bionics
     */

    public class GS_Recipe_InstallImplant : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, delegate (BodyPartRecord record)
            {
                IEnumerable<Hediff> source = pawn.health.hediffSet.hediffs.Where((Hediff x) => x.Part == record);
                if (typeof(Hediff_AddedPart).IsAssignableFrom(recipe.addsHediff.hediffClass))
                {
                    if (source.Count() == 1 && source.First().def == recipe.addsHediff)
                    {
                        return false;
                    }
                }
                else if (source.Any((Hediff hd) => hd.def == recipe.addsHediff))
                {
                    return false;
                }
                if (record.parent != null && !pawn.health.hediffSet.GetNotMissingParts().Contains(record.parent))
                {
                    return false;
                }
                return (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record) || pawn.health.hediffSet.HasDirectlyAddedPartFor(record)) ? true : false;
            });
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            }
            pawn.health.AddHediff(recipe.addsHediff, part);
        }
    }
}
