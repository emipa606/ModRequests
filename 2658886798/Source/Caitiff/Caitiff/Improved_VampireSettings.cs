using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Vampire
{
	[StaticConstructorOnStartup]
	public static class Improved_VampireSettings
	{
		//        public static GameMode caitiffmode = CaitiffMode.Default;
		//		  public static int Caitiff XP = 5; (I know this isn't necessary, but I'm following the original author's steps. Fuck off.)

		public static bool ShouldUseSettings => Find.Scenario.AllParts.FirstOrDefault(x =>
													x is ScenPart_ForcedHediff y && Traverse.Create(y)
														.Field("hediff").GetValue<HediffDef>() ==
													VampDefOf.ROM_Vampirism) == null;

		public static WorldComponent_Improved_VampireSettings Get => Find.World.GetComponent<WorldComponent_Improved_VampireSettings>();

	}

	public enum CaitiffMode
	{
		Default = 0,
		Custom = 1
		//Toreador = 1
	}
}
