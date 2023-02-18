using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Crystosentient
{
	// Token: 0x02000002 RID: 2
	public class CompTempControl : ThingComp
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002744 File Offset: 0x00000944
		public CompProperties_TempControl Props
		{
			get
			{
				return (CompProperties_TempControl)this.props;
			}
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002764 File Offset: 0x00000964
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			bool flag = this.targetTemperature < -2000f;
			if (flag)
			{
				this.targetTemperature = this.Props.defaultTargetTemperature;
			}
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002050 File Offset: 0x00000250
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<float>(ref this.targetTemperature, "targetTemperature", 0f, false);
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000027A0 File Offset: 0x000009A0
		private float RoundedToCurrentTempModeOffset(float celsiusTemp)
		{
			return GenTemperature.ConvertTemperatureOffset((float)Mathf.RoundToInt(GenTemperature.CelsiusToOffset(celsiusTemp, Prefs.TemperatureMode)), Prefs.TemperatureMode, TemperatureDisplayMode.Celsius);
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002071 File Offset: 0x00000271
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
			{
				yield return gizmo;
			}
			IEnumerator<Gizmo> enumerator = null;
			float offset2 = this.RoundedToCurrentTempModeOffset(-10f);
			yield return new Command_Action
			{
				action = delegate()
				{
					this.InterfaceChangeTargetTemperature(offset2);
				},
				defaultLabel = offset2.ToStringTemperatureOffset("F0"),
				defaultDesc = "CommandLowerTempDesc".Translate(),
				hotKey = KeyBindingDefOf.Misc5,
				icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower", true)
			};
			float offset3 = this.RoundedToCurrentTempModeOffset(-1f);
			yield return new Command_Action
			{
				action = delegate()
				{
					this.InterfaceChangeTargetTemperature(offset3);
				},
				defaultLabel = offset3.ToStringTemperatureOffset("F0"),
				defaultDesc = "CommandLowerTempDesc".Translate(),
				hotKey = KeyBindingDefOf.Misc4,
				icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower", true)
			};
			yield return new Command_Action
			{
				action = delegate()
				{
					this.targetTemperature = 21f;
					SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
					this.ThrowCurrentTemperatureText();
				},
				defaultLabel = "CommandResetTemp".Translate(),
				defaultDesc = "CommandResetTempDesc".Translate(),
				hotKey = KeyBindingDefOf.Misc1,
				icon = ContentFinder<Texture2D>.Get("UI/Commands/TempReset", true)
			};
			float offset4 = this.RoundedToCurrentTempModeOffset(1f);
			yield return new Command_Action
			{
				action = delegate()
				{
					this.InterfaceChangeTargetTemperature(offset4);
				},
				defaultLabel = "+" + offset4.ToStringTemperatureOffset("F0"),
				defaultDesc = "CommandRaiseTempDesc".Translate(),
				hotKey = KeyBindingDefOf.Misc2,
				icon = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise", true)
			};
			float offset = this.RoundedToCurrentTempModeOffset(10f);
			yield return new Command_Action
			{
				action = delegate()
				{
					this.InterfaceChangeTargetTemperature(offset);
				},
				defaultLabel = "+" + offset.ToStringTemperatureOffset("F0"),
				defaultDesc = "CommandRaiseTempDesc".Translate(),
				hotKey = KeyBindingDefOf.Misc3,
				icon = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise", true)
			};
			yield break;
			yield break;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002081 File Offset: 0x00000281
		private void InterfaceChangeTargetTemperature(float offset)
		{
			SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
			this.targetTemperature += offset;
			this.targetTemperature = Mathf.Clamp(this.targetTemperature, -273.15f, 1000f);
			this.ThrowCurrentTemperatureText();
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000027D0 File Offset: 0x000009D0
		private void ThrowCurrentTemperatureText()
		{
			MoteMaker.ThrowText(this.parent.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), this.parent.Map, this.targetTemperature.ToStringTemperature("F0"), Color.white, -1f);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002830 File Offset: 0x00000A30
		public override string CompInspectStringExtra()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("TargetTemperature".Translate() + ": ");
			stringBuilder.AppendLine(this.targetTemperature.ToStringTemperature("F0"));
			stringBuilder.Append("PowerConsumptionMode".Translate() + ": ");
			bool flag = this.operatingAtHighPower;
			if (flag)
			{
				stringBuilder.Append("PowerConsumptionHigh".Translate().CapitalizeFirst());
			}
			else
			{
				stringBuilder.Append("PowerConsumptionLow".Translate().CapitalizeFirst());
			}
			return stringBuilder.ToString();
		}

		// Token: 0x04000001 RID: 1
		[Unsaved(false)]
		public bool operatingAtHighPower;

		// Token: 0x04000002 RID: 2
		public float targetTemperature = -99999f;

		// Token: 0x04000003 RID: 3
		private const float DefaultTargetTemperature = 21f;
	}
}
