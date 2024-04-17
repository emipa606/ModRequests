using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RBank = CryptoRestore.ResourceBank;

namespace CryptoRestore
{
    public class Building_CryptoRestore : Building_CryptosleepCasket
    {
        protected int ChronicHediffHealTime;
        protected int LameHediffHealTime;
        protected int LuciferiumStabilizeTime;

        protected int enterTime;
        protected int endTime;

        protected int ageDownTo;

        protected bool hasJob;
        protected bool needYoungenig;
        protected bool hasAgeHediffs;
        protected bool hasInjuries;
        protected bool hasLameHediffs;
        protected bool nonStabilizedluciferium;

        protected CompPowerTrader power;
        protected CompRefuelable refuelable;

        private static bool HasChronicHediffs(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs.Any(h => h.def.chronic);
        }

        private static bool HasInjuryHediffs(Pawn pawn)
        {
            return pawn.health.hediffSet.GetHediffs<Hediff_Injury>().Any();
        }

        private static bool HasLameHediffs(Pawn pawn)
        {
            foreach (var hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff.def.chronic) continue;
                if (hediff.def.spawnThingOnRemoved != null) continue;
                if (hediff.def.isBad) return true;
            }

            return false;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            refuelable = GetComp<CompRefuelable>();
            power = GetComp<CompPowerTrader>();

            if (respawningAfterLoad && HasAnyContents)
            {
                Pawn pawn = ContainedThing as Pawn;
                ageDownTo = (int)(pawn.def.race.lifeStageAges?.Last().minAge * GenDate.TicksPerYear);
                if (ageDownTo <= 20) ageDownTo = 20;

                needYoungenig = pawn.ageTracker.AgeBiologicalTicks > ageDownTo;
                hasAgeHediffs = HasChronicHediffs(pawn);
                hasInjuries = HasInjuryHediffs(pawn);
                hasLameHediffs = HasLameHediffs(pawn);
                nonStabilizedluciferium = pawn.health.hediffSet.HasHediff(RBank.HediffDefOf.LuciferiumAddiction) || !pawn.health.hediffSet.HasHediff(RBank.HediffDefOf.LuciferiumHigh);

                hasJob = needYoungenig || hasAgeHediffs || hasInjuries || hasLameHediffs || nonStabilizedluciferium;
                if (!hasJob) power.PowerOutput = 0;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref LuciferiumStabilizeTime, "LuciferiumStabilizeTime", 0);
            Scribe_Values.Look(ref ChronicHediffHealTime, "chronicHediffCooldown", 0);
            Scribe_Values.Look(ref LameHediffHealTime, "lameHediffCooldown", 0);
            Scribe_Values.Look(ref enterTime, "enterTime");
            Scribe_Values.Look(ref endTime, "endTime");
        }

        private void TryHealChronic(Pawn pawn)
        {
            if (pawn != null)
            {
                IEnumerable<Hediff> healedHediffs;
                HediffComp_Immunizable immunizableHediff;
                foreach (HediffDef chronicInjury in RBank.AgeHediffs)
                {
                    healedHediffs = pawn.health.hediffSet.hediffs.Where(h => h.def == chronicInjury);
                    if (healedHediffs.Any())
                    {
                        if (healedHediffs.First().TryGetComp<HediffComp_Immunizable>() != null)
                        {
                            foreach (Hediff hediff in healedHediffs)
                            {
                                immunizableHediff = hediff.TryGetComp<HediffComp_Immunizable>();
                                hediff.Severity -= 5 * Math.Abs(immunizableHediff.Props.severityPerDayNotImmune) - 5 * Math.Abs(immunizableHediff.Props.severityPerDayImmune);
                                ChronicHediffHealTime += RBank.Settings.SmallHealChronicHediffCooldownBase;

                                if (hediff.Severity <= hediff.def.minSeverity)
                                {
                                    HealthUtility.CureHediff(hediff);
                                    ChronicHediffHealTime += RBank.Settings.HealChronicHediffCooldownBase;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            HealthUtility.CureHediff(healedHediffs.First());
                            ChronicHediffHealTime += RBank.Settings.HealChronicHediffCooldownBase;
                        }

                        return;
                    }
                }
            }

            return;
        }

        private void TryHealBadHediff(Pawn pawn)
        {
            Hediff hediff = FindLifeThreateningHediff(pawn);
            if (hediff != null)
            {
                HealthUtility.CureHediff(hediff);
                LameHediffHealTime += RBank.Settings.HealBadHediffCooldownBase;
                return;
            }
            hediff = FindImmunizableHediffWhichCanKill(pawn);
            if (hediff != null)
            {
                HealthUtility.CureHediff(hediff);
                LameHediffHealTime += RBank.Settings.HealBadHediffCooldownBase;
                return;
            }
            hediff = FindNonInjuryMiscBadHediff(pawn, onlyIfCanKill: true);
            if (hediff != null)
            {
                HealthUtility.CureHediff(hediff);
                LameHediffHealTime += RBank.Settings.HealBadHediffCooldownBase;
                return;
            }
            BodyPartRecord bodyPartRecord = FindBiggestMissingBodyPart(pawn);
            if (bodyPartRecord != null)
            {
                RestorePart(pawn, bodyPartRecord);
                hasInjuries = true;
                LameHediffHealTime += RBank.Settings.HealBadHediffCooldownBase;
                return;
            }
            hediff = FindAddiction(pawn);
            if (hediff != null)
            {
                HealthUtility.CureHediff(hediff);
                LameHediffHealTime += RBank.Settings.HealBadHediffCooldownBase;
                return;
            }
            hediff = FindNonInjuryMiscBadHediff(pawn, onlyIfCanKill: false);
            if (hediff != null)
            {
                HealthUtility.CureHediff(hediff);
                LameHediffHealTime += RBank.Settings.HealBadHediffCooldownBase;
                return;
            }

            foreach (var hed in pawn.health.hediffSet.hediffs)
            {
                if (hed.def.chronic) continue;
                if (hediff.def.spawnThingOnRemoved != null) continue;
                if (hed is Hediff_Injury) continue;
                if (hed.def.isBad)
                {
                    HealthUtility.CureHediff(hed);
                    LameHediffHealTime += RBank.Settings.HealBadHediffCooldownBase;
                    return;
                }
            }
        }

        private void TryHealInjuries(Pawn pawn, int ticks)
        {
            var healrate = pawn.HealthScale * RBank.Settings.InjuryHealPerTick * ticks;
            foreach (Hediff_Injury injury in pawn.health.hediffSet.GetHediffs<Hediff_Injury>())
            {
                injury.Heal(healrate);
                if (injury.Severity <= RBank.Settings.InjuryHealPerTick)
                    HealthUtility.CureHediff(injury);
            }
        }

        private void TryDecreaseAge(Pawn pawn, int ticks)
        {
            pawn.ageTracker.AgeBiologicalTicks = Math.Max((pawn.ageTracker.AgeBiologicalTicks - RBank.Settings.AgeDownTickRate * ticks), ageDownTo);
        }

        private void TryStabilizeLuciferium(Pawn pawn)
        {
            if (pawn.health.hediffSet.HasHediff(RBank.HediffDefOf.LuciferiumAddiction))
            {
                if (pawn.health.hediffSet.HasHediff(RBank.HediffDefOf.LuciferiumHigh))
                {
                    pawn.health.hediffSet.hediffs.RemoveAll(hediff => hediff.def == RBank.HediffDefOf.LuciferiumAddiction);
                    pawn.needs.AddOrRemoveNeedsAsAppropriate();
                }
                else
                {
                    pawn.health.AddHediff(RBank.HediffDefOf.LuciferiumHigh);
                    var need = pawn.needs.TryGetNeed(RBank.NeedDefOf.Chemical_Luciferium);
                    need.SetInitialLevel();
                }
            }
            else
            {
                pawn.health.AddHediff(RBank.HediffDefOf.LuciferiumHigh);
                pawn.health.AddHediff(RBank.HediffDefOf.LuciferiumAddiction);
                pawn.needs.AddOrRemoveNeedsAsAppropriate();
            }

            LuciferiumStabilizeTime += (int)(GenDate.TicksPerDay * (1 / refuelable.Props.fuelConsumptionRate));
        }

        public override string GetInspectString()
        {
            if (Prefs.DevMode)
            {
                return base.GetInspectString() + "\n" + LameHediffHealTime + "\n" + ChronicHediffHealTime;
            }
            return base.GetInspectString();
        }

        public override void Tick()
        {
            if (HasAnyContents)
            {
                if (hasJob)
                {
                    if (refuelable.HasFuel && power.PowerOn)
                    {
                        refuelable.CompTick();

                        if (Find.TickManager.TicksGame % 250 == 0) TickPeriod(250);
                    }
                    else
                    {
                        if (hasAgeHediffs) ++ChronicHediffHealTime;
                        if (hasLameHediffs) ++LameHediffHealTime;
                        if (nonStabilizedluciferium) ++LuciferiumStabilizeTime;

                        power.PowerOutput = 0;
                    }
                }
                else
                {
                    endTime = Find.TickManager.TicksGame;
                    power.PowerOutput = 0;
                }
            }
            else
                power.PowerOutput = 0;
        }

        protected virtual void TickPeriod(int ticks = 1)
        {
            Pawn pawn = ContainedThing as Pawn;

            if (needYoungenig)
            {
                TryDecreaseAge(pawn, ticks);
                needYoungenig = pawn.ageTracker.AgeBiologicalTicks > ageDownTo;
            }

            if (hasInjuries)
            {
                TryHealInjuries(pawn, ticks);
                hasInjuries = HasInjuryHediffs(pawn);
            }

            if (hasAgeHediffs && ChronicHediffHealTime <= Find.TickManager.TicksGame)
            {
                TryHealChronic(pawn);
                hasAgeHediffs = HasChronicHediffs(pawn);
            }

            if (hasLameHediffs && LameHediffHealTime <= Find.TickManager.TicksGame)
            {
                TryHealBadHediff(pawn);
                hasLameHediffs = HasLameHediffs(pawn);
            }

            if (nonStabilizedluciferium && LuciferiumStabilizeTime <= Find.TickManager.TicksGame)
            {
                TryStabilizeLuciferium(pawn);
                nonStabilizedluciferium = pawn.health.hediffSet.HasHediff(RBank.HediffDefOf.LuciferiumAddiction) || !pawn.health.hediffSet.HasHediff(RBank.HediffDefOf.LuciferiumHigh);
            }

            hasJob = needYoungenig || hasAgeHediffs || hasInjuries || hasLameHediffs || nonStabilizedluciferium;
        }

        protected override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);

            if (signal == CompRefuelable.RefueledSignal)
            {
                if (HasAnyContents)
                {
                    if (refuelable.HasFuel && hasJob)
                        power.PowerOutput = -power.Props.basePowerConsumption;
                }
            }
        }

        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (base.TryAcceptThing(thing, allowSpecialEffects))
            {
                Pawn pawn = thing as Pawn;

                enterTime = Find.TickManager.TicksGame;
                ChronicHediffHealTime = enterTime + RBank.Settings.HealChronicHediffCooldownBase;
                LameHediffHealTime = enterTime + RBank.Settings.HealBadHediffCooldownBase;
                endTime = -1;

                LuciferiumStabilizeTime = enterTime + (int)(GenDate.TicksPerDay * (1 / refuelable.Props.fuelConsumptionRate));

                ageDownTo = (int)(pawn.def.race.lifeStageAges?.Max(stage => stage.minAge) * GenDate.TicksPerYear);
                if (ageDownTo <= RBank.Settings.AgeDownTickRate) ageDownTo = RBank.Settings.AgeDownTickRate;

                hasAgeHediffs = HasChronicHediffs(pawn);
                hasInjuries = HasInjuryHediffs(pawn);
                hasLameHediffs = HasLameHediffs(pawn);
                needYoungenig = ageDownTo < pawn.ageTracker.AgeBiologicalTicks;
                nonStabilizedluciferium = pawn.health.hediffSet.HasHediff(RBank.HediffDefOf.LuciferiumAddiction) || !pawn.health.hediffSet.HasHediff(RBank.HediffDefOf.LuciferiumHigh);

                hasJob = needYoungenig || hasAgeHediffs || hasInjuries || hasLameHediffs || nonStabilizedluciferium;

                if (refuelable.HasFuel && hasJob)
                    power.PowerOutput = -power.Props.basePowerConsumption;

                return true;
            }
            return false;
        }

        public override void EjectContents()
        {
            Pawn pawn = ContainedThing as Pawn;

            bool psycheBroken = false;

            if ((Find.TickManager.TicksGame - enterTime) >= GenDate.TicksPerDay * (1 / refuelable.Props.fuelConsumptionRate))
            {
                ApplyPersonalNightmare(pawn);

                if ((Find.TickManager.TicksGame - enterTime) >= GenDate.TicksPerDay * (2 / refuelable.Props.fuelConsumptionRate))
                {
                    psycheBroken = true;
                }
            }

            power.PowerOutput = 0;
            base.EjectContents();

            if (psycheBroken)
            {
                if (Rand.Value < 0.1f) pawn.mindState.mentalBreaker.TryDoRandomMoodCausedMentalBreak();
                else pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, false);
            }
        }

        private void ApplyPersonalNightmare(Pawn pawn)
        {
            int nightmareTicks = -1;

            if (endTime < 0)
            {
                nightmareTicks = Find.TickManager.TicksGame - enterTime - (int)(GenDate.TicksPerDay * (1 / refuelable.Props.fuelConsumptionRate));
            }
            else
            {
                nightmareTicks = endTime - enterTime - (int)(GenDate.TicksPerDay * (1 / refuelable.Props.fuelConsumptionRate));
            }
            

            if (nightmareTicks < 0) return;


            float nightmarePercent = nightmareTicks / (GenDate.TicksPerDay * (1 / refuelable.Props.fuelConsumptionRate));

            foreach (var need in pawn.needs.AllNeeds)
            {
                need.CurLevel = need.CurLevel - nightmarePercent;
            }

            if (pawn.needs.mood != null)
            {
                foreach (var memory in pawn.needs.mood.thoughts.memories.Memories)
                {
                    memory.age = memory.age + nightmareTicks;
                }
                pawn.needs.mood.thoughts.memories.MemoryThoughtInterval();

                ApplyPersonalNightmareTicks(pawn, nightmareTicks);
            }
        }

        private void ApplyPersonalNightmareTicks(Pawn pawn, int nightmareTicks)
        {
            int nightmareStage = 0;

            if (nightmareTicks >= RBank.Settings.NightmareTicks)
            {
                nightmareStage = 1;
            }
            else
            {
                nightmareStage = 0;
            }

            Thought_Memory thought = pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(RBank.ThoughtDefOf.PsycheNightmare);

            if (thought != null)
            {
                thought.SetForcedStage(nightmareStage);
                thought.age = Math.Max(thought.age - nightmareTicks, 0);
            }
            else
            {
                thought = ThoughtMaker.MakeThought(RBank.ThoughtDefOf.PsycheNightmare, nightmareStage);

                thought.age = thought.def.DurationTicks - nightmareTicks;

                pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
            }

        }

        private static Hediff FindLifeThreateningHediff(Pawn pawn)
        {
            Hediff hediff = null;
            float num = -1f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (!hediffs[i].Visible || !hediffs[i].def.everCurableByItem || hediffs[i].FullyImmune() || hediffs[i].def.chronic)
                {
                    continue;
                }
                bool num2 = hediffs[i].CurStage?.lifeThreatening ?? false;
                bool flag = hediffs[i].def.lethalSeverity >= 0f && hediffs[i].Severity / hediffs[i].def.lethalSeverity >= 0.8f;
                if (num2 || flag)
                {
                    float num3 = (hediffs[i].Part != null) ? hediffs[i].Part.coverageAbsWithChildren : 999f;
                    if (hediff == null || num3 > num)
                    {
                        hediff = hediffs[i];
                        num = num3;
                    }
                }
            }
            return hediff;
        }

        private static Hediff FindImmunizableHediffWhichCanKill(Pawn pawn)
        {
            Hediff hediff = null;
            float num = -1f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (!hediffs[i].def.chronic && hediffs[i].TryGetComp<HediffComp_Immunizable>() != null && !hediffs[i].FullyImmune() && CanEverKill(hediffs[i]))
                {
                    float severity = hediffs[i].Severity;
                    if (hediff == null || severity > num)
                    {
                        hediff = hediffs[i];
                        num = severity;
                    }
                }
            }
            return hediff;
        }

        private static bool CanEverKill(Hediff hediff)
        {
            if (hediff.def.stages != null)
            {
                for (int i = 0; i < hediff.def.stages.Count; i++)
                {
                    if (hediff.def.stages[i].lifeThreatening)
                    {
                        return true;
                    }
                }
            }
            return hediff.def.lethalSeverity >= 0f;
        }

        private static Hediff FindNonInjuryMiscBadHediff(Pawn pawn, bool onlyIfCanKill)
        {
            Hediff hediff = null;
            float num = -1f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (!hediffs[i].def.chronic && hediffs[i].def.isBad && hediffs[i].def.everCurableByItem && !(hediffs[i] is Hediff_Injury) && !(hediffs[i] is Hediff_MissingPart) && !(hediffs[i] is Hediff_Addiction) && !(hediffs[i] is Hediff_AddedPart) && (!onlyIfCanKill || CanEverKill(hediffs[i])))
                {
                    float num2 = (hediffs[i].Part != null) ? hediffs[i].Part.coverageAbsWithChildren : 999f;
                    if (hediff == null || num2 > num)
                    {
                        hediff = hediffs[i];
                        num = num2;
                    }
                }
            }
            return hediff;
        }

        private static Hediff_Addiction FindAddiction(Pawn pawn)
        {
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff_Addiction hediff_Addiction = hediffs[i] as Hediff_Addiction;
                if (hediff_Addiction != null && hediff_Addiction.def.everCurableByItem)
                {
                    return hediff_Addiction;
                }
            }
            return null;
        }

        private static BodyPartRecord FindBiggestMissingBodyPart(Pawn pawn, float minCoverage = 0f)
        {
            BodyPartRecord bodyPartRecord = null;
            foreach (Hediff_MissingPart missingPartsCommonAncestor in pawn.health.hediffSet.GetMissingPartsCommonAncestors())
            {
                if (!(missingPartsCommonAncestor.Part.coverageAbsWithChildren < minCoverage) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(missingPartsCommonAncestor.Part) && (bodyPartRecord == null || missingPartsCommonAncestor.Part.coverageAbsWithChildren > bodyPartRecord.coverageAbsWithChildren))
                {
                    bodyPartRecord = missingPartsCommonAncestor.Part;
                }
            }
            return bodyPartRecord;
        }

        private void RestorePart(Pawn pawn, BodyPartRecord bodyPartRecord)
        {
            RestorePartRecursiveInt(pawn, bodyPartRecord);
            pawn.health.hediffSet.DirtyCache();
            pawn.health.CheckForStateChange(null, null);
        }

        private void RestorePartRecursiveInt(Pawn pawn, BodyPartRecord part)
        {
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int num = hediffs.Count - 1; num >= 0; num--)
            {
                Hediff hediff = hediffs[num];
                if (hediff.Part == part)
                {
                    hediffs.RemoveAt(num);
                    hediff.PostRemoved();
                }
            }

            Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(HediffDefOf.Scratch, pawn);
            hediff_Injury.Part = part;
            hediff_Injury.Severity = (part.def.GetMaxHealth(pawn) - 1.0f);
            pawn.health.AddHediff(hediff_Injury, null, null, null);

            for (int i = 0; i < part.parts.Count; i++)
            {
                RestorePartRecursiveInt(pawn, part.parts[i]);
            }
        }
    }
}