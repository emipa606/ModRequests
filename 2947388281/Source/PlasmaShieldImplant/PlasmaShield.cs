using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PlasmaShieldImplant
{

	public class VirCompShieldProp : CompProperties
	{
		public int startingTicksToReset = 3200;

		public float minDrawSize = 1.2f;

		public float maxDrawSize = 1.55f;

		public float energyLossPerDamage = 0.033f;

		public float energyOnReset = 0.2f;

		public VirCompShieldProp()
		{
			compClass = typeof(VirCompShield2);
		}
	}

	[StaticConstructorOnStartup]	
	public class VirCompShield2 : ThingComp
	{
		public VirCompShieldProp Props => (VirCompShieldProp)props;

		public float Energy = 20;
		
		public float MaxEnergy = 20;

		public float BaseMaxEnergy = 20;

		private Vector3 impactAngleVect;

		private int lastAbsorbDamageTick = -9999;

		protected int ticksToReset = -1;

		public int StartingTicksToReset = 300;

		public float RegenerationRate = 0.001f;

		public bool PsychicShield = false;

		public bool PsychicRegen = false;

		public bool EMPResistant = false;

		public bool Adaptive = false;

		public float EnergyOnReset = 0;

		public float NumOfHediffs = 0;

		public bool CanFunction = false;

		public int Hardened = 0;

		public int AdaptiveLevel = 0;

		public DamageDef AdaptedDMG = null;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref Energy, "Energy", 0f);
			Scribe_Values.Look(ref MaxEnergy, "MaxEnergy", 20f);
			Scribe_Values.Look(ref BaseMaxEnergy, "BaseMaxEnergy", 20f);
			Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
			Scribe_Values.Look(ref PsychicShield, "PsychicShield", false);
			Scribe_Values.Look(ref PsychicRegen, "PsychicRegen", false);
			Scribe_Values.Look(ref CanFunction, "CanFunction", false);
			Scribe_Values.Look(ref NumOfHediffs, "NumOfHediffs", 0f);
			Scribe_Values.Look(ref Hardened, "Hardened", 0);
			Scribe_Values.Look(ref EnergyOnReset, "EnergyOnReset", 0f);
			Scribe_Deep.Look(ref AdaptedDMG, "AdaptedDMG", null);
		}
		public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetWornGizmosExtra())
			{
				yield return item;
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			foreach (Gizmo gizmo in GetGizmos())
			{
				yield return gizmo;
			}
		}

		private IEnumerable<Gizmo> GetGizmos()
		{
			if ((parent.Faction == Faction.OfPlayer || (parent is Pawn pawn && pawn.RaceProps.IsMechanoid)) && Find.Selector.SingleSelectedThing == parent && CanFunction)
			{
				Gizmo_PlasmaShieldStatus gizmo_PlasmaShieldStatus = new Gizmo_PlasmaShieldStatus();
				gizmo_PlasmaShieldStatus.shield = this;
				yield return gizmo_PlasmaShieldStatus;
			}
		}
		private void AbsorbedDamage(DamageInfo dinfo)
		{
			SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
			impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
			Vector3 loc = parent.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
			float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
			FleckMaker.Static(loc, parent.Map, FleckDefOf.ExplosionFlash, num);
			int num2 = (int)num;
			for (int i = 0; i < num2; i++)
			{
				FleckMaker.ThrowDustPuff(loc, parent.Map, Rand.Range(0.8f, 1.2f));
			}
			lastAbsorbDamageTick = Find.TickManager.TicksGame;
		}
		private void Break()
		{
			float scale = Mathf.Lerp(1.2f, 1.55f, Energy);
			EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, scale);
			FleckMaker.Static(parent.TrueCenter(), parent.Map, FleckDefOf.ExplosionFlash, 12f);
			for (int i = 0; i < 6; i++)
			{
				FleckMaker.ThrowDustPuff(parent.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), parent.Map, Rand.Range(0.8f, 1.2f));
			}
			Energy = 0f;
			ticksToReset = StartingTicksToReset;
		}

		private void Reset()
		{
			if (parent.Spawned)
			{
				SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
				FleckMaker.ThrowLightningGlow(parent.TrueCenter(), parent.Map, 3f);
			}
			ticksToReset = -1;
			Energy = EnergyOnReset;
		}
		public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			absorbed = false;
			Pawn Tested = (Pawn)parent;
			CompShield ShieldComp = null;
			foreach (Apparel item in Tested.apparel.WornApparel)
			{
				ShieldComp = item.GetComp<CompShield>();
				if (ShieldComp != null)
				{
					ShieldComp.PostPreApplyDamage(dinfo, out bool absorbed2);
					if (absorbed2)
					{
						absorbed = true;
						return;
					}
				}
			}
			ShieldComp = parent.GetComp<CompShield>();
			if (ShieldComp != null)
			{
				ShieldComp.PostPreApplyDamage(dinfo, out bool absorbed2);
				if (absorbed2)
				{
					absorbed = true;
					return;
				}
			}
			if (ShieldState != 0 || !CanFunction)
			{
				return;
			}
			if (dinfo.Def == DamageDefOf.EMP && !EMPResistant)
			{
				Energy = 0;
				Break();
			}
			else
			{
				float OriginalEnergy = Energy;
				if (Hardened > 0)
				{
					if (dinfo.Amount > Hardened)
                    {
						Energy -= Hardened;
					}
					else
                    {
						Energy -= dinfo.Amount;
					}

				}
				else if (dinfo.Def == DamageDefOf.EMP && EMPResistant)
				{
					Energy -= dinfo.Amount / 2;
                }
				else if (Adaptive)
                {
					float DamageVal = 0;
					Log.Message(dinfo.Def.LabelCap);
					Log.Message(dinfo.Amount.ToString());

					if (dinfo.Def == AdaptedDMG)
                    {
						Energy -= dinfo.Amount * (1.2f - (0.2f * AdaptiveLevel));
						DamageVal = dinfo.Amount * (1.2f - (0.2f * AdaptiveLevel));
						AdaptiveLevel++;
						if (AdaptiveLevel > 5)
							AdaptiveLevel = 5;
					}
					else
                    {
						Energy -= dinfo.Amount * 1.2f;
						DamageVal = dinfo.Amount * 1.2f;
						AdaptiveLevel = 1;
						AdaptedDMG = dinfo.Def;
					}
					Log.Message(DamageVal.ToString());

				}
				else 
				{
					Energy -= dinfo.Amount;
				}
				if (Energy < 0f)
				{
					Break();
					if (dinfo.Amount > OriginalEnergy * 2 && OriginalEnergy < MaxEnergy / 2)
					{
						absorbed = false;
					}
					else
					{
						absorbed = true;
					}
				}
				else
				{
					AbsorbedDamage(dinfo);
					absorbed = true;
				}
			}
		}
		public override void CompTick()
		{
			base.CompTick();
			if (!CanFunction)
            {
				Energy = 0f;
				return;
            }
			if (parent == null)
			{
				Energy = 0f;
				return;
			}
			if (PsychicShield)
			{
				MaxEnergy = BaseMaxEnergy;
				MaxEnergy = BaseMaxEnergy / 2 + parent.GetStatValue(StatDefOf.PsychicSensitivity, true) * 20f;
			}
			else
			{
				MaxEnergy = BaseMaxEnergy;
			}
			if (Hardened > 0)
            {
				MaxEnergy = MaxEnergy / 2;
			}

			if (ShieldState == ShieldState.Resetting)
			{
				ticksToReset--;
				if (ticksToReset <= 0)
				{
					Reset();
				}
			}
			else if (ShieldState == ShieldState.Active)
			{

				if (!PsychicRegen)
					Energy += RegenerationRate;// 0.001f;
				else
					Energy = Energy + RegenerationRate + parent.GetStatValue(StatDefOf.PsychicSensitivity, true) * 0.001f;// 0.001f;
				if (Energy > MaxEnergy)
				{
					Energy = MaxEnergy;
				}
			}
			
		}
		public ShieldState ShieldState
		{
			get
			{
				if (parent is Pawn p && (p.IsCharging() || p.IsSelfShutdown()))
				{
					return ShieldState.Disabled;
				}
				CompCanBeDormant comp = parent.GetComp<CompCanBeDormant>();
				if (comp != null && !comp.Awake)
				{
					return ShieldState.Disabled;
				}
				if (ticksToReset <= 0)
				{
					return ShieldState.Active;
				}
				return ShieldState.Resetting;
			}
		}
		
	}
	public class HediffPlasmaShieldCompProperties : HediffCompProperties
    {
		public float MaxEnergy = 200;
	
		public int StartingTicksToReset = 300;

		public float RegenerationRate = 0f;

		public bool PsychicShield = false;

		public bool PsychicRegen = false;

		public bool EMPResistant = false;

		public bool Adaptive = false;

		public float EnergyOnReset = 0;

		public bool CanFunction = false;

		public bool FunctionalModule = false;

		public int Hardened = 0;

		public HediffPlasmaShieldCompProperties()
        {
			compClass = typeof(HediffCompPlasmaShield);
        }
	}
	public class HediffCompPlasmaShield : HediffComp
	{
		public HediffPlasmaShieldCompProperties Props => (HediffPlasmaShieldCompProperties)props;
	}
	public class HediffPlasmaShield_Implant : Hediff_Implant
    {
		public VirCompShield2 ShieldComp;

		public float MaxEnergy = 0;
		
		public int StartingTicksToReset = 0;

		public float RegenerationRate = 0f;

		public bool PsychicShield = false;

		public bool PsychicRegen = false;

		public bool EMPResistant = false;

		public bool Adaptive = false;

		public float EnergyOnReset = 0;

		public bool CanFunction = false;

		public int Hardened = 0;

		public bool Active = true;

		public bool FunctionalModule = false;

		public float CurrentEnergy = 0;

		public int AdaptiveLevel = 0;

		public DamageDef AdaptedDMG = null;

		public override void PostTick()
        {
			//Log.Message(MaxEnergy.ToString());
			if (ShieldComp == null)
            {
				PostMake();
				ShieldComp.Energy = CurrentEnergy;
				ShieldComp.AdaptiveLevel = AdaptiveLevel;
				ShieldComp.AdaptedDMG = AdaptedDMG;
			}
			else
            {
				CurrentEnergy = ShieldComp.Energy;
				AdaptiveLevel = ShieldComp.AdaptiveLevel;
				AdaptedDMG = ShieldComp.AdaptedDMG;
			}
		}
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref MaxEnergy, "MaxEnergy", 0f);
			Scribe_Values.Look(ref StartingTicksToReset, "StartingTicksToReset", 0);
			Scribe_Values.Look(ref PsychicShield, "PsychicShield", false);
			Scribe_Values.Look(ref PsychicRegen, "PsychicRegen", false);
			Scribe_Values.Look(ref EMPResistant, "EMPResistant", false);
			Scribe_Values.Look(ref Adaptive, "Adaptive", false);
			Scribe_Values.Look(ref EnergyOnReset, "EnergyOnReset", 0);
			Scribe_Values.Look(ref CanFunction, "CanFunction", false);
			Scribe_Values.Look(ref Hardened, "Hardened", 0);
			Scribe_Values.Look(ref CurrentEnergy, "CurrentEnergy", 0);
			Scribe_Values.Look(ref RegenerationRate, "RegenerationRate", 0f);
			Scribe_Values.Look(ref Active, "Active", true);
			Scribe_Values.Look(ref AdaptiveLevel, "AdaptiveLevel", 0);
			Scribe_Deep.Look(ref AdaptedDMG, "AdaptedDMG", null);
			//			Scribe_Deep.Look(ref ShieldComp, "ShieldComp");
		}
		public override string Label
		{
			get
			{
				string labelInBrackets = LabelInBrackets;
				if (Active)
					return LabelBase + (labelInBrackets.NullOrEmpty() ? "" : (" (" + labelInBrackets + ")"));
				else
					return LabelBase + (labelInBrackets.NullOrEmpty() ? "" : (" (" + labelInBrackets + ")")) + " (Disabled)";

			}
		}

		public virtual void DeactiveModule()
        {
			Active = false;
			HediffCompPlasmaShield BaseData = this.TryGetComp<HediffCompPlasmaShield>();
			if (BaseData == null)
			{
				Log.Error("Error - Failed to find HediffCompPlasmaShield");
				return;
			}
			ShieldComp = pawn.GetComp<VirCompShield2>();
			ShieldComp.MaxEnergy -= BaseData.Props.MaxEnergy;
			ShieldComp.BaseMaxEnergy -= BaseData.Props.MaxEnergy;

			if (BaseData != null)
			{
				ShieldComp.StartingTicksToReset -= BaseData.Props.StartingTicksToReset;

				if (BaseData.Props.PsychicShield)
					ShieldComp.PsychicShield = false;
				if (BaseData.Props.CanFunction)
					ShieldComp.CanFunction = false;
				if (BaseData.Props.PsychicRegen)
					ShieldComp.PsychicRegen = false;
				if (BaseData.Props.EMPResistant)
					ShieldComp.EMPResistant = false;
				if (BaseData.Props.Adaptive)
					ShieldComp.Adaptive = false;

				ShieldComp.EnergyOnReset -= BaseData.Props.EnergyOnReset;
				ShieldComp.RegenerationRate -= BaseData.Props.RegenerationRate;
				ShieldComp.Hardened -= BaseData.Props.Hardened;
			}
		}
		public virtual void ActivateModule()
        {
			Active = true;
			HediffCompPlasmaShield BaseData = this.TryGetComp<HediffCompPlasmaShield>();
			if (BaseData == null)
			{
				Log.Error("Error - Failed to find HediffCompPlasmaShield");
				return;
			}
			ShieldComp = pawn.GetComp<VirCompShield2>();
			ShieldComp.MaxEnergy += BaseData.Props.MaxEnergy;
			ShieldComp.BaseMaxEnergy += BaseData.Props.MaxEnergy;

			if (BaseData != null)
			{
				ShieldComp.StartingTicksToReset += BaseData.Props.StartingTicksToReset;
				if (!(ShieldComp.PsychicShield))
					ShieldComp.PsychicShield = BaseData.Props.PsychicShield;
				if (!(ShieldComp.CanFunction))
					ShieldComp.CanFunction = BaseData.Props.CanFunction;
				if (!(ShieldComp.PsychicRegen))
					ShieldComp.PsychicRegen = BaseData.Props.PsychicRegen;
				if (!(ShieldComp.EMPResistant))
					ShieldComp.EMPResistant = BaseData.Props.EMPResistant;
				if (!(ShieldComp.Adaptive))
					ShieldComp.Adaptive = BaseData.Props.Adaptive;
				ShieldComp.EnergyOnReset += BaseData.Props.EnergyOnReset;
				ShieldComp.RegenerationRate += BaseData.Props.RegenerationRate;
				ShieldComp.Hardened += BaseData.Props.Hardened;
			}

		}
		public override void PostMake()
		{
			//Log.Message("PostMake");
//			Log.Message(this.Label);
			float Value = 0f;
			base.PostMake();
			HediffCompPlasmaShield Test2 = this.TryGetComp<HediffCompPlasmaShield>();
			if (Test2 == null)
            {
				Log.Error("Error - Failed to find HediffCompPlasmaShield");
				return;
            }

			if (Test2 != null)
			{
				Value = Test2.Props.MaxEnergy;
			}
			if (Test2.Props.FunctionalModule)
            {
				this.FunctionalModule = true;
            }
			//			else
			//            {
			//				Log.Message("Welp");
			//			}
			//			Log.Message("Post Make!");
			//			Log.Message("Checking Value" + Value);

			ShieldComp = pawn.GetComp<VirCompShield2>();
            if (ShieldComp != null)
			{
				//				Log.Message("Non-Null!");
				//				Test.VirShieldActive = true;
				int Count = 0;

				List<Hediff> allHediffs = pawn.health.hediffSet.hediffs;
//				Log.Message(pawn.Label);
//				Log.Message(Label);
				for (int i = 0; i < allHediffs.Count; i++)
				{
					if (allHediffs[i] is HediffPlasmaShield_Implant)
					{
						HediffPlasmaShield_Implant Truehediff = allHediffs[i] as HediffPlasmaShield_Implant;
//						Log.Message(Truehediff.Label);
						if (Truehediff.Active && Truehediff.FunctionalModule)
						{
							if (this == Truehediff)
							{
//								Log.Message("Self, Skip");
							}
							else
							{ 
//							Log.Message("+1");
							Count++;
							}
						}
					}
				}
//				Log.Message("---");
//				Log.Message(Count.ToString());
				if ((FunctionalModule && Count > 0) || Active == false)
				{
					Active = false;
				}
				else
				{
					ShieldComp.MaxEnergy += Value;
					ShieldComp.BaseMaxEnergy += Value;
					if (Test2 != null)
					{
						ShieldComp.StartingTicksToReset += Test2.Props.StartingTicksToReset;
						if (!(ShieldComp.PsychicShield))
							ShieldComp.PsychicShield = Test2.Props.PsychicShield;
						if (!(ShieldComp.CanFunction))
							ShieldComp.CanFunction = Test2.Props.CanFunction;
						if (!(ShieldComp.PsychicRegen))
							ShieldComp.PsychicRegen = Test2.Props.PsychicRegen;
						if (!(ShieldComp.EMPResistant))
							ShieldComp.EMPResistant = Test2.Props.EMPResistant;
						if (!(ShieldComp.Adaptive))
							ShieldComp.Adaptive = Test2.Props.Adaptive;
						ShieldComp.EnergyOnReset += Test2.Props.EnergyOnReset;
						ShieldComp.RegenerationRate += Test2.Props.RegenerationRate;
						ShieldComp.Hardened += Test2.Props.Hardened;
					}
				}
				ShieldComp.NumOfHediffs++;
			}
			if (ShieldComp == null)
			{
				//				Log.Message("Null!");
				//				Log.Message("Adding Comp");
				Type T = typeof(VirCompShield2);

				//				ShieldComp = new VirCompShield2();
				ShieldComp = (VirCompShield2)Activator.CreateInstance(T);
				ShieldComp.parent = pawn;
				//int TestVal = pawn.AllComps.Count();
				//Log.Message((TestVal.ToString()));
				pawn.AllComps.Add(ShieldComp);
				ShieldComp.Initialize(ShieldComp.props);
					ShieldComp.MaxEnergy = Value;
					ShieldComp.Energy = ShieldComp.MaxEnergy;
					ShieldComp.BaseMaxEnergy = ShieldComp.MaxEnergy;
				if (Active)
				{
					if (Test2 != null)
					{
						ShieldComp.StartingTicksToReset = Test2.Props.StartingTicksToReset;
						ShieldComp.PsychicShield = Test2.Props.PsychicShield;
						ShieldComp.PsychicRegen = Test2.Props.PsychicRegen;
						ShieldComp.EMPResistant = Test2.Props.EMPResistant;
						ShieldComp.Adaptive = Test2.Props.Adaptive;
						ShieldComp.EnergyOnReset = Test2.Props.EnergyOnReset;
						ShieldComp.RegenerationRate = Test2.Props.RegenerationRate;
						ShieldComp.CanFunction = Test2.Props.CanFunction;
						ShieldComp.Hardened = Test2.Props.Hardened;
					}
				}
				ShieldComp.NumOfHediffs++;
				//TestVal = pawn.AllComps.Count();
				//Log.Message((TestVal.ToString()));

			}
			this.MaxEnergy = ShieldComp.MaxEnergy;
		}
		public override void PostRemoved()
		{
			base.PostRemoved();
			//			Log.Message("Post Remove!");
			//ShieldComp = pawn.GetComp<VirCompShield2>();
//			pawn.InitializeComps();
			if (ShieldComp != null)
			{
				if (!Active)
                {
					return;
                }
//				Log.Message("Non-Null!");
				HediffCompPlasmaShield Test2 = this.TryGetComp<HediffCompPlasmaShield>();
				if (Test2 != null)
				{
					ShieldComp.StartingTicksToReset -= Test2.Props.StartingTicksToReset;
					if (Test2.Props.PsychicShield)
						ShieldComp.PsychicShield = false; // Only a single instance can set this.
					if (Test2.Props.CanFunction)
						ShieldComp.CanFunction = false; // Only a single instance can set this.
					if (Test2.Props.PsychicRegen)
						ShieldComp.PsychicRegen = false; // Only a single instance can set this.
					if (Test2.Props.EMPResistant)
						ShieldComp.EMPResistant = false; // Only a single instance can set this.
					if (Test2.Props.Adaptive)
						ShieldComp.Adaptive = false; // Only a single instance can set this.
					ShieldComp.EnergyOnReset -= Test2.Props.EnergyOnReset;
					ShieldComp.RegenerationRate -= Test2.Props.RegenerationRate;
					ShieldComp.Hardened -= Test2.Props.Hardened;
				}
				ShieldComp.NumOfHediffs--;
				// Needs safety if more than one hediff affects this
				if (ShieldComp.NumOfHediffs <= 0)
					pawn.AllComps.Remove(ShieldComp);
				ShieldComp = null;
			}
//			if (Test == null)
//			{
//				Log.Message("Null!");
//			}
		}
	}



	[StaticConstructorOnStartup]
	public class Gizmo_PlasmaShieldStatus : Gizmo
	{
		public VirCompShield2 shield;

		private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

		private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

		public Gizmo_PlasmaShieldStatus()
		{
			Order = -100f;
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
			Widgets.Label(rect3, "PlasmaShieldLabel".Translate().Resolve());
//			Widgets.Label(rect3, "Plasma Shield");
			Rect rect4 = rect2;
			rect4.yMin = rect2.y + rect2.height / 2f;
			float fillPercent = shield.Energy / shield.MaxEnergy;
			Widgets.FillableBar(rect4, fillPercent, FullShieldBarTex, EmptyShieldBarTex, doBorder: false);
			Text.Font = GameFont.Small;
            if (shield.AdaptedDMG != null)
            {
				Text.Anchor = TextAnchor.LowerCenter;
				Widgets.Label(rect4, shield.AdaptedDMG.LabelCap);
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(rect4, (shield.Energy).ToString("F0") + " / " + (shield.MaxEnergy).ToString("F0"));
			}
			else
            {
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect4, (shield.Energy).ToString("F0") + " / " + (shield.MaxEnergy).ToString("F0"));
			}
			Text.Anchor = TextAnchor.UpperLeft;
			if (shield.Hardened > 0)
				TooltipHandler.TipRegion(rect2, "PlasmaShieldHardenedTip".Translate());
			else if (shield.EMPResistant)
				TooltipHandler.TipRegion(rect2, "PlasmaShieldResistantTip".Translate());
			else if (shield.Adaptive)
				TooltipHandler.TipRegion(rect2, "PlasmaShieldAdaptiveTip".Translate());
			else
				TooltipHandler.TipRegion(rect2, "PlasmaShieldTip".Translate());
//			TooltipHandler.TipRegion(rect2, "This pawn is shielded by a Plasma Shield. The shield protects from any attacks, and does not impair attacking from inside. EMP damage will still destroy it immediately.");
			return new GizmoResult(GizmoState.Clear);
		}
	}

	public class Recipe_VirInstallImplant : Recipe_Surgery
	{
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, delegate (BodyPartRecord record)
			{
				//Log.Message("Test");

				if (!pawn.health.hediffSet.GetNotMissingParts().Contains(record))
				{
					//Log.Message("Missing");

					return false;
				}
//				if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record))
//				{
//					Log.Message("Ancestor");
//					return false;
//				}
				return (!pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == record && (x.def == recipe.addsHediff || !recipe.CompatibleWithHediff(x.def)))) ? true : false;
			});
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			if (billDoer != null)
			{
				if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
				{
					return;
				}
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
			}
			pawn.health.AddHediff(recipe.addsHediff, part);
		}
	}

	public class Recipe_VirToggleImplant : Recipe_Surgery
	{
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			List<Hediff> allHediffs = pawn.health.hediffSet.hediffs;
			int Count = 0;
			for (int i = 0; i < allHediffs.Count; i++)
			{
				if (allHediffs[i] is HediffPlasmaShield_Implant)
				{
					HediffPlasmaShield_Implant Truehediff = allHediffs[i] as HediffPlasmaShield_Implant;
					if (Truehediff.Active && Truehediff.FunctionalModule)
					{
						if (allHediffs[i].def != recipe.removesHediff)
						{
							Count++;
							Log.Message(Count.ToString());
						}
					}
				}
			}
			for (int i = 0; i < allHediffs.Count; i++)
			{

				if (allHediffs[i].Part != null && allHediffs[i].def == recipe.removesHediff && allHediffs[i].Visible)
				{
					HediffPlasmaShield_Implant Truehediff = allHediffs[i] as HediffPlasmaShield_Implant;
					Log.Message(Truehediff.Active.ToString());
					Log.Message(Count.ToString());

					if ((Truehediff.Active == false && Count == 0) || Truehediff.Active)
						yield return allHediffs[i].Part;
				}
			}
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			MedicalRecipesUtility.IsClean(pawn, part);
			bool flag = IsViolationOnPawn(pawn, part, Faction.OfPlayer);
			if (billDoer != null)
			{
//				if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
//				{
//					return;
//				}
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
				if (!pawn.health.hediffSet.GetNotMissingParts().Contains(part))
				{
					return;
				}
				Hediff hediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => x.def == recipe.removesHediff);
				//HediffPlasmaShield_Implant Truehediff = pawn.health.hediffSet.GetHediffs<HediffPlasmaShield_Implant>();

			if (hediff != null)
				{
					HediffPlasmaShield_Implant Truehediff = hediff as HediffPlasmaShield_Implant;

					if (Truehediff.Active == true)
						Truehediff.DeactiveModule();
					else
						Truehediff.ActivateModule();

					//pawn.health.RemoveHediff(hediff);
				}
			}
			if (flag)
			{
				ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
			}
		}
	}

}