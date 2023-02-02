using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FixedIdeoStyle
{
    [StaticConstructorOnStartup]
    public class Command_SetStyle : Command
    {
        private static readonly FieldInfo cachedGraphic = AccessTools.Field(typeof(Blueprint_Build), "cachedGraphic");

        private readonly Thing thing;
        private readonly ThingDef thingDef;
        private readonly List<ThingStyleDef> styles;

        public Command_SetStyle(Thing thing, ThingDef thingDef, List<ThingStyleDef> styles)
        {
            this.thing = thing;
            this.thingDef = thingDef;
            this.styles = styles;

            this.defaultLabel = "Styles".Translate().CapitalizeFirst();
            this.icon = thingDef.uiIcon;
            this.iconAngle = thingDef.uiIconAngle;
            this.iconDrawScale = GenUI.IconDrawScale(thingDef);
            this.iconOffset = thingDef.uiIconOffset;
            this.defaultIconColor = thingDef.uiIconColor;
            this.defaultDesc = "ChangeStyle".Translate().CapitalizeFirst();

            var style = thing.StyleDef;
            if (style != null)
            {
                this.defaultLabel = style.Category.LabelCap;
                this.icon = style.UIIcon;
            }
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var result = base.GizmoOnGUI(topLeft, maxWidth, parms);

            Designator_Dropdown.DrawExtraOptionsIcon(topLeft, this.GetWidth(maxWidth));

            return result;
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);

            var options = new List<FloatMenuOption>
            {
                new FloatMenuOption("None".Translate(), delegate
                {
                    SetThingStyle(null);
                }, this.thingDef)
            };

            options.AddRange(this.styles.Select(s => new FloatMenuOption(s.Category.LabelCap, delegate
            {
                SetThingStyle(s);
            }, s.UIIcon, this.thingDef.uiIconColor)));

            Find.WindowStack.Add(new FloatMenu(options));
        }

        public void SetThingStyle(ThingStyleDef style)
        {
            this.thing.StyleDef = style;

            if (this.thing is Blueprint_Build blueprint)
            {
                cachedGraphic.SetValue(blueprint, null);
                blueprint.DirtyMapMesh(blueprint.Map);
            }
        }
    }
}
