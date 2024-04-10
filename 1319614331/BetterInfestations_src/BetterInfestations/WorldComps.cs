using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Verse;

namespace BetterInfestations
{
    public class WorldObjectCompProperties_ExterminateBugs : WorldObjectCompProperties
    {
        public WorldObjectCompProperties_ExterminateBugs()
        {
            compClass = typeof(ExterminateBugsComp);
        }

        [DebuggerHidden]
        public override IEnumerable<string> ConfigErrors(WorldObjectDef parentDef)
        {
            foreach (string item in base.ConfigErrors(parentDef))
            {
                yield return item;
            }
            if (!typeof(MapParent).IsAssignableFrom(parentDef.worldObjectClass))
            {
                yield return parentDef.defName + " has WorldObjectCompProperties_ExterminateBugs but it's not MapParent.";
            }
        }
    }
    public class ExterminateBugsComp : WorldObjectComp
    {
        private bool active = true;
        public override void CompTick()
        {
            base.CompTick();
            if (active)
            {
                MapParent mapParent = parent as MapParent;
                if (mapParent != null)
                {
                    CheckAllBugsExterminated(mapParent);
                }
            }
        }
        public static bool IsDefeated(Map map, Faction faction)
        {
            List<Pawn> pawns = map.mapPawns.SpawnedPawnsInFaction(faction);
            List<Thing> hives = map.listerThings.ThingsOfDef(ThingDefOf.BI_Hive);
            if (!pawns.NullOrEmpty() || !hives.NullOrEmpty())
            {
                return false;
            }
            return true;
        }
        private void CheckAllBugsExterminated(MapParent mapParent)
        {
            if (mapParent.HasMap && IsDefeated(mapParent.Map, Faction.OfInsects))
            {
                SendLetter();
            }
        }
        private void SendLetter()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("BI_LetterInfestationDefeated".Translate(parent.Label));
            foreach (Faction allFaction in Find.FactionManager.AllFactions)
            {
                if (!allFaction.def.hidden && !allFaction.IsPlayer && allFaction != parent.Faction && allFaction.HostileTo(parent.Faction))
                {
                    FactionRelationKind playerRelationKind = allFaction.PlayerRelationKind;
                    if (allFaction.TryAffectGoodwillWith(Faction.OfPlayer, 20, canSendMessage: false, canSendHostilityLetter: false))
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine();
                        stringBuilder.Append("RelationsWith".Translate(allFaction.Name) + ": " + 20.ToStringWithSign());
                        allFaction.TryAppendRelationKindChangedInfo(stringBuilder, playerRelationKind, allFaction.PlayerRelationKind);
                    }
                }
            }
            Find.LetterStack.ReceiveLetter("BI_LetterLabelInfestationDefeated".Translate(), stringBuilder.ToString(), LetterDefOf.PositiveEvent, new GlobalTargetInfo(parent.Tile), parent.Faction);
            active = false;
        }
    }
}