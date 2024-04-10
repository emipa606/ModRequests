using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace BetterInfestations
{
    public class GenStep_Infestation : GenStep
    {
        public override int SeedPart => 62568453;

        public override void Generate(Map map, GenStepParams parms)
        {
            CellRect mapRegion = new CellRect(0, 0, map.Size.x, map.Size.z);
            mapRegion.ClipInsideMap(map);
            BaseGen.globalSettings.map = map;
            CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) => v.Standable(map), map, 0f, out IntVec3 result);
            MapGenerator.PlayerStartSpot = result;
            ResolveParams resolveParams = default;
            resolveParams.rect = mapRegion;
            resolveParams.faction = Faction.OfInsects;
            BaseGen.symbolStack.Push("BI_Infestation_Symbol", resolveParams);
            BaseGen.Generate();
        }
    }
}