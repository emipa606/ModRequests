using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Fixable_Mood_Debuffs_Alert
{
    public class No_Long_Distance_Relationships_Alert : Alert
    {
        private List<Pawn> allLDRPawns;

        private List<Pawn> AllLDRPawns {
            get => allLDRPawns = AllPawnsInLongDistanceRelationships();
            set => allLDRPawns = value;
        }

        private List<PawnRelationDef> loverDefs = new List<PawnRelationDef>()
            { PawnRelationDefOf.Spouse, PawnRelationDefOf.Lover, PawnRelationDefOf.Fiance };

        public No_Long_Distance_Relationships_Alert()
        {
            defaultLabel = "Pawns want to sleep together";
            defaultPriority = AlertPriority.Medium;
        }

        public override TaggedString GetExplanation()
        {
            return string.Format("{0} colonists on this map want to sleep together:\n\n{1}",
                allLDRPawns.Count, FormatString());
        }

        public override AlertReport GetReport()
        {
            return AllLDRPawns.NullOrEmpty() ? false : AlertReport.CulpritIs(allLDRPawns[0]);
        }

        private string FormatString()
        {
            string ret = "";
            List<Pawn> listedPawns = new List<Pawn>();

            foreach (Pawn p in allLDRPawns) {
                if (listedPawns.Contains(p)) {
                    continue;
                }

                Pawn lover = p.relations.DirectRelations.Find(
                    relation => loverDefs.Contains(relation.def)
                ).otherPawn;

                listedPawns.Add(p);
                listedPawns.Add(lover);

                ret += p.Name.ToStringShort + " - " + lover.Name.ToStringShort + "\n";
            }

            return ret;
        }

        private List<Pawn> AllPawnsInLongDistanceRelationships()
        {
            return (new List<Pawn>(Find.CurrentMap.mapPawns.FreeColonists)).FindAll(p => {
                List<Thought> outThoughts = new List<Thought>();
                p.needs.mood.thoughts.GetAllMoodThoughts(outThoughts);
                return outThoughts.Any(thought =>
                    thought.LabelCap == "Sleeping alone");
            });
        }
    }
}