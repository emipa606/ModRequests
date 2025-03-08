using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
