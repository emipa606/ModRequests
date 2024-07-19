using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FinishingTouchesByTheBestBuilder
{
    public enum ThresholdType
    {
        Static,
        Dynamic,
    }
    public static class ThresholdTypeLabelGetter
    {
        public static string Label(this ThresholdType thresholdType) => $"FT_ThresholdType{thresholdType}Label".Translate();
    }

    public class DataContext : WorldComponent
    {
        public static DataContext current = null;

        public bool enabled = false;
        public int choiceBestPawnOptionPriority = 3;
        public bool notBuildInspireCheck;
        public int addLevelByInspire = 4;
        public int addLevelByRoleEffect = 2;
        public bool allowMultiBuilderMode;
        public ThresholdType thresholdType = ThresholdType.Static;
        public int staticSkillThresholdGE = 0;
        public int dynamicSkillNegativeCorrection = 0;

        [Unsaved]
        public int staticSkillThresholdMax = 20;
        [Unsaved]
        public int dynamicSkillNegativeCorrectionMax = 20;


        public SettingDialog settingDialog = new SettingDialog();
        public Dictionary<int, HashSet<PawnState>> bestPawnStatesMap = new Dictionary<int, HashSet<PawnState>>();
        public Dictionary<int, int> bestPawnLastRefreshTicks = new Dictionary<int, int>();
        public Dictionary<int, HashSet<PawnState>> pawnStates = new Dictionary<int, HashSet<PawnState>>();
        public DataContext(World world) : base(world)
        {
            current = this;
        }

        public static void UpdatePawnState(Pawn pawn, WorkTypeDef def)
        {
            if (def == WorkTypeDefOf.Construction)
            {
                UpdatePawnState(pawn);
                DataContext.current.RefreshPawnStates();
            }
        }
        public static void UpdatePawnState(Pawn pawn)
        {
            if (pawn == null || !CommonHelper.ValidPawn(pawn))
            {
                return;
            }
            int mapId = pawn.Map.uniqueID;
            HashSet<PawnState> bestPawnStates = null;
            if (!current.bestPawnStatesMap.TryGetValue(mapId, out bestPawnStates))
            {
                current.bestPawnStatesMap[mapId] = bestPawnStates = new HashSet<PawnState>();
            }
            HashSet<PawnState> states;
            if (!current.pawnStates.TryGetValue(mapId, out states))
            {
                current.pawnStates[mapId] = states = new HashSet<PawnState>();
            }
            int skill;
            PawnState pawnState = states.FirstOrDefault(x => x.pawn == pawn);
            if (pawnState == null)
            {
                pawnState = new PawnState(pawn);
                states.Add(pawnState);
                skill = pawnState.skill;
            }
            else
            {
                skill = pawnState.RefreshSkill();
            }
            int pawnPriority = CommonHelper.GetSettingPriority(pawn);
            if (pawnPriority > 0 && pawnPriority <= current.choiceBestPawnOptionPriority)
            {
                if (current.allowMultiBuilderMode)
                {
                    switch (current.thresholdType)
                    {
                        case ThresholdType.Static:
                            if (skill >= current.staticSkillThresholdGE && !bestPawnStates.Contains(pawnState))
                            {
                                bestPawnStates.Add(pawnState);
                            }
                            break;
                        case ThresholdType.Dynamic:
                            if (!bestPawnStates.Contains(pawnState))
                            {
                                bestPawnStates.Add(pawnState);
                                int maxSkill = bestPawnStates.Max(x => x.skill);
                                bestPawnStates.RemoveWhere(x => x.skill < (maxSkill - current.dynamicSkillNegativeCorrection));
                            }
                            break;
                    }
                    //PawnState bestPawnState = bestPawnStates.FirstOrDefault();
                    //if (bestPawnState == null || skill > bestPawnState.skill)
                    //{
                    //    current.bestPawnStatesMap[mapId] = pawnState;
                    //}
                }
                else
                {
                    PawnState bestPawnState = bestPawnStates.FirstOrDefault();
                    if (bestPawnState == null || skill > bestPawnState.skill)
                    {
                        bestPawnStates.Clear();
                        bestPawnStates.Add(pawnState);
                    }
                }
            }
        }

        public void ValidateAndRefreshPawnStates()
        {
            //Pawn設定の妥当性チェック＆削除
            foreach (KeyValuePair<int, HashSet<PawnState>> states in this.pawnStates)
            {
                Queue<PawnState> removables = new Queue<PawnState>(states.Value.Where(x => !CommonHelper.ValidPawn(x.pawn) || states.Key != x.pawn.Map.uniqueID));
                while (removables.Any())
                {
                    PawnState removable = removables.Dequeue();
                    if (current.allowMultiBuilderMode)
                    {
                        if (this.bestPawnStatesMap.TryGetValue(states.Key, out HashSet<PawnState> pawnStates) && pawnStates != null)
                        {
                            pawnStates.RemoveWhere(x => x.pawn == removable.pawn);
                        }
                    }
                    else
                    {
                        if (this.bestPawnStatesMap.TryGetValue(states.Key, out HashSet<PawnState> pawnStates) && pawnStates != null)
                        {
                            PawnState pawnState = pawnStates.FirstOrDefault();
                            if (pawnState != null && pawnState.pawn == removable.pawn)
                            {
                                this.bestPawnStatesMap[states.Key].Clear();
                            }
                        }
                    }
                    states.Value.Remove(removable);
                }
            }

            //残存Pawnのスキル更新
            RefreshPawnStates();
        }

        void RefreshPawnStates()
        {
            DataContext dc = DataContext.current;
            foreach (KeyValuePair<int, HashSet<PawnState>> states in this.pawnStates)
            {
                int mapId = states.Key;
                if (!this.bestPawnStatesMap.TryGetValue(mapId, out HashSet<PawnState> bestPawns))
                {
                    this.bestPawnStatesMap[mapId] = bestPawns = new HashSet<PawnState>();
                }
                if (current.allowMultiBuilderMode)
                {
                    bestPawns.Clear();
                    foreach (PawnState state in states.Value.Where(x => {
                        if (notBuildInspireCheck && CommonHelper.Inspired(x.pawn))
                        {
                            return false;
                        }
                        int pawnPriority = CommonHelper.GetSettingPriority(x.pawn);
                        if (pawnPriority < 1 || pawnPriority > dc.choiceBestPawnOptionPriority)
                        {
                            return false;
                        }
                        return true;
                    }))
                    {
                        state.RefreshSkill();
                        bestPawns.Add(state);
                    }
                    switch (dc.thresholdType)
                    {
                        case ThresholdType.Static:
                            bestPawns.RemoveWhere(x => x.skill < dc.staticSkillThresholdGE);
                            break;
                        case ThresholdType.Dynamic:
                            int thresholdSkill = bestPawns.Any() ? bestPawns.Max(x => x.skill) - dc.dynamicSkillNegativeCorrection : 0;
                            bestPawns.RemoveWhere(x => x.skill < thresholdSkill);
                            break;
                    }
                }
                else
                {
                    PawnState bestPawn = bestPawns.FirstOrDefault();
                    int bestPawnLevel = -1, bestPawnPriority = int.MaxValue;
                    if (bestPawn?.pawn != null && (!notBuildInspireCheck || !CommonHelper.Inspired(bestPawn.pawn)))
                    {
                        bestPawnPriority = CommonHelper.GetSettingPriority(bestPawn.pawn);
                        if (bestPawnPriority <= dc.choiceBestPawnOptionPriority)
                        {
                            bestPawnLevel = bestPawn.RefreshSkill();
                        }
                        else
                        {
                            bestPawnPriority = int.MaxValue;
                        }
                    }
                    foreach (PawnState state in states.Value)
                    {
                        if (bestPawnLevel >= 0 && state == bestPawn)
                        {
                            continue;
                        }
                        int pawnSkill = -1, pawnPriority = int.MaxValue;
                        if (state?.pawn == null || (notBuildInspireCheck && CommonHelper.Inspired(state.pawn)))
                        {
                            continue;
                        }
                        pawnPriority = CommonHelper.GetSettingPriority(state.pawn);
                        if (pawnPriority > dc.choiceBestPawnOptionPriority)
                        {
                            continue;
                        }
                        pawnSkill = state.RefreshSkill();
                        if (pawnSkill > bestPawnLevel)
                        {
                            bestPawnLevel = pawnSkill;
                            bestPawn = state;
                        }
                        else if (pawnSkill == bestPawnLevel && pawnPriority < bestPawnPriority)
                        {
                            bestPawnLevel = pawnSkill;
                            bestPawn = state;
                        }
                    }
                    bestPawns.Clear();
                    if (bestPawnLevel >= 0)
                    {
                        bestPawns.Add(bestPawn);
                    }
                }
            }
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (Find.TickManager.TicksGame % CommonHelper.TICK_INTERVAL_CONTEXT == 0)
            {
                ValidateAndRefreshPawnStates();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref enabled, "enabled", false);
            Scribe_Values.Look<int>(ref choiceBestPawnOptionPriority, "choiceBestPawnOptionPriority", 3);
            Scribe_Values.Look<bool>(ref notBuildInspireCheck, "notBuildInspireCheck", false);
            Scribe_Values.Look<int>(ref addLevelByInspire, "addLevelByInspire", 4);
            Scribe_Values.Look<int>(ref addLevelByRoleEffect, "addLevelByRoleEffect", 2);

            Scribe_Values.Look<bool>(ref allowMultiBuilderMode, "allowMultiBuilderMode", false);
            Scribe_Values.Look<ThresholdType>(ref thresholdType, "thresholdType", ThresholdType.Static);
            Scribe_Values.Look<int>(ref staticSkillThresholdGE, "staticSkillThresholdGE", 0);
            Scribe_Values.Look<int>(ref dynamicSkillNegativeCorrection, "dynamicSkillNegativeCorrection", 0);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                bool onLoadDucksInsaneSkills = ModsConfig.ActiveModsInLoadOrder.Any(x => x.PackageId.ToLower() == "ducks.insaneskills");
                staticSkillThresholdMax = onLoadDucksInsaneSkills ? 100 : 20;
                dynamicSkillNegativeCorrectionMax = onLoadDucksInsaneSkills ? 100 : 20;
                staticSkillThresholdGE = Math.Min(staticSkillThresholdGE, staticSkillThresholdMax);
                dynamicSkillNegativeCorrection = Math.Min(dynamicSkillNegativeCorrection, dynamicSkillNegativeCorrectionMax);
                //Log.Message($"@@@1 enabled={enabled}, choiceBestPawnOptionPriority={choiceBestPawnOptionPriority}, notBuildInspireCheck={notBuildInspireCheck}, addLevelByInspire={addLevelByInspire}, addLevelByRoleEffect={addLevelByRoleEffect}");
                //Log.Message($"@@@2 onLoadDucksInsaneSkills={onLoadDucksInsaneSkills}");
                //Log.Message($"@@@3 allowMultiBuilderMode={allowMultiBuilderMode}, thresholdType={thresholdType.Label()}, staticSkillThresholdGE={staticSkillThresholdGE}, staticSkillThresholdMax={staticSkillThresholdMax}, dynamicSkillNegativeCorrection={dynamicSkillNegativeCorrection}");
            }
        }
    }
}
