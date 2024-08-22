using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MeleePsycasts
{
    public static class MeleePsycastsUtils {
        public static void DamageRandomBodyPart(CompProperties_AbilityBasic props, Pawn pawn, Pawn target, int repeat = 0, BodyPartHeight height = BodyPartHeight.Undefined, BodyPartDepth depth = BodyPartDepth.Undefined)
        {
            for(int i = 0; i < repeat; i++)
            {
                target.TakeDamage(
                new DamageInfo(
                    props.damageDef,
                    props.damage,
                    props.armourPen,
                    -1, pawn,
                    target?.health.hediffSet.GetRandomNotMissingPart(props.damageDef, BodyPartHeight.Middle, BodyPartDepth.Outside), pawn.equipment?.Primary?.def));
            }
        }
    }

    public class BaseCompAbilityEffect : CompAbilityEffect
    {
        public override bool GizmoDisabled(out string reason)
        {
            if (parent.pawn.equipment.Primary == null)
            {
                reason = "Pawn requires a melee weapon";
                return true;
            }
            else if (!parent.pawn.equipment.Primary.def.IsMeleeWeapon)
            {
                reason = "Pawn requires a melee weapon";
                return true;
            }

            reason = "Pawn has a melee weapon";
            return false;
        }
    }

    public class CompProperties_AbilityBasic : CompProperties_AbilityEffect
    {
        public DamageDef damageDef = null;
        public float damage;
        public float armourPen;
    }

    public class CompAbilityEffect_Slice : BaseCompAbilityEffect
    {
        CompProperties_AbilityBasic _Props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            _Props = (CompProperties_AbilityBasic)Props;

            if (target.Pawn == null || _Props == null)
                return;

            List<IntVec3> positionsAround = new List<IntVec3>();
            positionsAround.Add(target.Pawn.Position + IntVec3.North);
            positionsAround.Add(target.Pawn.Position + IntVec3.South);
            positionsAround.Add(target.Pawn.Position + IntVec3.East);
            positionsAround.Add(target.Pawn.Position + IntVec3.West);

            foreach(IntVec3 pos in positionsAround)
            {
                if (!pos.Impassable(target.Pawn.Map))
                {
                    parent.pawn.Position = pos;

                    MeleePsycastsUtils.DamageRandomBodyPart(_Props, parent.pawn, target.Pawn, 1, BodyPartHeight.Undefined, BodyPartDepth.Undefined);
                    MoteMaker.ThrowText(target.Pawn.PositionHeld.ToVector3(), target.Pawn.MapHeld, "Psychic Slice!", 3f);
                    return;
                }
            }

            MoteMaker.ThrowText(parent.pawn.PositionHeld.ToVector3(), parent.pawn.MapHeld, "Failed Psychic Slice", 3f);
        }
    }

    public class CompProperties_AbilitySkewer : CompProperties_AbilityBasic
    {
        public BodyPartDef torsoDef;

        public CompProperties_AbilitySkewer()
        {
            compClass = typeof(CompAbilityEffect_Skewer);
        }
    }

    public class CompAbilityEffect_Skewer : BaseCompAbilityEffect
    {
        CompProperties_AbilitySkewer _Props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            _Props = (CompProperties_AbilitySkewer)Props;

            if (target.Pawn == null || _Props == null)
                return;

            BodyPartRecord torso = target.Pawn?.health.hediffSet.GetNotMissingParts(0, 0, null, null).FirstOrDefault(x => x.def == _Props.torsoDef);

            if (torso == null)
                return;

            target.Pawn.TakeDamage(new DamageInfo(_Props.damageDef, _Props.damage, _Props.armourPen, -1, parent.pawn, torso, parent.pawn.equipment.Primary.def));
            MoteMaker.ThrowText(target.Pawn.PositionHeld.ToVector3(), target.Pawn.MapHeld, "Skewer!", 6f);
        }
    }

    public class CompAbilityEffect_DisablingStrike : BaseCompAbilityEffect
    {
        CompProperties_AbilityBasic _Props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            _Props = (CompProperties_AbilityBasic)Props;

            if (target.Pawn == null || _Props == null)
                return;

            MeleePsycastsUtils.DamageRandomBodyPart(_Props, parent.pawn, target.Pawn, 3, BodyPartHeight.Middle, BodyPartDepth.Outside);

            MoteMaker.ThrowText(target.Pawn.PositionHeld.ToVector3(), target.Pawn.MapHeld, "Disable!", 3f);
        }

        public override bool GizmoDisabled(out string reason)
        {
            reason = "Disable does not require a melee weapon";
            return false;
        }
    }

    public class CompAbilityEffect_Charge : BaseCompAbilityEffect
    {
        CompProperties_AbilityBasic _Props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            _Props = (CompProperties_AbilityBasic)Props;

            if (target.Pawn == null || _Props == null)
                return;

            List<IntVec3> positionsAround = new List<IntVec3>();
            positionsAround.Add(target.Pawn.Position + IntVec3.North);
            positionsAround.Add(target.Pawn.Position + IntVec3.South);
            positionsAround.Add(target.Pawn.Position + IntVec3.East);
            positionsAround.Add(target.Pawn.Position + IntVec3.West);

            foreach (IntVec3 pos in positionsAround)
            {
                if (!pos.Impassable(target.Pawn.Map))
                {
                    parent.pawn.Position = pos;

                    MeleePsycastsUtils.DamageRandomBodyPart(_Props, parent.pawn, target.Pawn, 2, BodyPartHeight.Undefined, BodyPartDepth.Outside);

                    MoteMaker.ThrowText(target.Pawn.PositionHeld.ToVector3(), target.Pawn.MapHeld, "Charge!", 3f);
                    return;
                }
            }

            MoteMaker.ThrowText(parent.pawn.PositionHeld.ToVector3(), parent.pawn.MapHeld, "Failed Charge", 6f);
        }
    }

    public class CompAbilityEffect_ChargeSkewer : BaseCompAbilityEffect
    {
        CompProperties_AbilitySkewer _Props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            _Props = (CompProperties_AbilitySkewer)Props;

            if (target.Pawn == null || _Props == null)
                return;

            List<IntVec3> positionsAround = new List<IntVec3>();
            positionsAround.Add(target.Pawn.Position + IntVec3.North);
            positionsAround.Add(target.Pawn.Position + IntVec3.South);
            positionsAround.Add(target.Pawn.Position + IntVec3.East);
            positionsAround.Add(target.Pawn.Position + IntVec3.West);

            foreach (IntVec3 pos in positionsAround)
            {
                if (!pos.Impassable(target.Pawn.Map))
                {
                    parent.pawn.Position = pos;

                    MeleePsycastsUtils.DamageRandomBodyPart(_Props, parent.pawn, target.Pawn, 2, BodyPartHeight.Undefined, BodyPartDepth.Undefined);

                    BodyPartRecord torso = target.Pawn?.health.hediffSet.GetNotMissingParts(0, 0, null, null).FirstOrDefault(x => x.def == _Props.torsoDef);

                    if (torso == null || target == null)
                        return;

                    target.Pawn.TakeDamage(new DamageInfo(_Props.damageDef, _Props.damage, _Props.armourPen, -1, parent.pawn, torso, parent.pawn.equipment.Primary.def));

                    MoteMaker.ThrowText(target.Pawn.PositionHeld.ToVector3(), target.Pawn.MapHeld, "Charge!", 3f);
                    return;
                }
            }

            MoteMaker.ThrowText(parent.pawn.PositionHeld.ToVector3(), parent.pawn.MapHeld, "Failed Charge", 6f);
        } 
    }
    public class CompProperties_AbilityRepeatAttack : CompProperties_AbilityBasic
    {
        public int repeatAmount = 2;
        public bool requiresMelee = true;

        public CompProperties_AbilityRepeatAttack()
        {
            compClass = typeof(CompAbilityEffect_RepeatAttack);
        }
    }
    public class CompAbilityEffect_RepeatAttack : BaseCompAbilityEffect
    {
        CompProperties_AbilityRepeatAttack _Props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            _Props = (CompProperties_AbilityRepeatAttack)Props;

            if (target.Pawn == null || _Props == null)
                return;

            MeleePsycastsUtils.DamageRandomBodyPart(_Props, parent.pawn, target.Pawn, _Props.repeatAmount, BodyPartHeight.Undefined, BodyPartDepth.Undefined);
        }

        public override bool GizmoDisabled(out string reason)
        {
            _Props = (CompProperties_AbilityRepeatAttack)Props;

            if (_Props == null)
            {
                reason = "";
                return false;
            }

            if (_Props.requiresMelee)
            {
                return base.GizmoDisabled(out reason);
            }
            else
            {
                reason = "";
                return false;
            }
        }
    }
    public class CompAbilityEffect_SpinCut : BaseCompAbilityEffect
    {
        CompProperties_AbilityBasic _Props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            _Props = (CompProperties_AbilityBasic)Props;

            if (_Props == null)
                return;

            if (target.Cell.Impassable(parent.pawn.Map))
            {
                MoteMaker.ThrowText(parent.pawn.PositionHeld.ToVector3(), parent.pawn.MapHeld, "Failed SpinCut", 3f);
                return;
            }

            parent.pawn.Position = target.Cell;

            List<IntVec3> positionsAround = GenRadial.RadialCellsAround(target.Cell, 2, false).ToList();

            foreach(IntVec3 pos in positionsAround)
            {
                List<Pawn> pawnList = parent.pawn.Map.mapPawns.AllPawns.ListFullCopy();

                foreach(Pawn pawn in pawnList)
                {
                    if(pawn != null && pos == pawn.Position)
                    {
                        MeleePsycastsUtils.DamageRandomBodyPart(_Props, parent.pawn, pawn, 1, BodyPartHeight.Undefined, BodyPartDepth.Undefined);
                    }
                }
            }
            MoteMaker.ThrowText(parent.pawn.PositionHeld.ToVector3(), parent.pawn.MapHeld, "SPIN CUT!", 3f);
        }
    }

    public class CompProperties_AbilityLegCut : CompProperties_AbilityBasic
    {
        public BodyPartDef legDef;

        public CompProperties_AbilityLegCut()
        {
            compClass = typeof(CompAbilityEffect_LegCut);
        }
    }

    public class CompAbilityEffect_LegCut : BaseCompAbilityEffect
    {
        CompProperties_AbilityLegCut _Props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            _Props = (CompProperties_AbilityLegCut)Props;

            if (target.Pawn == null || _Props == null)
                return;

            List<BodyPartRecord> bps = target.Pawn?.health.hediffSet.GetNotMissingParts(0, 0, null, null).ToList();
            BodyPartRecord leg = bps.FirstOrDefault(x => x.def == _Props.legDef);

            target.Pawn.TakeDamage(new DamageInfo(_Props.damageDef, _Props.damage, _Props.armourPen, -1, parent.pawn, leg, parent.pawn.equipment.Primary.def));

            bps = target.Pawn?.health.hediffSet.GetNotMissingParts(BodyPartHeight.Bottom, BodyPartDepth.Outside, null, null).ToList();
            leg = bps.FirstOrDefault(x => x.def == _Props.legDef);

            if(leg.def.defName != _Props.legDef.defName)
            {
                bps = target.Pawn?.health.hediffSet.GetNotMissingParts(BodyPartHeight.Bottom, BodyPartDepth.Outside, null, null).ToList();
                leg = bps.FirstOrDefault(x => x.def == _Props.legDef);

                target.Pawn.TakeDamage(new DamageInfo(_Props.damageDef, _Props.damage, _Props.armourPen, -1, parent.pawn, leg, parent.pawn.equipment.Primary.def));
            }

            target.Pawn.TakeDamage(new DamageInfo(_Props.damageDef, _Props.damage, _Props.armourPen, -1, parent.pawn, leg, parent.pawn.equipment.Primary.def));

            MoteMaker.ThrowText(target.Pawn.PositionHeld.ToVector3(), target.Pawn.MapHeld, "Leg Cut!", 3f);
        }
    }
    public class CompProperties_AbilityAttackLimb : CompProperties_AbilityBasic
    {
        public BodyPartDef limbDef;
        public string successMessage = "";

        public CompProperties_AbilityAttackLimb()
        {
            compClass = typeof(CompAbilityEffect_AttackLimb);
        }
    }

    public class CompAbilityEffect_AttackLimb : BaseCompAbilityEffect
    {
        CompProperties_AbilityAttackLimb _Props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            _Props = (CompProperties_AbilityAttackLimb)Props;

            if (target.Pawn == null || _Props == null)
                return;

            BodyPartRecord bp = target.Pawn?.health.hediffSet.GetNotMissingParts(0, 0, null, null).FirstOrDefault(x => x.def == _Props.limbDef);

            if (bp == null)
            {
                MoteMaker.ThrowText(target.Pawn.PositionHeld.ToVector3(), target.Pawn.MapHeld, "Failed!", 3f);
                return;
            }

            target.Pawn.TakeDamage(new DamageInfo(_Props.damageDef, _Props.damage, _Props.armourPen, -1, parent.pawn, bp, parent.pawn.equipment.Primary.def));
            MoteMaker.ThrowText(target.Pawn.PositionHeld.ToVector3(), target.Pawn.MapHeld, _Props.successMessage, 3f);
        }
    }

    public class CompProperties_AbilityApplyHediff: CompProperties_AbilityBasic
    {
        public HediffDef hediffDef;
        public bool requiresMelee = true;

        public CompProperties_AbilityApplyHediff()
        {
            compClass = typeof(CompAbilityEffect_ApplyHediff);
        }
    }
    public class CompAbilityEffect_ApplyHediff : BaseCompAbilityEffect
    {
        CompProperties_AbilityApplyHediff _Props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            _Props = (CompProperties_AbilityApplyHediff)Props;

            if (target.Pawn == null || _Props == null)
                return;

            target.Pawn?.health.AddHediff(HediffMaker.MakeHediff(_Props.hediffDef, target.Pawn));
        }

        public override bool GizmoDisabled(out string reason)
        {
            _Props = (CompProperties_AbilityApplyHediff)Props;

            if (_Props == null)
            {
                reason = "";
                return false;
            }

            if (_Props.requiresMelee)
            {
                return base.GizmoDisabled(out reason);
            }
            else
            {
                reason = "";
                return false;
            }
        }
    }
}
