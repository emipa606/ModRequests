using System;
using RimWorld;
using RimWorldRealFoW;
using RimWorldRealFoW.Utils;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x0200001C RID: 28
	public static class _MoteBubble
	{
		// Token: 0x0600007C RID: 124 RVA: 0x00008D38 File Offset: 0x00006F38
		public static bool Draw_Prefix(MoteBubble __instance)
		{
			bool flag = __instance.link1.Linked && __instance.link1.Target != null && __instance.link1.Target.Thing != null;
			return !flag || __instance.link1.Target.Thing.fowIsVisible(false);
		}
	}
}
