using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using USEUtilityButton;
using Verse;

namespace USEHitTheDeck
{
    [StaticConstructorOnStartup]
    public static class StaticConstructorClass
    {
        static StaticConstructorClass()
        {
            var harmony = new Harmony("USE.HitTheDeck");

            harmony.PatchAll(Assembly.GetExecutingAssembly());

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null))
            {
                //def.comps.Add(new USERenamerCompProp());
                def.comps.Add(new USEHitTheDeckCompProperties());
            }
        }
    }

}
