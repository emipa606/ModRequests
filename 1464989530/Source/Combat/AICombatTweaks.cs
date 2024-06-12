// Nightvision NightVision AICombatTweaks.cs
// 
// 31 10 2018
// 
// 31 10 2018

using Verse;

namespace NightVision
{
    public static class AICombatTweaks
    {
        public static float ModifyTargetAcquireRadiusForGlow(float radius, Pawn pawn)
        {
            if (radius.IsNonTrivial() && pawn != null)
            {
                var glowFactor = GlowFor.FactorOrFallBack(pawn);

                if (glowFactor.FactorIsNonTrivial())
                {
                    radius = radius * glowFactor * glowFactor;
                }
            }
            else
            {
                Log.Message($"Odd Parameters for ModifyTargetAquireRadius: radius ={radius} with pawn={pawn}");

            }

            return radius;
        }
    }
}