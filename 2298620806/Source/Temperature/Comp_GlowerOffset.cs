using System;
using RimWorld;
using Verse;

namespace WallHeater
{
	public class CompGlowerOffset : ThingComp
	{

		private Building building;
		public Thing glower;
		public IntVec3 glowPos;
		private bool glowOnInt;

		public CompProperties_GlowerOffset Props
		{
			get
			{
				return (CompProperties_GlowerOffset)this.props;
			}
		}

		public IntVec3 dx
		{
			get
			{
				if (!this.parent.Spawned)
				{
					return IntVec3.Zero;
				}
				else
				{
					return IntVec3.North.RotatedBy(this.parent.Rotation);
				}
			}
		}

			private bool shouldBeLitNow
			{
				get
				{
					bool flag = !this.parent.Spawned;
					bool result;
					if (flag)
					{
						result = false;
					}
					else
					{
						bool flag2 = !FlickUtility.WantsToBeOn(this.parent);
						if (flag2)
						{
							result = false;
						}
						else
						{
							CompPowerTrader compPowerTrader = this.parent.TryGetComp<CompPowerTrader>();
							bool flag3 = compPowerTrader != null && !compPowerTrader.PowerOn;
							if (flag3)
							{
								result = false;
							}
							else
							{
								result = true;
							}
						}
					}
					return result;
				}
			}

			public override void ReceiveCompSignal(string signal)
			{
				bool flag = signal == "PowerTurnedOn" || signal == "PowerTurnedOff" || signal == "FlickedOn" || signal == "FlickedOff";
				if (flag)
				{
					this.UpdateLit();
				}
			}

			public void UpdateLit()
			{
				bool flag = this.glowOnInt != this.shouldBeLitNow;
				if (flag)
				{
					this.glowOnInt = this.shouldBeLitNow;
					bool flag2 = !this.glowOnInt;
					if (flag2)
					{
						this.DespawnGlower();
					}
					else
					{
						this.SpawnGlower();
					}
				}
			}

			public override void PostSpawnSetup(bool respawningAfterLoad)
			{
				base.PostSpawnSetup(respawningAfterLoad);
				this.building = (this.parent as Building);
				this.UpdateLit();
			}

			public override void PostDeSpawn(Map map)
			{
				base.PostDeSpawn(map);
				this.UpdateLit();
			}

			public void SpawnGlower()
			{
				this.glowPos = this.parent.Position + dx;
				this.glower = ThingMaker.MakeThing(ThingDef.Named(this.Props.glowerDefName), null);
				bool flag = this.building == null;
				if (flag)
				{
					this.building = (this.parent as Building);
				}
				GenSpawn.Spawn(this.glower, this.glowPos, this.parent.Map, this.parent.Rotation, WipeMode.Vanish, false);
			}

			public void DespawnGlower()
			{
				bool flag = this.glower != null && this.glower.Spawned;
				if (flag)
				{
					this.glower.DeSpawn(DestroyMode.Vanish);
					this.glower = null;
				}
			}

		}
	}