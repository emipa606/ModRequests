using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CompToggleFireMode
{
	// Token: 0x02000003 RID: 3
	public class CompToggleFireMode : ThingComp
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000002 RID: 2 RVA: 0x0000206C File Offset: 0x0000026C
		public CompProperties_ToggleFireMode Props
		{
			get
			{
				return (CompProperties_ToggleFireMode)this.props;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000003 RID: 3 RVA: 0x0000208C File Offset: 0x0000028C
		protected virtual Pawn GetWearer
		{
			get
			{
				bool flag = base.ParentHolder != null && base.ParentHolder is Pawn_EquipmentTracker;
				bool flag2 = flag;
				Pawn result;
				if (flag2)
				{
					result = (Pawn)base.ParentHolder.ParentHolder;
				}
				else
				{
					result = null;
				}
				return result;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000004 RID: 4 RVA: 0x000020D8 File Offset: 0x000002D8
		protected virtual bool IsWorn
		{
			get
			{
				return this.GetWearer != null;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000005 RID: 5 RVA: 0x000020F4 File Offset: 0x000002F4
		public CompEquippable Equippable
		{
			get
			{
				return this.parent.TryGetComp<CompEquippable>();
			}
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002111 File Offset: 0x00000311
		public void SwitchFireMode(int x)
		{
			this.fireMode = x;
		}

		// Token: 0x06000007 RID: 7 RVA: 0x0000211C File Offset: 0x0000031C
		public override void CompTick()
		{
			base.CompTick();
			bool flag = this.GetWearer != this.lastWearer;
			bool flag2 = flag;
			if (flag2)
			{
				this.lastWearer = this.GetWearer;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000008 RID: 8 RVA: 0x00002158 File Offset: 0x00000358
		public VerbProperties Active
		{
			get
			{
				bool flag = this.parent != null && this.parent != null;
				bool flag2 = flag;
				VerbProperties result;
				if (flag2)
				{
					result = this.parent.def.Verbs[this.fireMode];
				}
				else
				{
					result = null;
				}
				return result;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000009 RID: 9 RVA: 0x000021AC File Offset: 0x000003AC
		public Verb ActiveVerb
		{
			get
			{
				bool flag = this.parent != null && this.parent != null;
				bool flag2 = flag;
				Verb verb;
				if (flag2)
				{
					verb = this.Equippable.AllVerbs[this.fireMode];
					verb.verbProps = this.Active;
				}
				else
				{
					verb = null;
				}
				return verb;
			}
		}

		public bool drawMenu;
		public virtual IEnumerable<Gizmo> EquippedGizmos()
		{
			Thing pawn = (IsWorn ? GetWearer : parent);
			if (Find.Selector.SingleSelectedThing != GetWearer)
			{
				yield break;
			}

			var verbProps = Active;
			Command_ActionActive command_Action = new Command_ActionActive
			{
				icon = verbProps.defaultProjectile.uiIcon,
				comp = this,
				defaultLabel = verbProps.label,
				defaultDesc = verbProps.label,
				hotKey = KeyBindingDefOf.Misc10,
				activateSound = SoundDefOf.Click,
				action = delegate
				{
					drawMenu = true;
					SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
				},
				verbProps = verbProps,
				pawn = pawn
			};
			if (GetWearer.Faction != Faction.OfPlayer)
			{
				command_Action.Disable("CannotOrderNonControlled".Translate());
			}
			yield return command_Action;
		}

		// Token: 0x04000002 RID: 2
		public Pawn lastWearer;

		// Token: 0x04000003 RID: 3
		public bool GizmosOnEquip = true;

		// Token: 0x04000004 RID: 4
		public bool Toggled = false;

		// Token: 0x04000005 RID: 5
		public int fireMode = 0;
	}
}
