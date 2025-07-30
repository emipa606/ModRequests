using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace CM_Wheres_Daddy_Going
{
    public class IncidentWorker_BackFromGettingMilk : IncidentWorker_WandererJoin
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            Map map = (Map)parms.target;
            return CanSpawnJoiner(map) && RandomDad() != null;
        }

        public Pawn RandomDad()
        {
            WhereIsDad memoriesOfDad = WheresDaddyGoingMod.Instance.MemoriesOfDad;
            if (memoriesOfDad == null)
                return null;

            Pawn dad = null;
            while (dad == null)
            {
                dad = memoriesOfDad.RandomParent();
                if (dad == null)
                    break;

                if (dad.Dead || dad.relations == null || dad.relations.ChildrenCount == 0 || dad.relations.Children.FirstOrDefault(child => child.IsColonist && !child.Dead && !Find.WorldPawns.Contains(child)) == null)
                {
                    Logger.MessageFormat(this, "Invalid parent selected: {0}", dad);
                    memoriesOfDad.RemoveParent(dad);
                    dad = null;
                }
            }

            return dad;
        }

        public override Pawn GeneratePawn()
        {
            Pawn dad = RandomDad();
            if (dad == null)
            {
                Logger.MessageFormat(this, "Could not generate valid parent.");
                return null;
            }

            WhereIsDad memoriesOfDad = WheresDaddyGoingMod.Instance.MemoriesOfDad;
            if (memoriesOfDad == null)
                return null;

            bool dadGotTheThing = Rand.Chance(0.5f);

            if (dadGotTheThing)
            {
                ThingDef thingDef = memoriesOfDad.WhatIsParentGetting(dad);
                if (thingDef != null)
                {
                    ThingDef stuffDef = GenStuff.RandomStuffFor(thingDef);
                    Thing thing = ThingMaker.MakeThing(thingDef, stuffDef);
                    if (thing != null)
                    {
                        if (thingDef.stackLimit > 1)
                            thing.stackCount = Rand.RangeInclusive(1, 10);

                        dadGotTheThing = dad.inventory.innerContainer.TryAdd(thing, canMergeWithExistingStacks: true);
                    }
                }
                else
                {
                    Logger.ErrorFormat(this, "Could not find what {0} went out to get", dad);
                }
            }

            if (!memoriesOfDad.SetGotTheThing(dad, dadGotTheThing))
            {
                Logger.ErrorFormat(this, "Could not set 'gotIt' for {0}", dad);
            }

            return dad;
        }

        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map), map, CellFinder.EdgeRoadChance_Neutral, out cell);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            WhereIsDad memoriesOfDad = WheresDaddyGoingMod.Instance.MemoriesOfDad;
            if (memoriesOfDad == null)
                return false;

            Map map = (Map)parms.target;
            if (!CanSpawnJoiner(map))
            {
                Logger.MessageFormat(this, "Could not spawn joiner.");
                return false;
            }

            Pawn parent = GeneratePawn();
            if (parent == null)
            {
                Logger.MessageFormat(this, "Could not generate valid parent.");
                return false;
            }

            List<Pawn> childrenOnMap = parent.relations.Children.Where(pawn => pawn.Map == map).ToList();
            Pawn child = null;
            if (childrenOnMap.Count > 0)
                child = childrenOnMap.RandomElement();

            if (child == null)
            {
                Logger.MessageFormat(this, "Could not find child for: {0}", parent);
                return false;
            }

            parent.SetFaction(Faction.OfPlayer);
            SpawnJoiner(map, parent);
            if (def.pawnHediff != null)
            {
                parent.health.AddHediff(def.pawnHediff);
            }

            ThingDef thingDef = memoriesOfDad.WhatIsParentGetting(parent);
            bool? didTheyGetIt = memoriesOfDad.DidParentGetTheThing(parent);

            if (parent.gender == Gender.None)
            {
                // Should be impossible, but hey
                TaggedString text = ((def.pawnHediff != null) ? def.letterText.Formatted(parent.Named("PAWN"), NamedArgumentUtility.Named(def.pawnHediff, "HEDIFF")).AdjustedFor(parent) : def.letterText.Formatted(parent.Named("PAWN")).AdjustedFor(parent));
                TaggedString title = def.letterLabel.Formatted(parent.Named("PAWN")).AdjustedFor(parent);
                PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref title, parent);
                SendStandardLetter(title, text, LetterDefOf.PositiveEvent, parms, parent);
            }
            else if (parent.gender == Gender.Male)
            {
                TaggedString title = "CM_Wheres_Daddy_Going_Incident_Daddy_Returns_Label".Translate().Formatted(parent.Named("PAWN")).AdjustedFor(parent);
                TaggedString text = "CM_Wheres_Daddy_Going_Incident_Returns_Text".Translate(parent.NameShortColored, thingDef.label);
                PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref title, parent);
                SendStandardLetter(title, text, LetterDefOf.PositiveEvent, parms, parent);

            }
            else if (parent.gender == Gender.Female)
            {
                TaggedString title = "CM_Wheres_Daddy_Going_Incident_Mommy_Returns_Label".Translate().Formatted(parent.Named("PAWN")).AdjustedFor(parent);
                TaggedString text = "CM_Wheres_Daddy_Going_Incident_Returns_Text".Translate(parent.NameShortColored, thingDef.label);
                PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref title, parent);
                SendStandardLetter(title, text, LetterDefOf.PositiveEvent, parms, parent);
            }

            if (didTheyGetIt.HasValue && didTheyGetIt.Value == true)
            {
                memoriesOfDad.RemoveParent(parent);
                TaleRecorder.RecordTale(WheresDaddyDefOf.CM_Wheres_Daddy_Going_Tale_Returning_With_Milk, parent, child, thingDef);
            }
            else
            {
                TaleRecorder.RecordTale(WheresDaddyDefOf.CM_Wheres_Daddy_Going_Tale_Returning_Without_Milk, parent, child, thingDef);
            }

            return true;
        }

    }
}