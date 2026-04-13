using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BionicleKanohiMasksOfPower
{

    [StaticConstructorOnStartup]
	public class Gizmo_EnergyShieldStatus : Gizmo
	{
		public Apparel_HauMask shield;

		private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

		private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

		public Gizmo_EnergyShieldStatus()
		{
			order = -100f;
		}

		public override float GetWidth(float maxWidth)
		{
			return 140f;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Rect rect2 = rect.ContractedBy(6f);
			Widgets.DrawWindowBackground(rect);
			Rect rect3 = rect2;
			rect3.height = rect.height / 2f;
			Text.Font = GameFont.Tiny;
			Widgets.Label(rect3, shield.LabelCap);
			Rect rect4 = rect2;
			rect4.yMin = rect2.y + rect2.height / 2f;
			float fillPercent = shield.Energy / Mathf.Max(1f, shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax));
			Widgets.FillableBar(rect4, fillPercent, FullShieldBarTex, EmptyShieldBarTex, doBorder: false);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect4, (shield.Energy * 100f).ToString("F0") + " / " + (shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax) * 100f).ToString("F0"));
			Text.Anchor = TextAnchor.UpperLeft;
			return new GizmoResult(GizmoState.Clear);
		}
	}
	[StaticConstructorOnStartup]
	public class Apparel_HauMask : Apparel
	{
		private float energy;

		private int ticksToReset = -1;

		private int lastKeepDisplayTick = -9999;

		private Vector3 impactAngleVect;

		private int lastAbsorbDamageTick = -9999;

		private const float MinDrawSize = 1.2f;

		private const float MaxDrawSize = 1.55f;

		private const float MaxDamagedJitterDist = 0.05f;

		private const int JitterDurationTicks = 8;

		private int StartingTicksToReset = 600;

		private float EnergyOnReset = 0.2f;

		private float EnergyLossPerDamage = 0.033f;

		private int KeepDisplayingTicks = 1000;

		private float ApparelScorePerEnergyMax = 0.25f;

		private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

		private float EnergyMax => this.GetStatValue(StatDefOf.EnergyShieldEnergyMax);

		private float EnergyGainPerTick => this.GetStatValue(StatDefOf.EnergyShieldRechargeRate) / 60f;

		public float Energy => energy;

		public ShieldState ShieldState
		{
			get
			{
				if (ticksToReset > 0)
				{
					return ShieldState.Resetting;
				}
				return ShieldState.Active;
			}
		}

		private bool ShouldDisplay
		{
			get
			{
				if (!this.IsMasterworkOrLegendary())
				{
					return false;
				}
				Pawn wearer = base.Wearer;
				if (!wearer.Spawned || wearer.Dead || wearer.Downed)
				{
					return false;
				}
				if (wearer.InAggroMentalState)
				{
					return true;
				}
				if (wearer.Drafted)
				{
					return true;
				}
				if (wearer.Faction.HostileTo(Faction.OfPlayer) && !wearer.IsPrisoner)
				{
					return true;
				}
				if (Find.TickManager.TicksGame < lastKeepDisplayTick + KeepDisplayingTicks)
				{
					return true;
				}
				return false;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref energy, "energy", 0f);
			Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
			Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick", 0);
		}

		public override IEnumerable<Gizmo> GetWornGizmos()
		{
			foreach (Gizmo wornGizmo in base.GetWornGizmos())
			{
				yield return wornGizmo;
			}
			if (Find.Selector.SingleSelectedThing == base.Wearer && this.IsMasterworkOrLegendary())
			{
				Gizmo_EnergyShieldStatus gizmo_EnergyShieldStatus = new Gizmo_EnergyShieldStatus();
				gizmo_EnergyShieldStatus.shield = this;
				yield return gizmo_EnergyShieldStatus;
			}
		}

		public override float GetSpecialApparelScoreOffset()
		{
			return EnergyMax * ApparelScorePerEnergyMax;
		}

		public override void Tick()
		{
			base.Tick();
			if (!this.IsMasterworkOrLegendary())
            {
				return;
            }
			if (base.Wearer == null)
			{
				energy = 0f;
			}
			else if (ShieldState == ShieldState.Resetting)
			{
				ticksToReset--;
				if (ticksToReset <= 0)
				{
					Reset();
				}
			}
			else if (ShieldState == ShieldState.Active)
			{
				energy += EnergyGainPerTick;
				if (energy > EnergyMax)
				{
					energy = EnergyMax;
				}
			}
		}

		public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
		{
			if (!this.IsMasterworkOrLegendary())
			{
				return false;
			}
			if (ShieldState != 0)
			{
				return false;
			}
			if (dinfo.Def == DamageDefOf.EMP)
			{
				energy = 0f;
				Break();
				return false;
			}
			if (dinfo.Def.isRanged || dinfo.Def.isExplosive || dinfo.Weapon is null || dinfo.Weapon.race != null || dinfo.Weapon.IsMeleeWeapon)
			{
				energy -= dinfo.Amount * EnergyLossPerDamage;
				if (energy < 0f)
				{
					Break();
				}
				else
				{
					AbsorbedDamage(dinfo);
				}
				return true;
			}
			if (dinfo.Weapon is null || dinfo.Weapon.race != null || dinfo.Weapon.IsMeleeWeapon)
            {
				var instigator = dinfo.Instigator;
				if (instigator is Pawn attacker)
                {
					attacker.stances.stunner.StunFor(60, this.Wearer);
                }
            }
			return false;
		}

		public void KeepDisplaying()
		{
			lastKeepDisplayTick = Find.TickManager.TicksGame;
		}

		private void AbsorbedDamage(DamageInfo dinfo)
		{
			SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map));
			impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
			Vector3 loc = base.Wearer.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
			float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
			FleckMaker.Static(loc, base.Wearer.Map, FleckDefOf.ExplosionFlash, num);
			int num2 = (int)num;
			for (int i = 0; i < num2; i++)
			{
				FleckMaker.ThrowDustPuff(loc, base.Wearer.Map, Rand.Range(0.8f, 1.2f));
			}
			lastAbsorbDamageTick = Find.TickManager.TicksGame;
			KeepDisplaying();
		}

		private void Break()
		{
			SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map));
			FleckMaker.Static(base.Wearer.TrueCenter(), base.Wearer.Map, FleckDefOf.ExplosionFlash, 12f);
			for (int i = 0; i < 6; i++)
			{
				FleckMaker.ThrowDustPuff(base.Wearer.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), base.Wearer.Map, Rand.Range(0.8f, 1.2f));
			}
			energy = 0f;
			ticksToReset = StartingTicksToReset;
			foreach (var cell in this.OccupiedRect().AdjacentCells)
            {
				foreach (var pawn in cell.GetThingList(Map).OfType<Pawn>())
                {
					if (pawn.HostileTo(Wearer))
                    {
						pawn.stances.stunner.StunFor(60, Wearer);
                    }
                }
            }
		}

		private void Reset()
		{
			if (base.Wearer.Spawned)
			{
				SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map));
				FleckMaker.ThrowLightningGlow(base.Wearer.TrueCenter(), base.Wearer.Map, 3f);
			}
			ticksToReset = -1;
			energy = EnergyOnReset;
		}

		public override void DrawWornExtras()
		{
			if (ShieldState == ShieldState.Active && ShouldDisplay)
			{
				float num = Mathf.Lerp(1.2f, 1.55f, energy);
				Vector3 drawPos = base.Wearer.Drawer.DrawPos;
				drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
				int num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
				if (num2 < 8)
				{
					float num3 = (float)(8 - num2) / 8f * 0.05f;
					drawPos += impactAngleVect * num3;
					num -= num3;
				}
				float angle = Rand.Range(0, 360);
				Vector3 s = new Vector3(num, 1f, num);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
			}
		}
/*
		public override bool AllowVerbCast(Verb verb)
		{
			if (!this.IsMasterworkOrLegendary())
			{
				return true;
			}
			return !(verb is Verb_LaunchProjectile);
		}
		*/
	}
}