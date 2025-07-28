using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResearchableStatUpgrades
{
    public class ResearchMod_EditCompProperties : ResearchMod_SingleRegisterable
    {
        public override void Register(WorldComponent_DefEditingResearchManager comp)
        {
            base.WorldComp = comp;
            FieldInfo field = this.type.GetField(this.fieldName, RSUUtil.universal);
            CompProperties compProperties = this.def.comps.Find((CompProperties c) => c.GetType() == this.type || c.GetType().IsSubclassOf(this.type));
            if (compProperties == null)
            {
                Log.Error("CompProperties type " + this.type.FullName + " was not found.");
            }
            else
            {
                LogicFieldEditor editor = new LogicFieldEditor(field, field.GetValue(compProperties), field.Parse(this.value), compProperties);
                base.Editor = editor;
                base.WorldComp.AddEditor(base.Editor, false);
            }
        }

        public ThingDef def;
        public Type type;
        public string fieldName;
        public string value;
    }
}
