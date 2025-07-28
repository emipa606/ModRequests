using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace ResearchableStatUpgrades
{
	public class StatPart_ResearchDependent : StatPart
    {
        internal void AddFactor(ResearchFactor researchFactor)
        {
            this.researchFactors.Add(researchFactor);
        }

        public override string ExplanationPart(StatRequest req)
        {
            float num = 1f;
            this.TransformValue(req, ref num);
            string text = GenText.ToStringPercent(num);
            return (text == "100%" ? new TaggedString(string.Empty) : (Translator.Translate("RSU_FactorFromResearch") + text));
        }

		private static bool ValidBuilding(StatRequest req)
        {
			if (!req.HasThing) return false;

			ThingDef def = req.Thing.def;
			if (def == null) return false;

			bool isBuilding = def.building != null;
			if (!isBuilding) return false;

			bool isClaimable = def.building.claimable;
			bool isPlayerAcquireable = def.PlayerAcquirable;
			bool isTerrain = def.building.naturalTerrain != null || def.building.mineableThing != null;
            bool isPlant = def.IsPlant;

            return (isBuilding && (isClaimable || isPlayerAcquireable) && !(isTerrain || isPlant));
		}

		private static bool ValidPawn(StatRequest req, ResearchFactor researchFactor)
        {

			bool hasRace = req.Thing.def.race != null;
			if (!hasRace) return false;

			bool applyingToNonHumanlike = researchFactor.applyToNonHumanlike == true;
			bool isHumanlike = req.Thing.def.race.Humanlike == true;

			return (hasRace && (applyingToNonHumanlike || (!applyingToNonHumanlike && isHumanlike)));
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing)
			{
				bool hasRace = req.Thing.def.race != null;
				if (!(ValidBuilding(req) || hasRace)) return;
				for (int i = 0; i < this.researchFactors.Count; i++)
				{
					ResearchFactor researchFactor = this.researchFactors[i];
					if (!ValidPawn(req,researchFactor) && hasRace) continue;

					bool flag2 = ShouldApplyFactorToRequest(req, researchFactor);
					if (flag2)
					{
						val *= researchFactor.factor;
					}
				}
				for (int j = 0; j < this.repeatables.Count; j++)
				{
					ResearchFactor researchFactor2 = this.repeatables[j];
					if (!ValidPawn(req,researchFactor2) && hasRace) continue;

					bool flag3 = ShouldApplyFactorToRequest(req, researchFactor2);
					if (flag3)
					{
						int factorFor = RSUUtil.RepeatableResearchManager.GetFactorFor(researchFactor2.def);
						for (j = 0; j < factorFor; j++)
						{
							val *= researchFactor2.factor;
						}
					}
				}
			}
		}

		private static bool ShouldApplyFactorToRequest(StatRequest req, ResearchFactor factor)
        {
			if (ValidPawn(req, factor)) return ShouldApplyFactorToPawn(req, factor);
			if (ValidBuilding(req)) return ShouldApplyFactorToBuilding(req, factor);
			return false;
        }

		private static bool ShouldApplyFactorToPawn(StatRequest req, ResearchFactor factor)
		{
			if (!((factor.def.IsFinished || factor.def.IsRepeatableResearch()) && req.HasThing && req.Thing.Spawned)) return false;
			
			int thingIDNumber = req.Thing.thingIDNumber;
			Predicate<Pawn> predicate = new Predicate<Pawn>((Pawn comparison) => { return comparison.thingIDNumber == thingIDNumber; });
			Pawn foundPawn = req.Thing.Map.mapPawns.AllPawns.Find(predicate);

			if (foundPawn == null) return false;

			if (foundPawn.def.race.Humanlike || factor.applyToNonHumanlike)
			{
				if ((req.Thing.Faction == Faction.OfPlayer || (factor.applyToSlave && foundPawn.IsSlave)) || factor.applyToNonColonistFaction) return true;
			}
			return false;
		}

		private static Building FindBuilding(StatRequest req, ResearchFactor factor)
        {
			if (!((factor.def.IsFinished || factor.def.IsRepeatableResearch()) && req.HasThing && req.Thing.Spawned)) return null;

			string thingID = req.Thing.GetUniqueLoadID();
			Predicate<Building> predicate = new Predicate<Building>((Building comparison) => { return comparison.GetUniqueLoadID() == thingID; });

			//Check player-owned buildings
			Building foundBuilding = req.Thing.Map.listerBuildings.allBuildingsColonist.Find(predicate);

			if (foundBuilding == null) return null;

			if (factor.applyToNonColonistFaction != true)
			{
				return foundBuilding;
			}
			//Check non-player-owned buildings
			Building foundBuilding2 = req.Thing.Map.listerBuildings.allBuildingsNonColonist.Find(predicate);
			return (foundBuilding != null ? foundBuilding : foundBuilding2);
		}
		private static bool ShouldApplyFactorToBuilding(StatRequest req, ResearchFactor factor)
        {
			if (!((factor.def.IsFinished || factor.def.IsRepeatableResearch()) && req.HasThing && req.Thing.Spawned)) return false;

			Building foundBuilding = FindBuilding(req,factor);

			return foundBuilding != null;
        }

		public List<ResearchFactor> researchFactors = new List<ResearchFactor>();
		public List<ResearchFactor> repeatables = new List<ResearchFactor>();
	}
}