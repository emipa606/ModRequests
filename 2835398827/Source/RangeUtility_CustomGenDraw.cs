using RimWorld;
using RimWorld.Planet;
using Verse;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShardMods
{
    [StaticConstructorOnStartup]
    public static class GenDraw_Custom
    {
        private static readonly Material TargetSquareMatSingle = MaterialPool.MatFrom("UI/Overlays/TargetHighlight_Square", ShaderDatabase.Transparent); // Keep
        private static List<IntVec3> ringDrawCells = new List<IntVec3>(); // Keep
        private static bool maxRadiusMessaged = false; // Keep
        private static bool[] rotNeeded = new bool[4]; // Keep
        private static BoolGrid fieldGrid; // Keep

        public static Material CurTargetingMat // Keep
        {
            get
            {
                GenDraw_Custom.TargetSquareMatSingle.color = GenDraw_Custom.CurTargetingColor;
                return GenDraw_Custom.TargetSquareMatSingle;
            }
        }

        public static Color CurTargetingColor // Keep
        {
            get
            {
                float num = (float)Math.Sin((double)Time.time * 8.0) * 0.2f + 0.8f;
                return new Color(1f, num, num);
            }
        }


        public static void DrawTargetHighlight(LocalTargetInfo targ) // Keep
        {
            if (targ.Thing != null)
                GenDraw_Custom.DrawTargetingHighlight_Thing(targ.Thing);
            else
                GenDraw_Custom.DrawTargetingHighlight_Cell(targ.Cell);
        }

        private static void DrawTargetingHighlight_Cell(IntVec3 c) // Keep
        {
            GenDraw_Custom.DrawTargetHighlightWithLayer(c, AltitudeLayer.Building);
        }

        public static void DrawTargetHighlightWithLayer(IntVec3 c, AltitudeLayer layer) // Keep
        {
            Vector3 shiftedWithAltitude = c.ToVector3ShiftedWithAltitude(layer);
            Graphics.DrawMesh(MeshPool.plane10, shiftedWithAltitude, Quaternion.identity, GenDraw_Custom.CurTargetingMat, 0);
        }

        public static void DrawTargetHighlightWithLayer(Vector3 c, AltitudeLayer layer) // Keep
        {
            Vector3 position = new Vector3(c.x, layer.AltitudeFor(), c.z);
            Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, GenDraw_Custom.CurTargetingMat, 0);
        }

        private static void DrawTargetingHighlight_Thing(Thing t) // Keep
        {
            Graphics.DrawMesh(MeshPool.plane10, t.TrueCenter() + Altitudes.AltIncVect, t.Rotation.AsQuat, GenDraw_Custom.CurTargetingMat, 0);
        }

        public static void DrawRadiusRing(
          IntVec3 center,
          float radius,
          Color color,
          Func<IntVec3, bool> predicate = null)
        {
            if ((double)radius > (double)GenRadial_Custom.MaxRadialPatternRadius)
            {
                if (GenDraw_Custom.maxRadiusMessaged)
                    return;
                Log.Error("Cannot draw radius ring of radius " + (object)radius + ": not enough squares in the precalculated list.");
                GenDraw_Custom.maxRadiusMessaged = true;
            }
            else
            {
                GenDraw_Custom.ringDrawCells.Clear();
                int num = GenRadial_Custom.NumCellsInRadius(radius);
                for (int index = 0; index < num; ++index)
                {
                    IntVec3 intVec3 = center + GenRadial_Custom.RadialPattern[index];
                    if (predicate == null || predicate(intVec3))
                        GenDraw_Custom.ringDrawCells.Add(intVec3);
                }
                GenDraw_Custom.DrawFieldEdges(GenDraw_Custom.ringDrawCells, color, new float?());
            }
        }

        public static void DrawRadiusRing(IntVec3 center, float radius)
        {
            GenDraw_Custom.DrawRadiusRing(center, radius, Color.white, (Func<IntVec3, bool>)null);
        }

        public static void DrawFieldEdges(List<IntVec3> cells)
        {
            GenDraw_Custom.DrawFieldEdges(cells, Color.white, new float?());
        }

        public static void DrawFieldEdges(List<IntVec3> cells, Color color, float? altOffset = null)
        {
            Map currentMap = Find.CurrentMap;
            Material material = MaterialPool.MatFrom(new MaterialRequest()
            {
                shader = ShaderDatabase.Transparent,
                color = color,
                BaseTexPath = "UI/Overlays/TargetHighlight_Side"
            });
            material.GetTexture("_MainTex").wrapMode = TextureWrapMode.Clamp;
            if (GenDraw_Custom.fieldGrid == null)
                GenDraw_Custom.fieldGrid = new BoolGrid(currentMap);
            else
                GenDraw_Custom.fieldGrid.ClearAndResizeTo(currentMap);
            int x = currentMap.Size.x;
            int z = currentMap.Size.z;
            int count = cells.Count;
            float? nullable = altOffset;
            float y = nullable.HasValue ? nullable.GetValueOrDefault() : (float)((double)Rand.ValueSeeded(color.ToOpaque().GetHashCode()) * 0.0405405387282372 / 10.0);
            for (int index = 0; index < count; ++index)
            {
                if (cells[index].InBounds(currentMap))
                    GenDraw_Custom.fieldGrid[cells[index].x, cells[index].z] = true;
            }
            for (int index = 0; index < count; ++index)
            {
                IntVec3 cell = cells[index];
                if (cell.InBounds(currentMap))
                {
                    GenDraw_Custom.rotNeeded[0] = cell.z < z - 1 && !GenDraw_Custom.fieldGrid[cell.x, cell.z + 1];
                    GenDraw_Custom.rotNeeded[1] = cell.x < x - 1 && !GenDraw_Custom.fieldGrid[cell.x + 1, cell.z];
                    GenDraw_Custom.rotNeeded[2] = cell.z > 0 && !GenDraw_Custom.fieldGrid[cell.x, cell.z - 1];
                    GenDraw_Custom.rotNeeded[3] = cell.x > 0 && !GenDraw_Custom.fieldGrid[cell.x - 1, cell.z];
                    for (int newRot = 0; newRot < 4; ++newRot)
                    {
                        if (GenDraw_Custom.rotNeeded[newRot])
                            Graphics.DrawMesh(MeshPool.plane10, cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays) + new Vector3(0.0f, y, 0.0f), new Rot4(newRot).AsQuat, material, 0);
                    }
                }
            }
        }
    }


    public static class GenRadial_Custom
    {
        public static IntVec3[] RadialPattern = new IntVec3[100000]; // Keep
        private static float[] RadialPatternRadii = new float[100000]; // Keep
        private const int RadialPatternCount = 100000; // Keep

        public static float MaxRadialPatternRadius // Keep
        {
            get
            {
                return GenRadial_Custom.RadialPatternRadii[GenRadial_Custom.RadialPatternRadii.Length - 1];
            }
        }

        static GenRadial_Custom() // Keep
        {
            GenRadial_Custom.SetupRadialPattern();
        }


        private static void SetupRadialPattern() // Keep
        {
            List<IntVec3> intVec3List = new List<IntVec3>();
            for (int newX = -160; newX < 160; ++newX)
            {
                for (int newZ = -160; newZ < 160; ++newZ)
                    intVec3List.Add(new IntVec3(newX, 0, newZ));
            }
            intVec3List.Sort((Comparison<IntVec3>)((A, B) =>
            {
                float horizontalSquared1 = (float)A.LengthHorizontalSquared;
                float horizontalSquared2 = (float)B.LengthHorizontalSquared;
                if ((double)horizontalSquared1 < (double)horizontalSquared2)
                    return -1;
                return (double)horizontalSquared1 == (double)horizontalSquared2 ? 0 : 1;
            }));
            for (int index = 0; index < 100000; ++index)
            {
                GenRadial_Custom.RadialPattern[index] = intVec3List[index];
                GenRadial_Custom.RadialPatternRadii[index] = intVec3List[index].LengthHorizontal;
            }
        }


        public static int NumCellsInRadius(float radius) // Keep
        {
            if ((double)radius >= (double)GenRadial_Custom.MaxRadialPatternRadius)
            {
                Log.Error("Not enough squares to get to radius " + (object)radius + ". Max is " + (object)GenRadial_Custom.MaxRadialPatternRadius);
                return 100000;
            }
            float num = radius + float.Epsilon;
            for (int index = 0; index < 100000; ++index)
            {
                if ((double)GenRadial_Custom.RadialPatternRadii[index] > (double)num)
                    return index;
            }
            return 100000;
        }

        public static float RadiusOfNumCells(int numCells)
        {
            return GenRadial_Custom.RadialPatternRadii[numCells];
        }

        public static IEnumerable<IntVec3> RadialPatternInRadius(float radius)
        {
            int numSquares = GenRadial_Custom.NumCellsInRadius(radius);
            for (int i = 0; i < numSquares; ++i)
                yield return GenRadial_Custom.RadialPattern[i];
        }

        public static IEnumerable<IntVec3> RadialCellsAround(
          IntVec3 center,
          float radius,
          bool useCenter)
        {
            int numSquares = GenRadial_Custom.NumCellsInRadius(radius);
            for (int i = useCenter ? 0 : 1; i < numSquares; ++i)
                yield return GenRadial_Custom.RadialPattern[i] + center;
        }

        public static IEnumerable<IntVec3> RadialCellsAround(
          IntVec3 center,
          float minRadius,
          float maxRadius)
        {
            int numSquares = GenRadial_Custom.NumCellsInRadius(maxRadius);
            for (int i = 0; i < numSquares; ++i)
            {
                if ((double)GenRadial_Custom.RadialPattern[i].LengthHorizontal >= (double)minRadius)
                    yield return GenRadial_Custom.RadialPattern[i] + center;
            }
        }

        public static IEnumerable<Thing> RadialDistinctThingsAround(
          IntVec3 center,
          Map map,
          float radius,
          bool useCenter)
        {
            int numCells = GenRadial_Custom.NumCellsInRadius(radius);
            HashSet<Thing> returnedThings = (HashSet<Thing>)null;
            for (int i = useCenter ? 0 : 1; i < numCells; ++i)
            {
                IntVec3 c = GenRadial_Custom.RadialPattern[i] + center;
                if (c.InBounds(map))
                {
                    List<Thing> thingList = c.GetThingList(map);
                    for (int j = 0; j < thingList.Count; ++j)
                    {
                        Thing thing = thingList[j];
                        if (thing.def.size.x > 1 || thing.def.size.z > 1)
                        {
                            if (returnedThings == null)
                                returnedThings = new HashSet<Thing>();
                            if (!returnedThings.Contains(thing))
                                returnedThings.Add(thing);
                            else
                                continue;
                        }
                        yield return thing;
                    }
                    thingList = (List<Thing>)null;
                }
            }
        }

    }


}
