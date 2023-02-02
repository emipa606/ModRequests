using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using System.Collections.Generic;
using RimWorld.Planet;
using System.Linq;
using System;
using RimWorld.BaseGen;



namespace MechanoidAssimilation
{

    

    //Setting the Harmony instance
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.mechanoidassimilation");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }


    }


   



}
