using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Fixable_Mood_Debuffs_Alert
{
    public class Night_Owl_Alert : Alert
    {
        private List<Pawn> nightOwls;

        private List<Pawn> NightOwls {
            get => nightOwls = AllPawnsInLongDistanceRelationships();
            set => nightOwls = value;
        }
        
        public Night_Owl_Alert()
        {
            defaultLabel = "Unhappy night owls";
            defaultPriority = AlertPriority.Medium;
        }

        public override TaggedString GetExplanation()
        {
            return string.Format("{0} colonists on this map want a better schedule:\n\n{1}",
                nightOwls.Count, FormatString());
        }

        public override AlertReport GetReport()
        {
            return Fixable_Mood_Debuffs_Alert.settings.alertOnNightOwlInDay && !NightOwls.NullOrEmpty() ? AlertReport.CulpritIs(nightOwls[0]) : false;
        }

        private string FormatString()
        {
            string ret = "";
            List<Pawn> listedPawns = new List<Pawn>();

            foreach (Pawn p in nightOwls) {
                ret += p.Name.ToStringShort + "\n";
            }

            return ret;
        }

        private List<Pawn> AllPawnsInLongDistanceRelationships()
        {
            return (new List<Pawn>(Find.CurrentMap.mapPawns.FreeColonists)).FindAll(p => {
                List<Thought> outThoughts = new List<Thought>();
                p.needs.mood.thoughts.GetAllMoodThoughts(outThoughts);
                return outThoughts.Any(thought =>
                    thought.def.workerClass == typeof(ThoughtWorker_IsDayForNightOwl));
            });
        }
    }
}