using System;
using Verse;
using RimWorld;

namespace WallHeater
{
	public class CompProperties_GlowerOffset : CompProperties
	{
		public CompProperties_GlowerOffset()
		{
			this.compClass = typeof(CompGlowerOffset);
		}

		public string glowerDefName = "";
	}
}