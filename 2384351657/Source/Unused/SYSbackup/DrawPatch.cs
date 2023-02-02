using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace SYS
{
	// Token: 0x02000002 RID: 2
	[StaticConstructorOnStartup]
	public static class DrawPatch
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		static DrawPatch()
		{
			var harmony = new Harmony("com.SYS.rimworld.mod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}
