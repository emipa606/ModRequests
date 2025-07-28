using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace NoobertNebulous
{
    [HotSwappable]
    public class NoobertNebulousStoryteller : GameComponent
    {
        public List<FiringIncident> incidentsToFire;

        public int queueSize;

        public bool startOver = true;

        public float lastWealthCounted;

        public Quadrum checkedQuadrum = Quadrum.Undefined;

        public static NoobertNebulousStoryteller Instance;

        public static bool StorytellerActive => Find.Storyteller?.def == NN_DefOf.NN_NoobertNebulous;

        public static IncidentCategoryDef[] negativeIncidentCategoryDefs = new IncidentCategoryDef[]
        {
            IncidentCategoryDefOf.ThreatBig, IncidentCategoryDefOf.ThreatSmall
        };

        public Dictionary<Pawn, VidtubeChannel> vidtubers = new Dictionary<Pawn, VidtubeChannel>();

        public NoobertNebulousStoryteller()
        {
            Instance = this;
        }

        public NoobertNebulousStoryteller(Game game)
        {
            Instance = this;
        }

        public void CheckWealth()
        {
            if (incidentsToFire is null || startOver)
            {
                StartOver();
                //Log.Message("New incident queue is formed of size " + queueSize + ", we start from weath " + lastWealthCounted);
            }
            else
            {
                //Log.Message("Processing incident queue is of size " + queueSize + ", we start from weath " + lastWealthCounted);
                var diff = WealthUtility.PlayerWealth - lastWealthCounted;
                var diffAbs = Mathf.Abs(diff);
                while (diffAbs >= NoobertNebulousSettings.wealthInterval)
                {
                    var comp = Find.Storyteller.storytellerComps.OfType<StorytellerComp_NoobertNebulous>().First();
                    var count = (int)(diffAbs / NoobertNebulousSettings.wealthInterval);
                    bool positive = diff < 0;

                    //Log.Message("BEFORE: Diff: " + diff + " - diffAbs: " + diffAbs + " - count: " + count + " - positive: " + positive);
                    for (var i = 0; i < count; i++)
                    {
                        if (incidentsToFire.Count == queueSize)
                        {
                            //Log.Message("Breaking cycle");
                            break;
                        }
                        else
                        {
                            diffAbs -= NoobertNebulousSettings.wealthInterval;
                            if (positive)
                            {
                                var inc = comp.TryGetIncident();
                                if (inc != null)
                                {
                                    //Log.Message("Adding positive incident: " + inc);
                                    incidentsToFire.Add(inc);
                                }
                            }
                            else
                            {
                                var inc = comp.TryGetIncident(negativeIncidentCategoryDefs.RandomElement());
                                if (inc != null)
                                {
                                    //Log.Message("Negative positive incident: " + inc);
                                    incidentsToFire.Add(inc);
                                }
                            }
                        }
                    }

                    if (incidentsToFire.Count == queueSize)
                    {
                        foreach (var inc in incidentsToFire)
                        {
                            Find.Storyteller.TryFire(inc);
                        }
                        incidentsToFire.Clear();
                        queueSize = NoobertNebulousSettings.queueSizeRange.RandomInRange;
                        startOver = true;
                        //Log.Message("Firing incidents, creating new queue of size " + queueSize);
                    }

                    //Log.Message("AFTER: Diff: " + diff + " - diffAbs: " + diffAbs + " - count: " + count + " - positive: " + positive);
                    lastWealthCounted = WealthUtility.PlayerWealth - diffAbs;
                }
            }
        }

        private void StartOver()
        {
            incidentsToFire = new List<FiringIncident>();
            queueSize = NoobertNebulousSettings.queueSizeRange.RandomInRange;
            lastWealthCounted = WealthUtility.PlayerWealth;
            startOver = false;
        }

        public int prevDay;

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % 2500 == 0)
            {
                if (StorytellerActive)
                {
                    CheckWealth();
                }
                else
                {
                    startOver = true;
                }

                var day = GenDate.DayOfYear(Find.TickManager.TicksAbs, 0);
                if (prevDay != day)
                {
                    prevDay = day;
                    foreach (var channel in vidtubers.Values)
                    {
                        channel.TickOnDay();
                    }
                    var thisQuadrum = GenDate.Quadrum(Find.TickManager.TicksAbs, 0);
                    if (checkedQuadrum == Quadrum.Undefined)
                    {
                        checkedQuadrum = thisQuadrum;
                    }
                    else if (checkedQuadrum != thisQuadrum)
                    {
                        checkedQuadrum = thisQuadrum;
                        foreach (var channel in vidtubers.Values)
                        {
                            channel.GainRevenue(checkedQuadrum);
                        }
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref incidentsToFire, "queuedIncidents", LookMode.Deep);
            Scribe_Values.Look(ref lastWealthCounted, "lastWealthCounted");
            Scribe_Values.Look(ref queueSize, "queueSize");
            Scribe_Values.Look(ref prevDay, "prevDay");
            Scribe_Values.Look(ref startOver, "startOver");
            Scribe_Values.Look(ref checkedQuadrum, "checkedQuadrum", Quadrum.Undefined);
            Scribe_Collections.Look(ref vidtubers, "vidtubers", LookMode.Reference, LookMode.Deep, ref pawnKeys, ref channelValues);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                vidtubers ??= new Dictionary<Pawn, VidtubeChannel>();
                startOver = StorytellerActive is false;
            }
        }

        private List<Pawn> pawnKeys = new List<Pawn>();
        private List<VidtubeChannel> channelValues = new List<VidtubeChannel>();
    }
}
