using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;


namespace CCLGlower
{
    //Scraped with bits o Hospitality mod help :D
    [StaticConstructorOnStartup]
    internal static class InjectorThingy
    {
        static InjectorThingy()
		{
            LongEventHandler.QueueLongEvent(new Action(InjectorThingy.InjectStuff), "Initializing", true, null);
		}

        private static Assembly Assembly
        {
            get
            {
                return Assembly.GetAssembly(typeof(InjectorThingy));
            }
        }

        public static void InjectStuff()
        {            

            if (ModLister.AllInstalledMods.Where<ModMetaData>(s => s.Active == true && s.Name == "Clutter Furniture Module") != null)
            {
                MethodInfo Verse_CompGlower_ShouldBeLitNow_Getter = typeof(CompGlower).GetProperty("ShouldBeLitNow", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true);
                MethodInfo CCL_CompGlower_ShouldBeLitNow = typeof(_CompGlower).GetMethod("_ShouldBeLitNow", BindingFlags.Static | BindingFlags.NonPublic);

                if (!Detours.TryDetourFromTo(Verse_CompGlower_ShouldBeLitNow_Getter, CCL_CompGlower_ShouldBeLitNow))
                {
                    Log.Error(InjectorThingy.AssemblyName + " failed to get injected properly.");
                }
            }
            

        }

        private static string AssemblyName
        {
            get
            {
                return InjectorThingy.Assembly.FullName.Split(new char[]
				{
					','
				}).First<string>();
            }
        }

    }
}
