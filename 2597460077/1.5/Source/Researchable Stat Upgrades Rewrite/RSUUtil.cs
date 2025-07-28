using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace ResearchableStatUpgrades
{
    public static class RSUUtil
    {
        public static WorldComponent_DefEditingResearchManager DefEditingResearchManager
        {
            get
            {
                return Find.World.GetComponent<WorldComponent_DefEditingResearchManager>();
            }
        }

        public static WorldComponent_RepeatableResearchManager RepeatableResearchManager
        {
            get
            {
                return Find.World.GetComponent<WorldComponent_RepeatableResearchManager>();
            }
        }

		public static bool IsInst(this Type t, Type a)
		{
            return t.IsSubclassOf(a) || t == a;
		}

		public static bool IsInst(this Type t, params Type[] types)
		{
			return t != null && types.Any((Type t2) => t.IsInst(t2));
		}

		public static ResearchMod GetResearchMod(this ResearchProjectDef def, Type type)
		{
			List<ResearchMod> list = (List<ResearchMod>)researchModsField.GetValue(def);
			return list?.Find((ResearchMod m) => m.GetType().IsInst(type));
		}

		public static T GetResearchMod<T>(this ResearchProjectDef def) where T : ResearchMod
		{
			return (T)(def.GetResearchMod(typeof(T)));
		}

		public static bool IsRepeatableResearch(this ResearchProjectDef def)
		{
			return def.GetResearchMod<ResearchMod_Repeatable>() != null;
		}

		public static void LoadAndEditField(this FieldInfo fieldInfo, string value, object instance)
		{
			object value2 = fieldInfo.Parse(value);
			fieldInfo.SetValue(instance, value2);
		}

		public static object Parse(this FieldInfo fieldInfo, string value)
		{
			object result;
			try
			{
				object obj;
				if (fieldInfo.FieldType.IsSubclassOf(typeof(Def)))
				{
					obj = GenDefDatabase.GetDef(fieldInfo.FieldType, value, true);
				}
				else
				{
					obj = ParseHelper.FromString(value, fieldInfo.FieldType);
				}
				result = obj;
			}
			catch (Exception ex)
			{
				string str = "Researchable Stat Upgrades :: Exception parsing string: ";
                throw new Exception(str + (ex?.ToString()));
			}
			return result;
		}

		public static readonly BindingFlags universal = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;
		public static readonly FieldInfo researchModsField = typeof(ResearchProjectDef).GetField("researchMods", universal);
	}
}