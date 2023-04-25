using System.Reflection;
using HarmonyLib;
using Verse;

namespace Euphoric.DecorativeGrassAndPlants
{
    [StaticConstructorOnStartup]
    // ReSharper disable once UnusedType.Global
    public static class DecorativeGrassAndPlantsPatch
    {
        static DecorativeGrassAndPlantsPatch()
        {
            //Harmony.DEBUG = true;
            
            var harmonyInstance = new Harmony("Euphoric.DecorativeGrassAndPlants");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}