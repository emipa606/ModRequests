using RimWorld;
using HarmonyLib;
using Verse;

namespace Autotend
{
    [StaticConstructorOnStartup]
    public class SelfTendOnJoin
    {
        private static readonly SelfTendOnJoinSettings _selfTendOnJoinSettings;

        static SelfTendOnJoin()
        {
            var harmony = new Harmony("mehni.autotend.selftendonjoin");
            harmony.Patch(AccessTools.Method(typeof(StoryWatcher_PopAdaptation), nameof(StoryWatcher_PopAdaptation.Notify_PawnEvent)), 
                postfix: new HarmonyMethod(typeof(SelfTendOnJoin), nameof(Notify_PawnEvent_PostFix)));

            _selfTendOnJoinSettings = LoadedModManager.GetMod<AutoSelfTendOnJoin>().Settings;
        }

        public static void Notify_PawnEvent_PostFix(Pawn p, PopAdaptationEvent ev)
        {
            if (ev == PopAdaptationEvent.GainedColonist && p.RaceProps.Humanlike && !p.Dead && p.IsColonist)
            {
                p.playerSettings.selfTend = p.skills.GetSkill(SkillDefOf.Medicine).Level >= _selfTendOnJoinSettings.MinimumSkillForSelfTending;

                if (p.WorkTypeIsDisabled(WorkTypeDefOf.Doctor))
                {
                    p.playerSettings.selfTend = false;
                    Messages.Message("MessageCannotSelfTendEver".Translate(p.LabelShort, p), MessageTypeDefOf.RejectInput, historical: false);
                    return;
                }

                SetWorkPriority(p);
            }
        }

        private static void SetWorkPriority(Pawn pawn)
        {
            if (!_selfTendOnJoinSettings.AutoSetPriorities)
            {
                return;
            }

            var notch = _selfTendOnJoinSettings.OneUpForPassions && 
                        pawn.skills.MaxPassionOfRelevantSkillsFor(WorkTypeDefOf.Doctor) >= Passion.Minor
                ? -1 
                : 0;

            var doctorSkill = pawn.skills.GetSkill(SkillDefOf.Medicine).Level;
            var priority = 0;

            if (_selfTendOnJoinSettings.PriorityNone.Includes(doctorSkill))
            {
                priority = 0;
            }
            if (_selfTendOnJoinSettings.PriorityFour.Includes(doctorSkill))
            {
                 priority = 4 + notch;
            }
            if (_selfTendOnJoinSettings.PriorityThree.Includes(doctorSkill))
            {
                priority = 3 + notch;
            }
            if (_selfTendOnJoinSettings.PriorityTwo.Includes(doctorSkill))
            {
                priority = 2 + notch;
            }
            if (_selfTendOnJoinSettings.PriorityOne.Includes(doctorSkill))
            {
                priority = 1;
            }

            pawn.workSettings.SetPriority(WorkTypeDefOf.Doctor, priority);
        }
    }
}
