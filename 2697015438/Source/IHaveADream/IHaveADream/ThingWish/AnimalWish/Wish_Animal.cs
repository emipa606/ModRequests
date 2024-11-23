using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace HDream
{
    public class Wish_Animal : Wish_ThingPossession<AnimalWishInfo>
    {

        public new AnimalWishDef Def => (AnimalWishDef)def;

        protected override ThingDef GetThingDef(AnimalWishInfo thing) => thing.animal;
        protected override LookMode ExposeLookModeT() => LookMode.Deep;
        protected override List<AnimalWishInfo> GetThingsFromDef()
        {
            if (!Def.shouldBeExotic) return Def.Animals;
            List<AnimalWishInfo> infos = new List<AnimalWishInfo>(Def.Animals);
            List<PawnKindDef> pawnKinds = new List<PawnKindDef>(DefDatabase<PawnKindDef>.AllDefsListForReading);
            List<BiomeDef> biomes = DefDatabase<BiomeDef>.AllDefsListForReading;
            pawnKinds.RemoveAll(kind => pawn.Map.Biome.CommonalityOfAnimal(kind) > 0 || biomes.Find(biome => biome.CommonalityOfAnimal(kind) > 0) == null);
            infos.RemoveAll(info => pawnKinds.Find(kind => kind.race == info.animal) == null);
            return infos;
        }

        protected override int CountMatch()
        {
            int count = 0;
            List<Pawn> colonyAnimals = pawn.Map.mapPawns.PawnsInFaction(Faction.OfPlayer).FindAll(p => p.RaceProps.Animal);
            for (int i = 0; i < ThingsWanted.Count; i++)
            {
                count += AdjustForSpecifiedCount(ThingMatching(colonyAnimals, ThingsWanted[i]), ThingsWanted[i].amount);
            }
            return count;
        }

        protected override int ThingMatching(IEnumerable<Thing> things, AnimalWishInfo match)
        {
            if (things.EnumerableNullOrEmpty()) return 0;
            things = things.Where(thing => thing.def == match.animal);
            if(Def.countAmountPerInfo && match.amount > things.Count()) return 0;
            int count = 0;
            for (int i = 0; i < things.Count(); i++)
            {
                if (match.shouldBeBonded
                    && pawn.relations.DirectRelations.Find(rel => rel.otherPawn == things.ElementAt(i) && rel.def == PawnRelationDefOf.Bond) == null) continue;
                if(match.includedStage != null
                    && !match.includedStage.Contains((things.ElementAt(i) as Pawn).ageTracker.CurLifeStageIndex)) continue;

                count++;
                if (count >= match.amount) return count;
            }
            return count;
        }

        protected override void SortList(List<AnimalWishInfo> list)
        {
            base.SortList(list);
            list.Sort(delegate (AnimalWishInfo x, AnimalWishInfo y)
                {
                    return x.animal.label.CompareTo(y.animal.label);
                });
        }

        protected override string FormateListThing(List<AnimalWishInfo> things)
        {
            if (things.NullOrEmpty()) return base.FormateListThing(things);

            string listingBaseCount = "";
            string listingOtherCount = "";

            for (int i = 0; i < things.Count; i++)
            {
                if(things[i].amount <= Def.specificAmount 
                    && things[i].shouldBeBonded == Def.shouldBeBonded
                    && things[i].includedStage == Def.includedStage)
                {
                    if (listingBaseCount == "") 
                    { 
                        if(!Def.countAmountPerInfo && Def.specificAmount < Def.amountNeeded) listingBaseCount += " x" + Def.specificAmount.ToString();
                        if (things[i].shouldBeBonded) listingBaseCount += " (" + PawnRelationDefOf.Bond.LabelCap + ")";
                        if (!things[i].includedStage.NullOrEmpty())
                        {
                            listingBaseCount += "(";
                            for (int j = 0; j < things[i].includedStage.Count; j++)
                            {
                                listingBaseCount += things[i].animal.race.lifeStageAges[things[i].includedStage[j]].def.LabelCap;
                                listingBaseCount += j == things[i].includedStage.Count - 1 ? ")" : "/";
                            }
                        }
                        listingBaseCount += " => ";
                    }
                    else listingBaseCount += Def.listing_Separator;
                    listingBaseCount += things[i].animal.label;
                }
                else
                {
                    listingOtherCount += " x" + things[i].amount.ToString() + " ";
                    listingOtherCount += things[i].animal.label;
                    if (things[i].shouldBeBonded) listingOtherCount += "(" + PawnRelationDefOf.Bond.LabelCap + ")";
                    if (!things[i].includedStage.NullOrEmpty())
                    {
                        listingOtherCount += "(";
                        for (int j = 0; j < things[i].includedStage.Count; j++)
                        {
                            listingOtherCount += things[i].animal.race.lifeStageAges[things[i].includedStage[j]].def.LabelCap;
                            listingOtherCount += j == things[i].includedStage.Count - 1 ? ")" : "/";
                        }
                    }
                    if (i != things.Count - 1) listingOtherCount += def.listing_Separator;
                }
            }
            if (listingBaseCount != "")
            {
                if (listingOtherCount != "") listingBaseCount += "\n\n" + listingOtherCount;
            }
            else listingBaseCount += listingOtherCount;
            return listingBaseCount;
        }

    }
}
