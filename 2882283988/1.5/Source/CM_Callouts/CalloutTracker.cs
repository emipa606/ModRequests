using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Grammar;

using CM_Callouts.PendingCallouts;

namespace CM_Callouts
{
    public class CalloutTracker : WorldComponent
    {
        private int ticksBetweenCallouts = 240;
        private int checkTicks = 60;
        private int hashCache = 0;

        private Dictionary<Pawn, int> pawnCalloutExpireTick = new Dictionary<Pawn, int>();

        private static Dictionary<int, TextMoteQueueTickBased> textMoteQueuesTickBased = new Dictionary<int, TextMoteQueueTickBased>();
        private static Dictionary<int, TextMoteQueueRealTime> textMoteQueuesRealTime = new Dictionary<int, TextMoteQueueRealTime>();

        private static int maxRulesSeen = 1;

        public CalloutTracker(World world) : base(world)
        {
            hashCache = this.GetHashCode();

            // New world, new queues
            textMoteQueuesTickBased = new Dictionary<int, TextMoteQueueTickBased>();
            textMoteQueuesRealTime = new Dictionary<int, TextMoteQueueRealTime>();
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            int currentTickPlusHash = Find.TickManager.TicksGame + hashCache;

            // Replace dictionary with new one without expired values
            if (currentTickPlusHash % checkTicks == 0)
                pawnCalloutExpireTick = pawnCalloutExpireTick.Where(kvp => kvp.Value > currentTickPlusHash).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            UpdateTextMoteQueuesTickBased();
        }

        private static void UpdateTextMoteQueuesTickBased()
        {
            bool cleanup = false;
            foreach(TextMoteQueue textMoteQueue in textMoteQueuesTickBased.Values)
            {
                cleanup = (cleanup || !textMoteQueue.Update());
            }

            if (cleanup)
                textMoteQueuesTickBased = textMoteQueuesTickBased.Where(kvp => kvp.Value.Valid()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static void UpdateTextMoteQueuesRealTime()
        {
            bool cleanup = false;
            foreach (TextMoteQueue textMoteQueue in textMoteQueuesRealTime.Values)
            {
                cleanup = (cleanup || !textMoteQueue.Update());
            }

            if (cleanup)
                textMoteQueuesRealTime = textMoteQueuesRealTime.Where(kvp => kvp.Value.Valid()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static Thing ThrowText(Thing thing, IntVec3 location, Map map, WipeMode wipeMode = WipeMode.Vanish)
        {
            MoteText moteText = thing as MoteText;

            if (moteText != null)
            {
                int hash = (map.Index * 1000000) + (location.x * 1000) + location.z;

                if (Find.TickManager.Paused)
                {
                    //if (!textMoteQueuesRealTime.ContainsKey(hash))
                    //    textMoteQueuesRealTime[hash] = new TextMoteQueueRealTime(location, map);

                    //textMoteQueuesRealTime[hash].AddMote(moteText);

                    // Meh, the situations I can think of where this is happening (changing temperature on a heater for example) queueing it makes it worse
                    GenSpawn.Spawn(thing, location, map);
                }
                else if (!CalloutMod.settings.queueTextMotes)
                {
                    thing.def = CalloutDefOf.CM_Callouts_Thing_Mote_Text_Ticked;
                    GenSpawn.Spawn(thing, location, map);
                }
                else
                {
                    if (!textMoteQueuesTickBased.ContainsKey(hash))
                        textMoteQueuesTickBased[hash] = new TextMoteQueueTickBased(location, map);

                    textMoteQueuesTickBased[hash].AddMote(moteText);
                }

                //Log.Message("Throwing text - " + (thing as MoteText).text + " - at " + location + " - " + map);
            }
            return thing;
        }

        public bool CanCalloutNow(Pawn pawn)
        {
            return (pawn != null && !pawn.Dead && pawn.Spawned && (CalloutMod.settings.allowCalloutsForAnimals || pawn.def.race.Humanlike) && !pawnCalloutExpireTick.ContainsKey(pawn) && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking));
        }

        public bool CheckCalloutChance(CalloutCategory category, RulePackDef rulePackDef, string keyword = "rule")
        {
            float calloutChance = CalloutMod.settings.baseCalloutChance * ScaledCalloutFrequency(category, rulePackDef);
            float randChance = Rand.Value;
            Logger.MessageFormat(this, "calloutChance of {0} = {1}/{2}", rulePackDef, randChance, calloutChance);
            return randChance <= calloutChance;
        }

        private float ScaledCalloutFrequency(CalloutCategory category, RulePackDef rulePackDef, string keyword = "rule")
        {
            if (category != CalloutCategory.Combat)
                return 1.0f;

            int numberOfRules = rulePackDef.RulesPlusIncludes.Where(rule => rule.keyword == keyword).Count();

            if (numberOfRules > maxRulesSeen)
                maxRulesSeen = numberOfRules;

            float scaledChance = (float)numberOfRules / maxRulesSeen;

            return scaledChance;
        }

        public static bool HasHediffOfStage(Pawn pawn, HediffAndStage hdas)
        {
            HediffSet hs = pawn.health.hediffSet;
            for (int i = 0; i < hs.hediffs.Count; i++)
            {
                if (hs.hediffs[i].def == hdas.hediffDef && hs.hediffs[i].CurStageIndex == hdas.stage)
                {
                    return true;
                }
            }
            return false;
        }

        public void RequestCallout(Pawn pawn, RulePackDef rulePack, GrammarRequest grammarRequest)
        {
            
            if (!CanCalloutNow(pawn))
                return;
    
            if (pawn.InMentalState)
            {
                grammarRequest.Constants["SPICY"] = "true";
            }
            if (pawn.story != null && pawn.story.traits != null)
            {
                foreach(CalloutConstantByTraitDef constantByTraitDef in DefDatabase<CalloutConstantByTraitDef>.AllDefs)
                {
                    foreach (TraitDef traitDef in constantByTraitDef.traits)
                    {
                        if (pawn.story.traits.allTraits.Any(trait => trait.def == traitDef))
                        {
                            //Logger.MessageFormat(this, "{0}, {1}, {2}", pawn, constantByTraitDef.name, constantByTraitDef.value);
                            grammarRequest.Constants[constantByTraitDef.name] = constantByTraitDef.value;
                            break;
                        }
                    }
                }
            }
            foreach (CalloutConstantByPawnkindDef calloutConstantByPawnkindDef in DefDatabase<CalloutConstantByPawnkindDef>.AllDefs)
            {
                if (calloutConstantByPawnkindDef.pawnKindDefs.Any((PawnKindDef pkd) => pkd.defName == pawn.kindDef.defName))
                {
                    grammarRequest.Constants[calloutConstantByPawnkindDef.name] = calloutConstantByPawnkindDef.value;
                }
            }
            foreach (CalloutConstantByThingDef calloutConstantByThingDef in DefDatabase<CalloutConstantByThingDef>.AllDefs)
            {
                if (calloutConstantByThingDef.thingDefs.Any((ThingDef td) => td.defName == pawn.def.defName))
                {
                    grammarRequest.Constants[calloutConstantByThingDef.name] = calloutConstantByThingDef.value;
                }
            }
            if (pawn.health != null && pawn.health.hediffSet != null)
            {
                foreach (CalloutConstantByHediffDef calloutConstantByHediffDef in DefDatabase<CalloutConstantByHediffDef>.AllDefs)
                {
                    if (calloutConstantByHediffDef.hediffDefs.Any((HediffDef hd) => pawn.health.hediffSet.HasHediff(hd)))
                    {
                        grammarRequest.Constants[calloutConstantByHediffDef.name] = calloutConstantByHediffDef.value;
                    }
                }
                foreach (CalloutConstantByHediffStage calloutConstantByHediffStage in DefDatabase<CalloutConstantByHediffStage>.AllDefs)
                {
                    if (calloutConstantByHediffStage.hediffsAndStages.Any((HediffAndStage hdas) => HasHediffOfStage(pawn, hdas)))
                    {
                        grammarRequest.Constants[calloutConstantByHediffStage.name] = calloutConstantByHediffStage.value;
                    }
                }
            }
            if (pawn.needs != null)
            {
                foreach (CalloutConstantByNeedDef calloutConstantByNeedDef in DefDatabase<CalloutConstantByNeedDef>.AllDefs)
                {
                    Need need = pawn.needs.TryGetNeed(calloutConstantByNeedDef.needDef);
                    if (need != null)
                    {
                        bool above = need.CurLevel > calloutConstantByNeedDef.needLevel;
                        if (calloutConstantByNeedDef.aboveLevel ? above : !above)
                        {
                            grammarRequest.Constants[calloutConstantByNeedDef.name] = calloutConstantByNeedDef.value;
                        }
                    }
                }
            }
            string text = GrammarResolver.Resolve("rule", grammarRequest);
            if (!text.NullOrEmpty())
            {
                Logger.MessageFormat(this, "Callout resolved.");

                if (CalloutMod.settings.attachCalloutText)
                    CreateAttachedCalloutText(pawn, text, Color.white);
                else
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.MapHeld, text);
            }
            else
            {
                Logger.WarningFormat(this, " Could not find text for requested {1} by {0}", pawn, rulePack);
            }

            // Mark tick when pawn can callout again
            int expireTick = Find.TickManager.TicksGame + ticksBetweenCallouts + hashCache;

            pawnCalloutExpireTick.Add(pawn, expireTick);
        }

        private void CreateAttachedCalloutText(Thing caller, string text, Color color, float timeBeforeStartFadeout = -1f)
        {
            IntVec3 location = caller.PositionHeld;
            Map map = caller.MapHeld;

            if (location.InBounds(map))
            {
                MoteAttachedText moteText = (MoteAttachedText)ThingMaker.MakeThing(CalloutDefOf.CM_Callouts_Thing_Mote_Attached_Text);

                //moteText.exactPosition = loc;
                moteText.text = text;
                moteText.textColor = color;
                if (timeBeforeStartFadeout >= 0f)
                {
                    moteText.overrideTimeBeforeStartFadeout = timeBeforeStartFadeout;
                }
                GenSpawn.Spawn(moteText, location, map);
                moteText.Attach(caller);
            }
        }

        public static void CreateWoundTextMote(Vector3 loc, Map map, string text, Color color, float timeBeforeStartFadeout = -1f)
        {
            IntVec3 intVec = loc.ToIntVec3();
            if (intVec.InBounds(map))
            {
                MoteText moteText = (MoteText)ThingMaker.MakeThing(CalloutDefOf.CM_Callouts_Thing_Mote_Text_Wound);
                moteText.exactPosition = loc;
                moteText.SetVelocity(Rand.Range(5, 35), Rand.Range(0.42f, 0.45f));
                moteText.text = text;
                moteText.textColor = color;
                if (timeBeforeStartFadeout >= 0f)
                {
                    moteText.overrideTimeBeforeStartFadeout = timeBeforeStartFadeout;
                }
                ThrowText(moteText, intVec, map);
            }
        }
    }
}
