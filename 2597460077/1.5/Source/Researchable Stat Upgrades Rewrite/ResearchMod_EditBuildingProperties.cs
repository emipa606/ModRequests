using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResearchableStatUpgrades
{
    public class ResearchMod_EditBuildingProperties : ResearchMod_SingleRegisterable
    {
        public override void Register(WorldComponent_DefEditingResearchManager comp)
        {
            base.WorldComp = comp;
            FieldInfo field = typeof(BuildingProperties).GetField(this.fieldName, RSUUtil.universal);
            LogicFieldEditor editor = new LogicFieldEditor(field, field.GetValue(this.def.building), field.Parse(this.value), this.def.building);
            base.Editor = editor;
            base.WorldComp.AddEditor(base.Editor, false);
        }

        public ThingDef def;
        public string fieldName;
        public string value;
    }
}
