using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResearchableStatUpgrades
{
	public class ResearchMod_EditDef : ResearchMod_SingleRegisterable
    {
        public override void Register(WorldComponent_DefEditingResearchManager comp)
		{
			base.WorldComp = comp;
			FieldInfo fieldInfo = this.def.GetType().GetFields(RSUUtil.universal).ToList<FieldInfo>().Find((FieldInfo field) => field.Name == this.fieldName);
			bool flag = fieldInfo == null;
			if (flag)
			{
				Log.Error(string.Format("Cannot find field {0} in Def {1}", this.fieldName, this.def.defName));
			}
			LogicFieldEditor editor = new LogicFieldEditor(fieldInfo, fieldInfo.GetValue(this.def), fieldInfo.Parse(this.value), this.def);
			base.Editor = editor;
			base.WorldComp.AddEditor(base.Editor, false);
		}

		public Def def;
		public string fieldName;
		public string value;
    }
}
