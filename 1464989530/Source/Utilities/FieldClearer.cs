#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using System.Collections.Generic;
using Verse;

namespace NightVision
{
    public static class FieldClearer
    {

        public static List<Traverse> SettingsDependentFieldTraverses = new List<Traverse>();

        public static void FindSettingsDependentFields()
        {
#if DEBUG

            Log.Message("Finding settings dependant fields");
#endif
            var traverses = new List<Traverse>();

            var markedTypes = GenTypes.AllTypesWithAttribute<NVHasSettingsDependentFieldAttribute>();

            foreach (var type in markedTypes)
            {
                var fields = AccessTools.GetDeclaredFields(type)
                    .FindAll(fi => fi.HasAttribute<NVSettingsDependentFieldAttribute>());

#if DEBUG

                Log.Message($"Type: {type}");
#endif
                foreach (var info in fields)
                {
                    var traverse = new Traverse(type);
                    traverse = traverse.Field(info.Name);


#if DEBUG

                    Log.Message($"Field: {info.Name}");
#endif

                    traverses.Add(traverse);
                }
            }


            SettingsDependentFieldTraverses = traverses;
        }


        public static void ResetSettingsDependentFields()
        {
            try
            {
                if (SettingsDependentFieldTraverses.Count == 0)
                {
#if DEBUG

                    Log.Message("No fields to reset found.");
#endif
                    return;
                }

                foreach (var fieldTraverse in SettingsDependentFieldTraverses)
                {
                    if (!fieldTraverse.FieldExists())
                    {
                        Log.Warning($"SettingsDependentFieldTraverses included a field that did not exist.");
                        continue;
                    }

                    var type = fieldTraverse.GetValueType();

                    if (type == typeof(TriBool))
                    {
                        fieldTraverse.SetValue(TriBool.Undefined);
                    }
                    else if (type == typeof(int))
                    {
                        fieldTraverse.SetValue(-9999);
                    }
                    else if (type == typeof(float))
                    {
                        fieldTraverse.SetValue(-9999f);
                    }
                    else if (type.IsClass)
                    {
                        fieldTraverse.SetValue(null);
                    }
                    else
                    {
                        Log.Warning(
                            $"FieldClearer: unsupported settings type. {fieldTraverse.GetValueType()}, {fieldTraverse.GetValue()}");
                    }

                }
            }
            catch
            {

            }
        }
    }
}