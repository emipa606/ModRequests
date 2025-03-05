using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using HarmonyLib;

namespace Krelinos_BiocodeIt
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatch
    {
        static HarmonyPatch()
        {
            var harmony = new Harmony("krelinos.BiocodeIt");
            harmony.PatchAll();
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(StealAIUtility))]
    [HarmonyLib.HarmonyPatch("TryFindBestItemToSteal")]
    class Patch_IgnoreBiocodedThingsWhenStealing
    {
        static void Postfix(IntVec3 root, Map map, float maxDist, ref Thing item, Pawn thief, List<Thing> disallowed, ref bool __result)
        {
            if (__result)
            {
                if (CompBiocodable.IsBiocoded(item))
                {
                    // 2 Nov 2022 (post 1.4) - Only steal from player's non-slave colonist
                    // Previously, raiders would steal their own fallen comrades' weapons if it was valuable enough, which was silly.
                    CompBiocodable itemCompBiocodable = item.TryGetComp<CompBiocodable>();
                    if( itemCompBiocodable.CodedPawn.IsFreeNonSlaveColonist
                        && BiocodeIt_Settings.biocodedStealChancePercent > 0
                        && UnityEngine.Random.Range( 0, 100 ) > BiocodeIt_Settings.biocodedStealChancePercent )
                    {
                        // Thing is biocoded to a player pawn and the steal chance (0% by default) proc-ed.
                        if ( BiocodeIt_Settings.notifyPlayerOfSpite )
                        {
                            Messages.Message( String.Format( "BiocodedNotifyPlayerOfSpiteAlert".Translate(), thief.NameShortColored, item.LabelShort ), thief, MessageTypeDefOf.NegativeEvent, true );
                        }
                    }

                    else    // Thing is biocoded to a non player pawn, or the steal chance didn't proc.
                    {
                        if ( disallowed == null )
                        {
                            disallowed = new List<Thing>();
                        }

                        disallowed.Add( item );   // Blacklist the item from future scans...
                        // ...And find a different Thing to steal
                        // NOTE: This makes TryFindBestItemToSteal recursive. I hope it doesn't affect things too badly.
                        __result = StealAIUtility.TryFindBestItemToSteal( root, map, maxDist, out item, thief, disallowed );
                    }
                }
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(StatPart_Biocoded))]
    [HarmonyLib.HarmonyPatch("TransformValue")]
    class Patch_BiocodedThingsStillHaveValueMaybe_ItDependsOnYourSettings
    {
        static bool Prefix(StatRequest req, ref float val)
        {
            if (req.HasThing && CompBiocodable.IsBiocoded(req.Thing))
            {
                val *= BiocodeIt_Settings.biocodedMarketValuePercent / 100f;
            }
            return false;
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(StatPart_Biocoded))]
    [HarmonyLib.HarmonyPatch("ExplanationPart")]            // If you ever get an error about an "Undefined target method for patch method",
               // it's because this ^ is mispelt... hhhhhhhhhhhhhhhhhhhhhhh
    class Patch_ExplainWhyBiocodedThingsAreMaybeWorthLessThanNonBiocodedThings
    {
        static void Postfix(StatRequest req, ref string __result)
        {
            if (req.HasThing && CompBiocodable.IsBiocoded(req.Thing))
            {
                __result = "BiocodedMarketValueMultiplier".Translate() + ": x" + BiocodeIt_Settings.biocodedMarketValuePercent + "%";
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(StatWorker))]
    [HarmonyLib.HarmonyPatch("DisplayTradeStats")]
    class Patch_BiocodedWeaponsNowShowMarketValue
    {
        static void Postfix(StatRequest req, ref bool __result)
        {
            if (!__result)
            {
                if( CompBiocodable.IsBiocoded(req.Thing))
                {
                    __result = true;
                }
            }
            //            ThingDef thingDef;
            //            __result = (thingDef = (req.Def as ThingDef)) != null
            //                && !req.HasThing                                                                                    // Yeah... this is a copy and paste from
            //                && (                                                                                                // the unpatched function but the check
            //                    (thingDef.category == ThingCategory.Building && thingDef.Minifiable)                            // for biocode is altered. If you ever wondered
            //                    || (TradeUtility.EverPlayerSellable(thingDef) || CompBiocodable.IsBiocoded(req.Thing))          // why the game never reports the value of
            //                    || (thingDef.tradeability.TraderCanSell()                                                       // non-minifiable builings in the info window,
            //                        && (thingDef.category == ThingCategory.Item || thingDef.category == ThingCategory.Pawn)     // this is why. Maybe I'll make a mod that
            //                       )                                                                                            // lets you. Or hey, maybe you can!
            //                   );
        }
    }
}