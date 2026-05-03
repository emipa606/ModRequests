global using Mod = Glitterpaths.Mod;

namespace Glitterpaths;

public sealed class Mod : Verse.Mod
{
    public static Mod Instance { get; private set; }
    public Mod(ModContentPack content) : base(content)
    {
        Instance = this;
    }
}