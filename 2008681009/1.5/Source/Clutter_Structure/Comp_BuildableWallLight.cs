using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using System.Reflection;

namespace Clutter_Structure
{
    public class Comp_BuildableWallLight : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }
        public bool AllowsPlacing()
        {            
                foreach (Thing thing in this.parent.Position.GetThingList(this.parent.Map))
                {                   
                    if (thing.def.IsBlueprint)
                    {
                        return false;
                    }
                }
            if (!ResearchProjectDef.Named("ColoredLights").IsFinished)
            {
                return false;
            }
                        return true;
        }

        public void StuffPlacment()
        {
            ThingDef Thingie = ThingDef.Named("ClutterUWallLight");

            int stuffint = this.parent.def.costStuffCount;

            ThingDef stuffdef = this.parent.Stuff;

            List<Thing> stuffInCell = this.parent.Position.GetThingList(this.parent.Map);

            foreach (Thing t in stuffInCell)
            {
                if (t.def.IsBlueprint)
                {
                    Blueprint_Build bP = t as Blueprint_Build;
                    bP.Destroy();
                    break;
                }
            }
            if (Thingie.blueprintDef == null)
            {
                Log.Message("no blueprint def");
            }
            Blueprint_Build blueprint = GenConstruct.PlaceBlueprintForBuild(Thingie, this.parent.Position, this.parent.Map, this.parent.Rotation, Faction.OfPlayer, stuffdef);
            this.parent.Destroy();

        }

        public virtual void HandlePopup()
        {

            ThingDef BP = ThingDef.Named("ClutterUWallLight");
            StringBuilder stringBuilder = new StringBuilder();
            if (this.parent.def.MadeFromStuff)
            {
                stringBuilder.AppendLine(parent.Stuff.LabelCap + ": " + BP.costStuffCount);
            }
            if (BP.costList != null)
            {
                for (int c = 0; c < BP.costList.Count; c++)
                {
                    stringBuilder.AppendLine(BP.costList.ElementAt(c).thingDef.LabelCap + ": " + BP.costList.ElementAt(c).count);
                }
            }
            string text = null;//"ClutterStructureUpgradeText".Translate(new object[]
      //{        
      //  BP.LabelCap,
      //  stringBuilder,
      //});
            DiaNode diaNode = new DiaNode(text);
            DiaOption diaOption = new DiaOption("ClutterStructureAccept".Translate());
            diaOption.action = delegate
            {
                StuffPlacment();
            };
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            string text2 = "ClutterStructureCancel".Translate();
            DiaOption diaOption2 = new DiaOption(text2);
            diaOption2.resolveTree = true;
            diaNode.options.Add(diaOption2);

            Find.WindowStack.Add(new Dialog_NodeTree(diaNode));
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            if (AllowsPlacing())
            {

                IEnumerator<Gizmo> enumerator = base.CompGetGizmosExtra().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Gizmo current = enumerator.Current;

                    yield return current;
                }

                if (this.parent.Faction == Faction.OfPlayer)
                {

                    yield return new Command_Action
                    {
                        action = delegate
                        {
                            HandlePopup();
                        },
                        hotKey = KeyBindingDefOf.Misc1,
                        icon = ContentFinder<Texture2D>.Get("Clutter/Ui/Work_ico", true),
                        defaultLabel = "ClutterButtonMountLights".Translate()
                    };

                }
            }
            yield break;

        }
    }
}
