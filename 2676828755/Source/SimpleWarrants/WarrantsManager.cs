using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace SimpleWarrants
{
    public class LordCollection : IExposable
    {
        public List<Lord> lords;

        public void ExposeData()
        {
            Scribe_Collections.Look(ref lords, "lords", LookMode.Reference);
        }
    }
    public class WarrantsManager : GameComponent
    {
        private const int TYPE_ARTIFACT = 0;
        private const int TYPE_ANIMAL = 1;
        private const int TYPE_PAWN = 2;
        private const int TYPE_TAME = 3;

        public static WarrantsManager Instance;
        private static readonly float[] warrantTypeToWeight = new float[4];
        private static readonly int[] warrantTypes = new int[4];

        static WarrantsManager()
        {
            for (int i = 0; i < warrantTypes.Length; i++)
            {
                warrantTypes[i] = i;
            }
        }

        public List<Warrant> acceptedWarrants; // Warrants accepted by the player, to be completed by the player.
        public List<Warrant> availableWarrants; // Available warrants, created by AI.
        public List<Warrant> createdWarrants; // Warrants created by the player, to be completed by AI.
        public bool initialized;
        public int lastWarrantID;
        public Dictionary<Warrant_Pawn, LordCollection> raidLords;
        public List<Warrant> takenWarrants;
        private List<LordCollection> lordValues;

        private List<Warrant_Pawn> warrantKeys;

        public WarrantsManager()
        {
            Instance = this;
        }

        public WarrantsManager(Game game)
        {
            Instance = this;
        }

        public void PreInit()
        {
            Instance = this;
            availableWarrants ??= new List<Warrant>();
            acceptedWarrants ??= new List<Warrant>();
            createdWarrants ??= new List<Warrant>();
            takenWarrants ??= new List<Warrant>();
            raidLords ??= new Dictionary<Warrant_Pawn, LordCollection>();
        }

        public override void StartedNewGame()
        {
            PreInit();
            base.StartedNewGame();
            if (!initialized && !availableWarrants.Any())
            {
                PopulateWarrants(Rand.RangeInclusive(3, 5));
                initialized = true;
            }
        }

        public override void LoadedGame()
        {
            PreInit();
            base.LoadedGame();
            if (!initialized && !availableWarrants.Any())
            {
                PopulateWarrants(Rand.RangeInclusive(3, 5));
                initialized = true;
            }
        }

        public void PopulateWarrants(int amountToPopulate)
        {
            int num = 0;
            var count = 0;
            while (count < amountToPopulate && num < amountToPopulate * 5)
            {
                num++;

                var warrant = GenerateRandomWarrant(false);
                if (warrant == null || !warrant.CanPlayerReceive())
                    continue;

                availableWarrants.Add(warrant);
                count++;
            }

            if (count < num)
                Log.Warning($"Generated {count}/{amountToPopulate} warrants in {num} tries. Colony wealth may be too low.");
        }

        private Warrant GenerateRandomWarrant(bool includeColonists = true)
        {
            warrantTypeToWeight[TYPE_ARTIFACT] = SimpleWarrantsMod.Settings.enableWarrantsOnArtifact ? 0.25f : 0f;
            warrantTypeToWeight[TYPE_PAWN] = 1f;
            warrantTypeToWeight[TYPE_ANIMAL] = SimpleWarrantsMod.Settings.enableWarrantsOnAnimals ? 0.2f : 0f;
            warrantTypeToWeight[TYPE_TAME] = SimpleWarrantsMod.Settings.enableTamingWarrants ? 0.2f : 0f;

            int type = warrantTypes.RandomElementByWeight(t => warrantTypeToWeight[t]);

            switch (type)
            {
                case TYPE_PAWN:
                    #region PAWN WARRANT
                    // Human warrant.
                    var humanlikeColonists = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Where(x => x.HomeFaction == Faction.OfPlayer && x.RaceProps.Humanlike).ToList();

                    bool placeOnColonist = includeColonists &&
                                           SimpleWarrantsMod.Settings.enableWarrantsOnColonists &&
                                           humanlikeColonists.Any(CanPutWarrantOn) &&
                                           Rand.Chance(SimpleWarrantsMod.Settings.chanceOfWarrantsMadeOnColonist);

                    var warrant = new Warrant_Pawn
                    {
                        loadID = GetWarrantID(),
                        createdTick = Find.TickManager.TicksGame,
                        thing = PawnGenerator.GeneratePawn(Utils.AllWorthAnimalDefs.RandomElement())
                    };

                    if (placeOnColonist)
                    {
                        // Give to a colonist that does not already have a warrant.
                        var colonist = humanlikeColonists.Where(CanPutWarrantOn).RandomElement();
                        warrant.thing = colonist;
                        warrant.issuer = Utils.AnyHostileToPlayerFaction();

                        if (warrant.issuer == null)
                        {
                            Log.Error("Failed to find a valid faction to issue warrant (non-hostile).");
                            return null;
                        }

                        Find.LetterStack.ReceiveLetter("SW.WarrantOnYourColonist".Translate(colonist.Named("PAWN")), "SW.WarrantOnYourColonistDesc".Translate(colonist.Named("PAWN")), LetterDefOf.NegativeEvent, colonist);
                    }
                    else
                    {
                        // Generate a random pawn to give a warrant to.
                        // The pawn is made part of a random human-like faction.
                        var randomKind = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.Humanlike && x.defaultFactionType != Faction.OfPlayer.def).RandomElement();
                        Faction faction = null;

                        if (randomKind.defaultFactionType != null)
                            faction = Find.FactionManager.FirstFactionOfDef(randomKind.defaultFactionType);

                        faction ??= Find.FactionManager.AllFactions.Where(x => x.def.humanlikeFaction && !x.defeated && !x.IsPlayer && !x.Hidden).RandomElement();

                        Find.FactionManager.AllFactions.Where(fac =>
                            fac.def.humanlikeFaction &&
                            !fac.defeated &&
                            !fac.Hidden &&
                            !fac.IsPlayer &&
                            !fac.HostileTo(Faction.OfPlayer) &&
                            Find.World.worldObjects.Settlements.Any(settlement => settlement.Faction == fac)).TryRandomElement(out warrant.issuer);

                        if (warrant.issuer == null)
                        {
                            Log.Error("Failed to find a valid faction to issue warrant (non-hostile humanlike w/ fac base).");
                            return null;
                        }

                        warrant.thing = PawnGenerator.GeneratePawn(randomKind, faction);
                    }

                    warrant.reason = Utils.GenerateTextFromRule(SW_DefOf.SW_WantedFor, warrant.pawn.thingIDNumber);
                    AssignRewards(warrant);
                    return warrant;
                #endregion

                case TYPE_ANIMAL:
                    #region ANIMAL WARRANT
                    warrant = new Warrant_Pawn
                    {
                        loadID = GetWarrantID(),
                        createdTick = Find.TickManager.TicksGame,
                        thing = PawnGenerator.GeneratePawn(Utils.AllWorthAnimalDefs.RandomElement())
                    };

                    Find.FactionManager.AllFactions.Where(faction =>
                        faction.def.humanlikeFaction &&
                        !faction.defeated &&
                        !faction.Hidden &&
                        !faction.IsPlayer &&
                        faction.RelationKindWith(Faction.OfPlayer) != FactionRelationKind.Hostile &&
                        Find.World.worldObjects.Settlements.Any(settlement => settlement.Faction == faction)).TryRandomElement(out warrant.issuer);

                    if (warrant.issuer == null)
                    {
                        Log.Error("Failed to find a valid faction to issue warrant (non-hostile humanlike w/ fac base).");
                        warrant.thing.Destroy();
                        return null;
                    }

                    AssignRewards(warrant);
                    return warrant;
                #endregion

                case TYPE_ARTIFACT:
                    #region ARTIFACT WARRANT
                    // Artifact warrant.
                    var artWarrant = new Warrant_Artifact
                    {
                        loadID = GetWarrantID(),
                        createdTick = Find.TickManager.TicksGame
                    };

                    Find.FactionManager.AllFactions.Where(faction =>
                        faction.def.humanlikeFaction &&
                        !faction.defeated &&
                        !faction.Hidden &&
                        !faction.IsPlayer &&
                        !faction.HostileTo(Faction.OfPlayer) &&
                        Find.World.worldObjects.Settlements.Any(settlement => settlement.Faction == faction)).TryRandomElement(out artWarrant.issuer);

                    if (artWarrant.issuer == null)
                    {
                        Log.Error("Failed to find a valid faction to issue warrant (non-hostile humanlike w/ fac base).");
                        return null;
                    }

                    var artifacts = Utils.AllArtifactDefs;
                    var randomArtifact = artifacts.RandomElement();
                    artWarrant.thing = ThingMaker.MakeThing(randomArtifact);
                    artWarrant.reward = (int)(artWarrant.thing.MarketValue * Rand.Range(0.5f, 2f));
                    DoWealthScaling(artWarrant);
                    return artWarrant;
                #endregion

                case TYPE_TAME:
                    var tameWarrant = new Warrant_TameAnimal()
                    {
                        loadID = GetWarrantID(),
                        createdTick = Find.TickManager.TicksGame
                    };

                    Find.FactionManager.AllFactions.Where(faction =>
                        faction.def.humanlikeFaction &&
                        !faction.defeated &&
                        !faction.Hidden &&
                        !faction.IsPlayer &&
                        !faction.HostileTo(Faction.OfPlayer) &&
                        Find.World.worldObjects.Settlements.Any(settlement => settlement.Faction == faction)).TryRandomElement(out tameWarrant.issuer);

                    if (tameWarrant.issuer == null)
                    {
                        Log.Error("Failed to find a valid faction to issue warrant (non-hostile humanlike w/ fac base).");
                        return null;
                    }

                    var playerHome = Find.AnyPlayerHomeMap;
                    if (playerHome == null)
                        return null;

                    // Get a random animal kind that can spawn in the player map
                    // in the current season, and is tameable.
                    var allAnimals = (from animal in playerHome.Biome.AllWildAnimals
                                      where playerHome.mapTemperature.SeasonAcceptableFor(animal.race) &&
                                            animal.RaceProps.wildness < 1f
                                      select animal).ToList();

                    if (!allAnimals.TryRandomElementByWeight(a => a.race.GetStatValueAbstract(StatDefOf.MarketValue), out tameWarrant.AnimalRace))
                    {
                        Log.Error($"Failed to find animal type to spawn for tame warrant. There were {allAnimals.Count} candidates.");
                        return null;
                    }

                    float marketValue = tameWarrant.AnimalRace.race.GetStatValueAbstract(StatDefOf.MarketValue);

                    tameWarrant.Reward = (int)(marketValue * Rand.Range(0.7f, 2.5f));
                    DoWealthScaling(tameWarrant);

                    // Cap reward at 350% of animal market value.
                    tameWarrant.Reward = (int)Mathf.Min(marketValue * 3.5f, tameWarrant.Reward);
                    return tameWarrant;

                default:
                    Log.Error($"Warrant type switch invalid: {type}");
                    return null;
            }
        }

        private static void AssignRewards(Warrant_Pawn warrant)
        {
            var awardForLiving = (int)(warrant.pawn.MarketValue * Rand.Range(0.5f, 2f));
            int rewardForDead  = (int)(awardForLiving * Rand.Range(0.3f, 0.7f));

            if (warrant.pawn.def.race.Animal)
            {
                warrant.rewardForDead = rewardForDead;
            }
            else
            {
                warrant.rewardForLiving = awardForLiving;
                if (Rand.Chance(0.3f))
                    warrant.rewardForDead = rewardForDead;
            }

            DoWealthScaling(warrant);
        }

        private static void DoWealthScaling(Warrant warrant)
        {
            if (!SimpleWarrantsMod.Settings.warrantRewardScaling)
                return;

            const float SCALING = 0.025f;
            float playerWealth = Find.AnyPlayerHomeMap.wealthWatcher.WealthTotal;
            int increment = (int)(playerWealth * SCALING);

            if (increment <= 0)
                return;

            switch (warrant)
            {
                case Warrant_Pawn pawn:
                    if (pawn.rewardForLiving > 0)
                        pawn.rewardForLiving += increment;
                    if (pawn.rewardForDead > 0)
                        pawn.rewardForDead += increment;
                    return;

                case Warrant_Artifact art:
                    art.reward += increment;
                    return;

                case Warrant_TameAnimal tame:
                    tame.Reward += increment;
                    return;
            }
        }

        public bool CanPutWarrantOn(Pawn pawn)
        {
            return availableWarrants.OfType<Warrant_Pawn>().All(x => x.pawn != pawn) && (!pawn.IsColonist || SimpleWarrantsMod.Settings.enableWarrantsOnColonists);
        }

        public void PutWarrantOn(Pawn victim, string reason, Faction issuer = null)
        {
            if (issuer == Faction.OfPlayer)
            {
                return;// seems that one of method is calling this with faction player argument, it should prevent the issue
            }
            var warrant = new Warrant_Pawn 
            {
                loadID = GetWarrantID(),
                createdTick = Find.TickManager.TicksGame
            };
            warrant.thing = victim;
            if (issuer != null)
            {
                warrant.issuer = issuer;
            }
            else
            {
                warrant.issuer = Find.FactionManager.AllFactions.Where(faction => faction.def.humanlikeFaction && !faction.defeated && !faction.Hidden 
                    && !faction.IsPlayer && faction.RelationKindWith(victim.Faction) == FactionRelationKind.Hostile && Find.World.worldObjects.Settlements.Any(settlement => settlement.Faction == faction))
                     .RandomElement();
            }
            warrant.reason = reason;
            Find.LetterStack.ReceiveLetter("SW.WarrantOnYourColonistReason".Translate(victim.Named("PAWN"), reason), 
                "SW.WarrantOnYourColonistDesc".Translate(victim.Named("PAWN")), LetterDefOf.NegativeEvent, victim);
            AssignRewards(warrant);
            availableWarrants.Add(warrant);
        }

        public string GetWarrantID()
        {
            lastWarrantID++;
            return "Warrant" + lastWarrantID;
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            HandleAvailableWarrants();
            HandleAcceptedWarrants();
            HandleCreatedWarrants();
            HandleFactionsTakenWarrants();
        }

        private void HandleAvailableWarrants()
        {
            // Remove expired warrants.
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                for (int num = availableWarrants.Count - 1; num >= 0; num--)
                {
                    if ((Find.TickManager.TicksGame - availableWarrants[num].createdTick).TicksToDays() >= 15 || availableWarrants[num].createdTick == -1)
                    {
                        availableWarrants.RemoveAt(num);
                    }
                }
            }

            if (Rand.MTBEventOccurs(SimpleWarrantsMod.Settings.warrantGenMTB, GenDate.TicksPerDay, 1))
            {
                var warrant = GenerateRandomWarrant();

                // Would it be better to re-roll warrant if CanPlayerReceive() is false?
                if (warrant != null && warrant.CanPlayerReceive())
                {
                    availableWarrants.Add(warrant);
                }
            }

            // Available warrants that target a colonist trigger raids periodically.
            foreach (var warrant in availableWarrants.Where(x => x.IsThreatForPlayer()))
            {
                if (Rand.MTBEventOccurs(SimpleWarrantsMod.Settings.bountyHunterMTB, GenDate.TicksPerDay, 1))
                {
                    var map = Find.AnyPlayerHomeMap;
                    if (map == null)
                        break;

                    var parameters = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
                    Find.FactionManager.AllFactionsVisible.Where(x => x.def.humanlikeFaction && x.HostileTo(Faction.OfPlayer)).TryRandomElement(out parameters.faction);
                    parameters.points *= SimpleWarrantsMod.Settings.bountyHunterRaidScale;

                    if (parameters.faction == null)
                    {
                        Log.Error("Failed to find raider faction for warrant hunters (hostile humanlike).");
                        continue;
                    }

                    IncidentWorker_Raid_TryGenerateRaidInfo_Patch.huntForWarrant = true;
                    IncidentDefOf.RaidEnemy.Worker.TryExecute(parameters);
                    IncidentWorker_Raid_TryGenerateRaidInfo_Patch.huntForWarrant = false;
                }
            }
        }

        public void HandleAcceptedWarrants()
        {
            // Update warrants accepted by the player.
            // If the player lets a warrant expire, they may be accused of fraud.

            for (int num = acceptedWarrants.Count - 1; num >= 0; num--)
            {
                var warrant = acceptedWarrants[num];
                if (!warrant.IsWarrantActive())
                {
                    warrant.End();

                    // Always reduce relationship (can be changed in settings)
                    int relationshipDamage = SimpleWarrantsMod.Settings.failedAIWarrantRelationshipDamage;
                    if (relationshipDamage > 0)
                        warrant.issuer.TryAffectGoodwillWith(Faction.OfPlayer, -relationshipDamage);

                    // 25% chance to accuse of fraud.
                    if (Rand.Chance(0.25f))
                    {
                        var pawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists_NoSlaves.Where(CanPutWarrantOn);
                        if (pawns.TryRandomElement(out var pawn))
                        {
                            PutWarrantOn(pawn, "SW.Fraud".Translate(), warrant.issuer);
                        }
                    }
                    acceptedWarrants.RemoveAt(num);
                }
            }
        }

        public void HandleCreatedWarrants()
        {
            for (int num = createdWarrants.Count - 1; num >= 0; num--)
            {
                var warrant = createdWarrants[num];
                var chance = warrant.AcceptChance() / (GenDate.TicksPerDay * 7);
                var success = Rand.Chance(chance);
                if (success)
                {
                    bool IsValidFaction(Faction faction)
                        => faction.def.humanlikeFaction &&
                           !faction.defeated &&
                           !faction.Hidden &&
                           !faction.IsPlayer &&
                           warrant.issuer != faction &&
                           faction.RelationKindWith(Faction.OfPlayer) != FactionRelationKind.Hostile &&
                           Find.World.worldObjects.Settlements.Any(settlement => settlement.Faction == faction);

                    if (!Find.FactionManager.AllFactions.Where(IsValidFaction).TryRandomElement(out var takerFaction))
                    {
                        Log.ErrorOnce("Failed to find any valid faction to accept player warrant.", warrant.GetHashCode());
                        continue;
                    }

                    warrant.AcceptBy(takerFaction);
                    createdWarrants.RemoveAt(num);
                    takenWarrants.Add(warrant);
                    warrant.tickToBeCompleted = Find.TickManager.TicksGame + (GenDate.TicksPerDay * (int)Rand.Range(3f, 15f));
                    Messages.Message("SW.FactionTookYourWarrant".Translate(takerFaction.Named("FACTION"), warrant.thing.LabelCap), MessageTypeDefOf.PositiveEvent);
                }
            }
        }

        private void HandleFactionsTakenWarrants()
        {
            // Tick player-created warrants taken by other factions.
            for (int num = takenWarrants.Count - 1; num >= 0; num--)
            {
                var warrant = takenWarrants[num];
                if (warrant.accepteer.HostileTo(Faction.OfPlayer))
                {
                    takenWarrants.RemoveAt(num);
                    createdWarrants.Add(warrant);
                    Messages.Message("SW.FactionDroppedWarrant".Translate(warrant.accepteer.Named("FACTION"), warrant.thing.LabelCap), MessageTypeDefOf.NegativeEvent);
                    continue;
                }

                // Wait until the warrant is ready to be completed.
                if (Find.TickManager.TicksGame <= warrant.tickToBeCompleted)
                    continue;

                takenWarrants.RemoveAt(num);
                var chance = warrant.SuccessChance();
                var success = Rand.Chance(chance);
                if (success)
                {
                    var reward = 0;
                    bool dead = false;
                    switch (warrant)
                    {
                        case Warrant_Pawn wp:
                        {
                            // Chance to be returned alive is the ratio between living and dead reward.
                            float chanceReturnedAlive = Mathf.Clamp01((float)wp.rewardForLiving / (wp.rewardForLiving + wp.rewardForDead));
                            if (wp.rewardForDead > 0 && !Rand.Chance(chanceReturnedAlive))
                            {
                                dead = true;
                            }
                            reward = dead ? wp.rewardForDead : wp.rewardForLiving;
                            break;
                        }

                        case Warrant_Artifact wa:
                            reward = wa.reward;
                            break;
                    }
                    var map = Find.AnyPlayerHomeMap;
                    var silvers = map.listerThings.ThingsOfDef(ThingDefOf.Silver).Where(x => !x.Position.Fogged(x.Map) && (map.areaManager.Home[x.Position] || x.IsInAnyStorage())).ToList();

                    string title = "SW.FactionCompletedWarrant".Translate(warrant.accepteer.Named("FACTION"));
                    DiaNode diaNode = new DiaNode("SW.FactionCompletedWarrantDesc".Translate(warrant.accepteer.Named("FACTION"), warrant.thing.LabelCap, reward));
                    DiaOption payOption = new DiaOption("SW.Pay".Translate(reward));
                    payOption.action = delegate
                    {
                        while (reward > 0)
                        {
                            Thing thing = silvers.RandomElement();
                            silvers.Remove(thing);
                            if (thing == null)
                            {
                                break;
                            }
                            int num = Math.Min(reward, thing.stackCount);
                            thing.SplitOff(num).Destroy();
                            reward -= num;
                        }

                        var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.FactionArrival, map);
                        parms.faction = warrant.accepteer;
                        var toDeliver = warrant.thing;
                        if (dead)
                        {
                            var pawn = warrant.thing as Pawn;
                            pawn.Kill(null);
                            toDeliver = pawn.Corpse;
                        }
                        else
                        {
                            if (warrant.thing is Pawn pawn)
                            {
                                //HealthUtility.DamageUntilDowned(pawn);
                                HealthUtility.DamageLegsUntilIncapableOfMoving(pawn, false);
                            }
                        }
                        ((IncidentWorker_Visitors)SW_DefOf.SW_Visitors.Worker).SpawnVisitors(toDeliver, parms);
                    };
                    payOption.resolveTree = true;
                    if (silvers.Sum(x => x.stackCount) < reward)
                    {
                        payOption.Disable("SW.NotEnoughSilver".Translate());
                    }
                    diaNode.options.Add(payOption);

                    DiaOption refuseOption = new DiaOption("SW.Refuse".Translate());
                    refuseOption.action = delegate
                    {
                        warrant.accepteer.TryAffectGoodwillWith(Faction.OfPlayer, -100);
                        var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
                        parms.faction = warrant.accepteer;
                        IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
                    };
                    refuseOption.resolveTree = true;
                    diaNode.options.Add(refuseOption);
                    Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, warrant.accepteer, delayInteractivity: true, radioMode: false, title));
                    Find.Archive.Add(new ArchivedDialog(diaNode.text, title, warrant.accepteer));
                }
                else
                {
                    Messages.Message("SW.FactionFailedWarrant".Translate(warrant.accepteer.Named("FACTION"), warrant.thing.LabelCap), MessageTypeDefOf.NegativeEvent);

                    int relationshipDamage = SimpleWarrantsMod.Settings.failedPlayerWarrantRelationshipDamage;
                    if (relationshipDamage > 0)
                        warrant.accepteer.TryAffectGoodwillWith(Faction.OfPlayer, -relationshipDamage);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref availableWarrants, "warrants", LookMode.Deep);
            Scribe_Collections.Look(ref acceptedWarrants, "acceptedWarrants", LookMode.Deep);
            Scribe_Collections.Look(ref createdWarrants, "givenWarrants", LookMode.Deep);
            Scribe_Collections.Look(ref takenWarrants, "takenWarrants", LookMode.Deep);
            Scribe_Values.Look(ref initialized, "initialized");
            Scribe_Values.Look(ref lastWarrantID, "lastWarrantID");
            Scribe_Collections.Look(ref raidLords, "raidLords", LookMode.Reference, LookMode.Deep, ref warrantKeys, ref lordValues);
            PreInit();
        }
    }
}