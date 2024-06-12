using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace NightVision
{
    public static class SolarRaidGroupMaker
    {
        public static int PointMultiplier = 1;

        public static int MaxPawnCostMultiplier = 4;

        private static readonly SimpleCurve WeightingPreferPawnsCloseToHighestCost = new SimpleCurve
                                                                                     {
                                                                                         {new CurvePoint(x: 0.4f, y: 0.01f), true},
                                                                                         {new CurvePoint(x: 0.5f, y: 0.3f), true},
                                                                                         {new CurvePoint(x: 0.7f, y: 1f), true}
                                                                                     };

        public static void TryGetRandomPawnGroupMaker(PawnGroupMakerParms parms, out PawnGroupMaker pawnGroupMaker)
        {
            if (parms.faction.def.pawnGroupMakers.NullOrEmpty())
            {
                Log.Error(
                    text: string.Concat("Faction ", parms.faction, " of def ", parms.faction.def, " has no any PawnGroupMakers.")
                );
            }

            if (parms.seed != null)
            {
                Rand.PushState(replacementSeed: parms.seed.Value);
            }

            IEnumerable<PawnGroupMaker> source =
                        from gm in parms.faction.def.pawnGroupMakers
                        where gm.kindDef == parms.groupKind && gm.CanGenerateFrom(parms: parms)
                        select gm;

            bool result = source.TryRandomElementByWeight(weightSelector: gm => gm.commonality, result: out pawnGroupMaker);

            if (!source.Any())
            {
                Log.Message($"Found no pawn groups fit for purpose");

            }
            if (parms.seed != null)
            {
                Rand.PopState();
            }
        }

        private static bool MeleeConstraint(PawnGenOption pawnGenOption)
        {
            return pawnGenOption.kind.weaponTags.Count > 0
                   && pawnGenOption.kind.weaponTags.Any(predicate: str => str.Contains(value: "Melee") || str.Contains(value: "melee"));
        }

        public static IEnumerable<PawnGenOption> ChoosePawnGenByConstraint
                    (float pointsTotal, List<PawnGenOption> options, PawnGroupMakerParms groupParms)
        {
            if (groupParms.seed != null)
            {
                Rand.PushState(replacementSeed: groupParms.seed.Value);
            }

            float maxPawnCost = MaxPawnCostMultiplier
                                * PawnGroupMakerUtility.MaxPawnCost(
                                    faction: groupParms.faction,
                                    totalPoints: pointsTotal,
                                    raidStrategy: groupParms.raidStrategy,
                                    groupKind: groupParms.groupKind
                                );

            var candidates = new List<PawnGenOption>();
            var bestOptions = new List<PawnGenOption>();
            float remTotal = pointsTotal * PointMultiplier;
            var foundLeader = false;
            float highestCost = -1f;

            for (; ; )
            {
                candidates.Clear();

                for (var i = 0; i < options.Count; i++)
                {
                    PawnGenOption pawnGenOption = options[index: i];

                    if (pawnGenOption.Cost <= remTotal)
                    {
                        if (pawnGenOption.Cost <= maxPawnCost)
                        {
                            highestCost = HighestCost(
                                groupParms: groupParms,
                                pawnGenOption: pawnGenOption,
                                bestOptions: bestOptions,
                                flag: foundLeader,
                                highestCost: highestCost,
                                candidates: candidates
                            );
                        }
                    }
                }

                if (candidates.Count == 0)
                {
                    break;
                }

                Func<PawnGenOption, float> weightSelector = delegate (PawnGenOption gr)
                {
                    float selectionWeight = gr.selectionWeight;

                    return selectionWeight * WeightingPreferPawnsCloseToHighestCost.Evaluate(x: gr.Cost / highestCost);
                };

                PawnGenOption pawnGenOption2 = candidates.RandomElementByWeight(weightSelector: weightSelector);
                bestOptions.Add(item: pawnGenOption2);
                remTotal -= pawnGenOption2.Cost;

                if (pawnGenOption2.kind.factionLeader)
                {
                    foundLeader = true;
                }
            }

            if (bestOptions.Count == 1 && remTotal > pointsTotal / 2f)
            {
                Log.Warning(
                    text: string.Concat("Used only ", pointsTotal - remTotal, " / ", pointsTotal, " points generating for ", groupParms.faction)
                );
            }

            if (groupParms.seed != null)
            {
                Rand.PopState();
            }

            return bestOptions;
        }

        private static float HighestCost
        (
            PawnGroupMakerParms groupParms, PawnGenOption pawnGenOption, List<PawnGenOption> bestOptions, bool flag, float highestCost,
            List<PawnGenOption> candidates
        )
        {
            if (pawnGenOption.kind.isFighter)
            {
                // 16.08.2021 1.3 RW added required parameter to CanUsePawnGenOption for points
                // Using groupParms.points though unsure of effects
                if (groupParms.raidStrategy == null
                    || groupParms.raidStrategy.Worker.CanUsePawnGenOption(groupParms.points, g: pawnGenOption, chosenGroups: bestOptions))
                {
                    if (!groupParms.dontUseSingleUseRocketLaunchers || !pawnGenOption.kind.weaponTags.Contains(item: "GunHeavy"))
                    {
                        if (MeleeConstraint(pawnGenOption: pawnGenOption))
                        {
                            if (!flag || !pawnGenOption.kind.factionLeader)
                            {
                                if (pawnGenOption.Cost > highestCost)
                                {
                                    highestCost = pawnGenOption.Cost;
                                }

                                candidates.Add(item: pawnGenOption);
                            }
                        }
                    }
                }
            }

            return highestCost;
        }
    }
}