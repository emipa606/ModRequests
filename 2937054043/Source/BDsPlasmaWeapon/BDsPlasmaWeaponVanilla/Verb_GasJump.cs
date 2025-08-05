using UnityEngine;
using RimWorld;
using Verse;
using System.Security.Cryptography;
using System.Runtime.Remoting.Messaging;

namespace BDsPlasmaWeaponVanilla
{
    public class Verb_GasJump : Verb_Jump
    {
        public DefModExtension_GasJump Data
        {
            get
            {
                return ReloadableCompSource.parent.def.GetModExtension<DefModExtension_GasJump>();
            }
        }

        public new CompReloadableFromFiller ReloadableCompSource => DirectOwner as CompReloadableFromFiller;

        public override bool MultiSelect => true;
        public override float EffectiveRange
        {
            get
            {
                float radius = Data.radius;
                int consumption = Data.maxConsumption;
                if (ReloadableCompSource.RemainingCharges < consumption)
                {
                    radius *= ReloadableCompSource.RemainingCharges / (float)consumption;
                }
                return radius;
            }
        }

        protected override bool TryCastShot()
        {
            if (!ModLister.CheckRoyaltyOrBiotech("Jumping"))
            {
                return false;
            }
            if (ReloadableCompSource != null && !ReloadableCompSource.CanBeUsed)
            {
                return false;
            }
            ReloadableCompSource?.UsedOnce();
            IntVec3 position = CasterPawn.Position;
            IntVec3 cell = currentTarget.Cell;
            Map map = CasterPawn.Map;
            GenExplosion.DoExplosion(CasterPawn.Position, CasterPawn.Map, Data.blastCloudRadius, RimWorld.DamageDefOf.Extinguish, null, -1, -1f, null, null, null, null, null, 0f, 1, GasType.BlindSmoke);
            ReloadableCompSource.DrawGas(Data.maxConsumption);
            bool flag = Find.Selector.IsSelected(CasterPawn);
            PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(RimWorld.ThingDefOf.PawnFlyer, CasterPawn, cell, verbProps.flightEffecterDef, verbProps.soundLanding, verbProps.flyWithCarriedThing);
            if (pawnFlyer != null)
            {
                FleckMaker.ThrowDustPuff(position.ToVector3Shifted() + Gen.RandomHorizontalVector(0.5f), map, 2f);
                GenSpawn.Spawn(pawnFlyer, cell, map);
                if (flag)
                {
                    Find.Selector.Select(CasterPawn, playSound: false, forceDesignatorDeselect: false);
                }
                return true;
            }
            return false;
        }
        public override void DrawHighlight(LocalTargetInfo target)
        {
            if (EffectiveRange <= 0)
            {
                return;
            }
            if (caster == null || caster.Spawned)
            {
                if (target.IsValid && JumpUtility.ValidJumpTarget(caster.Map, target.Cell))
                {
                    GenDraw.DrawTargetHighlightWithLayer(target.CenterVector3, AltitudeLayer.MetaOverlays);
                }
                GenDraw.DrawRadiusRing(caster.Position, EffectiveRange, Color.white, (IntVec3 c) => GenSight.LineOfSight(caster.Position, c, caster.Map) && JumpUtility.ValidJumpTarget(caster.Map, c));
            }
        }
    }

    public class DefModExtension_GasJump : DefModExtension
    {
        public string icon = "UI/Commands/DesirePower";
        public string label = "BDP_GasJumpLabel";
        public string description = "BDP_GasJumpDesc";
        public float radius = 5;
        public int maxConsumption = 100;
        public float blastCloudRadius = 2;
    }
}
