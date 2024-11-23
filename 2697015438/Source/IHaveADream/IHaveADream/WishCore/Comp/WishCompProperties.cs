using RimWorld;
using HarmonyLib;
using Verse;
using System;
using System.Collections.Generic;

namespace HDream
{
    public class WishCompProperties
    {

		public Type compClass;

		public virtual void PostLoad()
		{
		}

		public virtual IEnumerable<string> ConfigErrors(WishDef parentDef)
		{
			if (compClass == null)
			{
				yield return "compClass is null";
			}
			for (int i = 0; i < parentDef.comps.Count; i++)
			{
				if (parentDef.comps[i] != this && parentDef.comps[i].compClass == compClass)
				{
					yield return "two comps with same compClass: " + compClass;
				}
			}
		}
	}
}
