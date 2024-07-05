using System;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW
{
	public class CompProvideVision : ThingComp
	{
		public CompProperties_ProvideVision Props
		{
			get
			{
				return (CompProperties_ProvideVision)this.props;
			}
		}
	}
}
