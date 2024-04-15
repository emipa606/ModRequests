using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace SR
{
    public static class Utilities
    {
		public static void DropThing(Map map, IntVec3 cell, ThingDef thingDef, int count, ThingPlaceMode tpm = ThingPlaceMode.Direct)
		{
			Thing thing = ThingMaker.MakeThing(thingDef);
			thing.stackCount = count;
			GenPlace.TryPlaceThing(thing, cell, map, tpm);
		}

		public static void DropThings(Map map, IntVec3 cell, List<ThingDefCountClass> thingCountList, ThingPlaceMode tpm = ThingPlaceMode.Near)
        {
			foreach (var thingCount in thingCountList)
				DropThing(map, cell, thingCount.thingDef, thingCount.count);
        }


		public static void DropThings(Map map, IntVec3 cell, List<ThingDefCountClass> thingCountList, int downwardCountVariance, int upwardCountVariance, ThingPlaceMode tpm = ThingPlaceMode.Near)
		{
			foreach (var thingCount in thingCountList)
				DropThing(map, cell, thingCount.thingDef, Rand.Range(thingCount.count - downwardCountVariance, thingCount.count + upwardCountVariance), tpm);
		}

        //public static T DeepCopy<T>(this T original) where T : class
        //{
        //    if (original == null) //If it's just null..
        //        return null; //No need to do the work, just return null.
        //    var type = typeof(T);
        //    var clone = (T)Activator.CreateInstance(type);
        //    //if (T is new()) //lol this breaks C# compiler.
        //    //    clone = new T();
        //    var properties = type.GetProperties();
        //    foreach (var pi in properties)
        //        if (pi.CanWrite)
        //        {
        //            if (pi.PropertyType.IsClass)
        //                pi.SetValue(clone, pi.GetValue(original).DeepCopy());
        //            else
        //                pi.SetValue(clone, pi.GetValue(original));
        //        }
        //    var fields = type.GetFields();
        //    foreach (var fi in fields)
        //        if (fi.FieldType.IsClass)
        //            fi.SetValue(clone, fi.GetValue(original).DeepCopy());
        //        else
        //            fi.SetValue(clone, fi.GetValue(original));
        //    return clone;
        //}
    }
}