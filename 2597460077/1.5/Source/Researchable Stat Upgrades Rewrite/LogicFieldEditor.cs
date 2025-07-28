using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ResearchableStatUpgrades
{
    public class LogicFieldEditor
    {
        //kd8lvt here: Yay more reflection... >.>
        public LogicFieldEditor(FieldInfo address, object valueFalse, object valueTrue, object instance)
        {
            this.instance = instance;
            this.address = address;
            this.valueFalse = valueFalse;
            this.valueTrue = valueTrue;
        }

        public void Resolve(bool b)
        {
            this.address.SetValue(this.instance, b ? this.valueTrue : this.valueFalse);
        }

        public readonly FieldInfo address;
        public readonly object valueFalse;
        public readonly object valueTrue;
        public readonly object instance;
    }
}
