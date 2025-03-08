using Verse;

namespace EstupendoBase.Buildings
{
	public class ThingDef_Plinth : ThingDef
	{

	}

	public class Building_Plinth : Building
	{
		public ThingDef_Plinth Def => this.Def as ThingDef_Plinth;
	}

}
