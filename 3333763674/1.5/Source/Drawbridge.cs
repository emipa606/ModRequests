using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Drawbridges
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
    [HotSwappable]
    public class Drawbridge : Building
    {
        public Dictionary<IntVec3, TerrainDef> oldTerrains = new Dictionary<IntVec3, TerrainDef>();
        public bool raised;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!raised && !respawningAfterLoad)
            {
                foreach (var cell in this.OccupiedRect())
                {
                    oldTerrains[cell] = cell.GetTerrain(map);
                    map.terrainGrid.SetTerrain(cell, TerrainDefOf.Concrete);
                }
            }
        }

        private Graphic overlayGraphic; // Cache field for the overlay graphic

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            // Draw the base graphic
            Graphic.Draw(drawLoc, Rotation, this);

            // Draw the overlay graphic if the drawbridge is not raised
            if (!raised)
            {
                // If overlayGraphic is not yet initialized, load it
                if (overlayGraphic == null)
                {
                    string basePath = def.graphicData.texPath.Replace("_ground", "");
                    string overlayPath = basePath + "_overlay";
                    overlayGraphic = GraphicDatabase.Get<Graphic_Multi>(overlayPath, Graphic.Shader, def.graphicData.drawSize, DrawColor, DrawColorTwo);
                }
                // Draw the cached overlay graphic
                overlayGraphic.Draw(drawLoc + new Vector3(0, 5, 0), Rotation, this);
            }
        }

        public float RaiseProgress()
        {
            if (raised)
            {
                return 1f;
            }
            return 0f;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            RecoverTerrain();
            base.Destroy(mode);
        }

        private void SpawnBridge(Drawbridge newBridge)
        {
            bool wasSelected = Find.Selector.IsSelected(this);
            newBridge.HitPoints = HitPoints;
            newBridge.SetFaction(Faction);
            if (newBridge.raised)
            {
                RecoverTerrain();
            }
            var pos = Position;
            var map = Map;
            Destroy();
            GenSpawn.Spawn(newBridge, pos, map, Rotation);
            if (wasSelected)
            {
                Find.Selector.Select(newBridge);
            }
        }

        private void RecoverTerrain()
        {
            Log.Message(oldTerrains + " - " + oldTerrains.Select(x => x.Key + " - " + x.Value).ToStringSafeEnumerable());
            if (oldTerrains.Any())
            {
                foreach (var cell in this.OccupiedRect())
                {
                    Map.terrainGrid.SetTerrain(cell, oldTerrains[cell]);
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (!raised)
            {
                var openButton = new Command_Action
                {
                    defaultLabel = "Raise".Translate(),
                    defaultDesc = "RaiseDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("Drawbridges/Raise"),
                    action = delegate
                    {
                        var raised = ThingMaker.MakeThing(ThingDef.Named(def.defName + "Raised")) as Drawbridge;
                        raised.raised = true;
                        SpawnBridge(raised);
                    }
                };
                yield return openButton;
            }
            else if (raised)
            {
                var closeButton = new Command_Action
                {
                    defaultLabel = "Lower".Translate(),
                    defaultDesc = "LowerDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("Drawbridges/Lower"),
                    action = delegate
                    {
                        var lowered = ThingMaker.MakeThing(ThingDef.Named(def.defName.Replace("Raised", ""))) as Drawbridge;
                        lowered.raised = false;
                        SpawnBridge(lowered);
                    }
                };
                yield return closeButton;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref raised, "raised");
            Scribe_Collections.Look(ref oldTerrains, "oldTerrains", LookMode.Value, LookMode.Def);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                oldTerrains ??= new Dictionary<IntVec3, TerrainDef>();
            }
        }
    }
}
