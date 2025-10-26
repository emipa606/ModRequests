using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using System.Reflection;

namespace ClutterFurniture
{
    public class Comp_PopUpUpgrade : ThingComp
    {
        public ThingDef MarkTwo;
        public List<ResearchProjectDef> ResearchNeeded=new List<ResearchProjectDef>();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
 	        base.PostSpawnSetup(respawningAfterLoad);

            ReadXML();
        }

        public void ReadXML()
        {
            CompProps_Upgradable clutterCompProps = (CompProps_Upgradable)this.props as CompProps_Upgradable;
            if(clutterCompProps!=null)
            {
                if(!clutterCompProps.UpgradableDef.NullOrEmpty())
                {
                    this.MarkTwo = ThingDef.Named(clutterCompProps.UpgradableDef);
                    
                    
                }
                if(clutterCompProps.ResearchNeeded.Count>0)
                {
                    foreach(string t in clutterCompProps.ResearchNeeded)
                    {
                     if(ResearchProjectDef.Named(t)!=null)
                     {
                        ResearchNeeded.Add(ResearchProjectDef.Named(t));
                     }
                    }
                   
                }
            }
        }
        
        public bool ResearchCheck()
        {
            if (ResearchNeeded.Count>0)
            {
                foreach(ResearchProjectDef rDef in ResearchNeeded)
                {
                    if(!rDef.IsFinished)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (this.MarkTwo != null)
            {
                if (DebugSettings.godMode || ResearchCheck())
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
                                HandlePopup(this.MarkTwo);
                            },
                            hotKey = KeyBindingDefOf.Misc1,
                            icon = ContentFinder<Texture2D>.Get("Clutter/Ui/Upgrade", true),
                            defaultLabel = "ClutterFurnitureUpgradeTarget".Translate() + this.MarkTwo.LabelCap,
                            defaultDesc = "ClutterFurnitureUpgradeTarget".Translate() + this.MarkTwo.LabelCap

                        };

                    }
                }
            }
           
            yield break;
        }

        public virtual void HandlePopup(ThingDef BP)
        {           
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
            string text = "ClutterFurnitureUpgradeText".Translate(new object[]
      {        
        BP.LabelCap,
        stringBuilder,
        this.parent.def.LabelCap
      });
            DiaNode diaNode = new DiaNode(text);
            DiaOption diaOption = new DiaOption("ClutterFurnitureAccept".Translate());
            diaOption.action = delegate
            {
                ThingSpawner(BP);
            };
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            string text2 = "ClutterFurnitureeCancel".Translate();
            DiaOption diaOption2 = new DiaOption(text2);
            diaOption2.resolveTree = true;
            diaNode.options.Add(diaOption2);

            Find.WindowStack.Add(new Dialog_NodeTree(diaNode));
        }

        public void SetMissingDefs(ThingDef thingie)
        {
            if (thingie.blueprintDef == null)
            {
                MethodInfo generateBlueprint = typeof(ThingDefGenerator_Buildings).GetMethod("NewBlueprintDef_Thing", BindingFlags.NonPublic | BindingFlags.Static);
                generateBlueprint.Invoke(this, new object[] { thingie, false, null });
                thingie.blueprintDef.PostLoad();

            }
            if (thingie.frameDef == null)
            {
                MethodInfo generateFrame = typeof(ThingDefGenerator_Buildings).GetMethod("NewFrameDef_Thing", BindingFlags.NonPublic | BindingFlags.Static);
                generateFrame.Invoke(this, new object[] { thingie });
                thingie.frameDef.stuffCategories = thingie.stuffCategories;
                thingie.frameDef.PostLoad();

            }
        }

        public void ThingSpawner(ThingDef TDthing)
        {
            SetMissingDefs(TDthing);
            if (TDthing.blueprintDef != null && TDthing.frameDef != null)
            {
                foreach (Thing t in base.parent.Position.GetThingList(this.parent.Map))
                {
                    if (t.def.IsBlueprint)
                    {
                        Blueprint_Build bP = t as Blueprint_Build;
                        bP.Destroy();
                        break;
                    }
                }
                Map thizMapz = this.parent.Map;
                ThingDef stuffie = base.parent.Stuff;
                this.parent.Destroy(DestroyMode.Deconstruct);
                GenConstruct.PlaceBlueprintForBuild(TDthing, base.parent.Position, thizMapz, base.parent.Rotation, Faction.OfPlayer, stuffie);
            }
        }
    }
}
