using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace Necro
{
	
	public class NecronoidStuffSpawner : ThingComp
	{
		
		
		public CompProperties_NecronoidStuffSpawner PropsSpawner
		{
			get
			{
				return (CompProperties_NecronoidStuffSpawner)this.props;
			}
		}

		
		
		private bool PowerOn
		{
			get
			{
				CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
				return comp != null && comp.PowerOn;
			}
		}

		
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad)
			{
				this.ResetCountdown();
			}
		}

		
		public override void CompTick()
		{
			this.TickInterval(1);
		}

		
		public override void CompTickRare()
		{
			this.TickInterval(250);
		}

		
		private void TickInterval(int interval)
		{
			if (this.parent.Spawned)
			{
				CompCanBeDormant comp = this.parent.GetComp<CompCanBeDormant>();
				if (comp != null)
				{
					if (!comp.Awake)
					{
						return;
					}
				}
				else if (this.parent.Position.Fogged(this.parent.Map))
				{
					return;
				}
				if (!this.PropsSpawner.requiresPower || this.PowerOn)
				{
					this.ticksUntilSpawn -= interval;
					this.CheckShouldSpawn();
				}
			}
		}

		
		private void CheckShouldSpawn()
		{
			if (this.ticksUntilSpawn <= 0)
			{
				this.ResetCountdown();
				this.TryDoSpawn();
			}
		}

		
		public bool TryDoSpawn()
		{
			bool result;
			if (!this.parent.Spawned)
			{
				result = false;
			}
			else
			{
				if (this.PropsSpawner.spawnMaxAdjacent >= 0)
				{
					int num = 0;
					for (int i = 0; i < 9; i++)
					{
						IntVec3 c = this.parent.Position + GenAdj.AdjacentCellsAndInside[i];
						if (c.InBounds(this.parent.Map))
						{
							List<Thing> thingList = c.GetThingList(this.parent.Map);
							for (int j = 0; j < thingList.Count; j++)
							{
								if (thingList[j].def == this.PropsSpawner.thingToSpawn)
								{
									num += thingList[j].stackCount;
									if (num >= this.PropsSpawner.spawnMaxAdjacent)
									{
										return false;
									}
								}
							}
						}
					}
				}
				IntVec3 center;
				if (CompSpawner.TryFindSpawnCell(this.parent, this.PropsSpawner.thingToSpawn, this.PropsSpawner.spawnCount, out center))
				{
					Thing thing = ThingMaker.MakeThing(this.PropsSpawner.thingToSpawn, null);
					Find.FactionManager.FirstFactionOfDef(FactionDefOf.Necronoid);
					thing.stackCount = this.PropsSpawner.spawnCount;
					if (thing == null)
					{
						string str = "Could not spawn anything for ";
						ThingWithComps parent = this.parent;
						Log.Error(str + ((parent != null) ? parent.ToString() : null));
					}
					if (this.PropsSpawner.inheritFaction && thing.Faction != this.parent.Faction)
					{
						thing.SetFaction(this.parent.Faction, null);
					}
					Thing t;
					GenPlace.TryPlaceThing(thing, center, this.parent.Map, ThingPlaceMode.Direct, out t, null, null, default(Rot4));
					if (this.PropsSpawner.spawnForbidden)
					{
						t.SetForbidden(true, true);
					}
					if (this.PropsSpawner.showMessageIfOwned && this.parent.Faction == Find.FactionManager.FirstFactionOfDef(FactionDefOf.Necronoid))
					{
						Messages.Message("MessageCompSpawnerSpawnedItem".Translate(this.PropsSpawner.thingToSpawn.LabelCap), thing, MessageTypeDefOf.NegativeEvent, true);
					}
					result = true;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		
		public static bool TryFindSpawnCell(Thing parent, ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
		{
			foreach (IntVec3 intVec in GenAdj.CellsAdjacent8Way(parent).InRandomOrder(null))
			{
				if (intVec.Walkable(parent.Map))
				{
					Building edifice = intVec.GetEdifice(parent.Map);
					if (edifice == null || !thingToSpawn.IsEdifice())
					{
						Building_Door building_Door = edifice as Building_Door;
						if ((building_Door == null || building_Door.FreePassage) && (parent.def.passability == Traversability.Impassable || GenSight.LineOfSight(parent.Position, intVec, parent.Map, false, null, 0, 0)))
						{
							bool flag = false;
							List<Thing> thingList = intVec.GetThingList(parent.Map);
							for (int i = 0; i < thingList.Count; i++)
							{
								Thing thing = thingList[i];
								thing.SetFaction(Find.FactionManager.FirstFactionOfDef(FactionDefOf.Necronoid), null);
								if (thing.def.category == ThingCategory.Item && (thing.def != thingToSpawn || thing.stackCount > thingToSpawn.stackLimit - spawnCount))
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								result = intVec;
								return true;
							}
						}
					}
				}
			}
			result = IntVec3.Invalid;
			return false;
		}

		
		private void ResetCountdown()
		{
			this.ticksUntilSpawn = this.PropsSpawner.spawnIntervalRange.RandomInRange;
		}

		
		public override void PostExposeData()
		{
			string str = this.PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : (this.PropsSpawner.saveKeysPrefix + "_");
			Scribe_Values.Look<int>(ref this.ticksUntilSpawn, str + "ticksUntilSpawn", 0, false);
		}

		
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (DebugSettings.ShowDevGizmos)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Spawn " + this.PropsSpawner.thingToSpawn.label,
					icon = TexCommand.DesirePower,
					action = delegate()
					{
						this.ResetCountdown();
						this.TryDoSpawn();
					}
				};
			}
			yield break;
		}

		
		public override string CompInspectStringExtra()
		{
			string result;
			if (this.PropsSpawner.writeTimeLeftToSpawn && (!this.PropsSpawner.requiresPower || this.PowerOn))
			{
				result = "NextSpawnedItemIn".Translate(GenLabel.ThingLabel(this.PropsSpawner.thingToSpawn, null, this.PropsSpawner.spawnCount)).Resolve() + ": " + this.ticksUntilSpawn.ToStringTicksToPeriod(true, false, true, true, false).Colorize(ColoredText.DateTimeColor);
			}
			else
			{
				result = null;
			}
			return result;
		}

		
		public NecronoidStuffSpawner()
		{
		}
		
		private int ticksUntilSpawn;
	}
}
