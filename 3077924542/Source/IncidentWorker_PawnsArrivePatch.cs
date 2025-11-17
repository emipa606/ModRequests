using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace DressForTheWeather
{
	[HarmonyPatch]
	public static class IncidentWorker_PawnsArrivePatch {
		private const int maxTries = 100;

		private static DressForTheWeatherSettings? settings;

		private static DressForTheWeatherSettings Settings => settings ??=
			LoadedModManager.GetMod<DressForTheWeatherMod>().GetSettings<DressForTheWeatherSettings>();

		private static List<ThingStuffPair>? allApparelPairs;

		private static void InitializeApparelPairs() {
			allApparelPairs =
				ThingStuffPair.AllWith(td => td.IsApparel && !td.apparel.layers.Contains(ApparelLayerDefOf.Belt) && !td.apparel.mechanitorApparel && td.apparel.canBeGeneratedToSatisfyWarmth && Settings.ApparelFilter.Allows(td));
        }

		[HarmonyPatch(typeof(IncidentWorker_Raid), nameof(IncidentWorker_Raid.TryGenerateRaidInfo))]
		[HarmonyPostfix]
		public static void Postfix(ref bool __result, IncidentWorker_Raid __instance, ref List<Pawn> pawns, IncidentParms parms) {


			if(pawns is null || parms is null || parms.target is null) return;

			//get map from parms
			Map map = (Map)parms.target;

			if (Settings.RaidersComePrepared) {
				InitializeApparelPairs();
                foreach (Pawn pawn in pawns)
					DressPawnAppropriately(pawn, map);
			}
		}

		[HarmonyPatch(typeof(IncidentWorker_NeutralGroup), "SpawnPawns")]
		[HarmonyPostfix]
		public static void Postfix(ref List<Pawn> __result, IncidentParms parms) {
			InitializeApparelPairs();
			//get map from parms
			Map map = (Map)parms.target;

			foreach (Pawn pawn in __result)
				DressPawnAppropriately(pawn, map);
		}


		private static void DressPawnAppropriately(Pawn pawn, Map map) {
			if (pawn.apparel is null) return;
			bool isWearingGasMask = false;
			//if pawn is not wearing a gas mask and should have one and biotech is enabled
			if (ModLister.BiotechInstalled && (map.pollutionGrid.TotalPollutionPercent > 0.5 || map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))) {

				Apparel gasMask;

				if (pawn.Faction?.def?.techLevel >= TechLevel.Industrial) {
					//get gas mask
					ThingDef gasMaskDef =  ThingDefOf.Apparel_GasMask;
					//create new gas mask
					gasMask = (Apparel)ThingMaker.MakeThing(gasMaskDef);
				} else {
					ThingDef warVeilDef = DefDatabase<ThingDef>.GetNamed("Apparel_WarVeil");
					var tsp = allApparelPairs!.Where(p => p.thing == warVeilDef).RandomElementByWeight(p => p.Commonality);
					gasMask = (Apparel)ThingMaker.MakeThing(tsp.thing, tsp.stuff);

				}
				//initialize apparel
				gasMask.InitializeComps();
				//add apparel to pawn
				pawn.apparel.Wear(gasMask, false);
				PawnGenerator.PostProcessGeneratedGear(gasMask, pawn);
				isWearingGasMask = true;
            }

            //get minimum comfortable temperature for pawn
            float minTemp = pawn.GetStatValue(StatDefOf.ComfyTemperatureMin);
			//get maximum comfortable temperature for pawn
			float maxTemp = pawn.GetStatValue(StatDefOf.ComfyTemperatureMax);
			//get current temperature
			float currentTemp = map.mapTemperature.OutdoorTemp;
			int tries = 0;

			while (currentTemp < minTemp && tries < maxTries) {
				tries++;
				//generate random apparel
				var apparel = GetNewApparel(pawn);
				if (apparel is null) continue;

                //get cold insulation
                float coldInsulation = apparel.GetStatValue(StatDefOf.Insulation_Cold);
				//compare to currently worn apparel in same slot

				List<Apparel> replacedApparel = GetApparelReplacedBy(pawn, apparel).ToList();

				if (replacedApparel.Any(a => !Settings.ReplaceableFilter.Allows(a)) || (isWearingGasMask && replacedApparel.Any(a => a.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead)))) continue;

                //if currently worn apparel has more cold insulation, don't wear the new apparel
                if (replacedApparel.Sum(a => a.GetStatValue(StatDefOf.Insulation_Cold)) > coldInsulation )
					continue;


				pawn.apparel.Wear(apparel, false);
			}

			while (currentTemp > maxTemp && tries < maxTries) {
				tries++;
				//generate random apparels
				var apparel = GetNewApparel(pawn);
				if(apparel is null) continue;
				//get heat insulation
				float heatInsulation = apparel.GetStatValue(StatDefOf.Insulation_Heat);
				//compare to currently worn apparel in same slot

				List<Apparel> replacedApparel = GetApparelReplacedBy(pawn, apparel).ToList();

				if (replacedApparel.Any(a => !Settings.ReplaceableFilter.Allows(a)) || (isWearingGasMask && replacedApparel.Any(a => a.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead)))) continue;
				
				//if currently worn apparel has more heat insulation, don't wear the new apparel
                if (replacedApparel.Sum(a => a.GetStatValue(StatDefOf.Insulation_Heat)) > heatInsulation )
					continue;

				pawn.apparel.Wear(apparel, false);
                //write to log
				Log.Message($"Wearing {apparel.def.label} for {pawn.Name}");
			}

		}

		private static IEnumerable<Apparel> GetApparelReplacedBy(Pawn pawn, Apparel apparel) {
			return pawn.apparel.WornApparel.Where(x =>
				x.def.apparel.bodyPartGroups.Intersect(apparel.def.apparel.bodyPartGroups).Any() &&
				x.def.apparel.layers.Intersect(apparel.def.apparel.layers).Any());
		}

		private static void Test(Pawn pawn) {
			PawnApparelGenerator.GenerateStartingApparelFor(pawn, new PawnGenerationRequest(pawn.kindDef, pawn.Faction, forceAddFreeWarmLayerIfNeeded:true, fixedGender: pawn.gender, developmentalStages: pawn.DevelopmentalStage));
		}

		private static Apparel? GetNewApparel(Pawn pawn) {
			while (true) {

				//get pawn faction tech level
				TechLevel techLevel = pawn.Faction?.def.techLevel ?? TechLevel.Archotech;
				//Log tech level

				if (!allApparelPairs!.Where(p => p.thing.techLevel <= techLevel && CanUseStuff(pawn, p))
					    .TryRandomElementByWeight(p => p.Commonality, out ThingStuffPair pair)) return null;

				if (pair.thing is null) return null;

				//Create new apparel
				Apparel apparel = (Apparel)ThingMaker.MakeThing(pair.thing, pair.stuff);
				if (!apparel.PawnCanWear(pawn)) continue;
				apparel.InitializeComps();
				PawnGenerator.PostProcessGeneratedGear(apparel, pawn);
				return apparel;
			}
		}


		private static bool CanUseStuff(Pawn pawn, ThingStuffPair pair) {
			List<SpecificApparelRequirement> apparelRequirements = pawn.kindDef.specificApparelRequirements;
			if (apparelRequirements != null) {
				for (int index = 0; index < apparelRequirements.Count; ++index) {
					if (!ApparelRequirementCanUseStuff(apparelRequirements[index], pair))
						return false;
				}
			}

			return pair.stuff == null || pawn.Faction == null || pawn.kindDef.ignoreFactionApparelStuffRequirements ||
			       pawn.Faction.def.CanUseStuffForApparel(pair.stuff);
		}


		private static bool ApparelRequirementCanUseStuff(
			SpecificApparelRequirement req,
			ThingStuffPair pair) {
			if (req.Stuff == null || !PawnApparelGenerator.ApparelRequirementHandlesThing(req, pair.thing))
				return true;
			return pair.stuff != null && req.Stuff == pair.stuff;
		}
	}
}
