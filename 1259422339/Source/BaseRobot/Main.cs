using HarmonyLib;
using System.Reflection;
using Verse;

namespace BaseRobot
{
    // Token: 0x02000019 RID: 25
    [StaticConstructorOnStartup]
	internal class Main
	{
		// Token: 0x06000077 RID: 119 RVA: 0x00005A88 File Offset: 0x00003C88
		static Main()
		{
			var harmonyInstance = new Harmony("com.github.harmony.rimworld.baserobots");
			harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}
