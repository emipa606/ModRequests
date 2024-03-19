using RimWorld;
using Verse;

namespace RBM_Minotaur
{
    public class CompAbilityEffect_Terrify : CompAbilityEffect
    {

        public new CompProperties_AbilityTerrify Props
        {
            get
            {
                return (CompProperties_AbilityTerrify)this.props;
            }
        }

        //Main 'See Red' ability method - terrifies pawns in an area and enrages the user.
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: (See Red) Apply(LocalTargetInfo target, LocalTargetInfo dest)"); }
            Pawn pawn = this.parent.pawn;
            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            float radius = MinotaurSettings.SeeRedFearRadius;

            base.Apply(target, dest);

            //pawn.health.AddHediff(RBM_DefOf.HeDiffSeeRed);
            GenExplosion.DoExplosion(target.Cell, map, radius, DamageDefOf.Smoke, null, -1, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null, null, true, 1f, 0f, true, null, 1f);
            RBM_Utils.TerrifyInArea(position, map, radius, pawn);
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: (See Red) public override void DrawEffectPreview(LocalTargetInfo target)"); }
            GenDraw.DrawRadiusRing(target.Cell, MinotaurSettings.SeeRedFearRadius);
        }
    }

    public class CompProperties_AbilityTerrify : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityTerrify()
        {
            this.compClass = typeof(CompAbilityEffect_Terrify);
        }
    }
}
