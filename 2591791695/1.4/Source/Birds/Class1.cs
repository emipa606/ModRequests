using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Sound;
using static Verse.DamageWorker;

namespace Birds
{
    public class AttackDamageFactor
    {
        public float targetBodySize;
        public float damageMultiplier;
    }
    public class CompProperties_FlyingPawn : CompProperties
    {
        public bool flyWhenFleeing;

        public float flyWhenWanderingChance;

        public bool flyWhenHunting;

        public float flyingMoveSpeedMultiplier;

        public AttackDamageFactor attackDamageFactor;

        public List<GraphicData> flyingBodyGraphicData;

        public List<GraphicData> flyingFemaleBodyGraphicData;

        public float evadeChanceWhenFlying = 1f;

        public bool attackEnemiesMasterAttacking;

        public SoundDef soundOnFly;
        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            if (flyingBodyGraphicData != null)
            {
                foreach (var graphicData in flyingBodyGraphicData)
                {
                    graphicData.graphicClass = typeof(Graphic_Multi);
                }
            }
            if (flyingFemaleBodyGraphicData != null)
            {
                foreach (var graphicData in flyingFemaleBodyGraphicData)
                {
                    graphicData.graphicClass = typeof(Graphic_Multi);
                }
            }
        }
        public CompProperties_FlyingPawn()
        {
            compClass = typeof(CompFlyingPawn);
        }
    }

    public class CompFlyingPawn : ThingComp
    {
        public CompProperties_FlyingPawn Props => base.props as CompProperties_FlyingPawn;

        public bool isFlyingCurrently;
        public Pawn pawn => this.parent as Pawn;
        public void ChangeGraphicToFlying()
        {
            var curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
            var curKindLifeStageInd = pawn.ageTracker.CurLifeStageIndex;
            if (pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null)
            {
                pawn.SetGraphic(this.Props.flyingBodyGraphicData[curKindLifeStageInd]);
            }
            else
            {
                pawn.SetGraphic(this.Props.flyingFemaleBodyGraphicData[curKindLifeStageInd]);
            }
        }
        public void ChangeGraphicToFlyingAndSound() // a joke
        {
            ChangeGraphicToFlying();
            if (this.Props.soundOnFly != null)
            {
                SoundInfo info = SoundInfo.InMap(new TargetInfo(pawn.PositionHeld, pawn.MapHeld));
                this.Props.soundOnFly.PlayOneShot(info);
            }
        }
        public void ChangeGraphicToVanilla()
        {
            PawnKindLifeStage curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
            if (pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null)
            {
                pawn.SetGraphic(curKindLifeStage.bodyGraphicData);
            }
            else
            {
                pawn.SetGraphic(curKindLifeStage.femaleGraphicData);
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isFlyingCurrently, "isFlyingCurrently");
        }
    }

    [StaticConstructorOnStartup]
    public static class Core
    {
        static Core()
        {
            var harmony = new Harmony("Birds.Mod");
            harmony.PatchAll();
        }

        private static Dictionary<Pawn, CompFlyingPawn> cachedComps = new Dictionary<Pawn, CompFlyingPawn>();
        public static bool IsFlyingPawn(this Pawn pawn, out CompFlyingPawn comp)
        {
            if (!cachedComps.TryGetValue(pawn, out comp))
            {
                cachedComps[pawn] = comp = pawn.TryGetComp<CompFlyingPawn>();
            }
            return comp != null;
        }
        public static void SetGraphic(this Pawn pawn, GraphicData graphicData)
        {
            pawn.Drawer.renderer.graphics.nakedGraphic = GraphicDatabase.Get(graphicData.graphicClass, graphicData.texPath, graphicData.shaderType?.Shader ?? ShaderDatabase.CutoutComplex, graphicData.drawSize, graphicData.color, graphicData.colorTwo);
            PortraitsCache.SetDirty(pawn);
            PortraitsCache.PortraitsCacheUpdate();
            GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    public static class ResolveAllGraphicsPrefix_Patch
    {
        public static void Postfix(PawnGraphicSet __instance)
        {
            if (__instance.pawn.IsFlyingPawn(out var comp) && comp.isFlyingCurrently)
            {
                comp.ChangeGraphicToFlying();
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
    public class Pawn_JobTracker_StartJob_Patch
    {
        private static void Postfix(Pawn_JobTracker __instance, Pawn ___pawn, Job newJob, JobTag? tag)
        {
            if (___pawn.IsFlyingPawn(out var comp))
            {
                var curJob = ___pawn.CurJob;
                if (curJob != null)
                {
                    if (comp.Props.flyWhenFleeing && (curJob.def == JobDefOf.Flee || curJob.def == JobDefOf.FleeAndCower)
                        || curJob.def == JobDefOf.GotoWander && Rand.Chance(comp.Props.flyWhenWanderingChance)
                        || curJob.def == JobDefOf.PredatorHunt && comp.Props.flyWhenHunting)
                    {
                        comp.isFlyingCurrently = true;
                        curJob.locomotionUrgency = LocomotionUrgency.Jog;
                        comp.ChangeGraphicToFlyingAndSound();
                    }
                }

            }
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), "CleanupCurrentJob")]
    public class Pawn_JobTracker_CleanupCurrentJob_Patch
    {
        public static void Prefix(Pawn_JobTracker __instance, Pawn ___pawn)
        {
            if (___pawn.IsFlyingPawn(out var comp))
            {
                if (comp.isFlyingCurrently)
                {
                    comp.ChangeGraphicToVanilla();
                    comp.isFlyingCurrently = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
    public static class GetStatValue_Patch
    {
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(Thing thing, StatDef stat, bool applyPostProcess, ref float __result)
        {
            if (stat == StatDefOf.MoveSpeed && thing is Pawn pawn && pawn.IsFlyingPawn(out var comp) && comp.isFlyingCurrently)
            {
                __result *= comp.Props.flyingMoveSpeedMultiplier;
            }
        }
    }

    [HarmonyPatch(typeof(DamageWorker_AddInjury), "ApplyDamageToPart")]
    public static class Patch_ApplyDamageToPart
    {
        public static void Prefix(ref DamageInfo dinfo, Pawn pawn, DamageResult result)
        {
            var instigator = dinfo.Instigator;
            if (instigator != null)
            {
                var compFlying = instigator.TryGetComp<CompFlyingPawn>();
                if (compFlying != null)
                {
                    var attackDamage = compFlying.Props.attackDamageFactor;
                    if (attackDamage != null)
                    {
                        if (pawn.BodySize <= attackDamage.targetBodySize)
                        {
                            dinfo.SetAmount(dinfo.Amount * attackDamage.damageMultiplier);
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Verb_MeleeAttack), "GetDodgeChance")]
    public static class Patch_GetDodgeChance
    {
        public static void Postfix(ref float __result, LocalTargetInfo target)
        {
            var pawn = target.Pawn;
            if (pawn != null)
            {
                var compFlying = pawn.TryGetComp<CompFlyingPawn>();
                if (compFlying != null && compFlying.isFlyingCurrently)
                {
                    __result *= compFlying.Props.evadeChanceWhenFlying;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ShotReport), "AimOnTargetChance_StandardTarget", MethodType.Getter)]
    public class AimOnTargetChance_StandardTarget_Patch
    {
        public static void Postfix(ref float __result, TargetInfo ___target)
        {
            if (___target.Thing is Pawn pawn)
            {
                var compFlying = pawn.TryGetComp<CompFlyingPawn>();
                if (compFlying != null && compFlying.isFlyingCurrently)
                {
                    __result *= (1f - compFlying.Props.evadeChanceWhenFlying);
                }
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_AIDefendPawn), "FindAttackTarget")]
    public static class Patch_FindAttackTarget
    {
        public static void Postfix(JobGiver_AIDefendPawn __instance, ref Thing __result, Pawn pawn)
        {
            if (pawn != null && __instance is JobGiver_AIDefendMaster)
            {
                var compFlying = pawn.TryGetComp<CompFlyingPawn>();
                if (compFlying != null && compFlying.Props.attackEnemiesMasterAttacking)
                {
                    Pawn defendee = pawn.playerSettings.Master;
                    if (defendee.CurJobDef == JobDefOf.AttackStatic || defendee.CurJobDef == JobDefOf.AttackMelee)
                    {
                        __result = defendee.CurJob.targetA.Thing;
                    }
                }
            }
        }
    }
}
