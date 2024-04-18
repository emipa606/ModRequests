using Verse;
using WildPlantPicker.HarmonyPatches;

namespace WildPlantPicker
{
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main()
        {
            Initializer.Initialize();
        }
    }
}
