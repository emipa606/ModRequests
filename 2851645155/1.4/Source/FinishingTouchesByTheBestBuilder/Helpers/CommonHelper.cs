using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;


namespace FinishingTouchesByTheBestBuilder
{
    internal static class CommonHelper
    {
        internal const int TICK_INTERVAL_COMP = GenDate.TicksPerHour / 2;
        internal const int TICK_INTERVAL_CONTEXT = GenDate.TicksPerHour;

        internal static void FinishingFrame(Frame frame)
        {
            //Log.Message($"@@@FinishingFrame: {frame.LabelCap} : きてます！");
            CompFinishingTouchFrame comp = frame.TryGetComp<CompFinishingTouchFrame>();
            if (comp != null)
            {
                comp.Finishing = true;
            }
        }

        internal static bool CanFinisheFrameAndBestPawn(Frame frame, Pawn pawn, bool forced, bool jobDriver)
        {
            CompFinishingTouchFrame comp;
            if (forced)
            {
                comp = frame.TryGetComp<CompFinishingTouchFrame>();
                if (comp != null)
                {
                    //Log.Message($"@@@手動指示なので{pawn}が最適か否かに関わらず仕上げまで行うフラグを立てます。");
                    comp.forciblePawnID = pawn.ThingID;
                }
                return true;
            }
            if (!DataContext.current.enabled)
            {
                return true;
            }
            else if (frame.Faction != Faction.OfPlayer)
            {
                return true;
            }
            comp = frame.TryGetComp<CompFinishingTouchFrame>();
            if (comp == null || !comp.Finishing)
            {
                return true;
            }

            //bool canFinisheFrameAndBestPawn = pawn == DataContext.current.bestPawnStatesMap[pawn.Map.uniqueID]?.pawn || (comp.forciblePawnID != null && pawn.ThingID == comp.forciblePawnID);
            bool canFinisheFrameAndBestPawn = (DataContext.current.bestPawnStatesMap[pawn.Map.uniqueID]?.Any(x => x.pawn == pawn) ?? false) || (comp.forciblePawnID != null && pawn.ThingID == comp.forciblePawnID);
            if (jobDriver && !canFinisheFrameAndBestPawn)
            {
                //Messages.Message($"@@@CanFinisheFrameAndBestPawn {pawn} : 最大のスキル保持者ではないので仕上げはしません。", MessageTypeDefOf.NeutralEvent);
                pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
                frame.workDone = frame.WorkToBuild - 1f;
            }
            return canFinisheFrameAndBestPawn;
        }

        internal static bool ValidPawn(Pawn pawn)
        {
            int mapId = pawn.Map?.uniqueID ?? -1;
            if (mapId < 0)
            {
                return false;
            }
            return !pawn.Dead && PawnUtility.InValidState(pawn) && pawn.IsColonistPlayerControlled;
        }

        internal static bool TryGetSkillWhenWorkSettingEnabled(Pawn pawn, out int skill)
        {
            skill = GetSkillWhenSkillEnabled(pawn);
            if (skill < 0 || !SettingEnabled(pawn))
            {
                skill = -1;
                return false;
            }
            DataContext dc = DataContext.current;
            if (dc.addLevelByInspire > 0 && Inspired(pawn))
            {
                skill += dc.addLevelByInspire;
            }
            if (dc.addLevelByRoleEffect > 0 && HasRoleEffect(pawn))
            {
                skill += dc.addLevelByRoleEffect;
            }
            return true;

        }
        internal static bool HasRoleEffect(Pawn pawn)
        {
            if (ModsConfig.IdeologyActive && pawn.Ideo != null)
            {
                Precept_Role role = pawn.Ideo.GetRole(pawn);
                if (role?.def.roleEffects != null)
                {
                    return role.def.roleEffects.Any(x => x is RoleEffect_ProductionQualityOffset);
                }
            }
            return false;
        }
        internal static bool Inspired(Pawn pawn) => pawn.InspirationDef == InspirationDefOf.Inspired_Creativity;
        internal static bool SettingEnabled(Pawn pawn) => pawn.workSettings != null && pawn.workSettings.WorkIsActive(WorkTypeDefOf.Construction) && pawn.workSettings.GetPriority(WorkTypeDefOf.Construction) > 0;
        internal static int GetSettingPriority(Pawn pawn) => pawn.workSettings?.GetPriority(WorkTypeDefOf.Construction) ?? 0;
        internal static int GetSkillWhenSkillEnabled(Pawn pawn)
        {
#if DEBUG
            if (pawn == null)
            {
                Log.Warning($"FinishingTouches DEBUG: {pawn}({pawn.def.defName}) is null!!");
                return -1;
            }
            else if (pawn.skills == null)
            {
                Log.Warning($"FinishingTouches DEBUG: {pawn}({pawn.def.defName}) SkillTracker is null!!");
                return -1;
            }
            else if (pawn.skills.skills == null)
            {
                Log.Warning($"FinishingTouches DEBUG: {pawn}({pawn.def.defName}) SkillRecords is null!!");
                return -1;
            }
#endif
            if (pawn == null)
            {
                return -1;
            }
            else if (pawn.skills == null)
            {
                return -1;
            }
            else if (pawn.skills.skills == null)
            {
                return -1;
            }
            else
            {
                SkillRecord skillRecord = pawn.skills.skills.FirstOrDefault(x => x.def == SkillDefOf.Construction);
                if (skillRecord == null && skillRecord.TotallyDisabled)
                {
                    return -1;
                }
                else
                {
                    return skillRecord.Level;
                }
            }
        }
    }
}
