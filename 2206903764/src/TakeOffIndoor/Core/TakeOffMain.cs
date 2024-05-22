using HarmonyLib;
using RimWorld.Planet;
using System;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Garam_RaceAddon;
using UnityEngine;

namespace TakeOffIndoor
{
    [StaticConstructorOnStartup]
    public static class TakeOffMain
    {
        public static Texture infoTex;
        public static Texture2D filterIcon;
        static TakeOffMain()
        {
            Harmony harmony = new Harmony("rimworld.TakeOffindoor");

            debug.WriteLine("start");

            ModUtil.GetOtherDllExists();

            if (ModUtil.ExistAppearanceClothes)
            {
                Harmony_WornApparel_Trans.PatchAppearanceClothes(harmony);
            }
            else
            {
                Harmony_WornApparel_Trans.PatchResolveApparelGraphics(harmony);
            }

            if (ModUtil.ExistJetPack) //JetPackがResolveApparelGraphicsを潰してしまっているのでこちらにもパッチする
            {
                Harmony_JetPack_ResolveApparelGraphics.PatchJetPack(harmony);
            }

            if (ModUtil.ExistHAR)
            {
                if (ModUtil.HarIsNew)
                {
                    debug.WriteLine("Har is new");
                    Harmony_AlienRace_ExtendedGraphics.PatchAlienRace(harmony);
                }
                else
                {
                    debug.WriteLine("Har is old");
                    Harmony_AlienRace_CanDrawAddon.PatchAlienRace(harmony);
                }
            }
            else
            {
                Log.Message("TakeOffCoat:Har not found.");
            }

            if (ModUtil.ExistFactionColors)
            {
                Harmony_FationColors.PatchFactionColors(harmony);
            }

            if (ModUtil.ExistGaramRace && ModUtil.ExistShowHair)
            {
                Harmony_ShowHair.PatchShowHair(harmony);
            }

            if (ModUtil.ExistBadHygiene)
            {
                Harmony_BadHygiene_H_RenderPawn.Patch(harmony);
            }


            Harmony_Render.Patch(harmony);

            debug.WriteLine("Try remain patch.");


            harmony.PatchAll(Assembly.GetExecutingAssembly());

            foreach (CustomLayerDictionary cld in DefDatabase<CustomLayerDictionary>.AllDefs)
            {
                cld.MakeDictionary();
            }

            if (TakeOffSettings.useRemoveAnimal)
            {
                TakeOffComp.RemoveFromAnimals();
            }
            if (TakeOffSettings.useRemoveMech)
            {
                TakeOffComp.RemoveFromMechanoid();
            }


            infoTex = ContentFinder<Texture2D>.Get("UI/Buttons/InfoButton", true);

            TakeOffSettings.customApparelLayer.MakeDefsDic();
            ApparelList.MakeDic();

            //foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            //{
            //    debug.WriteLine(asm.GetName().Name);
            //    foreach(Type t in asm.GetTypes())
            //    {
            //        debug.WriteLine("\t" + t.Name);
            //    }

            //}

            filterIcon = ContentFinder<Texture2D>.Get("UI/Icons/Filter");

        }


        //public static bool chkBit(ulong val,int digit)
        //{
        //    ulong tmp = bitOnLong(1, digit);
        //    return (val & tmp) == tmp;
        //}

        //public static LayerPriority chkLayer(ulong val,int digit) //3bit取り出してLayerPriorityにキャスト
        //{
        //    ulong tmp = bitOnLong(1, digit) + bitOnLong(1, digit + 1) + bitOnLong(1, digit + 2);
        //    int lp = (int)(val & tmp);
        //    lp = lp >> digit;
        //    if (lp >= (int)LayerPriority.Null && lp <= (int)LayerPriority.All)
        //    {
        //        return (LayerPriority)lp;
        //    }
        //    return LayerPriority.Null;
        //}

    }

}
