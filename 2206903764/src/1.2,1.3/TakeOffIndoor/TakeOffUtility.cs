using HarmonyLib;
using RimWorld.Planet;
using System;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Garam_RaceAddon;
using System.IO;
using UnityEngine;


namespace TakeOffIndoor
{
    public static class Util
    {
        //static Type NullType = null;

        public static TakeOffMod mod;
        public static bool TypeIsExistsInAssembly(string AssemblyName, string TypeNameFull, ref Type TypeBuffer)
        {
            Type t = LoadTypeInAssembly(AssemblyName, TypeNameFull);
            if (t != null)
            {
                TypeBuffer = t;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Type LoadTypeInAssembly(string AssemblyName, string TypeNameFull)
        {
            Assembly asm = Assembly.Load(AssemblyName);
            //Type t = asm.GetType(TypeNameFull);
            Type t = AccessTools.TypeByName(TypeNameFull);
            if (asm != null)
            {
                if (t != null)
                {
                    debug.WriteLine(AssemblyName + " is Loaded.");
                    return t;
                }
                else
                {
                    debug.WriteLine(AssemblyName + " type Not Found.");
                }
                return null;
            }
            else
            {
                debug.WriteLine(AssemblyName + " is Nothing.");
                return null;
            }

        }

        public static HarmonyMethod HM(Type t, string MethodName)
        {
            return new HarmonyMethod(t.GetMethod(MethodName));
        }
        public static uint bitOn(bool bit, int digit)
        {
            return bit ? ((uint)1 << digit) : 0;
        }
        public static uint bitOn(uint bit, int digit)
        {
            return bit << digit;
        }

        public static ulong bitOnLong(bool bit, int digit)
        {
            return bit ? ((ulong)1 << digit) : 0;
        }
        public static ulong bitOnLong(ulong bit, int digit)
        {
            return bit << digit;
        }

        public static bool TempSaveInt(string fileName, int value)
        {
            try
            {
                StreamWriter sw = new StreamWriter(fileName, false);
                sw.WriteLine(value);
                sw.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public static bool TempLoadInt(string fileName, out int value, bool delete = false)
        {
            value = 0;
            try
            {
                if (File.Exists(fileName))
                {
                    StreamReader sr = new StreamReader(fileName, false);
                    value = Int32.Parse(sr.ReadLine());
                    sr.Close();

                    if (delete)
                    {
                        File.Delete(fileName);
                    }
                }
                else
                {
                    return false;
                }

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool GetLeftClick()
        {
            return Event.current.type == EventType.MouseDown && Event.current.button == 0;
        }
        public static bool GetRightClick()
        {
            return Event.current.type == EventType.MouseDown && Event.current.button == 1;
        }


        public static bool AddOrWrite<TKey, TValue>(this Dictionary<TKey, TValue> dic,TKey key,TValue value)
        {
            if (dic.ContainsKey(key))
            {
                dic.Add(key, value);
                return true;
            }
            else
            {
                dic[key] = value;
                return false;
            }
        }

        public static void AddRangeFromList<TKey, TValue>(this Dictionary<TKey,TValue> dic, List<TKey> list,TValue value)
        {
            if (list != null)
            {
                foreach(TKey key in list)
                {
                    dic.AddOrWrite(key, value);
                }
            }
        }

        public static bool HasGraphic(ThingDef thingDef)
        {
            return RenderUtil.HasWornGraphicPath(thingDef.apparel);
        }

        public static TEnum ParseEnum<TEnum>(int num)
        {
            if(Enum.IsDefined(typeof(TEnum),num))
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), num);
            }
            else
            {
                return default(TEnum);
            }
        }
    }

    public static class ModUtil
    {

        public static bool ExistAppearanceClothes = false;
        public static bool ExistGaramRace = false;
        public static bool ExistShowHair = false;
        public static bool ExistFactionColors = false;
        public static bool ExistHAR = false;
        public static bool ExistJetPack = false;
        public static Type ShowHair_RPI;
        public static Type RaceAddonThingDef;
        public static Type AppearanceClothes_RGP;
        public static Type FactionColors_HP;
        public static Type AlienRace_APG;
        public static Type JetPack_RAG_JPPP;

        public static void GetOtherDllExists()
        {
            //chk FactionColors
            ExistFactionColors = Util.TypeIsExistsInAssembly("FactionColors", "FactionColors.HarmonyPatches", ref FactionColors_HP);

            //chk AppearanceClothes
            ExistAppearanceClothes = Util.TypeIsExistsInAssembly("AppearanceClothes", "AppearanceClothes.PawnGraphicSet_ResolveApparelGraphics_Patch", ref AppearanceClothes_RGP);

            //chk ShowHair
            ExistShowHair = Util.TypeIsExistsInAssembly("ShowHair", "ShowHair.Patch_PawnRenderer_RenderPawnInternal", ref ShowHair_RPI);

            //chk GaramRace
            ExistGaramRace = Util.TypeIsExistsInAssembly("Garam_RaceAddon", "Garam_RaceAddon.RaceAddonThingDef", ref RaceAddonThingDef);

            //chk HAR
            ExistHAR = Util.TypeIsExistsInAssembly("AlienRace", "AlienRace.AlienPartGenerator+BodyAddon", ref AlienRace_APG);

            //chk JetPack
            ExistJetPack = Util.TypeIsExistsInAssembly("JetPack", "JetPack.ResolveApparelGraphics_JPPostPatch", ref JetPack_RAG_JPPP);
        }

        public static bool PawnIsGaramRace(Pawn pawn)
        {
            if (ExistGaramRace)
            {
                return _IsGaramRace(pawn.def);
            }
            return false;
        }

        public static bool ThingIsGaramRace(ThingDef raceDef)
        {
            if (ExistGaramRace)
            {
                return _IsGaramRace(raceDef);
            }
            return false;
        }


        private static bool _IsGaramRace(ThingDef raceDef)
        {
            if (raceDef is RaceAddonThingDef)
            {
                return true;
            }
            return false;
        }

    }

}
