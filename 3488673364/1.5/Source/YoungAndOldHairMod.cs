using HarmonyLib;
using Verse;

namespace YoungAndOldHair
{
	public class YoungAndOldHairMod : Mod
	{
		public YoungAndOldHairMod(ModContentPack pack) : base(pack)
		{
			new Harmony("YoungAndOldHairMod").PatchAll();
		}
	}
}
