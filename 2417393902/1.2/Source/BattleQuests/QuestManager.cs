using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;

namespace BattleQuests
{
    public class QuestManager : GameComponent
    {
        public Dictionary<MapParent, Faction> allyFactions = new Dictionary<MapParent, Faction>();
        public Dictionary<MapParent, float> threatLevels = new Dictionary<MapParent, float>();
        public QuestManager()
        {

        }
        public QuestManager(Game game)
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref allyFactions, "allyFactions", LookMode.Reference, LookMode.Reference, ref sitePartKeys1, ref factionValues);
            Scribe_Collections.Look(ref threatLevels, "threatLevels", LookMode.Reference, LookMode.Value, ref sitePartKeys2, ref threatValues);
        }

        private List<MapParent> sitePartKeys1 = new List<MapParent>();
        private List<MapParent> sitePartKeys2 = new List<MapParent>();
        private List<Faction> factionValues = new List<Faction>();
        private List<float> threatValues = new List<float>();
    }
}
