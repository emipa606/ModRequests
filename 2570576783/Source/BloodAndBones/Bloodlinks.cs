// RimWorld.CompBladelinkWeapon

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace PorousBoat.BloodAndBones
{
	public class CompBloodlinkWeapon : CompBiocodable
	{
		protected int lastKillTick = -1;
		protected List<WeaponTraitDef_Bone> traits = new List<WeaponTraitDef_Bone>();
		protected static readonly IntRange TraitsRange = new IntRange(1, 2);
		protected bool oldBonded;
		protected string oldBondedPawnLabel;
		protected Pawn oldBondedPawn;
		public List<WeaponTraitDef_Bone> TraitsListForReading => traits;

		public int TicksSinceLastKill
		{
			get
			{
				if (lastKillTick < 0)
				{
					return 0;
				}

				return Find.TickManager.TicksAbs - lastKillTick;
			}
		}

		public override bool Biocodable
		{
			get
			{
				if (!traits.NullOrEmpty())
				{
					for (int i = 0; i < traits.Count; i++)
					{
						if (traits[i].neverBond)
						{
							return false;
						}
					}
				}

				return true;
			}
		}

		public override void PostPostMake()
		{
			InitializeTraits();
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			UnCode();
		}

        protected virtual void InitializeTraits()
        {
	        String traitName = "";
            switch (this.parent.def.defName)
            {
                case "BoneWeapon_Dagger":
	                traitName = "ShadowDance";
                    break;
                case "BoneWeapon_Sword":
	                traitName = "UnholyCuts";
                    break;
                case "BoneWeapon_Scythe":
	                traitName = "Reaping";
                    break;
                default:
	                traitName = "ShadowDance";
                    break;
            }

            List<WeaponTraitDef> boneTraits = new List<WeaponTraitDef>();
            boneTraits.Add(DefDatabase<WeaponTraitDef>.GetNamed("NeedBlood"));
	        boneTraits.Add(DefDatabase<WeaponTraitDef>.GetNamed(traitName));
	        foreach (var t in boneTraits)
	        {
		        traits.Add(new WeaponTraitDef_Bone(t));
	        }
        }

		public override void Notify_Equipped(Pawn pawn)
		{
			base.Notify_Equipped(pawn);
			if (!traits.NullOrEmpty())
			{
				for (int i = 0; i < traits.Count; i++)
				{
					traits[i].Worker_Bone.Notify_Equipped(pawn);
				}
			}
		}

		public override void CodeFor(Pawn pawn)
		{
			if (base.Biocodable)
			{
				if (pawn.IsColonistPlayerControlled && base.CodedPawn == null)
				{
					Find.LetterStack.ReceiveLetter(
						"LetterBladelinkWeaponBondedLabel".Translate(pawn.Named("PAWN"), parent.Named("WEAPON")),
						"LetterBladelinkWeaponBonded".Translate(pawn.Named("PAWN"), parent.Named("WEAPON")),
						LetterDefOf.PositiveEvent, new LookTargets(pawn));
				}

				base.CodeFor(pawn);
			}
		}

		protected override void OnCodedFor(Pawn pawn)
		{
			lastKillTick = GenTicks.TicksAbs;
			pawn.equipment.bondedWeapon = parent;
			if (!traits.NullOrEmpty())
			{
				for (int i = 0; i < traits.Count; i++)
				{
					traits[i].Worker_Bone.Notify_Bonded(pawn);
				}
			}
		}

		public override void Notify_KilledPawn(Pawn pawn)
		{
			lastKillTick = Find.TickManager.TicksAbs;
			if (!traits.NullOrEmpty())
			{
				for (int i = 0; i < traits.Count; i++)
				{
					traits[i].Worker_Bone.Notify_KilledPawn(pawn);
				}
			}
		}

		public void Notify_EquipmentLost(Pawn pawn)
		{
			if (!traits.NullOrEmpty())
			{
				for (int i = 0; i < traits.Count; i++)
				{
					traits[i].Worker_Bone.Notify_EquipmentLost(pawn);
				}
			}
		}

		public void Notify_WieldedOtherWeapon()
		{
			if (!traits.NullOrEmpty())
			{
				for (int i = 0; i < traits.Count; i++)
				{
					traits[i].Worker_Bone.Notify_OtherWeaponWielded(this);
				}
			}
		}

		public override void UnCode()
		{
			if (base.CodedPawn != null)
			{
				base.CodedPawn.equipment.bondedWeapon = null;
				if (!traits.NullOrEmpty())
				{
					for (int i = 0; i < traits.Count; i++)
					{
						traits[i].Worker_Bone.Notify_Unbonded(base.CodedPawn);
					}
				}
			}

			base.UnCode();
			lastKillTick = -1;
		}

		public override string CompInspectStringExtra()
		{
			string text = "";
			if (!traits.NullOrEmpty())
			{
				text += "Stat_Thing_BloodlinkWeaponTrait_Label".Translate() + ": " +
				        traits.Select((WeaponTraitDef x) => x.label).ToCommaList().CapitalizeFirst();
			}

			if (Biocodable)
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}

				text = ((base.CodedPawn != null)
					? (text + "BondedWith".Translate(base.CodedPawnLabel.ApplyTag(TagType.Name)).Resolve())
					: ((string) (text + "NotBonded".Translate())));
			}

			return text;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref lastKillTick, "lastKillTick", -1);
			Scribe_Collections.Look(ref traits, "traits", LookMode.Def);
			if (Scribe.mode != LoadSaveMode.Saving)
			{
				Scribe_Values.Look(ref oldBonded, "bonded", defaultValue: false);
				Scribe_Values.Look(ref oldBondedPawnLabel, "bondedPawnLabel");
				Scribe_References.Look(ref oldBondedPawn, "bondedPawn", saveDestroyedThings: true);
			}

			if (Scribe.mode != LoadSaveMode.PostLoadInit)
			{
				return;
			}

			if (oldBonded)
			{
				CodeFor(oldBondedPawn);
			}

			if (traits == null)
			{
				traits = new List<WeaponTraitDef_Bone>();
			}

			if (oldBondedPawn != null)
			{
				if (string.IsNullOrEmpty(oldBondedPawnLabel) || !oldBonded)
				{
					codedPawnLabel = oldBondedPawn.Name.ToStringFull;
					biocoded = true;
				}

				if (oldBondedPawn.equipment.bondedWeapon == null)
				{
					oldBondedPawn.equipment.bondedWeapon = parent;
				}
				else if (oldBondedPawn.equipment.bondedWeapon != parent)
				{
					UnCode();
				}
			}
		}

		public override string TransformLabel(string label)
		{
			return label;
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			if (traits.NullOrEmpty())
			{
				yield break;
			}

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Stat_Thing_BloodlinkWeaponTrait_Desc".Translate());
			stringBuilder.AppendLine();
			for (int i = 0; i < traits.Count; i++)
			{
				stringBuilder.AppendLine(traits[i].LabelCap + ": " + traits[i].description);
				if (i < traits.Count - 1)
				{
					stringBuilder.AppendLine();
				}
			}

			yield return new StatDrawEntry(StatCategoryDefOf.Weapon, "Stat_Thing_BloodlinkWeaponTrait_Label".Translate(),
				traits.Select((WeaponTraitDef x) => x.label).ToCommaList().CapitalizeFirst(), stringBuilder.ToString(),
				1104);
		}
	}

	public class WeaponTraitDef_Bone : WeaponTraitDef
	{
		protected WeaponTraitWorker_Bone worker_bone;

		public WeaponTraitDef_Bone(WeaponTraitDef t)
		{
			equippedStatOffsets = t.equippedStatOffsets;
			equippedHediffs = t.equippedHediffs;
			bondedHediffs = t.bondedHediffs;
			bondedThought = t.bondedThought;
			killThought = t.killThought;
			workerClass = t.workerClass;
			exclusionTags = t.exclusionTags;
			commonality = t.commonality;
			marketValueOffset = t.marketValueOffset;
			neverBond = t.neverBond;
		}
		public WeaponTraitWorker_Bone Worker_Bone
		{
			get
			{
				if (worker_bone == null)
				{
					worker_bone = (WeaponTraitWorker_Bone) Activator.CreateInstance(workerClass);
				}

				return worker_bone;
			}
		}
	}

	public class WeaponTraitWorker_Bone : WeaponTraitWorker
	{
		public void Notify_OtherWeaponWielded(CompBloodlinkWeapon weapon)
		{
			
		}
	}

	public class WeaponTraitWorker_BloodThirst : WeaponTraitWorker_Bone
	{
		
	}

	public class WeaponTraitWorker_ShadowDance : WeaponTraitWorker_Bone
	{
		public override void Notify_Equipped(Pawn pawn)
		{
			base.Notify_Equipped(pawn);
			
			TraitSet ts = new TraitSet(pawn);
			TraitDef td = DefDatabase<TraitDef>.GetNamed("Nimble");
			if (!ts.HasTrait(td))
			{
				Trait t = new Trait(td);
				ts.GainTrait(t);
			}
		}

		public override void Notify_EquipmentLost(Pawn pawn)
		{
			base.Notify_EquipmentLost(pawn);
			
			TraitSet ts = new TraitSet(pawn);
			TraitDef td = DefDatabase<TraitDef>.GetNamed("Nimble");
			if (!ts.HasTrait(td))
			{
				Trait t = new Trait(td);
				ts.RemoveTrait(t);
			}
		}
	}
}