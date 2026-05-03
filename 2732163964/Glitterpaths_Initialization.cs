namespace Glitterpaths;

[StaticConstructorOnStartup]
internal class Glitterpaths_Initialization
{
    static Glitterpaths_Initialization() => new Harmony("Glitterpaths").PatchAll(Assembly.GetExecutingAssembly());
}