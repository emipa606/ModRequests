using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Fixable_Mood_Debuffs_Alert
{
    public class Prosthophobe_Alert : Alert
    {
        private List<Pawn> pawns;

        private List<Pawn> Pawns {
            get => pawns = UnhappyProsthophobes();
            set => pawns = value;
        }

        public Prosthophobe_Alert()
        {
            defaultLabel = "Prosthophobes have bionics";
            defaultPriority = AlertPriority.Medium;
        }

        public override TaggedString GetExplanation()
        {
            return string.Format("{0} colonists on this map want less advanced prosthetics:\n\n{1}",
                pawns.Count, FormatString());
        }

        public override AlertReport GetReport()
        {
            return Fixable_Mood_Debuffs_Alert.settings.alertOnProsthophile && !Pawns.NullOrEmpty() ? AlertReport.CulpritIs(pawns[0]) : false;
        }

        private string FormatString()
        {
            string ret = "";
            List<Pawn> listedPawns = new List<Pawn>();
            foreach (Pawn p in pawns) {
                ret += p.Name.ToStringShort + "\n";
            }

            return ret;
        }

        private List<Pawn> UnhappyProsthophobes()
        {
            return (new List<Pawn>(Find.CurrentMap.mapPawns.FreeColonists)).FindAll(p => {
                List<Thought> outThoughts = new List<Thought>();
                p.needs.mood.thoughts.GetAllMoodThoughts(outThoughts);
                return outThoughts.Any(thought => thought.def == ThoughtDef.Named("ProsthophobeUnhappy"));
            });
        }
    }
}