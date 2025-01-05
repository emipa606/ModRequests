using Verse;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Globalization;
using RimWorld;
using static PawnTrackerMain.PawnTrackerUtility;
using System.Linq;
using static PawnTrackerMain.CHGameComp;
using System.Text;
using PawnTackerMain;
using System.Text.RegularExpressions;
using Mono.CompilerServices.SymbolWriter;

namespace PawnTrackerMain
{
    public static class PawnTrackerUtility
    {
        
        public static string SplitPascalCase(string input)
        {
            // Use a regular expression to insert a space before each uppercase letter, except the first one
            string result = Regex.Replace(input, "(?<!^)([A-Z])", " $1");
            return result.ToLower(); // Convert to lowercase
        }

        public static void DocumentUndocumentedChanges(Map map = null)
        {
            if (!GetComp().UseMod)
                return;
            foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive.Where(p =>
                p != null
                && p.RaceProps.Humanlike
                && !(p.Dead && p.PawnTrackerComp() != null && !p.PawnTrackerComp().ResurrectAfterDeath()))) //Pawn is dead and not-resurrected
            {
                var comp = pawn.GetComp<PawnTrackerComp>();
                if (comp == null)
                {
                    // Add new component
                    var newComp = new PawnTrackerComp();
                    pawn.AllComps.Add(newComp);
                    newComp.parent = pawn;

                    // Do some initialization if needed
                }
            }

            foreach (Pawn pawn in GetPawns(
                colonistStatus: CSO.Any, 
                pawnKind: PKO.Humanlike, 
                deadOrAlive: DOAO.Alive, 
                documentedStatus: PDSO.Documented
            ).Where(pawn => 
                    pawn != null &&
                    (
                        (map != null && pawn.Map == map) || 
                        (map == null && pawn.Map != null && pawn.Map.IsPlayerHome)
                    )
            ).ToList())
            {
                var comp = pawn.GetComp<PawnTrackerComp>();
                if (comp == null)
                {
                    // Add new component
                    var newComp = new PawnTrackerComp();
                    pawn.AllComps.Add(newComp);
                    newComp.parent = pawn;

                    // Do some initialization if needed
                }

                if (!pawn.RaceProps.Humanlike)
                    continue;

                if (!pawn.PawnTrackerComp().shouldBeHere)
                {
                    Log.Message("Found a pawn who was thought to be missing");
                    pawn.PawnTrackerComp().AddEvent(new ArriveEvent(pawn, DocumentedEventDefOf.UnknownArrive));
                    if (pawn.PawnTrackerComp().MostRecentEvent() == null || pawn.PawnTrackerComp().MostRecentEvent().def != DocumentedEventDefOf.UnknownArrive)
                    {
                        pawn.PawnTrackerComp().shouldBeHere = true;
                    }
                }
            }

            foreach (Pawn pawn in GetPawns(
                colonistStatus: CSO.Colonist, 
                pawnKind: PKO.Humanlike,
                documentedStatus: PDSO.Any
            ).Where(pawn => 
                pawn != null
                && pawn.RaceProps.Humanlike
                && (pawn.PawnTrackerComp() == null || pawn.PawnTrackerComp().pawnStatus == PawnStatus.Colonist)
                && (
                    (map != null && pawn.Map == map)
                    || (map == null && pawn.Map != null && pawn.Map.IsPlayerHome)
                )
            ).ToList())
            {
                if (pawn.PawnTrackerComp().EverJoined())
                    continue;
                Log.Message($"Add free colonist: {pawn.Name}");
                var comp = pawn.PawnTrackerComp();
                if (comp == null)
                {
                    Log.Warning("Comp is null");
                }
                comp.shouldBeHere = true;
                comp.AddEvent(new ArriveEvent(pawn, DocumentedEventDefOf.UnknownArrive));
                comp.AddEvent(new JoinEvent(pawn, DocumentedEventDefOf.UnknownJoin));
            }

            foreach (Pawn pawn in GetPawns(
                colonistStatus: CSO.Prisoner
            ).Where(pawn => 
                pawn != null 
                && pawn.RaceProps.Humanlike
                && (pawn.PawnTrackerComp().pawnStatus != PawnStatus.Prisoner)
                && (
                    (map != null && pawn.Map == map) || 
                    (map == null && pawn.Map != null && pawn.Map.IsPlayerHome)
                )
            ).ToList())
            {
                Log.Message($"Add prisoner: {pawn.Name}");
                var comp = pawn.PawnTrackerComp();
                if (comp == null)
                {
                    Log.Warning("Comp is null");
                }
                comp.shouldBeHere = true;
                comp.AddEvent(new ArriveEvent(pawn, DocumentedEventDefOf.UnknownArrive));
                comp.AddEvent(new LifeEvent(pawn, DocumentedEventDefOf.Captured));
            }

            foreach (Pawn pawn in GetPawns(
                colonistStatus: CSO.AnyNonColonistNonPrisoner, 
                pawnKind: PKO.Humanlike,
                documentedStatus: PDSO.Undocumented
            ).Where(pawn => 
                pawn != null
                && pawn.RaceProps.Humanlike
                && (
                    (map != null && pawn.Map == map) || 
                    (map == null && pawn.Map != null && pawn.Map.IsPlayerHome)
                )
            ).ToList())
            {
                Log.Message($"Add other non-colonist: {pawn.Name}");
                var comp = pawn.PawnTrackerComp();
                if (comp == null)
                {
                    Log.Warning("Comp is null");
                }

                comp.shouldBeHere = true;
                
                comp.AddEvent(new ArriveEvent(pawn, DocumentedEventDefOf.UnknownArrive));
            }

            /*
                Each pawn we are tracking that is no longer on the map (dead or alive), but
                we expect them to be (shouldBeHere is true...)
            */
            foreach (Pawn pawn in GetMissingTrackedPawns())
            {
                var comp = pawn.PawnTrackerComp();
                //If they're tracked, expected to be on map, and are NOT on map... add an UnknownLeave event
                comp.AddEvent(new LeaveEvent(pawn, DocumentedEventDefOf.UnknownLeave));
            }
        }

        public static List<Pawn> GetMissingTrackedPawns()
        {
            if (!GetComp().UseMod)
                return new List<Pawn>();
            if (Find.CurrentMap == null)
                return new List<Pawn>();
            MapPawns mapPawns = Find.CurrentMap.mapPawns;
            List<Pawn> allPawns = mapPawns.AllPawnsSpawned;
            List<Pawn> downedPawns = mapPawns.SpawnedDownedPawns;

            return GetComp().TrackedPawns.Where(p => 
                p != null 
                && p.RaceProps.Humanlike 
                && p.PawnTrackerComp().shouldBeHere 
                && !allPawns.Contains(p) 
                && !downedPawns.Contains(p)
                && p.PawnTrackerComp().documentedEvents != null
                && p.PawnTrackerComp().documentedEvents.Any()
                && p.PawnTrackerComp().MostRecentEvent() is not LeaveEvent
                && p.PawnTrackerComp().MostRecentEvent() is not DeathEvent).ToList();
        }

        public enum CSO { Any, Colonist, Prisoner, Friendly, Hostile, AnyNonColonist, AnyNonColonistNonPrisoner }
        public enum DOAO { Any, Dead, Alive }
        public enum DSO {Any, Newborn, Baby, Child, Juvenile, Adult,} //DevelopmentalStageOption ... cumbersome to type out
        public enum PKO {Any, Humanlike, WildMan, NonHumanlike,}
        public enum PDSO {Any, Documented, Undocumented,}

        public static void UpdateTrackedPawns()
        {
            if (!GetComp().UseMod)
                return;
            if (GetComp().TrackedPawns == null)
                return;

            GetComp().TrackedPawns = GetComp().TrackedPawns.Concat(PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead.Where(p => 
                p != null 
                && p.RaceProps.Humanlike 
                && !GetComp().TrackedPawns.Contains(p)
                && p.PawnTrackerComp().documentedEvents != null
                && p.PawnTrackerComp().documentedEvents.Any()).ToList()
            ).Where(p =>
                p != null
                && !GetComp().ManuallyUntracked.Contains(p)
                && (GetComp().DeadTrackedPawns == null || !GetComp().DeadTrackedPawns.ContainsKey(p.Name.ToString()))
            ).Distinct().ToList();
            
            return;
        }

        public static List<Pawn> GetPawnsFromSources()
        {
            if (Find.CurrentMap == null)
            {
                Log.Message("Map is null");
                return new List<Pawn>();
            }
            List<Pawn> outPawns = Find.CurrentMap.mapPawns.AllPawnsSpawned.Where(p => !GetComp().ManuallyUntracked.Contains(p)).ToList();
            outPawns.Concat(
                PawnsFinder.HomeMaps_FreeColonistsSpawned.Where(x => !outPawns.Contains(x) && !GetComp().ManuallyUntracked.Contains(x)).ToList()
            );

            foreach (Pawn pawn in GetComp().TrackedPawns.Where(p => p != null && p.RaceProps != null && p.RaceProps.Humanlike && !outPawns.Contains(p) && !GetComp().DeadTrackedPawns.ContainsKey(p.Name.ToString())).ToList())
            {
                try
                {
                    Pawn fixedPawn = GetCorpsePawn(pawn);
                    if (fixedPawn != null)
                        outPawns.Add(fixedPawn);
                }
                catch {}
            }
            return outPawns;

        }
        
        public static List<Pawn> GetPawns(
            PDSO documentedStatus = PDSO.Undocumented,
            CSO colonistStatus = CSO.Any, 
            DOAO deadOrAlive = DOAO.Alive,
            DSO developmentalStage = DSO.Any,
            PKO pawnKind = PKO.Humanlike)
            {
                //MapPawns mapPawns = Find.CurrentMap.mapPawns;
                List<Pawn> allPawns = GetPawnsFromSources();
                if (allPawns == null)
                {
                    return new List<Pawn>();
                }
                
                //List<Pawn> allPawns = PawnsFinder.HomeMaps_FreeColonistsSpawned;
                //allPawns.AddRange(mapPawns.SpawnedDownedPawns);
                
                List<Pawn> Matches = new List<Pawn>();
                if (documentedStatus == PDSO.Undocumented)
                {
                    Matches = allPawns.Where(x =>
                        !PawnDocumented(x)
                        && IsMatchColonistStatus(x, colonistStatus)
                        && IsMatchDeadOrAlive(x, deadOrAlive)
                        && IsMatchDevelopmentalStage(x, developmentalStage)
                        && IsMatchPawnKind(x, pawnKind)
                    ).ToList();
                }
                else if (documentedStatus == PDSO.Documented)
                {
                    Matches = allPawns.Where(x =>
                        PawnDocumented(x)
                        && IsMatchColonistStatus(x, colonistStatus)
                        && IsMatchDeadOrAlive(x, deadOrAlive)
                        && IsMatchDevelopmentalStage(x, developmentalStage)
                        && IsMatchPawnKind(x, pawnKind)
                    ).ToList();
                }
                else
                {
                    Matches = allPawns.Where(x =>
                        IsMatchColonistStatus(x, colonistStatus)
                        && IsMatchDeadOrAlive(x, deadOrAlive)
                        && IsMatchDevelopmentalStage(x, developmentalStage)
                        && IsMatchPawnKind(x, pawnKind)
                    ).ToList();
                }
                Matches = Matches.Where(p => !GetComp().ManuallyUntracked.Contains(p)).ToList();
                return Matches;
        }

        public static List<Pawn> GetAllHumanlike()
        {
            if (!GetComp().UseMod)
                return new List<Pawn>();
                
            List<Pawn> allPawns = GetPawnsFromSources();
            if (allPawns == null)
                return new List<Pawn>();
            
            return allPawns.Where(p => p.RaceProps.Humanlike).ToList();
        }
        
        private static bool IsMatchColonistStatus(Pawn pawn, CSO status)
        {
            if (!GetComp().UseMod)
                return false;

            switch (status)
            {
                case CSO.Colonist:
                {
                    return pawn.IsColonist;
                }
                case CSO.Prisoner:
                {
                    return pawn.IsPrisonerOfColony;
                }
                case CSO.Friendly:
                    return !pawn.IsColonist && !(pawn.Faction.PlayerRelationKind == FactionRelationKind.Hostile);
                case CSO.Hostile:
                    return !pawn.IsColonist && pawn.Faction.PlayerRelationKind == FactionRelationKind.Hostile; //add logic
                case CSO.AnyNonColonist:
                {
                    return !pawn.IsColonist;
                }
                case CSO.AnyNonColonistNonPrisoner:
                    return !pawn.IsColonist && !pawn.IsPrisonerOfColony;
                default:
                {
                    return true;
                }
            }
        }

        private static bool IsMatchDeadOrAlive(Pawn pawn, DOAO status)
        {
            if (!GetComp().UseMod)
                return false;

            switch (status)
            {
                case DOAO.Alive:
                    return !pawn.Dead;
                case DOAO.Dead:
                    return pawn.Dead;
                default:
                    return true;
            }
        }

        private static bool IsMatchDevelopmentalStage(Pawn pawn, DSO status)
        {
            if (!GetComp().UseMod)
                return false;

            switch (status)
            {
                case DSO.Newborn:
                {
                    return pawn.DevelopmentalStage.Newborn();
                }
                case DSO.Baby:
                    return pawn.DevelopmentalStage.Baby();
                case DSO.Child:
                {
                    return pawn.DevelopmentalStage.Child();
                }
                case DSO.Juvenile:
                {
                    return pawn.DevelopmentalStage.Juvenile();
                }
                case DSO.Adult:
                {
                    return pawn.DevelopmentalStage.Adult();
                }
                default:
                {
                    return true;
                }
            }
        }

        private static bool IsMatchPawnKind(Pawn pawn, PKO status)
        {
            if (!GetComp().UseMod)
                return false;
            switch (status)
            {
                case PKO.Humanlike:
                    return pawn.RaceProps.Humanlike;
                case PKO.WildMan:
                    return pawn.IsWildMan();
                case PKO.NonHumanlike:
                    return !pawn.RaceProps.Humanlike;
                default:
                    return true;
            }
        }

        public static List<T> GetDefsOfType<T>() where T : Def
        {
            List<T> result = new List<T>();

            foreach (FieldInfo field in typeof(DocumentedEventDefOf).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(T))
                {
                    result.Add(field.GetValue(null) as T);
                }
            }

            return result;
        }

        public static string DeathDescription(DeathEvent deathEvent, Pawn pawn = null, string Pronoun = null)
        {
            List<HediffDef> sayOnTheir = new List<HediffDef>() {HediffDefOf.Burn, HediffDefOf.Cut, HediffDefOf.Stab, HediffDefOf.Gunshot, HediffDefOf.Bruise, HediffDefOf.Bite, HediffDefOf.Scratch};
            if (!GetComp().UseMod || deathEvent == null)
                return null;

            if (deathEvent.eventSubType != EventSubType.Death)
                throw new ArgumentException("EventType must be a death event");
            
            if (deathEvent.def == DocumentedEventDefOf.DiedInChildbirth)
                return deathEvent.description;

            if (deathEvent.details != null)
            {
                PawnDeathDetails details = deathEvent.details;
                if (pawn == null)
                {
                    pawn = deathEvent.pawn;
                    if (pawn == null)
                        pawn = GetCorpsePawn(pawn);
                    if (pawn == null)
                    {
                        LogPTMDev.Warning("Pawn is super null; cannot return DeathDescription");
                        return null;
                    }
                        
                }

                string pronoun = pawn != null ? pawn.gender.GetPronoun() : Pronoun != null ? Pronoun : "they";
                string poss = pawn != null && pawn.PawnTrackerComp().PronounPossessive != null ? pawn.PawnTrackerComp().PronounPossessive : "their";

                if (details.instigator != null && details.instigator is Pawn instigator)
                {    
                    if (instigator.RaceProps.Humanlike)
                    {
                        if (sayOnTheir.Contains(details.hediffDef))
                            return $"was killed by {instigator.Name} of {instigator.Faction}. {pronoun.CapitalizeFirst()} died of a {details.hediffDef.label.ToLower()} on {poss} {details.bodyPart}.";
                        else
                            return $"was killed by {instigator.Name} of {instigator.Faction}. {pronoun.CapitalizeFirst()} died of a {details.hediffDef.label.ToLower()} {details.bodyPart}.";
                    }
                        
                    if (sayOnTheir.Contains(details.hediffDef))
                        return $"was killed by a wild {instigator.kindDef.label.ToLower()}. {pronoun.CapitalizeFirst()} died of a {details.hediffDef.label.ToLower()} on {poss} {details.bodyPart}.";
                    else
                        return $"was killed by a wild {instigator.kindDef.label.ToLower()}. {pronoun.CapitalizeFirst()} died of a {details.hediffDef.label.ToLower()} {details.bodyPart}.";
                }

                if (details.exactCulprit != null)
                {
                    if (details.exactCulprit.def == HediffDefOf.WoundInfection)
                    return $"died from an infection in {poss} {details.exactCulprit.Part.Label}";
                
                    if (details.exactCulprit.Part != null && details.exactCulprit.def != HediffDefOf.BloodLoss)
                    {
                        if (sayOnTheir.Contains(details.exactCulprit.def))
                            return $"died from a {details.exactCulprit.def.label.ToLower()} on {poss} {details.exactCulprit.Part.Label}";
                        else if (details.exactCulprit.def == HediffDefOf.MissingBodyPart)
                            return $"died from a missing {details.exactCulprit.Part.Label}";
                        else
                            return $"died from a {details.exactCulprit.def.label.ToLower()} {details.exactCulprit.Part.Label}";
                    }

                }

                List<Hediff> injuries = deathEvent.hediffsAtDeath.Where(h => h is Hediff_Injury).ToList();
                Dictionary<Pawn, int> instigators = new Dictionary<Pawn, int>();
                List<string> injuryTypes = new List<string>();
                List<string> causes = new List<string>();
                
                foreach (Hediff hed in injuries)
                {
                    if (!hed.IsPermanent() && hed.TendableNow())
                    {
                        causes.Add(hed.source.label);
                        if (hed.def == HediffDefOf.Shredded)
                            injuryTypes.Add("shredded body parts");
                        else
                            injuryTypes.Add(hed.def.defName.ToLower());
                    }
                }
                
                if (injuryTypes.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    injuryTypes = injuryTypes.Distinct().ToList();
                    sb.Append("died from ");
                    if (details.exactCulprit.def == HediffDefOf.BloodLoss)
                        sb.Append("blood loss associated with ");
                    if (injuryTypes.Count == 1)
                        sb.Append($"a recent {injuryTypes[0].ToLower()}");
                    else if (injuryTypes.Count == 2)
                        sb.Append($"multiple {injuryTypes[0].ToLower()}s and {injuryTypes[1].ToLower()}s");
                    else
                    {
                        sb.Append("multiple");
                        for (int i=0; i < injuryTypes.Count; i++)
                        {
                            if (injuryTypes[i] == injuryTypes.Last())
                                sb.Append($" and {injuryTypes[i].ToLower()}s");
                            else
                                sb.Append($" {injuryTypes[i].ToLower()}s,");
                        }                        
                    }
                    if (causes.Any())
                        sb.Append($" from a {causes[0]}");
                    return sb.ToString();
                }

                else if (details.exactCulprit != null && details.exactCulprit.def == HediffDefOf.BloodLoss)
                {
                    if (details.exactCulprit.Part != null)
                    {
                        return $"died from blood loss from {poss} {details.exactCulprit.Part.Label}";
                    }
                }

                if (details.exactCulprit != null)
                    return $"died from {details.exactCulprit.def.label.ToLower()}";
                //Log.Message($"damageDef: {details.damageDef}\ndamageInfo: {details.damageInfo}\ndamageType: {details.damageType}\ninstigator: {details.instigator}\ncombatLogText: {details.combatLogText}\nhediffDef: {details.hediffDef}\nhediffSource: {details.hediffSource}\nexactCulprit: {details.exactCulprit}\nbodyPart: {details.bodyPart}");
            }
            
            if (deathEvent.def == DocumentedEventDefOf.UnknownDeath)
            {
                if (deathEvent.associatedEvent == null)
                    return $"died of unknown causes";
                else
                {
                    deathEvent.def = DocumentedEventDefOf.GameEventInjuries;
                    deathEvent.description = deathEvent.def.description;
                    deathEvent.description = deathEvent.Description();
                }
            }
                
            if (deathEvent.def == DocumentedEventDefOf.Stillborn)
                return $"came into the world a stillborn";
            return $"died";
        }

        public static Pawn GetCorpsePawn(Pawn pawn)
        {
            if (!GetComp().UseMod)
                return null;
            try
            {
                Corpse corpse = (Corpse) pawn.Corpse;
                Pawn innerPawn = corpse.InnerPawn;
                return innerPawn;
            }
            catch
            {
                return pawn;
            }
        }

        public enum PawnStatus
        {
            Colonist,
            Prisoner,
            Other,
        }

        public static CHGameComp GetComp()
        {
            return Current.Game.GetComponent<CHGameComp>();
        }

        public static string PawnName(Pawn p)
        {
            if (!p.RaceProps.Humanlike)
            {
                return p.Name.ToString();
            }

            NameTriple tripleName = (NameTriple) p.Name;

            if (tripleName.Nick != tripleName.First && tripleName.Nick != tripleName.Last)
                return $"{tripleName.First} '{tripleName.Nick}' {tripleName.Last}";
            else
                return $"{tripleName.First} {tripleName.Last}";

        }

        public static string PawnName(NameTriple tripleName)
        {
            if (tripleName.Nick != tripleName.First && tripleName.Nick != tripleName.Last)
                return $"{tripleName.First} '{tripleName.Nick}' {tripleName.Last}";
            else
                return $"{tripleName.First} {tripleName.Last}";

        }


        public static List<JoinEventDef> JoinEvents = GetDefsOfType<JoinEventDef>();
        public static List<ArriveEventDef> ArriveEvents = GetDefsOfType<ArriveEventDef>();
        public static List<LeaveEventDef> LeaveEvents = GetDefsOfType<LeaveEventDef>();
        public static List<DeathEventDef> DeathEvents = GetDefsOfType<DeathEventDef>();
        public static List<LifeEventDef> LifeEvents = GetDefsOfType<LifeEventDef>();

        public static LifeEventDef RelationDefLifeEvent(Pawn pawn, PawnRelationDef relationDef)
        {
            Dictionary<PawnRelationDef, LifeEventDef> relationDefLifeEventDef = new Dictionary<PawnRelationDef, LifeEventDef>() {
                //{PawnRelationDefOf.Child, Pawn.gender == Gender.Male ? BecameFather : BecameMother},
                //{PawnRelationDefOf.Granchild, Pawn.gender == Gender.Male ? BecameGranfather : BecameGrandmother},
                {PawnRelationDefOf.Lover, DocumentedEventDefOf.NewLovers},
                {PawnRelationDefOf.ExLover, DocumentedEventDefOf.ExLovers},
                {PawnRelationDefOf.Spouse, DocumentedEventDefOf.GotMarried},
                {PawnRelationDefOf.ExSpouse, DocumentedEventDefOf.Divorced},
                {PawnRelationDefOf.NephewOrNiece, DocumentedEventDefOf.BecameUncleOrAunt},
                {PawnRelationDefOf.Child, pawn.gender == Gender.Male ? DocumentedEventDefOf.BecameFather : null},
                {PawnRelationDefOf.Grandchild, DocumentedEventDefOf.BecameGrandparent},
                {PawnRelationDefOf.Sibling, DocumentedEventDefOf.BecameSibling},
                {PawnRelationDefOf.HalfSibling, DocumentedEventDefOf.BecameHalfSibling},
                {PawnRelationDefOf.GreatGrandchild, DocumentedEventDefOf.BecameGreatGrandparent},
                {PawnRelationDefOf.Fiance, DocumentedEventDefOf.GotEngaged},
            };

            if (!relationDefLifeEventDef.ContainsKey(relationDef))
                return null;
            
            return relationDefLifeEventDef[relationDef];
        }

        public static LifeEventDef RelationDefLifeEvent(PawnRelationDef relationDef)
        {
            Dictionary<PawnRelationDef, LifeEventDef> relationDefLifeEventDef = new Dictionary<PawnRelationDef, LifeEventDef>() {
                //{PawnRelationDefOf.Child, Pawn.gender == Gender.Male ? BecameFather : BecameMother},
                //{PawnRelationDefOf.Granchild, Pawn.gender == Gender.Male ? BecameGranfather : BecameGrandmother},
                {PawnRelationDefOf.Lover, DocumentedEventDefOf.NewLovers},
                {PawnRelationDefOf.ExLover, DocumentedEventDefOf.ExLovers},
                {PawnRelationDefOf.Spouse, DocumentedEventDefOf.GotMarried},
                {PawnRelationDefOf.ExSpouse, DocumentedEventDefOf.Divorced},
                {PawnRelationDefOf.NephewOrNiece, DocumentedEventDefOf.BecameUncleOrAunt},
                {PawnRelationDefOf.Child, DocumentedEventDefOf.BecameFather},
                {PawnRelationDefOf.Grandchild, DocumentedEventDefOf.BecameGrandparent},
                {PawnRelationDefOf.Sibling, DocumentedEventDefOf.BecameSibling},
                {PawnRelationDefOf.HalfSibling, DocumentedEventDefOf.BecameHalfSibling},
                {PawnRelationDefOf.GreatGrandchild, DocumentedEventDefOf.BecameGreatGrandparent},
                {PawnRelationDefOf.Fiance, DocumentedEventDefOf.GotEngaged},

            };

            if (!relationDefLifeEventDef.ContainsKey(relationDef))
            {
                return null;
            }    
            
            return relationDefLifeEventDef[relationDef];
        }

        public static class LogPTMDev
        {
            public static void Message(string text)
            {
                if (Prefs.DevMode)
                    Log.Message(text);
            }
            public static void Warning(string text)
            {
                if (Prefs.DevMode)
                    Log.Warning(text);
            }
            public static void Error(string text)
            {
                if (Prefs.DevMode)
                    Log.Error(text);
            }
        }    

        public static string Plural(float count, string word, string plural = "s", string singleFollowing = null, string pluralFollowing = null)
        {
            string out_desc = "";
            if (count == 1f)
            {
                out_desc = $"{count} {word}";
            } 
            else if (count == 0f)
            {
                if (plural == "s")
                    out_desc = $"no {word}{plural}";
                else
                    out_desc = $"no {plural}";
            }
            else
            {
                if (plural == "s")
                    out_desc = $"{count} {word}{plural}";
                else
                    out_desc = $"{count} {plural}";
            }

            if (count == 1f && singleFollowing != null)
                out_desc += $" {singleFollowing}";
            else if (count != 0 && pluralFollowing != null)
                out_desc += $" {pluralFollowing}";
            
            return out_desc;
        }  

        public static string FormatListWithCommasAndAnd(List<string> items)
        {
            if (items == null || items.Count == 0)
                return string.Empty;

            if (items.Count == 1)
                return items[0];

            var sb = new StringBuilder();
            for (int i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    if (i == items.Count - 1)
                        sb.Append(" and ");
                    else
                        sb.Append(", ");
                }
                sb.Append(items[i]);
            }

            return sb.ToString();
        }

        public enum EventSide {Start, End, } // Start applies to beginnings and also to single-moment events

        public static int GID = 0;

        public class EventBasicsBase
        {
            public ThingDef def;
            public GameConditionDef gameConditionDef;
            public string description;
            public int sortTick = -1;
            public int sortHour = -1;
            public int sortDay = -1;
            public int sortYear = -1;
            public EventSide eventSide;
            public Pawn pawn;
            public object thing;
            public List<EventBasics> subEvents = new();
            public int gid;

            public EventBasicsBase(object thing, int gid, EventSide side = EventSide.Start)
            {
                this.def = def != null ? def : (ThingDef)thing.GetType().GetProperty("def")?.GetValue("def");

                DocumentedEvent de = thing is DocumentedEvent ? (DocumentedEvent)thing : null;
                DeadPawnEvent dpe = thing is DeadPawnEvent ? (DeadPawnEvent)thing : null;
                GameEvent ge = thing is GameEvent ? (GameEvent)thing : null;
                

                if (ge != null)
                {
                    this.sortTick = side == EventSide.Start ? ge.tickStart : ge.tickEnd;
                    this.sortHour = side == EventSide.Start ? ge.hourStart : ge.hourEnd;
                    this.sortDay = side == EventSide.Start ? ge.dayOfYearStart : ge.dayOfYearEnd;
                    this.sortYear = side == EventSide.Start ? ge.yearStart : ge.yearEnd;
                }
                else
                {
                    this.sortTick = de != null ? de.ticks : dpe != null ? dpe.ticks : -1;
                    this.sortHour = de != null ? de.hour : dpe != null ? dpe.hour : -1;
                    this.sortDay = de != null ? de.dayOfYear : dpe != null ? dpe.dayOfYear : -1;
                    this.sortYear = de != null ? de.year : dpe != null ? dpe.year : -1;
                }

                this.thing = thing;
                this.eventSide = side;
                this.gid = gid;
            }


        }

        public class EventBasics : EventBasicsBase
        {
            public EventBasics(DocumentedEvent docEvent, int gid) : base(docEvent, gid)
            {
                this.def = docEvent.def;
                this.description = PawnName(docEvent.pawn) + " " + docEvent.Description();
            }

            public EventBasics(ArriveEvent arriveEvent, LifeEvent mainEvent, int gid) : base(mainEvent, gid)
            {
                this.description = $"{PawnName(mainEvent.pawn)} {mainEvent.Description()} after {mainEvent.pawn.gender.GetPronoun()} {arriveEvent.Description()}";
            }

            public EventBasics(string pawnName, DeadPawnEvent deadPawnEvent, int gid)  : base(deadPawnEvent, gid)
            {
                this.def = deadPawnEvent.def;
                this.description = pawnName + " " + deadPawnEvent.description;
            }

            public EventBasics(GameMiscEvent gameEvent, int gid) : base(gameEvent, gid)
            {
                this.def = gameEvent.def;
                this.description = gameEvent.Description;
            }

            public EventBasics(GameEvent gre, EventSide side, int gid) : base(gre, gid, side: side)
            {
                if (gre.def == null)
                {
                    return;
                }
                if (!gre.def.canEnd)
                {
                    Log.Error("Don't use GameEvent constructor with side if the event can't be ended!");
                    return;
                }

                if (side == EventSide.Start)
                {
                    this.description = gre.BaseDescription();
                }
                else if (side == EventSide.End)
                {
                    this.description = gre.PostEventDescription();
                }
                else
                {
                    Log.Error("EventSide is, somehow, a bad value");
                }

            }


            public EventBasics(GameConditionEvent gce, int gid, EventSide side)  : base(gce, gid, side: side)
            {
                this.gameConditionDef = gce.gameConditionDef ?? null;
                if (gce.gameConditionDef != null)
                {
                    if (side == EventSide.End)
                    {
                        if (gce.DaysLasted >= 1)
                            this.description = $"The {SplitPascalCase(gce.gameConditionDef.defName)} ended after {Plural(gce.DaysLasted, "day")}";
                        else
                            this.description = $"The {SplitPascalCase(gce.gameConditionDef.defName)} ended after {Plural(gce.HoursLasted, "hour")}";
                    }
                    else
                        this.description = $"A {SplitPascalCase(gce.gameConditionDef.defName)} started";
                    
                }
                else
                    this.description = "<<<GAMECONDITION BUT DEF IS NULL>>>";
                this.sortTick = gce.tickStart;
                this.sortHour = gce.hourStart;
                this.sortDay = gce.dayOfYearStart;
                this.sortYear = gce.yearStart;
                this.thing = gce;
            }
        }

        public static List<EventBasics> AllCoreEvents()
        {
            List<EventBasics> result = new();
            foreach (Pawn pawn in GetComp().TrackedPawns.Where(p => p.PawnTrackerComp().everWasColonist || p.PawnTrackerComp().everWasPrisoner))
            {
                List<DocumentedEvent> documentedEvents = pawn.PawnTrackerComp().documentedEvents;
                for (int i = 0; i < documentedEvents.Count; i++)
                {
                    DocumentedEvent docEvent = documentedEvents[i];
                    GID++;
                    if (docEvent is JoinEvent || docEvent is LeaveEvent || docEvent is DeathEvent)
                        result.Add(new EventBasics(docEvent, GID));
                    else if (docEvent is ArriveEvent arriveEvent && arriveEvent.def.canJoinAfterRescue && i + 1 < documentedEvents.Count && documentedEvents[i+1].def == DocumentedEventDefOf.Rescued)
                        result.Add(new EventBasics(arriveEvent, (LifeEvent)documentedEvents[i+1], GID));
                    else if (docEvent is ArriveEvent arriveEvent2 && i + 1 < documentedEvents.Count && documentedEvents[i+1].eventSubType == EventSubType.Capture)
                        result.Add(new EventBasics(arriveEvent2, (LifeEvent)documentedEvents[i+1], GID));
                }
            }
            
            foreach (DeadPawnComp deadPawnComp in GetComp().DeadTrackedPawns.Values)
            {
                GID++;
                foreach (DeadPawnEvent deadPawnEvent in deadPawnComp.documentedEvents)
                {
                    if (deadPawnComp.everWasColonist || deadPawnComp.everWasPrisoner)
                        result.Add(new EventBasics(
                            deadPawnComp.PawnTripleName != null ? PawnName(deadPawnComp.PawnTripleName) : deadPawnComp.Name, 
                            deadPawnEvent,
                            GID
                        ));
                }
            }
            
            foreach (GameEvent gameEvent in GetComp().gameEvents.Where(ge => ge.def != null || ge.gameConditionDef != null))
            {
                GID++; //bc we don't increment between start and end, they'll have the same gid -- which is the whole point of this!
                if (gameEvent.def.canEnd)
                {
                    result.Add(new EventBasics(gameEvent, EventSide.Start, GID));
                    if (gameEvent.tickEnd > 0)
                        result.Add(new EventBasics(gameEvent, EventSide.End, GID));
                }
                else
                {
                    result.Add(new EventBasics((GameMiscEvent)gameEvent, GID));
                }
                    
            }

            result.Sort((x, y) => x.sortTick.CompareTo(y.sortTick));
            
            foreach (EventBasics basics in result)
            {
                // Add events that happened between start and end of thing
                if (basics.thing is GameRaidEvent || basics.thing is GameManhunterEvent)
                {
                    GameEvent gameEvent = (GameEvent)basics.thing;
                    basics.subEvents.AddRange(result.Where(b => b.thing != basics.thing && b.sortTick > gameEvent.tickStart && b.sortTick < gameEvent.tickEnd));
                }

            }

            return result;
        }

        public static bool VictimOfEvent(DeathEvent death, GameEvent gameEvent)
        {
            List<HediffDef> violentHediffs = new List<HediffDef>() {HediffDefOf.Bite, HediffDefOf.Burn, HediffDefOf.BloodLoss, HediffDefOf.Cut, HediffDefOf.Gunshot, HediffDefOf.MissingBodyPart, HediffDefOf.Scratch, HediffDefOf.Stab, HediffDefOf.Shredded};
            if (death.hediffsAtDeath != null && !death.hediffsAtDeath.Any())
                return false;

            // Non-permanent injuries that are either untended or are less than 6 hours old, that I decided to consider "violent" (like could occur from an attack)
            List<Hediff> recentMajorInjuries = death.hediffsAtDeath.Where(h => h is Hediff_Injury hed && !hed.IsPermanent() && (hed.TendableNow() || hed.ageTicks <= 2500 * 6) && (violentHediffs.Contains(hed.def) || hed.def.label.ToLower().Contains("missing"))).ToList();          
            if (recentMajorInjuries == null) 
            {
                return false;
            }
            else
            {
                int ticksSinceStart = GenTicks.TicksAbs - gameEvent.tickStart;
                int ticksSinceEnd = GenTicks.TicksAbs - gameEvent.tickEnd;
                // Any injury is newer than the start of the raid and older than the end of the raid (happened between start and end)
                return recentMajorInjuries.Any(h =>  h.ageTicks < ticksSinceStart && h.ageTicks > ticksSinceEnd);
            }
        }

        public static string GameEventInjuriesDescription(GameEvent associatedEvent)
        {
            string desc = "died from injuries sustained";
            if (associatedEvent is GameRaidEvent gre)
                desc += $" during a raid by {gre.raidersFaction}";
            else if (associatedEvent is GameManhunterEvent gme)
                desc += $" during an attack of manhunter {gme.animalDef.label}s";
            else
                desc = "was killed";
            return desc;
        }

    }
}