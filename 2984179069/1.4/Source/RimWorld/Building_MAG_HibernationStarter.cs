using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	internal class Building_MAG_HibernationStarter : Building
	{
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			foreach (Gizmo item in StartupGizmos())
			{
				yield return item;
			}
		}

		public IEnumerable<Gizmo> StartupGizmos()
		{
			yield return new Command_Action
			{
				defaultLabel = "Begin Charging",
				defaultDesc = "Does the thing",
				icon = ContentFinder<Texture2D>.Get("UI/MaG_ArchonexusCoreCharge"),
				action = delegate
				{
					MAG_StartupHibernatingParts();
				}
			};
		}

		public void MAG_StartupHibernatingParts()
		{
			CompMAG_ArchotechHibernatable compMAG_ArchotechHibernatable = this.TryGetComp<CompMAG_ArchotechHibernatable>();
			if (compMAG_ArchotechHibernatable != null && compMAG_ArchotechHibernatable.State == HibernatableStateDefOf.Hibernating)
			{
				compMAG_ArchotechHibernatable.Startup();
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			CancelStartup();
			base.Destroy(mode);
		}

		public void CancelStartup()
		{
			Messages.Message("MAG_HibernationHatching_Complete".Translate(), new GlobalTargetInfo(base.Position, base.Map), MessageTypeDefOf.NegativeEvent);
			CompMAG_ArchotechHibernatable compMAG_ArchotechHibernatable = this.TryGetComp<CompMAG_ArchotechHibernatable>();
			compMAG_ArchotechHibernatable.State = HibernatableStateDefOf.Hibernating;
		}
	}
}
