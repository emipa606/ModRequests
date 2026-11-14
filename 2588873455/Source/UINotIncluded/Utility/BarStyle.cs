using UnityEngine;
using Verse;

namespace UINotIncluded
{
    public abstract class BarStyle
    {
        public virtual string Name => "";

        public abstract void DoToolbarBackground(Rect rect);

        public abstract void DoWidgetBackground(Rect rect);
    }

    public class BarStyle_RustyOrange : BarStyle
    {
        public override string Name => "Rusty Orange";

        public override void DoToolbarBackground(Rect rect)
        {
            Widgets.DrawAtlas(rect, ModTextures.toolbarBackground);
        }

        public override void DoWidgetBackground(Rect rect)
        {
            Widgets.DrawAtlas(rect.ContractedBy(0f, 2f), ModTextures.toolbarWidgetBackground);
        }
    }

    public class BarStyle_VanillaBlue : BarStyle
    {
        public override string Name => "Vanilla Blue";

        public override void DoToolbarBackground(Rect rect)
        {
            Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
        }

        public override void DoWidgetBackground(Rect rect)
        {
        }
    }

    public class BarStyle_VanillaBluePlus : BarStyle
    {
        public override string Name => "Vanilla Blue Plus";

        public override void DoToolbarBackground(Rect rect)
        {
            Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
        }

        public override void DoWidgetBackground(Rect rect)
        {
            Widgets.DrawAtlas(rect.ContractedBy(0f, 2f), ModTextures.toolbarWidgetBackground);
        }
    }
}