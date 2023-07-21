using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace Renamon
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("Renamon.Mod").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Ideo), "MemberNamePlural", MethodType.Getter)]
    public static class Ideo_MemberNamePlural_Fix
    {
        public static void Prefix(Ideo __instance)
        {
            if (__instance.memberName is null)
            {
                __instance.memberName = "CaravanMembers".Translate().ToLower();
            }
        }
    }

    [StaticConstructorOnStartup]
    public class Command_ActionWithCooldown : Command_Action
    {
        private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(Color.grey.r, Color.grey.g, Color.grey.b, 0.6f));
        private int lastUsedTick;
        private int cooldownTicks;
        public Command_ActionWithCooldown(int lastUsedTick, int cooldownTicks)
        {
            this.lastUsedTick = lastUsedTick;
            this.cooldownTicks = cooldownTicks;
            this.disabled = this.lastUsedTick > 0 ? Find.TickManager.TicksGame - this.lastUsedTick < this.cooldownTicks : false;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
            if (this.lastUsedTick > 0)
            {
                var cooldownTicksRemaining = Find.TickManager.TicksGame - this.lastUsedTick;
                if (cooldownTicksRemaining < this.cooldownTicks)
                {
                    float num = Mathf.InverseLerp(this.cooldownTicks, 0, cooldownTicksRemaining);
                    Widgets.FillableBar(rect, Mathf.Clamp01(num), cooldownBarTex, null, doBorder: false);
                }
            }
            if (result.State == GizmoState.Interacted)
            {
                return result;
            }
            return new GizmoResult(result.State);
        }
    }
    public class CompProperties_PawnMorpher : CompProperties
    {
        public CompProperties_PawnMorpher()
        {
            this.compClass = typeof(CompPawnMorpher);
        }
    }

    [HarmonyPatch(typeof(GenSpawn), "Spawn", new System.Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool) })]
    public static class GenSpawn_Spawn_Patch
    {
        public static void Postfix(Thing newThing, ref WipeMode wipeMode, bool respawningAfterLoad)
        {
            if (!respawningAfterLoad && newThing is Pawn pawn && pawn.Faction != Faction.OfPlayer && pawn.Map.IsPlayerHome)
            {
                var comp = pawn.GetComp<CompPawnMorpher>();
                if (comp != null && !comp.transformedAlready)
                {
                    var hostilePawns = pawn.Map.attackTargetsCache.TargetsHostileToFaction(pawn.Faction).OfType<Pawn>()
                        .Where(x => x.RaceProps.Humanlike && x.def != pawn.def);
                    if (hostilePawns.TryRandomElement(out var hostile))
                    {
                        comp.pawnToTransform = hostile;
                    }
                }
            }
        }
    }
    public class CompPawnMorpher : ThingComp
    {
        public const int CooldownPeriodTicks = 30000;
        public int lastDuplicateTicks;
        public Pawn pawnToTransform;
        public bool transformedAlready;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (this.parent is Pawn pawn && pawn.RaceProps.Humanlike)
            {
                if (pawn.apparel is null)
                {
                    pawn.apparel = new Pawn_ApparelTracker(pawn);
                }
                if (pawn.equipment is null)
                {
                    pawn.equipment = new Pawn_EquipmentTracker(pawn);
                }
                if (pawn.inventory is null)
                {
                    pawn.inventory = new Pawn_InventoryTracker(pawn);
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (pawnToTransform != null)
            {
                this.MakePawnDuplicate(pawnToTransform);
                pawnToTransform = null;
            }
        }
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            var pawn = this.parent as Pawn;
            if (pawn.Faction == Faction.OfPlayer)
            {
                yield return new Command_ActionWithCooldown(lastDuplicateTicks, CooldownPeriodTicks)
                {
                    defaultLabel = "Renamon.DuplicateBody".Translate(),
                    defaultDesc = "Renamon.DuplicateBodyDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Abilities/Dupe_body"),
                    action = delegate
                    {
                        Find.Targeter.BeginTargeting(new TargetingParameters
                        {
                            canTargetHumans = true,
                            canTargetAnimals = false,
                            canTargetBuildings = false,
                            canTargetMechs = false,
                            validator = (TargetInfo x) => x.Thing is Pawn victim && victim.RaceProps.Humanlike 
                                && !victim.Dead && victim.def != this.parent.def
                        }, delegate (LocalTargetInfo x)
                        {
                            SoundDef.Named("Morph_Start").PlayOneShot(this.parent);
                            var otherPawn = x.Pawn;
                            MakePawnDuplicate(otherPawn);
                        });
                    },
                    disabled = pawn.Downed || pawn.InMentalState
                };
            }
        }
        
        public Pawn MakePawnDuplicate(Pawn source)
        {
            var origin = this.parent as Pawn;
            var drafted = origin.drafter != null ? origin.drafter.Drafted : false;
            var selected = Find.Selector.IsSelected(this.parent);
            NameTriple nameTriple = source.Name as NameTriple;
            Pawn newPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(source.kindDef, this.parent.Faction, PawnGenerationContext.NonPlayer,
                fixedGender: source.gender, fixedBirthName: nameTriple.First, fixedLastName: nameTriple.Last));
            newPawn.Name = new NameTriple(nameTriple.First, nameTriple.Nick, nameTriple.Last);

            if (origin.apparel != null)
            {
                newPawn.apparel = origin.apparel;
                origin.apparel = new Pawn_ApparelTracker(origin);
                newPawn.apparel.pawn = newPawn;
                foreach (var verb in newPawn.apparel.AllApparelVerbs)
                {
                    verb.caster = newPawn;
                }
            }
            if (origin.equipment != null)
            {
                newPawn.equipment = origin.equipment;
                origin.equipment = new Pawn_EquipmentTracker(origin);
                newPawn.equipment.pawn = newPawn;
                foreach (var verb in newPawn.equipment.AllEquipmentVerbs)
                {
                    verb.caster = newPawn;
                }
            }

            if (origin.inventory != null)
            {
                newPawn.inventory = origin.inventory;
                origin.inventory = new Pawn_InventoryTracker(origin);
                newPawn.inventory.pawn = newPawn;
            }

            ModCompatibility.SetAlienHead(newPawn, ModCompatibility.GetAlienHead(source));
            ModCompatibility.SetSkinColorFirst(newPawn, ModCompatibility.GetSkinColorFirst(source));
            ModCompatibility.SetSkinColorSecond(newPawn, ModCompatibility.GetSkinColorSecond(source));

            ModCompatibility.SetHairColorFirst(newPawn, ModCompatibility.GetHairColorFirst(source));
            ModCompatibility.SetHairColorSecond(newPawn, ModCompatibility.GetHairColorSecond(source));
            ModCompatibility.CopyBodyAddons(source, newPawn);

            newPawn.story.childhood = source.story.childhood;
            newPawn.story.adulthood = source.story.adulthood;

            newPawn.story.traits.allTraits.Clear();
            var traits = source.story?.traits?.allTraits;
            if (traits != null)
            {
                foreach (var trait in traits)
                {
                    newPawn.story.traits.GainTrait(trait);
                }
            }

            var skills = source.skills.skills;
            newPawn.skills.skills.Clear();
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    var newSkill = new SkillRecord(newPawn, skill.def);
                    newSkill.passion = skill.passion;
                    newSkill.levelInt = skill.levelInt;
                    newSkill.xpSinceLastLevel = skill.xpSinceLastLevel;
                    newSkill.xpSinceMidnight = skill.xpSinceMidnight;
                    newPawn.skills.skills.Add(newSkill);
                }
            }

            if (ModsConfig.RoyaltyActive)
            {
                if (source.HasPsylink)
                {
                    var psylink = source.GetPsylinkLevel();
                    Hediff_Level hediff_Level = newPawn.GetMainPsylinkSource();
                    if (hediff_Level == null)
                    {
                        hediff_Level = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, newPawn, newPawn.health.hediffSet.GetBrain()) as Hediff_Level;
                        newPawn.health.AddHediff(hediff_Level);
                    }
                    hediff_Level.SetLevelTo(psylink);
                }

                foreach (var ability in newPawn.abilities.abilities.Select(x => x.def).ToList())
                {
                    newPawn.abilities.RemoveAbility(ability);
                }

                var abilities = source.abilities.abilities;
                foreach (var ability in abilities)
                {
                    newPawn.abilities.GainAbility(ability.def);
                }
            }

            newPawn.story.bodyType = source.story.bodyType;
            newPawn.story.hairDef = source.story.hairDef;
            newPawn.story.hairColor = source.story.hairColor;
            newPawn.style.beardDef = source.style.beardDef;
            newPawn.style.faceTattoo = source.style.faceTattoo;
            newPawn.style.bodyTattoo = source.style.bodyTattoo;

            var map = this.parent.Map;
            var position = this.parent.Position;
            foreach (var verb in newPawn.VerbTracker.AllVerbs)
            {
                verb.caster = newPawn;
            }
            newPawn.Rotation = origin.Rotation;
            if (newPawn.drafter != null)
            {
                newPawn.drafter.Drafted = drafted;
            }
            var hediff = HediffMaker.MakeHediff(HediffDef.Named("Renamon_PawnDuplication"), newPawn) as Hediff_MorphedPawn;
            hediff.oldPawn = origin;
            newPawn.health.AddHediff(hediff);
            var lord = origin.GetLord();
            if (lord != null)
            {
                lord.AddPawn(newPawn);
                lord.RemovePawn(origin);
            }

            this.parent.DeSpawn();
            GenSpawn.Spawn(newPawn, position, map);
            
            if (selected)
            {
                Find.Selector.Select(newPawn);
            }
            if (newPawn.WorkTagIsDisabled(WorkTags.Violent) && newPawn.equipment.Primary != null)
            {
                newPawn.equipment.DropAllEquipment(newPawn.Position);
            }
            ThrowSmokePuffThick(newPawn.DrawPos, newPawn.Map, 2.3f, Color.white);
            transformedAlready = true;
            return newPawn;
        }

        public static void ThrowSmokePuffThick(Vector3 loc, Map map, float scale, Color color)
        {
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, FleckDefOf.Smoke, scale);
            dataStatic.rotationRate = Rand.Range(-60, 60);
            dataStatic.velocityAngle = 0;
            dataStatic.velocitySpeed = Rand.Range(0.6f, 0.75f);
            map.flecks.CreateFleck(dataStatic);
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastDuplicateTicks, "lastDuplicateTicks");
            Scribe_Values.Look(ref transformedAlready, "npcTransformedAlready");
        }
    }

    public class Hediff_MorphedPawn : HediffWithComps
    {
        public Pawn oldPawn;
        public override void PostRemoved()
        {
            base.PostRemoved();
            RemoveDeduplication();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            if (this.pawn.Faction == Faction.OfPlayer)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Renamon.CancelDuplication".Translate(),
                    defaultDesc = "Renamon.CancelDuplicationDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Abilities/CancelDupe_body"),
                    action = RemoveDeduplication
                };
            }
        }
        private void RemoveDeduplication()
        {
            SoundDef.Named("Morph_Fade").PlayOneShot(this.pawn);
            var drafted = pawn.drafter != null ? pawn.drafter.Drafted : false;
            var selected = Find.Selector.IsSelected(pawn);
            foreach (var injury in pawn.health.hediffSet.hediffs.Where(x => x is Hediff_Injury || x is Hediff_MissingPart).ToList())
            {
                if (injury is Hediff_Injury)
                {
                    Hediff firstHediffOfDef = oldPawn.health.hediffSet.hediffs.FirstOrDefault(x => x.def == injury.def && x.Part == injury.Part);
                    if (firstHediffOfDef != null)
                    {
                        firstHediffOfDef.Severity += injury.Severity;
                    }
                    else if (injury.Severity > 0f)
                    {
                        var part = oldPawn.health.hediffSet.GetNotMissingParts().Where(x => x.def == injury.Part.def)
                            .TryRandomElement(out var bodyPart) ? bodyPart : null;
                        firstHediffOfDef = HediffMaker.MakeHediff(injury.def, oldPawn, part);
                        firstHediffOfDef.Severity = injury.Severity;
                        oldPawn.health.AddHediff(firstHediffOfDef);
                    }
                }
                else if (oldPawn.def.race.body.GetPartsWithDef(injury.Part.def).TryRandomElement(out var bodyPart))
                {
                    oldPawn.health.AddHediff(HediffMaker.MakeHediff(injury.def, oldPawn, bodyPart));
                }
            }

            if (pawn.apparel != null)
            {
                oldPawn.apparel = pawn.apparel;
                pawn.apparel = new Pawn_ApparelTracker(pawn);
                oldPawn.apparel.pawn = oldPawn;
                foreach (var verb in oldPawn.apparel.AllApparelVerbs)
                {
                    verb.caster = oldPawn;
                }
            }
            
            if (pawn.equipment != null)
            {
                oldPawn.equipment = pawn.equipment;
                pawn.equipment = new Pawn_EquipmentTracker(pawn);
                oldPawn.equipment.pawn = oldPawn;
                foreach (var verb in oldPawn.equipment.AllEquipmentVerbs)
                {
                    verb.caster = oldPawn;
                }
            }
            
            if (pawn.inventory != null)
            {
                oldPawn.inventory = pawn.inventory;
                pawn.inventory = new Pawn_InventoryTracker(pawn);
                oldPawn.inventory.pawn = oldPawn;
            }

            if (oldPawn.drafter != null)
            {
                oldPawn.drafter.Drafted = drafted;
            }
            var lord = this.pawn.GetLord();
            if (lord != null)
            {
                lord.AddPawn(oldPawn);
                lord.RemovePawn(this.pawn);
            }
            if (oldPawn.Dead)
            {
                var corpse = oldPawn.Corpse ?? oldPawn.MakeCorpse(null, false, 0);
                GenSpawn.Spawn(corpse, pawn.PositionHeld, pawn.MapHeld);
            }
            else
            {
                GenSpawn.Spawn(oldPawn, pawn.PositionHeld, pawn.MapHeld);
                oldPawn.Rotation = pawn.Rotation;
                oldPawn.guest.SetGuestStatus(pawn.HostFaction, pawn.guest.GuestStatus);
            }

            if (selected)
            {
                Find.Selector.Select(oldPawn);
            }
            
            if (oldPawn.WorkTagIsDisabled(WorkTags.Violent) && oldPawn.equipment.Primary != null)
            {
                oldPawn.equipment.DropAllEquipment(oldPawn.Position);
            }
            oldPawn.GetComp<CompPawnMorpher>().lastDuplicateTicks = Find.TickManager.TicksGame;
            CompPawnMorpher.ThrowSmokePuffThick(oldPawn.DrawPos, oldPawn.MapHeld, 2.3f, Color.white);
            pawn.Destroy();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref oldPawn, "oldPawn");
        }
    }
    public class DeathActionWorker_EmpExplosion : DeathActionWorker
    {
        public override RulePackDef DeathRules => RulePackDefOf.Transition_DiedExplosive;

        public override bool DangerousInMelee => true;

        public override void PawnDied(Corpse corpse)
        {
            GenExplosion.DoExplosion(radius: 3, center: corpse.Position, map: corpse.Map, 
                damType: DamageDefOf.EMP, instigator: corpse.InnerPawn);
        }
    }
}
