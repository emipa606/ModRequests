using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Verse;
using Verse.AI.Group;
using Verse.Noise;

namespace BionicleKanohiMasksOfPower
{
    public class Hediff_Duplicate : HediffWithComps//duplicate hediff deletes pawn after 10 seconds
    {
        public int initTick;
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            initTick = Find.TickManager.TicksGame;
        }
        
        public override void Tick()
        {
            base.Tick();
            if ((Find.TickManager.TicksGame > initTick + 600)||this.pawn.Downed)//if pawn is downed, it dissappears before errors occur
            {
                this.pawn.Destroy();
                Find.WorldPawns.RemoveAndDiscardPawnViaGC(this.pawn);
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref initTick, "initTick");
        }
    }
	public class Apparel_Mahiki : Apparel
    {
		public const float EffectiveRange = 15f;
		public static bool CanHitTargetFrom(Pawn caster, IntVec3 root, LocalTargetInfo targ)//check for line of sight
		{
			float num = EffectiveRange * EffectiveRange;
			IntVec3 cell = targ.Cell;
			if ((float)caster.Position.DistanceToSquared(cell) <= num)
			{
				return GenSight.LineOfSight(root, cell, caster.Map);
			}
			return false;
		}
		public static void DrawHighlight(Pawn caster, LocalTargetInfo target)
		{
			if (target.IsValid && ValidJumpTarget(caster.Map, target.Cell))
			{
				GenDraw.DrawTargetHighlightWithLayer(target.CenterVector3, AltitudeLayer.MetaOverlays);
			}
			GenDraw.DrawRadiusRing(caster.Position, EffectiveRange, Color.white, (IntVec3 c) => GenSight.LineOfSight(caster.Position, c, caster.Map) && ValidJumpTarget(caster.Map, c));
		}

		public static bool ValidJumpTarget(Map map, IntVec3 cell)
		{
			if (!cell.IsValid || !cell.InBounds(map))
			{
				return false;
			}
			if (cell.Impassable(map) || !cell.Walkable(map) || cell.Fogged(map))
			{
				return false;
			}
			Building edifice = cell.GetEdifice(map);
			Building_Door building_Door;
			if (edifice != null && (building_Door = edifice as Building_Door) != null && !building_Door.Open)
			{
				return false;
			}
			return true;
		}

		public static TargetingParameters TargetingParameters(Pawn pawn)
		{
			return new TargetingParameters
			{
				canTargetLocations = true,
				canTargetPawns = false,
				canTargetBuildings = false,
				validator = (TargetInfo x) => CanHitTargetFrom(pawn, pawn.Position, x.Cell) && ValidJumpTarget(pawn.Map, x.Cell)
			};
        }


		public override IEnumerable<Gizmo> GetWornGizmos()
        {
            foreach (var g in base.GetWornGizmos())
            {
                yield return g;
            }
            if (Wearer.IsColonistPlayerControlled && this.IsMasterworkOrLegendary())
            {
				yield return new Command_Action()
				{
					defaultLabel = "Bionicle.CreateClone".Translate(),
					defaultDesc = "Bionicle.CreateCloneDesc".Translate(),
					action = delegate
					{
                        Find.Targeter.BeginTargeting(TargetingParameters(Wearer), delegate (LocalTargetInfo localTargetInfo)
                        {
                            DuplicatePawnAt(localTargetInfo);
                        }, highlightAction: (LocalTargetInfo x) =>
						{
                            GenDraw.DrawRadiusRing(Wearer.Position, EffectiveRange, Color.white, (IntVec3 c) => GenSight.LineOfSight(Wearer.Position, c, Wearer.Map) && ValidJumpTarget(Wearer.Map, c));
                            DrawHighlight(Wearer, x);
						}, null, Wearer);
					},
					onHover = delegate
					{
                        GenDraw.DrawRadiusRing(Wearer.Position, EffectiveRange, Color.white, (IntVec3 c) => GenSight.LineOfSight(Wearer.Position, c, Wearer.Map) && ValidJumpTarget(Wearer.Map, c));
					},
					icon = this.def.uiIcon,
				};
            }
        }

        private void DuplicatePawnAt(LocalTargetInfo localTargetInfo)
        {
            var pawn = PawnUtility.GetPawnDuplicate(Wearer, Wearer.kindDef);//create new pawn duplicate
            var hediff = HediffMaker.MakeHediff(BionicleDefOf.BKMOP_PawnDuplicate, pawn);//create duplicate hediff
            pawn.health.AddHediff(hediff);//add hediff to duplicate
            GenSpawn.Spawn(pawn, localTargetInfo.Cell, Wearer.Map);//spawn duplicate on map
            LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_DefendPoint(localTargetInfo.Cell, addFleeToil: false), Wearer.Map, Gen.YieldSingle(pawn));//give ai of pawn
            Find.Targeter.BeginTargeting(TargetingParameters(Wearer), delegate (LocalTargetInfo localTargetInfo2)
            {
                DuplicatePawnAt(localTargetInfo2);
            }, highlightAction: (LocalTargetInfo x) =>
            {
                GenDraw.DrawRadiusRing(Wearer.Position, EffectiveRange, Color.white, (IntVec3 c) => GenSight.LineOfSight(Wearer.Position, c, Wearer.Map) && ValidJumpTarget(Wearer.Map, c));
                DrawHighlight(Wearer, x);
            }, null, Wearer);
        }

        public override void ExposeData()
        {
            base.ExposeData();
			//Scribe_Values.Look(ref lastUsedTick, "lastUsedTick");
        }
    }

	public static class PawnUtility//creates new duplicate pawn
    {
        public static Pawn GetPawnDuplicate(Pawn origin, PawnKindDef newPawnKindDef)
        {
            NameTriple nameTriple = origin.Name as NameTriple;
            Pawn newPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(newPawnKindDef, origin.Faction, PawnGenerationContext.NonPlayer,
                fixedGender: origin.gender, fixedBirthName: nameTriple.First, fixedLastName: nameTriple.Last));

            newPawn.Name = new NameTriple(nameTriple.First, nameTriple.Nick, nameTriple.Last);
            newPawn.story.childhood = origin.story.childhood;
            newPawn.story.adulthood = origin.story.adulthood;
            newPawn.story.bodyType = origin.story.bodyType;
            newPawn.story.hairDef = origin.story.hairDef;
            newPawn.story.hairColor = origin.story.hairColor;

            newPawn.playerSettings = new Pawn_PlayerSettings(newPawn);
            newPawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
            newPawn.playerSettings.AreaRestriction = origin.playerSettings.AreaRestriction;
            newPawn.playerSettings.medCare = origin.playerSettings.medCare;
            newPawn.playerSettings.selfTend = origin.playerSettings.selfTend;

            if (newPawn.foodRestriction == null) newPawn.foodRestriction = new Pawn_FoodRestrictionTracker();
            if (origin.foodRestriction?.CurrentFoodRestriction != null) newPawn.foodRestriction.CurrentFoodRestriction = origin.foodRestriction?.CurrentFoodRestriction;
            if (newPawn.outfits == null) newPawn.outfits = new Pawn_OutfitTracker();
            if (origin.outfits?.CurrentOutfit != null) newPawn.outfits.CurrentOutfit = origin.outfits?.CurrentOutfit;
            if (newPawn.drugs == null) newPawn.drugs = new Pawn_DrugPolicyTracker();
            if (origin.drugs?.CurrentPolicy != null) newPawn.drugs.CurrentPolicy = origin.drugs?.CurrentPolicy;
            if (newPawn.timetable == null) newPawn.timetable = new Pawn_TimetableTracker(newPawn);
            if (origin.timetable?.times != null) newPawn.timetable.times = origin.timetable?.times;


            if (newPawn.Faction != origin.Faction)
            {
                newPawn.SetFaction(origin.Faction);
            }

            if (newPawn.needs?.mood?.thoughts?.memories?.Memories != null)
            {
                for (int num = newPawn.needs.mood.thoughts.memories.Memories.Count - 1; num >= 0; num--)
                {
                    newPawn.needs.mood.thoughts.memories.RemoveMemory(newPawn.needs.mood.thoughts.memories.Memories[num]);
                }
            }

            newPawn.story.traits.allTraits.Clear();
            var traits = origin.story?.traits?.allTraits;
            if (traits != null)
            {
                foreach (var trait in traits)
                {
                    newPawn.story.traits.GainTrait(trait);
                }
            }
            newPawn.relations.ClearAllRelations();
            var skills = origin.skills.skills;
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

            var apparels = origin.apparel?.WornApparel ?? new List<Apparel>();
            newPawn.apparel.DestroyAll();
            for (int num = apparels.Count - 1; num >= 0; num--)
            {
                var apparel = ThingMaker.MakeThing(apparels[num].def, apparels[num].Stuff) as Apparel;
                newPawn.apparel.Wear(apparel);
            }

            var equipments = origin.equipment?.AllEquipmentListForReading ?? new List<ThingWithComps>();
            newPawn.equipment.DestroyAllEquipment();
            for (int num = equipments.Count - 1; num >= 0; num--)
            {
                var equipment = ThingMaker.MakeThing(equipments[num].def, equipments[num].Stuff) as ThingWithComps;
                newPawn.equipment.AddEquipment(equipment);
            }

            var inventoryThings = origin.inventory?.innerContainer?.ToList() ?? new List<Thing>();
            newPawn.inventory.DestroyAll();
            //for (int num = inventoryThings.Count - 1; num >= 0; num--)
            //{
            //    var thing = ThingMaker.MakeThing(inventoryThings[num].def, inventoryThings[num].Stuff);
            //    thing.stackCount = inventoryThings[num].stackCount;
            //    newPawn.inventory.TryAddItemNotForSale(thing);
            //}

            newPawn.apparel.LockAll();//locks apparel on duplicate

            newPawn.ideo = null;
            return newPawn;
        }
    }
}