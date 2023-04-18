using Verse;

namespace SimpleWarrants
{
    public class GenStep_Animal : GenStep
	{
        public override int SeedPart => 543215473;

        public override void Generate(Map map, GenStepParams parms)
		{
			var pawn = (Pawn)parms.sitePart.things.Take(parms.sitePart.things[0]);
			var cell = CellFinder.RandomClosewalkCellNear(map.Center, map, 20);
			GenSpawn.Spawn(pawn, cell, map);
		}
    }
}