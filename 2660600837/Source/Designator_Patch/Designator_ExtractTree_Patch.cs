using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FilterableDesignator
{
	class Designator_ExtractTree_Patch : Designator_Patch
	{
		// singleton
		private readonly static Designator_ExtractTree_Patch instance = new Designator_ExtractTree_Patch();

		public static Designator_ExtractTree_Patch Instance
		{
			get
			{
				return instance;
			}
		}

		private Designator_ExtractTree_Patch()
		{
		}

		public override void ClearMode()
		{
			return;
		}

		private protected override string CommandLabel(Designator instance)
		{
			return instance.defaultLabel;
		}

		private protected override void BuildFloatMenuOption(Designator instance, Event ev)
		{
			return;
		}
	}
}
