// Nightvision NightVision AutoQualifier.cs
// 
// 26 06 2018
// 
// 21 07 2018

using RimWorld;

using Verse;

namespace NightVision
{
    public static class AutoQualifier
    {
        public static VisionType? HediffCheck(
            HediffDef hediffDef
        )
        {
            if (hediffDef.addedPartProps is AddedBodyPartProps abpp
             && (abpp.partEfficiency > 1.0f
              || hediffDef.stages?.Exists(
                     stage => stage.capMods?.Exists(
                                  pcm
                                      =>
                                      pcm
                                          .capacity
                                   == PawnCapacityDefOf
                                          .Sight
                                   && (!pcm.SetMaxDefined
                                    || pcm.setMax > 1.0001)
                                   && (pcm.offset
                                     > 0.0001
                                    || pcm.postFactor > 1.0001)
                              )
                           == true
                 )
              == true))

            {
                return VisionType.NVNightVision;
            }

            return null;
        }
    }
}