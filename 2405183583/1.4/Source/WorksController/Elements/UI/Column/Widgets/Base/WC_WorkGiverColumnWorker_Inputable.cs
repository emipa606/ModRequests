using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorksController
{
    public abstract class WC_WorkGiverColumnWorker_Inputable<T, Param> : WC_WorkGiverColumnWorker where T : WC_DataObject
    {
        protected virtual void SetValueAndNotify(T data, Param value)
        {
            SetValue(data, value);
            WC_DataContext.NotifyValueUpdated(data);
        }

        protected virtual void NotifyOnly(T data)
        {
            WC_DataContext.NotifyValueUpdated(data);
        }

        protected abstract Param GetValue(T data);

        protected abstract void SetValue(T data, Param value);


    }
}
