using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using static CherryPicker.CherryPickerUtility;
 
namespace CherryPicker
{
	[StaticConstructorOnStartup]
    internal static class DefUtility
	{
		public static Assembly rootAssembly;
		public static Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
		static HashSet<string> assumedNameSpaces = new HashSet<string>() {"Verse", "RimWorld"};
		static Dictionary<string, Assembly> assemblyCache = new Dictionary<string, Assembly>();

        static DefUtility()
		{
			rootAssembly = typeof(ThingDef).Assembly;
			typeCache.Add("DefList", typeof(DefList));
		}

		public static string ToKey(this Def def)
		{
			if (def == null) return "";
			Type type = def.GetType();
			
			string key = type.Name + "/" + def.defName;
            string nameSpace = type.Namespace;
            if (nameSpace == null || assumedNameSpaces.Contains(type.Namespace))
                return key;

            if (!assemblyCache.ContainsKey(nameSpace))
				assemblyCache.Add(nameSpace, type.Assembly);

            return key + "/" + nameSpace;
        }
		public static string ToDefName(this string def)
		{
			return def.Split('/')[1];
		}
		public static Type ToType(this string typeName, bool literal = false)
		{
			string nameSpace = "";
			if (!literal) 
			{
				var elements = typeName.Split('/');
				typeName = elements[0];
				
				if (elements.Length > 2) nameSpace = elements[2];
			}

			Type type;
			//If the namespace is specified, start there
			if (!nameSpace.NullOrEmpty())
			{
				//Check the cache for modded types
                if (typeCache.TryGetValue(typeName, out type) && type.Namespace == nameSpace)
                {
                    return type;
                }

				//Check the cache of assemblies by namespace
                if (assemblyCache.TryGetValue(nameSpace, out var assembly))
                {
                    type = assembly.GetType(nameSpace + "." + typeName);
				    if (type != null) return type;
                }

				//If all else fails, try vanilla
				type = rootAssembly.GetType(nameSpace + "." + typeName);
				if (type != null) return type;
			}
			
			//Fast handling for vanilla types
			type = rootAssembly.GetType("Verse." + typeName) ?? rootAssembly.GetType("RimWorld." + typeName);
			if (type != null) return type;
			
			//Check the cache for modded types
			if (typeCache.TryGetValue(typeName, out type))
			{
				return type;
			}

			return null;
		}
		public static string ToTypeString(this string def)
		{
			return def.Split('/')[0];
		}
		public static Def GetDef(string defName, Type type)
		{
			if (defName.NullOrEmpty() || type == null) return null;

			Def def = (Def)GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), type, nameof(DefDatabase<Def>.GetNamed), defName, false);
			if (def == null)
			{
				foreach (Def hardRemovedDef in processedDefs)
				{
					if (hardRemovedDef?.defName == defName && hardRemovedDef.GetType() == type) return hardRemovedDef;
				}
			}
			
			return def;
		}
	}
}