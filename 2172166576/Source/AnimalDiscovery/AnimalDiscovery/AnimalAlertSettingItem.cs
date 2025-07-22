using Verse;

namespace AnimalDiscovery
{
	public class AnimalAlertSettingItem : IExposable
	{
		public string Label
		{
			get
			{
				if (this.def == null)
				{
					return this.defName;
				}
				return this.def.LabelCap.ToString();
			}
		}

		public bool IsAvailable
		{
			get
			{
				return this.def != null;
			}
		}

		public AnimalAlertSettingItem()
		{
		}

		public AnimalAlertSettingItem(ThingDef def)
		{
			this.def = def;
			this.defName = def.defName;
		}

		public bool IsAlertTarget(Pawn p)
		{
			if (this.def != p.def)
			{
				return false;
			}
			if (!p.Spawned || p.Dead)
			{
				return false;
			}
			if (p.Faction != null)
			{
				return false;
			}
			if (!p.def.race.Animal)
			{
				return false;
			}
			return true;
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving && this.def != null)
			{
				this.defName = this.def.defName;
			}
			Scribe_Values.Look<string>(ref this.defName, "defName", null, false);
		}

		public bool ResolveDef()
		{
			this.def = DefDatabase<ThingDef>.GetNamed(this.defName, false);
			return this.IsAvailable;
		}

		public string defName;

		public ThingDef def;
	}
}