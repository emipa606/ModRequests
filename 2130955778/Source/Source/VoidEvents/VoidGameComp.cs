using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace VoidEvents
{
    public class VoidGameComp : GameComponent
    {
        public int tickToNextBaseConversion = 0;

        public Dictionary<Map, int> voidReinforcementsToSend;
        public VoidGameComp()
        {

        }
        public VoidGameComp(Game game)
        {

        }

        public void PreInit()
        {
            if (voidReinforcementsToSend == null)
            {
                voidReinforcementsToSend = new Dictionary<Map, int>();
            }
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            this.PreInit();
        }
        public override void StartedNewGame()
        {
            base.StartedNewGame();
            this.PreInit();
            tickToNextBaseConversion = Find.TickManager.TicksAbs + new IntRange(7 * GenDate.TicksPerDay, 30 * GenDate.TicksPerDay).RandomInRange;
        }
        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (VoidSettings.EnableVoidExpansion)
            {
                if (Find.TickManager.TicksAbs > tickToNextBaseConversion)
                {
                    var voidFaction = Find.FactionManager.FirstFactionOfDef(VoidDefOf.RH_VOID);
                    tickToNextBaseConversion = Find.TickManager.TicksAbs + new IntRange(7 * GenDate.TicksPerDay, 30 * GenDate.TicksPerDay).RandomInRange;
                    var randomVoidBase = Find.WorldObjects.SettlementBases.Where(x => x.Faction.def == VoidDefOf.RH_VOID).FirstOrDefault();
                    if (randomVoidBase == null)
                    {
                        var settlementToConvert = Find.WorldObjects.SettlementBases.Where(x => x.Faction.def != Faction.OfPlayer.def
                            && x.Faction.def != VoidDefOf.RH_VOID).InRandomOrder().FirstOrDefault();
                        if (settlementToConvert != null)
                        {
                            ConvertSettlement(settlementToConvert, voidFaction);
                        }
                    }
                    else
                    {
                        var settlementToConvert = Find.WorldObjects.SettlementBases.Where(x => x.Faction.def != Faction.OfPlayer.def
                            && x.Faction.def != VoidDefOf.RH_VOID).OrderBy(x => Find.WorldGrid.ApproxDistanceInTiles(randomVoidBase.Tile, x.Tile)).FirstOrDefault();
                        if (settlementToConvert != null)
                        {
                            ConvertSettlement(settlementToConvert, voidFaction);
                        }
                    }
                }
                if (this.voidReinforcementsToSend.Count > 0)
                {
                    foreach (var data in this.voidReinforcementsToSend)
                    {
                        if (Find.TickManager.TicksAbs == data.Value)
                        {
                            var voidFaction = Find.FactionManager.FirstFactionOfDef(VoidDefOf.RH_VOID);
                            IncidentParms parms = new IncidentParms
                            {
                                target = data.Key,
                                points = StorytellerUtility.DefaultThreatPointsNow(Find.World),
                                faction = voidFaction
                            };
                            IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
                        }
                    }
                }
            }
        }

        public void ConvertSettlement(Settlement settlementToConvert, Faction voidFaction)
        {
            Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
            settlement.SetFaction(voidFaction);
            settlement.Tile = settlementToConvert.Tile;
            settlement.Name = settlementToConvert.Name;
            Find.WorldObjects.Add(settlement);
            Find.LetterStack.ReceiveLetter("Void.SettlementIsDefeated".Translate(), "Void.SettlementIsDefeatedDesc".Translate(settlementToConvert.Name, settlementToConvert.Faction.Named("FACTION"))
                , VoidDefOf.Void_ThreatBig, settlement);
            settlementToConvert.Destroy();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tickToNextBaseConversion, "tickToNextBaseConversion");
            Scribe_Collections.Look<Map, int>(ref voidReinforcementsToSend, "voidReinforcementsToSend", LookMode.Reference, LookMode.Value, ref workMaps, ref workInts);
        }

        private List<Map> workMaps;
        private List<int> workInts;
    }
}