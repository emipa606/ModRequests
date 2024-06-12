// Nightvision NightVision DebugFlareRaidPawnGen.cs
// 
// 18 10 2018
// 
// 18 10 2018

using RimWorld;
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace NightVision.Testing
{
#if RW10
    // don't see much point in supporting this for 1.0
#else
    //[HasDebugOutput] - obsolete as of 1.1
    public class DebugFlareRaidPawnGen
    {

[DebugOutput("NightVision")]

        public static void FlareRaidPawnGroupsMade()
        {
            Dialog_DebugOptionListLister.ShowSimpleDebugMenu(
                elements: from fac in Find.FactionManager.AllFactions
                          where !fac.def.pawnGroupMakers.NullOrEmpty() && fac.def.humanlikeFaction && fac.def.techLevel >= TechLevel.Industrial
                          select fac,
                label: fac => fac.Name + " (" + fac.def.defName + ")",
                chosen: delegate (Faction fac)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Point multiplier = ;{SolarRaidGroupMaker.PointMultiplier};;Max pawn cost multiplier = ;{SolarRaidGroupMaker.MaxPawnCostMultiplier};;");
                    float minPointsToGen = fac.def.MinPointsToGeneratePawnGroup(groupKind: PawnGroupKindDefOf.Combat);
                    sb.AppendLine($"Faction =;{fac.def.defName};;Min pts to gen CombatGroup = ;{minPointsToGen};;");

                    Action<float> action = delegate (float points)
                    {
                        if (points < fac.def.MinPointsToGeneratePawnGroup(groupKind: PawnGroupKindDefOf.Combat))
                        {
                            return;
                        }

                        float originalPoints = points;
                        points = IncidentWorker_Raid.AdjustedRaidPoints(
                            points,
                            PawnsArrivalModeDefOf.CenterDrop,
                            RaidStrategyDefOf.ImmediateAttack,
                            fac,
                            PawnGroupKindDefOf.Combat
                        );
                        var pawnGroupMakerParms = new PawnGroupMakerParms
                        {
                            groupKind = PawnGroupKindDefOf.Combat,
                            tile = Find.CurrentMap.Tile,
                            points = points,
                            faction = fac,
                            raidStrategy = RaidStrategyDefOf.ImmediateAttack
                        };
                        pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Combat;



                        float maxPawnCost = PawnGroupMakerUtility.MaxPawnCost(
                            faction: fac,
                            totalPoints: points,
                            raidStrategy: RaidStrategyDefOf.ImmediateAttack,
                            groupKind: PawnGroupKindDefOf.Combat
                        );
                        sb.AppendLine(
                            value:
                                $"Adjusted Points =;{pawnGroupMakerParms.points};Original points;{originalPoints};Max pawn cost;{maxPawnCost};");

                        //Points: X; MaxPawnCost:
                        //$"{};{};{};{};{};{}"
                        //150;   Mercenary_Slasher; Gladius;  Y; Apparel_FlakJacket; NV_tinted_goggles
                        var num2 = 0f;
                        SolarRaidGroupMaker.TryGetRandomPawnGroupMaker(parms: pawnGroupMakerParms, out var groupMaker);
                        Log.Message(new string('-', 20));
                        Log.Message($"Random group maker result:");
                        Log.Message($"points = {points}");

                        Log.Message($"groupMaker. = {groupMaker.options.ConvertAll(opt => opt.kind.LabelCap).ToStringSafeEnumerable()}");
                        Log.Message(new string('-', 20));


                        foreach (Pawn pawn in SolarRaid_PawnGenerator.GeneratePawns(parms: pawnGroupMakerParms, groupMaker, false)
                                    .OrderBy(keySelector: pa => pa.kindDef.combatPower))
                        {

                            sb.Append($"  {pawn.kindDef.combatPower.ToString(format: "F0").PadRight(totalWidth: 6)};{pawn.kindDef.LabelCap};");

                            if (pawn.equipment.Primary != null)
                            {
                                pawn.equipment.AllEquipmentListForReading.Aggregate(sb, (builder, comps) => builder.Append(comps.def.LabelCap + ","));
                            }
                            else
                            {
                                sb.Append("no equipment");
                            }

                            sb.Append(";");

                            var wornApparel = pawn.apparel.WornApparel;
                            string torsoGear = "";
                            string eyeWear = "";
                            string shield = "";
                            for (int i = 0; i < wornApparel.Count; i++)
                            {
                                Apparel apparel = wornApparel[i];
                                for (int j = 0; j < apparel.def.apparel.bodyPartGroups.Count; j++)
                                {
                                    if (apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.Torso)
                                    {
                                        torsoGear += $"{apparel.def.LabelCap}, ";
                                    }
                                    else if (apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.Eyes)
                                    {
                                        eyeWear += $"{apparel.def.LabelCap}, ";
                                    }

                                    if (apparel is ShieldBelt && shield.NullOrEmpty())
                                    {
                                        shield = $"Y;";
                                    }
                                }
                            }

                            torsoGear = torsoGear.NullOrEmpty() ? "shirtless" : torsoGear.TrimEnd(' ', ',');

                            eyeWear = eyeWear.NullOrEmpty() ? "not bespectacled" : eyeWear.TrimEnd(' ', ',');

                            shield = shield.NullOrEmpty() ? "N" : shield;


                            sb.Append($"{shield};{torsoGear};{eyeWear};");

                            if (pawn.health.hediffSet.hediffs.Count > 0)
                            {
                                pawn.health.hediffSet.hediffs.Aggregate(sb, (builder, comps) => builder.Append(comps.def.LabelCap + ","));
                            }
                            else
                            {
                                sb.Append("no hediffs");
                            }

                            sb.AppendLine();
                            num2 += pawn.kindDef.combatPower;
                        }

                        sb.AppendLine($";;;;Final point cost;{num2};");
                        sb.AppendLine();
                    };

                    foreach (float num in /*Dialog_DebugActionsMenu*/DebugActionsUtility.PointsOptions(extended: false))
                    {
                        float obj = num;
                        action(obj: obj);
                    }

                    Log.Message(text: sb.ToString());
#if DEBUG
                    GUIUtility.systemCopyBuffer = sb.ToString();
#endif
                }
            );
        }
    }
#endif
}