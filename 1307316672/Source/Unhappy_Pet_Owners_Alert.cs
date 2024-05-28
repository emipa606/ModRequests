using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Fixable_Mood_Debuffs_Alert
{
    public class Unhappy_Pet_Owners_Alert : Alert
    {
        private List<Pawn> allUnhappyPawns;

        private List<Pawn> AllUnhappyPawns {
            get => allUnhappyPawns = AllPawnsWhoWantToMaster();
            set => allUnhappyPawns = value;
        }

        public Unhappy_Pet_Owners_Alert()
        {
            defaultLabel = "Pawns want to master their bonded pets";
            defaultPriority = AlertPriority.Medium;
        }

        public override TaggedString GetExplanation()
        {
            return string.Format("{0} colonists on this map want to master their pets:\n\n{1}",
                allUnhappyPawns.Count, FormatString());
        }

        public override AlertReport GetReport()
        {
            return Fixable_Mood_Debuffs_Alert.settings.alertOnWrongMaster && !AllUnhappyPawns.NullOrEmpty() ? AlertReport.CulpritIs(allUnhappyPawns[0]) : false;
        }

        private string FormatString()
        {
            string ret = "";
            List<Pawn> listedPawns = new List<Pawn>();

            foreach (Pawn p in allUnhappyPawns) {
                ret += p.Name.ToStringShort + "\n";
            }

            return ret;
        }

        private List<Pawn> AllPawnsWhoWantToMaster()
        {
            return (new List<Pawn>(Find.CurrentMap.mapPawns.FreeColonists)).FindAll(p => {
                List<Thought> outThoughts = new List<Thought>();
                p.needs.mood.thoughts.GetAllMoodThoughts(outThoughts);
                return outThoughts.Any(thought =>
                    thought.def.workerClass == typeof(ThoughtWorker_NotBondedAnimalMaster));
            });
        }
    }
}