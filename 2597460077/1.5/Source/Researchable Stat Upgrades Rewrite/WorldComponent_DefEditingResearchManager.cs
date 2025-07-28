using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace ResearchableStatUpgrades
{
	public class WorldComponent_DefEditingResearchManager : WorldComponent
	{
		public FieldInfo ResearchModField { get; }
		public World World { get; }

		public WorldComponent_DefEditingResearchManager(World world) : base(world)
		{
			this.World = world;
			FieldInfo fieldInfo = typeof(ResearchProjectDef).GetField("researchMods", RSUUtil.universal);
			this.ResearchModField = fieldInfo;
			Dictionary<string, object> cache = RSUCache.cache;
			bool flag = cache != null && cache.ContainsKey("WC_DERM_Dictionary");
			if (flag)
			{
				this.editors = (Dictionary<LogicFieldEditor, bool>)RSUCache.cache["WC_DERM_Dictionary"];
			}
			else
			{
				IEnumerable<ResearchMod> enumerable = DefDatabase<ResearchProjectDef>.AllDefs.SelectMany((ResearchProjectDef d) => ((List<ResearchMod>)fieldInfo.GetValue(d)) ?? new List<ResearchMod>());
				foreach (ResearchMod researchMod in enumerable)
				{
					IRegisterable registerable = researchMod as IRegisterable;
					bool flag2 = registerable != null;
					if (flag2)
					{
						registerable.Register(this);
					}
				}
			}
		}

		~WorldComponent_DefEditingResearchManager()
		{
			RSUCache.TryChangeOrAddValue("WC_DERM_Dictionary", this.editors);
		}

		public void AddEditor(LogicFieldEditor a, bool b)
		{
			this.editors.Add(a, b);
		}

		public void SetEditorValue(LogicFieldEditor a, bool b)
		{
			bool flag = !this.editors.ContainsKey(a);
			if (flag)
			{
				Log.Error("LogicFieldEditor was not found in the dictionary.");
			}
			else
			{
				this.editors[a] = b;
				bool flag2 = this.initialized;
				if (flag2)
				{
					a.Resolve(b);
				}
			}
		}

		public override void FinalizeInit()
		{
			base.FinalizeInit();
			this.ResearchFinalizeInit();
		}

		private void ResearchFinalizeInit()
		{
			foreach (KeyValuePair<LogicFieldEditor, bool> keyValuePair in this.editors)
			{
				keyValuePair.Key.Resolve(keyValuePair.Value);
			}
			this.initialized = true;
		}

		private bool initialized;
		private Dictionary<LogicFieldEditor, bool> editors = new Dictionary<LogicFieldEditor, bool>();
	}
}