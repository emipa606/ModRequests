using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Fixable_Mood_Debuffs_Alert
{
    public class Bedroom_Alert : Alert
    {
        private List<Pawn> pawns;

        private List<Pawn> Pawns {
            get => pawns = AllUnhappyPawns();
            set => pawns = value;
        }
        
        public Bedroom_Alert()
        {
            defaultLabel = "Unhappy with bedrooms";
            defaultPriority = AlertPriority.Medium;
        }

        public override TaggedString GetExplanation()
        {
            return string.Format("{0} colonists on this map are unhappy with their bedrooms:\n\n{1}",
                pawns.Count, FormatString());
        }

        public override AlertReport GetReport()
        {
            return Fixable_Mood_Debuffs_Alert.settings.alertOnBedroom && !Pawns.NullOrEmpty() ? AlertReport.CulpritIs(pawns[0]) : false;
        }

        private string FormatString()
        {
            string ret = "";
            foreach (Pawn p in pawns) {
                List<Thought> outThoughts = new List<Thought>();
                p.needs.mood.thoughts.GetAllMoodThoughts(outThoughts);
                outThoughts.FindAll(thought => {
                    return new List<System.Type>() { typeof(ThoughtWorker_BedroomJealous), typeof(ThoughtWorker_Ascetic), typeof(ThoughtWorker_Greedy) }.Contains(thought.def.workerClass) &&
                        thought.MoodOffset() < 0f;
                }).ForEach(thought => {
                    ret += string.Format("{0} ({1}): {2}\n", p.Name.ToStringShort, thought.LabelCap, thought.MoodOffset());
                });
            }

            return ret;
        }

        private List<Pawn> AllUnhappyPawns()
        {
            return (new List<Pawn>(Find.CurrentMap.mapPawns.FreeColonists)).FindAll(p => {
                List<Thought> outThoughts = new List<Thought>();
                p.needs.mood.thoughts.GetAllMoodThoughts(outThoughts);
                return outThoughts.Any(thought => {
                    return new List<System.Type>() { typeof(ThoughtWorker_BedroomJealous), typeof(ThoughtWorker_Ascetic), typeof(ThoughtWorker_Greedy) }.Contains(thought.def.workerClass) &&
                        thought.MoodOffset() < 0f;
                });
            });
        }
    }
}