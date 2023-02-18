using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ActiveTerrain
{
	// Token: 0x02000018 RID: 24
	[StaticConstructorOnStartup]
	[HarmonyPatch(typeof(TerrainGrid), "SetTerrain", null)]
	public static class _TerrainGrid
	{
		// Token: 0x06000079 RID: 121 RVA: 0x00003724 File Offset: 0x00001924
		static _TerrainGrid()
		{
			new Harmony("com.spdskatr.activeterrain.patches").PatchAll(Assembly.GetExecutingAssembly());
			Log.Message("Test patch. This mod uses Harmony (all patches are non-destructive): Verse.TerrainGrid.SetTerrain, Verse.TerrainGrid.RemoveTopLayer, Verse.MouseoverReadout.MouseoverReadoutOnGUI");
		}
	}
}