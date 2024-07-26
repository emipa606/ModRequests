using HarmonyLib;
using Verse;

namespace CryptoExpansion
{
  public class CryptoExpansionMod : Mod
  {
    public CryptoExpansionMod(ModContentPack content) : base(content)
    {
      new Harmony("CatLover366.CryptoBionics").PatchAll();
    }
  }
}
