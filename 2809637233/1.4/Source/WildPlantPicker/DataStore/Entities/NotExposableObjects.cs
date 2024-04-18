using System;
using UnityEngine;

namespace WildPlantPicker.DataStore.Entities
{
    internal abstract class Tripple<T1, T2, T3>
    {
        protected T1 item1;
        protected T2 item2;
        protected T3 item3;
        public Tripple(T1 item1, T2 item2, T3 item3)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
        }
    }
    internal class TargetPlantColumnConfig : Tripple<float, object, Action<TargetPlantCondition, Rect, bool>>
    {
        public float Width => this.item1;
        public object HeaderItem => this.item2;
        public Action<TargetPlantCondition, Rect, bool> BodyColumnWriterGetter => this.item3;
        public TargetPlantColumnConfig(float width, object headerItem, Action<TargetPlantCondition, Rect, bool> bodyColumnWriterGetter) : base(width, headerItem, bodyColumnWriterGetter)
        {
        }
        public bool TryGetTexture(out Texture2D tex)
        {
            tex = null;
            if (this.item2 is Texture2D)
            {
                tex = (Texture2D)this.item2;
                return true;
            }
            return false;
        }
    }
    internal class MapConditionColumnConfig : Tripple<float, object, Action<MapConditionUnit, Rect, bool>>
    {
        public float Width => this.item1;
        public object HeaderItem => this.item2;
        public Action<MapConditionUnit, Rect, bool> BodyColumnWriterGetter => this.item3;
        public bool m_ShouldShowHeaderDisabled;
        public MapConditionColumnConfig(float width, object headerItem, Action<MapConditionUnit, Rect, bool> bodyColumnWriterGetter) : base(width, headerItem, bodyColumnWriterGetter)
        {
        }
        public bool TryGetTexture(out Texture2D tex)
        {
            tex = null;
            if (this.item2 is Texture2D)
            {
                tex = (Texture2D)this.item2;
                return true;
            }
            return false;
        }
    }

}
