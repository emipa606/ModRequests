using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace BDsPlasmaWeapon
{
    public class Verb_ActiveVent : Verb
    {
        public DefModExtension_ActiveVent Data
        {
            get
            {
                return ReloadableCompSource.parent.def.GetModExtension<DefModExtension_ActiveVent>();
            }
        }

        public new CompReloadableFromFiller ReloadableCompSource => DirectOwner as CompReloadableFromFiller;

        protected override bool TryCastShot()
        {
            Pop(ReloadableCompSource);
            return true;
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            float radius = Data.radius;
            int consumption = Data.maxConsumption;
            if (ReloadableCompSource.remainingCharges < consumption)
            {
                radius = radius * ((float)ReloadableCompSource.remainingCharges / (float)consumption);
            }
            return radius;
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            DrawHighlightFieldRadiusAroundTarget(caster);
        }

        public void Pop(CompReloadableFromFiller comp)
        {
            if (comp != null && comp.CanBeUsed && ReloadableCompSource.compActiveVentData != null)
            {
                float radius = Data.radius;
                int consumption = Data.maxConsumption;
                if (comp.remainingCharges < consumption)
                {
                    radius = radius * ((float)comp.remainingCharges / consumption);
                    consumption = comp.remainingCharges;
                }
                Pawn wearer = comp.Wearer;
                GenExplosion.DoExplosion(wearer.Position, wearer.Map, radius, RimWorld.DamageDefOf.Extinguish, null, -1, -1f, null, null, null, null, null, 0f, 1, GasType.BlindSmoke);
                comp.DrawGas(consumption);
            }
        }
    }
    public class DefModExtension_ActiveVent : DefModExtension
    {
        public string icon = "UI/Commands/DesirePower";
        public string label = "BDP_ActiveVentLabel";
        public string description = "BDP_ActiveVentDesc";
        public float radius = 5;
        public int maxConsumption = 100;
        public float heatPushPerUnit = -1;
    }
}
