using System.Collections.Generic;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using System.Linq;

namespace CM_Meeseeks_Box
{
    public class JobDriver_AcquireEquipment : JobDriver
    {
        //private static NeededWarmth neededWarmth;

        private int duration;

        private int Duration => duration;

        private int unequipBuffer;

        private const TargetIndex ApparelInd = TargetIndex.A;

        private Thing TargetEquippable => job.GetTarget(TargetIndex.A).Thing;

        private Apparel TargetApparel => TargetEquippable as Apparel;

        private static int rescanEquipmentDelay = 250;

        private static StringBuilder debugSb;

        private static readonly SimpleCurve InsulationColdScoreFactorCurve_NeedWarm = new SimpleCurve
        {
            new CurvePoint(0f, 1f),
            new CurvePoint(30f, 8f)
        };

        private static readonly SimpleCurve HitPointsPercentScoreFactorCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(0.2f, 0.2f),
            new CurvePoint(0.22f, 0.6f),
            new CurvePoint(0.5f, 0.6f),
            new CurvePoint(0.52f, 1f)
        };

        private static HashSet<BodyPartGroupDef> tmpBodyPartGroupsWithRequirement = new HashSet<BodyPartGroupDef>();

        private static HashSet<ThingDef> tmpAllowedApparels = new HashSet<ThingDef>();

        private static HashSet<ThingDef> tmpRequiredApparels = new HashSet<ThingDef>();

        private static float MinMeleeWeaponDPSThreshold
        {
            get
            {
                List<Tool> tools = MeeseeksDefOf.MeeseeksRace.tools;
                float num = 0f;
                for (int i = 0; i < tools.Count; i++)
                {
                    if (tools[i].linkedBodyPartsGroup == BodyPartGroupDefOf.LeftHand || tools[i].linkedBodyPartsGroup == BodyPartGroupDefOf.RightHand)
                    {
                        num = tools[i].power / tools[i].cooldownTime;
                        break;
                    }
                }
                return num + 2f;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref duration, "duration", 0);
            Scribe_Values.Look(ref unequipBuffer, "unequipBuffer", 0);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetEquippable, job, 1, -1, null, errorOnFailed);
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            SetDuration();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil checkDuration = new Toil();
            checkDuration.initAction = delegate
            {
                this.SetDuration();
            };

            Toil startLabel = Toils_General.Label();
            yield return startLabel;

            this.FailOnBurningImmobile(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);

            Toil unequip = new Toil();
            unequip.tickAction = delegate
            {
                TryUnequipSomething();
            };
            unequip.WithProgressBarToilDelay(TargetIndex.A);
            unequip.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            unequip.defaultCompleteMode = ToilCompleteMode.Delay;
            unequip.defaultDuration = this.Duration;
            yield return unequip;

            yield return Toils_General.Do(delegate
            {
                Equip();
            });


            Toil findEquipment = new Toil();
            findEquipment.initAction = delegate
            {
                //Logger.MessageFormat(this, "Searching for equipment...");
                Thing foundEquipment = FindEquipment(pawn);
                if (foundEquipment != null && pawn.Reserve(foundEquipment, job, 1, -1, null, false))
                {
                    job.targetA = new LocalTargetInfo(foundEquipment);

                    this.SetDuration();
                    unequip.defaultDuration = this.Duration;
                    pawn.jobs.curDriver.JumpToToil(startLabel);
                }
            };
            yield return findEquipment;

            yield return Toils_General.Do(delegate
            {
                CompMeeseeksMemory compMeeseeksMemory = pawn.GetComp<CompMeeseeksMemory>();
                if (compMeeseeksMemory != null)
                    compMeeseeksMemory.acquiredEquipmentTick = Find.TickManager.TicksGame;
            });

        }

        private void SetDuration()
        {
            unequipBuffer = 0;

            if (TargetApparel == null)
            {
                duration = 2;
                return;
            }

            duration = (int)(TargetApparel.GetStatValue(StatDefOf.EquipDelay) * 60f);

            List<Apparel> wornApparel = pawn.apparel.WornApparel;
            for (int num = wornApparel.Count - 1; num >= 0; num--)
            {
                if (!ApparelUtility.CanWearTogether(TargetApparel.def, wornApparel[num].def, pawn.RaceProps.body))
                {
                    duration += (int)(wornApparel[num].GetStatValue(StatDefOf.EquipDelay) * 60f);
                }
            }
        }

        private void Equip()
        {
            if (TargetEquippable.def.IsWeapon)
                EquipWeapon();
            if (TargetApparel != null)
                EquipApparel();

            pawn.Map.reservationManager.ReleaseAllForTarget(TargetEquippable);
        }

        private void EquipWeapon()
        {
            ThingWithComps thingWithComps = (ThingWithComps)job.targetA.Thing;
            ThingWithComps thingWithComps2 = null;
            if (thingWithComps.def.stackLimit > 1 && thingWithComps.stackCount > 1)
            {
                thingWithComps2 = (ThingWithComps)thingWithComps.SplitOff(1);
            }
            else
            {
                thingWithComps2 = thingWithComps;
                thingWithComps2.DeSpawn();
            }
            pawn.equipment.MakeRoomFor(thingWithComps2);
            pawn.equipment.AddEquipment(thingWithComps2);
            if (thingWithComps.def.soundInteract != null)
            {
                thingWithComps.def.soundInteract.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            }
        }

        private void EquipApparel()
        {
            Apparel apparel = TargetApparel;
            pawn.apparel.Wear(apparel);
            if (pawn.outfits != null && job.playerForced)
            {
                pawn.outfits.forcedHandler.SetForced(apparel, forced: true);
            }
        }

        public static Thing FindEquipment(Pawn pawn)
        {
            debugSb = null;
            AddDebugLine(string.Concat("JobDriver_AcquireEquipment.FindEquipment - Scanning for ", pawn, " at ", pawn.Position));

            CompMeeseeksMemory compMeeseeksMemory = pawn.GetComp<CompMeeseeksMemory>();
            if (compMeeseeksMemory == null || compMeeseeksMemory.savedJob == null || compMeeseeksMemory.savedJob.targetA == null || !compMeeseeksMemory.savedJob.targetA.IsValid)
            {
                if (DebugViewSettings.debugApparelOptimize)
                {
                    AddDebugLine(string.Concat("Meeseeks, job or target invalid."));
                    Log.Message(debugSb.ToString());
                    debugSb = null;
                }
                return null;
            }

            AddDebugLine(string.Concat(compMeeseeksMemory.savedJob.def.ToString()));

            if (Find.TickManager.TicksGame < compMeeseeksMemory.acquiredEquipmentTick + rescanEquipmentDelay)
            {
                if (DebugViewSettings.debugApparelOptimize)
                {
                    AddDebugLine(string.Concat("Cannot find equipment for " + ((compMeeseeksMemory.acquiredEquipmentTick + rescanEquipmentDelay) - Find.TickManager.TicksGame).ToString() + " ticks."));
                    Log.Message(debugSb.ToString());
                    debugSb = null;
                }
                return null;
            }

            List<SkillDef> relevantSkills = null;
            if (compMeeseeksMemory.savedJob.workGiverDef == null)
            {
                AddDebugLine(string.Concat("No workGiverDef"));
                relevantSkills = new List<SkillDef>();
            }
            else
            {
                relevantSkills = new List<SkillDef>(compMeeseeksMemory.savedJob.workGiverDef.workType.relevantSkills);
            }

            // For some reason the entry for melee doesn't seem to get read for the kill job, so we'll do it manually...
            bool killJob = (compMeeseeksMemory.savedJob.def == MeeseeksDefOf.CM_Meeseeks_Box_Job_Kill);
            bool combatJob = (killJob || pawn.Drafted || compMeeseeksMemory.guardPosition.IsValid);

            if (compMeeseeksMemory.savedJob.def == JobDefOf.AttackMelee || combatJob)
                relevantSkills.Add(SkillDefOf.Melee);
            if (compMeeseeksMemory.savedJob.def == JobDefOf.AttackStatic || combatJob)
                relevantSkills.Add(SkillDefOf.Shooting);

            combatJob = (combatJob || (relevantSkills.Contains(SkillDefOf.Melee) || relevantSkills.Contains(SkillDefOf.Shooting)));

            AddDebugLine("combatJob == " + combatJob.ToString());

            foreach (SkillDef skill in relevantSkills)
                AddDebugLine(skill.defName);

            // I mean this shouldn't be possible but...
            if (combatJob && pawn.RaceProps.Humanlike && pawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                return null;
            }

            IntVec3 savedJobTarget = compMeeseeksMemory.savedJob.targetA.Cell;
            float distanceToTarget = savedJobTarget.DistanceTo(pawn.PositionHeld);
            float maxTotalDistance = distanceToTarget * 2.5f;
            float maxDistanceToEquipment = distanceToTarget * 0.75f;

            if (!killJob)
            {
                maxTotalDistance *= 2.0f;
                maxDistanceToEquipment *= 2.0f;
            }

            AddDebugLine("distanceToTarget: " + distanceToTarget.ToString());
            AddDebugLine("maxTotalDistance: " + maxTotalDistance.ToString());
            AddDebugLine("maxDistanceToEquipment: " + maxDistanceToEquipment.ToString());

            bool isWeapon = (pawn.equipment.Primary == null);
            Thing selectedEquipment = null;
            float selectedEquipmentScore = 0.0f;

            List<Thing> availableEquipment = new List<Thing>();

            if (isWeapon)
            {
                AddDebugLine("scanning for weapons...");
                availableEquipment = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Weapon).Where(t => CanEquipThis(combatJob, pawn, compMeeseeksMemory, t, relevantSkills)).ToList();
            }

            if (availableEquipment.Count == 0)
            {
                // If the weapon search fails, default to an apparel search
                isWeapon = false;
                AddDebugLine("scanning for apparel...");
                availableEquipment = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Apparel).Where(t => CanWearThis(pawn, compMeeseeksMemory, t, relevantSkills)).ToList();
            }

            AddDebugLine("availableEquipment: " + availableEquipment.Count.ToString());

            if (availableEquipment.Count == 0)
            {
                if (DebugViewSettings.debugApparelOptimize)
                {
                    Log.Message(debugSb.ToString());
                    debugSb = null;
                }
                return null;
            }

            foreach (Thing equippable in availableEquipment)
            {
                float distanceToEquippable = equippable.PositionHeld.DistanceTo(pawn.PositionHeld);
                float distanceEquippableToTarget = equippable.PositionHeld.DistanceTo(savedJobTarget);
                float totalDistance = distanceToTarget + distanceToEquippable + distanceEquippableToTarget;

                //AddDebugLine("distanceToEquippable: " + equippable.def.defName + " - " + distanceToEquippable);
                //AddDebugLine("distanceEquippableToTarget: " + equippable.def.defName + " - " + distanceEquippableToTarget);
                //AddDebugLine("totalDistance: " + equippable.def.defName + " - " + totalDistance);

                if (distanceToEquippable < maxDistanceToEquipment && totalDistance <= maxTotalDistance)
                {
                    float equipmentScore = GetEquipmentScore(pawn, equippable, relevantSkills);
                    float distanceFactor = totalDistance + 1.0f;
                    equipmentScore = (equipmentScore * equipmentScore) / (distanceFactor * distanceFactor);

                    //AddDebugLine("equipmentScore: " + equippable.def.defName + " - " + equipmentScore);
                    if (DebugViewSettings.debugApparelOptimize && equipmentScore > 0.0f)
                    {
                        debugSb.AppendLine(equippable.LabelCap + ": " + equipmentScore.ToString("F2"));
                    }

                    if (equipmentScore > selectedEquipmentScore)
                    {
                        selectedEquipment = equippable;
                        selectedEquipmentScore = equipmentScore;
                    }
                }
                else
                {
                    AddDebugLine("Too far away: " + equippable.GetUniqueLoadID());
                }
            }

            AddDebugLine("BEST: " + selectedEquipment);

            if (DebugViewSettings.debugApparelOptimize)
            {
                Log.Message(debugSb.ToString());
                debugSb = null;
            }

            if (selectedEquipment != null)
                return selectedEquipment;

            return null;
        }

        private static void AddDebugLine(string debugString)
        {
            if (DebugViewSettings.debugApparelOptimize)
            {
                if (debugSb == null)
                    debugSb = new StringBuilder();
                debugSb.AppendLine(debugString);
            }
        }

        private static bool CanEquipThis(bool combat, Pawn pawn, CompMeeseeksMemory compMeeseeksMemory, Thing weapon, List<SkillDef> relevantSkills)
        {
            if (weapon.IsForbidden(pawn))
            {
                AddDebugLine("Forbidden: " + weapon.GetUniqueLoadID());
                return false;
            }
            if (weapon.IsBurning())
            {
                AddDebugLine("Burning: " + weapon.GetUniqueLoadID());
                return false;
            }
            if (EquipmentUtility.IsBiocoded(weapon))
            {
                AddDebugLine("Biocoded: " + weapon.GetUniqueLoadID());
                return false;
            }
            if (!pawn.CanReserveAndReach(weapon, PathEndMode.OnCell, pawn.NormalMaxDanger()))
            {
                AddDebugLine("Can't touch this: " + weapon.GetUniqueLoadID());
                return false;
            }
            if (!combat && !HasReleventStatModifiers(weapon, relevantSkills))
            {
                AddDebugLine("Irrelevant: " + weapon.GetUniqueLoadID());
                return false;
            }

            return true;
        }

        private static bool CanWearThis(Pawn pawn, CompMeeseeksMemory compMeeseeksMemory, Thing apparel, List<SkillDef> relevantSkills)
        {
            if (apparel.IsForbidden(pawn))
                return false;
            if (apparel.IsBurning())
                return false;
            if (EquipmentUtility.IsBiocoded(apparel))
                return false;
            if (!ApparelUtility.HasPartsToWear(pawn, apparel.def))
                return false;
            if (!pawn.CanReserveAndReach(apparel, PathEndMode.OnCell, pawn.NormalMaxDanger()))
                return false;

            bool armor = false;
            foreach (ThingCategoryDef thingCategoryDef in apparel.def.thingCategories)
            {
                if (thingCategoryDef.defName.ToLower().Contains("armor".ToLower()))
                {
                    armor = true;
                    break;
                }
            }

            if (pawn.Drafted || compMeeseeksMemory.guardPosition.IsValid)
            {
                if (!armor)
                    return false;
            }
            else
            {
                if (armor)
                    return false;

                if (!HasReleventStatModifiers(apparel, relevantSkills))
                    return false;
            }

            List<Apparel> wornApparel = pawn.apparel.WornApparel;
            for (int i = 0; i < wornApparel.Count; i++)
            {
                if (!ApparelUtility.CanWearTogether(wornApparel[i].def, apparel.def, pawn.RaceProps.body))
                {
                    return false;
                }
            }

            return true;
        }

        private static float GetEquipmentScore(Pawn pawn, Thing equippable, List<SkillDef> relevantSkills)
        {
            if (equippable.def.IsWeapon)
                return GetWeaponScore(equippable, relevantSkills);
            else
                return GetApparelScore(pawn, equippable);
        }

        private static float GetWeaponScore(Thing weapon, List<SkillDef> relevantSkills)
        {
            if (weapon == null)
            {
                return 0.0f;
            }

            if (relevantSkills.Contains(SkillDefOf.Shooting))
            {
                if (weapon.def.IsRangedWeapon && weapon.GetStatValue(StatDefOf.RangedWeapon_Cooldown) > 0.0f && !weapon.def.Verbs.NullOrEmpty())
                {
                    float damage = 0.0f;
                    VerbProperties verb = weapon.def.Verbs.First((VerbProperties x) => x.isPrimary);
                    if (verb != null)
                    {
                        if (verb.defaultProjectile != null)
                            damage = verb.defaultProjectile.projectile.GetDamageAmount(weapon);
                        if (verb.burstShotCount != 0)
                            damage *= verb.burstShotCount;

                        return damage / weapon.GetStatValue(StatDefOf.RangedWeapon_Cooldown);
                    }
                }
            }

            if (relevantSkills.Contains(SkillDefOf.Melee))
            {
                if (weapon.def.IsMeleeWeapon)
                {
                    float averageDPS = weapon.GetStatValue(StatDefOf.MeleeWeapon_AverageDPS);
                    if (averageDPS > MinMeleeWeaponDPSThreshold)
                        return weapon.GetStatValue(StatDefOf.MeleeWeapon_AverageDPS);
                }
            }

            return 1.0f;
        }

        public static float GetApparelScore(Pawn pawn, Thing thing)
        {
            Apparel apparel = thing as Apparel;

            if (apparel is ShieldBelt && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsWeaponUsingProjectiles)
            {
                return -1000f;
            }

            float score = 0.1f + apparel.def.apparel.scoreOffset;
            float armorScore = apparel.GetStatValue(StatDefOf.ArmorRating_Sharp) + apparel.GetStatValue(StatDefOf.ArmorRating_Blunt) + apparel.GetStatValue(StatDefOf.ArmorRating_Heat);
            score += armorScore;

            if (apparel.def.useHitPoints)
            {
                float x = (float)apparel.HitPoints / (float)apparel.MaxHitPoints;
                score *= HitPointsPercentScoreFactorCurve.Evaluate(x);
            }

            score += apparel.GetSpecialApparelScoreOffset();

            //float warmthScore = 1f;
            //if (neededWarmth == NeededWarmth.Warm)
            //{
            //    float statValue = apparel.GetStatValue(StatDefOf.Insulation_Cold);
            //    warmthScore *= InsulationColdScoreFactorCurve_NeedWarm.Evaluate(statValue);
            //}
            //score *= warmthScore;

            // Human body is close enough
            float coverageScore = apparel.def.apparel.HumanBodyCoverage * 10.0f;
            score *= coverageScore;

            return score;
        }

        private static bool HasReleventStatModifiers(Thing weapon, List<SkillDef> relevantSkills)
        {
            List<StatModifier> statModifiers = weapon.def.equippedStatOffsets;
            if (relevantSkills != null && statModifiers != null)
            {
                //Logger.MessageFormat(this, "Found relevantSkills...");
                foreach (StatModifier statModifier in statModifiers)
                {
                    List<SkillNeed> skillNeedOffsets = statModifier.stat.skillNeedOffsets;
                    List<SkillNeed> skillNeedFactors = statModifier.stat.skillNeedFactors;

                    if (skillNeedOffsets != null)
                    {
                        //Logger.MessageFormat(this, "Found skillNeedOffsets...");
                        foreach (SkillNeed skillNeed in skillNeedOffsets)
                        {
                            if (relevantSkills.Contains(skillNeed.skill))
                            {
                                //Logger.MessageFormat(this, "{0} has {1}, relevant to {2}", weapon.Label, statModifier.stat.label, skillNeed.skill);
                                return true;
                            }
                        }
                    }

                    if (skillNeedFactors != null)
                    {
                        foreach (SkillNeed skillNeed in skillNeedFactors)
                        {
                            if (relevantSkills.Contains(skillNeed.skill))
                            {
                                //Logger.MessageFormat(this, "{0} has {1}, relevant to {2}", weapon.Label, statModifier.stat.label, skillNeed.skill);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void TryUnequipSomething()
        {
            if (TargetApparel == null)
                return;

            unequipBuffer++;
            Apparel apparel = TargetApparel;
            List<Apparel> wornApparel = pawn.apparel.WornApparel;
            for (int num = wornApparel.Count - 1; num >= 0; num--)
            {
                if (!ApparelUtility.CanWearTogether(apparel.def, wornApparel[num].def, pawn.RaceProps.body))
                {
                    int num2 = (int)(wornApparel[num].GetStatValue(StatDefOf.EquipDelay) * 60f);
                    if (unequipBuffer >= num2)
                    {
                        bool forbid = pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer);
                        if (!pawn.apparel.TryDrop(wornApparel[num], out var _, pawn.PositionHeld, forbid))
                        {
                            Log.Error(string.Concat(pawn, " could not drop ", wornApparel[num].ToStringSafe()));
                            EndJobWith(JobCondition.Errored);
                        }
                    }
                    break;
                }
            }
        }
    }
}
