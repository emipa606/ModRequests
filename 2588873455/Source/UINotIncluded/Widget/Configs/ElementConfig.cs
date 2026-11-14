using System;
using Verse;

namespace UINotIncluded.Widget.Configs
{
    public class ElementConfig : IExposable
    {
        public virtual bool Configurable => false;

        public virtual bool Repeatable => false;

        public virtual WidgetWorker Worker => throw new NotImplementedException();

        public virtual string SettingLabel => "undefined";

        public virtual void ExposeData()
        {
        }

        public virtual bool Equivalent(ElementConfig other)
        {
            return GetType() == other.GetType();
        }

        public virtual void Reset()
        {
        }
    }
}