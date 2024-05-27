using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace MeteorAttractor
{
	// Token: 0x02001223 RID: 4643
	public class CompMeteorAttractor : CompScanner
	{
		// Token: 0x170013BD RID: 5053
		// (get) Token: 0x06007156 RID: 29014 RVA: 0x0026488F File Offset: 0x00262A8F
		public new CompProperties_MeteorAttractor Props
		{
			get
			{
				return this.props as CompProperties_MeteorAttractor;
			}
		}

		// Token: 0x06007157 RID: 29015 RVA: 0x0026489C File Offset: 0x00262A9C
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Defs.Look<ThingDef>(ref this.targetMineable, "targetMineable");
			if (Scribe.mode == LoadSaveMode.PostLoadInit && this.targetMineable == null)
			{
				this.SetDefaultTargetMineral();
			}
		}

		// Token: 0x06007158 RID: 29016 RVA: 0x002648CA File Offset: 0x00262ACA
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			this.SetDefaultTargetMineral();
		}

		// Token: 0x06007159 RID: 29017 RVA: 0x002648D9 File Offset: 0x00262AD9
		private void SetDefaultTargetMineral()
		{
			this.targetMineable = ThingDefOf.MineableGold;
		}

		// Token: 0x0600715A RID: 29018 RVA: 0x002648E8 File Offset: 0x00262AE8
		protected override void DoFind(Pawn worker)
		{
            Map map = this.parent.Map;
			IntVec3 intVec;
            this.TryFindCell(out intVec, map);
			List<Thing> list = new List<Thing>();
			int randomInRange = (new IntRange(15, 25)).RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				Building building = (Building)ThingMaker.MakeThing(this.targetMineable, null);
				building.canChangeTerrainOnDestroyed = false;
				list.Add(building);
			}
			SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, intVec, map);
			Find.LetterStack.ReceiveLetter("Meteor", "Your pawns have successfully attracted a meteor to your colony!", LetterDefOf.PositiveEvent, new TargetInfo(intVec, map, false), null, null, null, null);
		}

		// Token: 0x0600715B RID: 29019 RVA: 0x0026498C File Offset: 0x00262B8C
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
			{
				yield return gizmo;
			}
			if (this.parent.Faction == Faction.OfPlayer)
			{
				ThingDef mineableThing = this.targetMineable.building.mineableThing;
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "CommandSelectMineralToScanFor".Translate() + ": " + mineableThing.LabelCap;
				command_Action.icon = mineableThing.uiIcon;
				command_Action.iconAngle = mineableThing.uiIconAngle;
				command_Action.iconOffset = mineableThing.uiIconOffset;
				command_Action.action = delegate()
				{
					List<ThingDef> mineables = ((GenStep_PreciousLump)GenStepDefOf.PreciousLump.genStep).mineables;
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					foreach (ThingDef localD2 in mineables)
					{
						ThingDef localD = localD2;
						FloatMenuOption item = new FloatMenuOption(localD.building.mineableThing.LabelCap, delegate()
						{
							foreach (object obj in Find.Selector.SelectedObjects)
							{
								Thing thing = obj as Thing;
								if (thing != null)
								{
									CompMeteorAttractor compMeteorAttractor = thing.TryGetComp<CompMeteorAttractor>();
									if (compMeteorAttractor != null)
									{
										compMeteorAttractor.targetMineable = localD;
									}
								}
							}
						}, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, localD.building.mineableThing), null, true, 0);
						list.Add(item);
					}
					Find.WindowStack.Add(new FloatMenu(list));
				};
				yield return command_Action;
			}
			yield break;
		}

		// Token: 0x0600715C RID: 29020 RVA: 0x0026486F File Offset: 0x00262A6F
		public CompMeteorAttractor()
		{
		}

		// Token: 0x04003EA2 RID: 16034
		private ThingDef targetMineable;
        private bool TryFindCell(out IntVec3 cell, Map map)
		{
			int maxMineables = ThingSetMaker_Meteorite.MineablesCountRange.max;
			return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, out cell, 10, default(IntVec3), -1, true, false, false, false, true, true, delegate(IntVec3 x)
			{
				int num = Mathf.CeilToInt(Mathf.Sqrt((float)maxMineables)) + 2;
				CellRect cellRect = CellRect.CenteredOn(x, num, num);
				int num2 = 0;
				foreach (IntVec3 c in cellRect)
				{
					if (c.InBounds(map) && c.Standable(map))
					{
						num2++;
					}
				}
				return num2 >= maxMineables;
			});
		}
	}
}
