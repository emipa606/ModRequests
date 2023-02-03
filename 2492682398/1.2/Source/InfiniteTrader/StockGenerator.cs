using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace InfiniteTrader
{
	public class StockGenerator_AllGoods : StockGenerator
	{
		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			List<Thing> generatedThings = new List<Thing>();
			foreach (var chosenThingDef in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => HandlesThingDef(d) && d.tradeability.TraderCanSell()))
			{
				try
                {
					if (chosenThingDef.race != null)
                    {
						if (InfiniteTraderMod.settings.disableAnimals && chosenThingDef.race.Animal)
                        {
							continue;
                        }
						if (InfiniteTraderMod.settings.disablePawns && chosenThingDef.race.Humanlike)
                        {
							continue;
                        }
						if (chosenThingDef.race.Humanlike)
                        {
							for (var i = 0; i < InfiniteTraderMod.settings.pawnCount; i++)
                            {
								PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Slave, null, PawnGenerationContext.NonPlayer, forTile);
								Pawn pawn = PawnGenerator.GeneratePawn(request);
								generatedThings.Add(pawn);
							}
                        }
						else
                        {
							for (var i = 0; i < InfiniteTraderMod.settings.animalCount; i++)
							{
								PawnGenerationRequest request = new PawnGenerationRequest(chosenThingDef.race.AnyPawnKind, null, PawnGenerationContext.NonPlayer, forTile);
								Pawn pawn = PawnGenerator.GeneratePawn(request);
								generatedThings.Add(pawn);
							}
						}

					}
					else
                    {
						if (chosenThingDef == ThingDefOf.Silver)
                        {
							foreach (Thing item in StockGeneratorUtility.TryMakeForStock(chosenThingDef, 9999999))
							{
								generatedThings.Add(item);
							}
						}
						else
                        {
							if (InfiniteTraderMod.settings.disableBuildings && chosenThingDef.building != null)
                            {
								continue;
                            }
							if (InfiniteTraderMod.settings.disableApparels && chosenThingDef.IsApparel)
                            {
								continue;
                            }
							if (InfiniteTraderMod.settings.disableWeapons && chosenThingDef.IsWeapon)
                            {
								continue;
                            }
							if (InfiniteTraderMod.settings.disableImplants && chosenThingDef.isTechHediff)
                            {
								continue;
                            }
							foreach (Thing item in StockGeneratorUtility.TryMakeForStock(chosenThingDef, chosenThingDef.stackLimit > 1 ? InfiniteTraderMod.settings.bulkAmount : 20))
							{
								generatedThings.Add(item);
							}
						}
					}

				}
				catch (Exception ex)
                {
					Log.Error("Error generating " + chosenThingDef.defName + " - " + ex);
                }
			}
			return generatedThings;
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			if (thingDef.BaseMarketValue > 0 && thingDef.tradeability != Tradeability.None && thingDef.thingClass != typeof(Building_Vent) && thingDef.thingClass != typeof(Building_PowerSwitch))
			{
				return true;
			}
			return false;
		}
    }

	public class StockGenerator_BuyEverything : StockGenerator
	{
		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			yield break;
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return true;
		}
	}
}
