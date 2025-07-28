using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResearchableStatUpgrades
{
	public class ResearchMod_EditVerbProperties : ResearchMod_SingleRegisterable
	{
		public override void Register(WorldComponent_DefEditingResearchManager comp)
		{
			base.WorldComp = comp;
			FieldInfo field = typeof(VerbProperties).GetField(this.fieldName, RSUUtil.universal);
			LogicFieldEditor editor = new LogicFieldEditor(field, field.GetValue(this.def.Verbs[this.index]), field.Parse(this.value), this.def.Verbs[this.index]);
			base.Editor = editor;
			base.WorldComp.AddEditor(base.Editor, false);
		}

		public ThingDef def;
		public int index;
		public string fieldName;
		public string value;
	}
}
