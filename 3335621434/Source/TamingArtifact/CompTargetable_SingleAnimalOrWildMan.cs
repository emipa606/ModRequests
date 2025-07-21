using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TamingArtifact
{
	// Token: 0x02000002 RID: 2
	internal class CompTargetable_SingleAnimalOrWildMan : CompTargetable
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public override bool PlayerChoosesTarget
		{
			get
			{
				return true;
			}
		}

        // Token: 0x06000002 RID: 2 RVA: 0x00002054 File Offset: 0x00000254
        public override TargetingParameters GetTargetingParameters()
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				canTargetHumans = true,
				canTargetMechs = false,
				canTargetSelf = false,
				canTargetAnimals = true,
				canTargetBuildings = false,
				onlyTargetPsychicSensitive = true,
				validator = delegate(TargetInfo x)
				{
					bool result;
					if (base.ValidateTarget(x.Thing))
					{
						Thing thing = x.Thing;
						result = (((thing != null) ? thing.Faction : null) == null);
					}
					else
					{
						result = false;
					}
					return result;
				}
			};
		}

		// Token: 0x06000003 RID: 3 RVA: 0x000020A9 File Offset: 0x000002A9
		public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
		{
			yield return targetChosenByPlayer;
			yield break;
		}
	}
}
