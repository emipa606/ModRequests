using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Fixable_Mood_Debuffs_Alert
{
    public class Nudist_Alert : Alert
    {
        private List<Pawn> nudists;

        private List<Pawn> Nudists {
            get => nudists = AllPawnsInLongDistanceRelationships();
            set => nudists = value;
        }

        public Nudist_Alert()
        {
            defaultLabel = "Nudists are too clothed";
            defaultPriority = AlertPriority.Medium;
        }

        public override TaggedString GetExplanation()
        {
            return string.Format("{0} colonists on this map want to wear less clothes:\n\n{1}",
                nudists.Count, FormatString());
        }

        public override AlertReport GetReport()
        {
            return Fixable_Mood_Debuffs_Alert.settings.alertOnClothedNudist && !Nudists.NullOrEmpty() ? AlertReport.CulpritIs(nudists[0]) : false;
        }

        private string FormatString()
        {
            string ret = "";
            List<Pawn> listedPawns = new List<Pawn>();

            foreach (Pawn p in nudists) {
                ret += p.Name.ToStringShort + "\n";
            }

            return ret;
        }

        private List<Pawn> AllPawnsInLongDistanceRelationships()
        {
            return (new List<Pawn>(Find.CurrentMap.mapPawns.FreeColonists)).FindAll(p => {
                List<Thought> outThoughts = new List<Thought>();
                p.needs.mood.thoughts.GetAllMoodThoughts(outThoughts);
                return outThoughts.Any(thought => thought.def == ThoughtDef.Named("ClothedNudist"));
            });
        }
    }
}