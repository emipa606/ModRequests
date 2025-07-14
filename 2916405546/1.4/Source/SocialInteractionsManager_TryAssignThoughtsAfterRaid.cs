using HarmonyLib;
using RimWorld;
using System.Linq;
using VanillaSocialInteractionsExpanded;
using Verse;
using Verse.AI.Group;

namespace VSIERationalTraitDevelopment
{
    [HarmonyPatch(typeof(SocialInteractionsManager), "TryAssignThoughtsAfterRaid")]
    public static class SocialInteractionsManager_TryAssignThoughtsAfterRaid
    {
        public static bool Prefix(SocialInteractionsManager __instance, Lord lord, bool raidersIsLosing)
        {
            TryAssignThoughtsAfterRaid(__instance, lord, raidersIsLosing);
            return false;
        }

        public static void TryAssignThoughtsAfterRaid(SocialInteractionsManager __instance, Lord lord, bool raidersIsLosing = false)
        {
            if (__instance.raidGroups != null)
            {
                for (int j = __instance.raidGroups.Count - 1; j >= 0; j--)
                {
                    if ((__instance.raidGroups[j].raiderLords?.Contains(lord) ?? false) && __instance.raidGroups[j].raiderLords.Count > 1)
                    {
                        __instance.raidGroups[j].raiderLords.Remove(lord);
                        return;
                    }
                }
                var raidGroup = __instance.raidGroups.Where(x => x.raiderLords?.Contains(lord) ?? false).FirstOrDefault();
                if (raidGroup != null)
                {
                    raidGroup.defenders.RemoveWhere(x => x is null);
                    raidGroup.raiders.RemoveWhere(x => x is null);

                    var remainedRaidersCount = raidGroup.raiders.Where(x => !x.Dead && !x.Downed).Count();
                    var remainedRaidersPercentFromTotal = (float)remainedRaidersCount / (float)raidGroup.raiders.Count();

                    var remainedDefenders = raidGroup.defenders.Where(x => !x.Dead && !x.Downed && x.Map == lord.Map).ToHashSet();
                    remainedDefenders.AddRange(lord.Map.mapPawns.AllPawnsSpawned.Where(x => x.RaceProps.Humanlike && !x.Downed && !x.Dead && !x.Fogged() && !x.IsPrisoner && x.Faction != null
                        && (x.Faction == Faction.OfPlayer || !x.HostileTo(Faction.OfPlayer)) && x.HostileTo(raidGroup.faction)));
                    var remainedDefendersPercentFromTotal = (float)remainedDefenders.Count() / (float)raidGroup.defenders.Count();

                    if (!raidersIsLosing && __instance.RaidersWon(raidGroup, lord.Map))
                    {
                        foreach (var pawn in remainedDefenders)
                        {
                            var traits = pawn.story?.traits;
                            if (traits != null)
                            {
                                var nerves = traits.GetTrait(TraitDefOf.Nerves);
                                var naturalMood = traits.GetTrait(TraitDefOf.NaturalMood);
                                if (nerves != null && (nerves.Degree == 2 || nerves.Degree == 1) ||
                                    traits.HasTrait(TraitDefOf.Kind) ||
                                    naturalMood != null && (naturalMood.Degree == 2 || naturalMood.Degree == 1))
                                {
                                    pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(VSIE_DefOf.VSIE_SufferedLossPositive);
                                }
                                else if (traits.HasTrait(TraitDefOf.Greedy) ||
                                    nerves != null && (nerves.Degree == -2 || nerves.Degree == -1) ||
                                    naturalMood != null && (naturalMood.Degree == -2 || naturalMood.Degree == -1))
                                {
                                    pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(VSIE_DefOf.VSIE_SufferedLossNegative);
                                }
                            }
                        }
                        __instance.postRaidPeriodTicks = Find.TickManager.TicksGame + (GenDate.TicksPerDay * 3);
                    }

                    if (((Find.TickManager.TicksGame - raidGroup.initTime) >= (GenDate.TicksPerDay / 4) || // if raid lasted over 6 in-game hours
                        raidGroup.defenders.Count / (float)raidGroup.defenders.Where(x => x.Downed).Count() >= 0.5f //  raid that at least half of your colony downed could be also considered as a tough raid
                        || raidGroup.defenders.Any(x => x.Dead)) // a raid any of your colonist died could be considered as a tough raid
                        && remainedDefenders.Any()) // if there are survivors to give them a trait)
                    {
                        var candidates = remainedDefenders.Where(x => !__instance.pawnsWithAdditionalTrait?.Contains(x) ?? false);
                        if (candidates.Any() && candidates.TryRandomElement(out Pawn pawn))
                        {
                            VSIE_Utils.TryDevelopNewTrait(pawn, "VSIE.ToughtRaidEvent".Translate(), 0.1f);
                        }
                    }
                    __instance.raidGroups.Remove(raidGroup);
                }
            }
        }
    }
}
