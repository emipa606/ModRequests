using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.Sound;

namespace BlockUnwantedMinutiae
{
    public class BUMSettings : ModSettings
    {
        public static IReadOnlyList<string> genericMessage_labels { get; } = new string[]
        {
            // Messages.xml
            // CORE
            "MessageSiegersAssaulting",
            "MessageRaidersBeginningAssault",
            "MessageRaidersDetectedEarlyAssault",
            "MessageRaidersKidnapping",
            "MessageRaidersStealing",
            "MessageCaravanDetectedRaidArrived",
            "MessageRaidersLeaving",
            "MessageRaidersGivenUpLeaving",
            "MessageRaidersSatisfiedLeaving",
            "MessageFightersFleeing",
            "MessageFriendlyFightersLeaving",
            "MessageVisitorsTakingWounded",
            "MessageVisitorsTrappedLeaving",
            "MessageVisitorsDangerousTemperature",
            "MessageWornApparelDeterioratedAway",
            "MessageDeterioratedAway",
            "MessageSeasonBegun",
            "MessageShipHasLeftCommsRange",
            "MessageNeedBeaconToTradeWithShip",
            "MessageNeedRoyalTitleToCallWithShip",
            "MessageConditionCauserDespawned",
            "MessageBillComplete",
            "MessageFullyHealed",
            "MessagePrisonerIsEscaping",
            "MessageOutOfNearbyShellsFor",
            "MessageOutOfNearbyFuelFor",
            "MessageAnimalsGoPsychoHunted",
            "MessageAnimalManhuntsOnTameFailed",
            "MessageInsectTameDesignated",
            "MessageInsectsHostileOnTaming",
            "LetterLabelMessageRecruitSuccess",
            "MessageRecruitSuccess",
            "MessageTameSuccess",
            "MessageTameAndNameSuccess",
            "MessageTameNoSuitablePens",
            "MessageRecruitJoinOfferAccepted",
            "MessageColonyCannotAfford",
            "MessageColonyNotEnoughSilver",
            "MessageCriticalAlert",
            "MessageMustDesignateHarvestable",
            "MessageMustDesignateHarvestableWood",
            "MessageMustDesignatePlants",
            "MessageMustDesignateHaulable",
            "MessageMustDesignateMineable",
            "MessageMustDesignateHuntable",
            "MessageMustDesignateTameable",
            "MessageMustDesignateClaimable",
            "MessageMustDesignatePaintableBuildings",
            "MessageMustDesignatePaintableFloors",
            "MessageMustDesignatePainted",
            "MessageMustDesignateDeconstructibleMechCluster",
            "MessageMustDesignateSmoothableSurface",
            "MessageNothingCanRemoveThickRoofs",
            "MessageAlreadyInStorage",
            "MessageMustDesignateStrippable",
            "MessageMustDesignateSlaughterable",
            "MessageMustDesignateOpenable",
            "MessageMustDesignateForbiddable",
            "MessageMustDesignateUnforbiddable",
            "MessageCannotClaimWhenThreatsAreNear",
            "MessageCannotAdoptWhileThreatsAreNear",
            "MessageRefusedArrest",
            "MessageNoMedicalBeds",
            "MessageNoAnimalBeds",
            "MessageWarningNoMedicineForRestriction",
            "PawnDiedBecauseOf",
            "PawnDied",
            "MessageNoLongerDowned",
            "MessageInvoluntarySleep",
            "MessageMedicalOperationWillAngerFaction",
            "MessageAnimalIsPregnant",
            "MessageMiscarriedStarvation",
            "MessageMiscarriedPoorHealth",
            "MessageGaveBirth",
            "MessagePsylinkNoSensitivity",
            "MessageLostImplantLevelFromHediff",
            "MessageReceivedBrainDamageFromHediff",
            "MessageWentOverPsychicEntropyLimit",
            "MessageNoHandlerSkilledEnough",
            "MessageEatenByPredator",
            "MessageAttackedByPredator",
            "MessageRoamerLeaving",
            "MessageHiveReproduced",
            "MessageTraderCaravanLeaving",
            "MessageCantSelectDeadPawn",
            "MessageCantSelectOffMapPawn",
            "MessageSocialFight",
            "MessageNewMarriageCeremony",
            "MessageMarriageCeremonyStarts",
            "MessageMarriageCeremonyCalledOff",
            "MessageNewlyMarried",
            "MessageMarriageCeremonyAfterPartyFinished",
            "MessageNewBondRelation",
            "MessageNewBondRelationNewName",
            "MessageBondedAnimalMentalBreak",
            "MessageNamedBondedAnimalMentalBreak",
            "MessageBondedAnimalsMentalBreak",
            "MessageSuccessfullyRemovedHediff",
            "MessageShipChunkDrop",
            "MessageCannotSellItemsReason",
            "MessageConstructionFailed",
            "MessageScreenshotSavedAs",
            "MessageNoLongerSocialFighting",
            "MessageNoLongerBingingOnDrug",
            "MessageNoLongerOnTargetedInsultingSpree",
            "MessageRottedAwayInStorage",
            "MessageFoodPoisoning",
            "MessagePawnLostWhileFormingCaravan",
            "MessagePawnLostWhileFormingCaravan_AllLost",
            "MessageCaravanFormationPaused",
            "MessageCaravanFormationUnpaused",
            "MessageCaravanMemberHasExtremeMentalBreak",
            "MessageMaxPlanetCoveragePerformanceWarning",
            "MessagePlantDiedOfCold",
            "MessagePlantDiedOfRot_LeftUnharvested",
            "MessagePlantDiedOfRot_ExposedToLight",
            "MessagePlantDiedOfRot",
            "MessagePlantDiedOfPoison",
            "MessagePlantDiedOfBlight",
            "MessagePawnBeingBurned",
            "MessageAccidentalOverdose",
            "MessageCaravanArrivedAtDestination",
            "MessagePawnLeftMapAndCreatedCaravan",
            "MessagePawnLeftMapAndCreatedCaravan_AnimalsWantToJoin",
            "MessageSettledInExistingMap",
            "MessagePermanentWoundHealed",
            "MessageRescueeDidntJoin",
            "MessageTransporterUnreachable",
            "MessageTransportersNotAdjacent",
            "MessageTransportersLoadCanceled_TransporterDestroyed",
            "MessageTransporterSingleLoadCanceled_TransporterDestroyed",
            "MessageTransportersLoadCanceled_FuelingPortGiverDeSpawned",
            "MessageTransportersLoadingProcessStarted",
            "MessageTransporterSingleLoadingProcessStarted",
            "MessageTransportPodsArrived",
            "MessageTransportPodsArrivedAndLost",
            "MessageTransportPodsArrivedAndAddedToCaravan",
            "MessageFinishedLoadingTransporters",
            "MessageFinishedLoadingTransporterSingle",
            "MessageCantLoadMoreIntoTransporters",
            "MessageTransportPodsDestinationIsInvalid",
            "MessageTransportPodsDestinationIsTooFar",
            "MessageReformedCaravan",
            "MessageCantBanishLastColonist",
            "MessageCantEquipCustom",
            "MessageCantEquipIncapableOfViolence",
            "MessageCantEquipIncapableOfShooting",
            "MessageCantEquipIncapableOfManipulation",
            "MessageCantWearApparelMissingBodyParts",
            "MessageCantUnequipLockedApparel",
            "MessageWouldReplaceLockedApparel",
            "MessageSlaughteringBondedAnimal",
            "MessageReleaseBondedAnimal",
            "MessageCaravanRanOutOfFood",
            "MessageCaravanDeathCorpseAddedToInventory",
            "MessageSelectOwnBaseToFormCaravan",
            "MessageScreenResTooSmallForUIScale",
            "MessageYouHaveToReformCaravanNow",
            "MessageDefendersAttacking",
            "MessageMechanoidsAssembled",
            "MessageMechanoidsLeftToAssemble",
            "MessageMechanoidsReinforcementsDrop",
            "MessageSelfTendUnsatisfied",
            "MessageCannotSelfTendEver",
            "MessageCantAddWaypointBecauseImpassable",
            "MessageCantAddWaypointBecauseUnreachable",
            "MessageCantAddWaypointBecauseLimit",
            "MessageCantRemoveWaypointBecauseFirst",
            "MessageCantDoExecutionBecauseNoWardenCapableOfViolence",
            "MessageCanReformCaravanNowNoMoreEnemies",
            "MessageCanReformCaravanNowNoMoreEnemiesButUnexploredAreas",
            "MessageCantBanishDownedPawn",
            "MessageSleepingPawnsWokenUp",
            "MessageMechClusterDefeated",
            "MessageCompSpawnerSpawnedItem",
            "MessageHediffCuredByItem",
            "MessageBodyPartCuredByItem",
            "MessageResearchProjectFinishedByItem",
            "MessagePawnResurrected",
            "MessageFailedToRescueRelative",
            "MessageRescuedRelative",
            "MessageTornadoLeftMap",
            "MessageTornadoDissipated",
            "MessagePeaceTalksNoDiplomat",
            "MessageWarningCavePlantsExposedToLight",
            "MessageTargetedTantrumChangedTarget",
            "MessageTargetedInsultingSpreeChangedTarget",
            "MessageMurderousRageChangedTarget",
            "MessagePlantIncompatibleWithRoof",
            "MessageRoofIncompatibleWithPlant",
            "MessagePlayerTriedToLeaveMapViaExitGrid_CanReform",
            "MessagePlayerTriedToLeaveMapViaExitGrid_CantReform",
            "MessageGoodwillChanged",
            "MessageGoodwillChangedWithReason",
            "MessageGiftGivenButNotAppreciated",
            "MessageCantGiveGiftBecauseCantCarryEncumbered",
            "MessageCantGiveGiftBecauseCantCarry",
            "MessageFormedCaravan",
            "MessageFormedCaravan_Orders",
            "MessageBillValidationStoreZoneDeleted",
            "MessageBillValidationStoreZoneUnavailable",
            "MessageBillValidationPawnUnavailable",
            "MessageBillValidationIncludeZoneDeleted",
            "MessageBillValidationIncludeZoneUnavailable",
            "MessageBillValidationStoreZoneInsufficient",
            "MessageAnimalReturnedWild",
            "MessageAnimalReturnedWildReleased",
            "MessageAnimalLostSkill",
            "MessageTranslationReportSaved",
            "MessageCaravanArrivalActionNoLongerValid",
            "MessageEnterCooldownBlocksEntering",
            "MessagePermitCooldownFinished",
            "MessagePredatorHuntingPlayerAnimal",
            "MessagePrisonerResistanceBroken",
            "MessagePrisonerResistanceBroken_RecruitAttempsWillBegin",
            "MessageModWithPackageIdAlreadyEnabled",
            "MessageDisableModsBeforeCleaningTranslationFiles",
            "MessageUnpackBeforeCleaningTranslationFiles",
            "MessageTranslationFilesCleanupDone",
            "MessageCantCleanupTranslationFilesBeucaseOfXmlError",
            "MessageCapturingWillAngerFaction",
            "MessageStrippingWillAngerFaction",
            "MessageCantShootInMelee",
            "MessageNoColonistCanAcceptQuest",
            "MessageCannotAcceptQuest",
            "MessageQuestAccepted",
            "MessagePawnLeaving",
            "MessagePawnsLeaving",
            "MessageMonumentDestroyedBecauseOfDisallowedBuilding",
            "MessageNoResearchBenchForTechprint",
            "NoValidDestinationFound",
            "MessageMinifiedTreeDied",
            "MessageResearchMenuWithoutBench",
            "MessageActivatorProximityTriggered",
            "MessageCannotSelectInvisibleStat",
            "MessagePrisonerCannotEquipWeapon",
            "MessageIdeoOpposedWorkTypeSelected",
            "MessageBedLostAssignment",
            "MessageBedDestroyed",
            "Spectate",
            "AssignToRole",
            "RitualBegun",
            "MaxPawnsPerRole",
            "RoleIsLocked",
            "MessageRitualNeedsAtLeastOnePerson",
            "MessageRitualNeedsAtLeastOneSpectator",
            "MessageRitualNeedsAtLeastOneRolePawn",
            "MessageRitualNeedsAtLeastNumRolePawn",
            "MessageRitualPawnDowned",
            "MessageRitualPawnPrisonerNotSecured",
            "MessageRitualPawnSlaveNotSecured",
            "MessageRitualPawnReleased",
            "MessageRitualPawnInjured",
            "MessageRitualRoleRequired",
            "MessageRitualRoleMustBePrisoner",
            "MessageRitualRoleMustBeAnimal",
            "MessageRitualRoleMustBeHumanlike",
            "MessageRitualRoleCannotBeABaby",
            "MessageRitualNoRolesAvailable",
            "MessageRitualNotOfPlayerIdeo",
            "MessageRitualRoleMustHaveEyes",
            "MessageRitualRoleMustRequireScarification",
            "MessageRitualRoleMustRequireBlinding",
            "MessageRitualRoleMustBePrisonerOrSlave",
            "MessageRitualRoleMustBeCapableOfFighting",
            "MessageRitualRoleCannotReplaceRequiredPawn",
            "MessageRitualRolePawnRecentlyTornConnection",
            "MessageRitualRolePawnHasSameIdeo",
            "MessageRitualRoleMustBeCapableOfWardening",
            "MessageRitualRoleMustBeCapableOfGeneric",
            "MessageRitualRoleMustNotBePrisonerToSpectate",
            "MessageRitualRoleMustHaveIdeoToSpectate",
            "MessageRitualRoleMustHaveIdeoToDoRole",
            "MessageRitualRoleMustBeColonist",
            "MessageRitualWontAttendExtremeTemperature",
            "MessageRitualRoleMustBeFreeColonist",
            "MessageRitualRoleMustHaveLargerBodySize",
            "MessageRitualPawnMentalState",
            "MessageRitualPawnIsAlreadyBelievingIdeo",
            "MessageRitualCannotAssignAnyRoleFromSpectating",
            "MessageRitualRoleMustNotBeImprisonedWildMan",
            "MessageRitualRoleMustNotBeImprisoned",
            "MessageRitualRoleCannotBeChild",
            "MessageRitualRoleCannotBeBaby",
            "MessagePlayerMustSelectTile",
            "CannotPlantThing",
            "MessageWarningNotEnoughFertility",
            "BlockedBy",
            "AdjacentSowBlocker",
            "TooCloseToOtherPlant",
            "TooCloseToOtherSeedPlantCell",
            "MessageWarningCutImportantPlant",
            "MessageIncapableOfManipulation",
            "MessageActivationCanceled",
            "NothingAvailableInCategory",
            "SettingsLinkedFor",
            "SettingsUnlinkedFor",
            "StorageSettingsCopiedToClipboard",
            "StorageSettingsPastedFromClipboard",
            // ROYALTY
            "ShuttleBlocked",
            "MessageShuttleArrived",
            "MessageShuttleArrivedContentsLost",
            "CannotCallShuttle",
            "ShuttleCannotLand_Fogged",
            "ShuttleCannotLand_Unwalkable",
            "MessageBestowerWaiting",
            "MessageBestowerUnreachable",
            "MessageBestowingTargetIsBusy",
            "MessageBestowingSpotUnreachable",
            "MessageBestowingDanger",
            "MessageBestowingDangerTemperature",
            "MessageBestowingInterrupted",
            "MessageBestowingCeremonyStarted",
            "MessageMissionGetBackToShuttle",
            "MessagePermitTransportDrop",
            "MessagePermitTransportDropCaravan",
            "MessageNoFactionForVerbMechCluster",
            // IDEOLOGY
            "MessageMustChooseIdeo",
            "MessageIdeoNameCantBeEmpty",
            "MessageIdeoIncompatiblePrecepts",
            "MessageIdeoWarnRoleApparelOverlapsDesiredApparel",
            "MessageRitualMissingTarget",
            "MessageBuildingMissingRitual",
            "MessagePawnUnwillingToDoDueToIdeo",
            "MessageFailedConvertIdeoAttempt",
            "MessageFailedConvertIdeoAttemptSocialFight",
            "MessageNotEnoughMemes",
            "MessageTooManyMemes",
            "MessageNotEnoughStructureMemes",
            "MessageIncompatibleMemes",
            "MessageNoRequiredRolePawnToBeginRitual",
            "MessageNotEnoughSpectators",
            "MessageRoleChangeChooseDifferentRole",
            "MessageNeedAssignedRoleToBeginRitual",
            "MessageNeedAtLeastOneParticipantOfIdeo",
            "MessageRoleAssigned",
            "MessageRoleUnassignedPrisoner",
            "MessageWarningPlayerDesignatedTreeChopped",
            "MessageWarningPlayerDesignatedMining",
            "MessageConnectedPawnDied",
            "MessageSlaveEmancipated",
            "MessagePrisonerEnslaved",
            "MessagePrisonerWillBroken",
            "MessagePrisonerWillBroken_RecruitAttempsWillBegin",
            "MessageNoWardenCapableOfEnslavement",
            "MessageNoWardenOfIdeo",
            "MessageCampDetected",
            "MessageCharityEventRefused",
            "MessageCharityEventFulfilled",
            "MessageBeggarsLeavingWithNoItems",
            "MessageBeggarsLeavingWithItems",
            "MessageWandererLeftToDie",
            "MessageWandererRecruited",
            "MessageWandererLeftHealthy",
            "MessageCharityQuestEndedFailed",
            "MessageCharityQuestEndedSuccess",
            "CannotRemove",
            "CannotRemoveMemeRequired",
            "CannotRemoveMemeRequiredPlayer",
            "RequiredByFaction",
            "OverlappingRoleApparel",
            "SelfDestructCountdown",
            "PreceptNameTooLong",
            "MessageAncientTerminalDiscovered",
            "MessageAncientComplexBackToShuttle",
            "MessageAncientSignalActivated",
            "MessageAncientSignalHostileDetected",
            "MessageAncientAltarThreatsWokenUp",
            "MessageAncientAltarThreatsAlerted",
            "MessageAnimalIsVeneratedForAllColonists",
            "MessageNewColonyMax",
            "MessageNewColoyRequiresOneColonist",
            "MessageFuelNodeTriggered",
            "MessageSleepingThreatDelayActivated",
            "MessageInfestationDelayActivated",
            "MessageFuelNodeDelayActivated",
            "MessageDevelopmentPointsEarned",
            "MessageFluidIdeoOneChangeAllowed",
            // BIOTECH
            "MessageColonistReaching2ndTrimesterPregnancy",
            "MessageColonistReaching3rdTrimesterPregnancy",
            "MessageColonistInFinalStagesOfLabor",
            "MessageMechChargerDestroyedMechGoesBerserk",
            "MessageCantUseOnResistingPerson",
            "MessageCannotUseOnSameXenotype",
            "MessageCanOnlyTargetColonistsPrisonersAndSlaves",
            "MessageCannotImplantInTempFactionMembers",
            "MessageXenogermCompleted",
            "MessageXenogermCancelledMissingPack",
            "MessageAbsorbingXenogermWillAngerFaction",
            "MessageCannotUseOnOtherFactions",
            "MessageCaravanAddingEscortingMech",
            "MessageCaravanRemovingEscortingMech",
            "MessageMechanitorCasketOpened",
            "MessageNoBloodfeedersPrisonerInteractionReset",
            "MessagePregnancyTerminated",
            "MessageWarningPollutedCell",
            "MessageWarningNotPollutedCell",
            "MessagePlantDiedOfPollution",
            "MessagePlantDiedOfNoPollution",
            "MessageWorldTilePollutionChanged",
            "MessageCocoonDisturbed",
            "MessageCocoonDisturbedPlural",
            "MessageTargetMustBeDownedToForceReimplant",
            "MessageWorkTypeDisabledAge",
            "MessageDeathrestingPawnCanWakeSafely",
            "MessageMechanitorLostControlOfMech",
            "MessageMechanitorDisconnectedFromMech",
            "MetabolismTooLowToCreateXenogerm",
            "ComplexityTooHighToCreateXenogerm",
            "MessageNoSelectedGenepacks",
            "MessageNoSelectedGenes",
            "CanOnlyStoreNumGenepacks",
            "XenotypeNameCannotBeEmpty",
            "MessageGeneMissingPrerequisite",
            "MessageDeathrestCapacityChanged",
            "SanguophagesArrivingSoon",
            "SanguophagesBegunMeeting",
            "SanguophagesLeavingTemperature",
            "MessagePawnWokenFromSunlight",
            "MessageBedExposedToSunlight",
            "MessagePawnKilledRipscanner",
            "MessageCannotResurrectDessicatedCorpse",
            "MessageAboutToExplode",
            "MessageChildcareDisabled",
            "MessageChildcareNotAssigned",
            "MessageTooManyCustomXenotypes",
            "MessageConflictingGenesPresent",
            "MessageDeathrestBuildingBound",
            "MessageDevelopmentalStageSelectionDisabledByScenario",
            "MessagePawnHadNotEnoughBloodToProduceHemogenPack",
            "MessageCannotStartHemogenExtraction",
            "MessageCannotPostponeGrowthMoment",
            "MessageDraftedPawnCarryingBaby",
            // CORE
            // DamageDefs
            "MessageDeathByBurning",
            "MessageDeathByFrostbite",
            "MessageDeathByTornado",
            "MessageDeathBySurgery",
            "MessageDeathByExecution",
            "MessageDeathByCutting",
            "MessageDeathByCrushing",
            "MessageDeathByBeating",
            "MessageDeathByStabbing",
            "MessageDeathByTearing",
            "MessageDeathByBiting",
            "MessageDeathByExplosion",
            "MessageDeathByShot",
            "MessageDeathByArrow",
            "MessageDeathByStun",
            "MessageDeathByEMP",
            // GameConditions_Misc.xml
            "MessageSolarFlare",
            "MessageEclipse",
            "MessagePsychicDrone",
            "MessagePsychicSoothe",
            "MessageToxicFallout",
            "MessageVolcanicWinter",
            "MessageHeatWave",
            "MessageColdSnap",
            "MessageFlashstorm",
            "MessageAurora",
            // Gatherings.xml
            "MessagePartyCalledOff",
            "MessagePartyFinished",
            // HediffDefs
            "MessagePregnant",
            "MessageOverdose",
            "MessageExcisedCarcinoma",
            "MessageCuredScaria",
            "MessageSterilized",
            // Inspirations.xml
            "MessageEndedInspireWorkFrenzy",
            "MessageEndedInspireGoFrenzy",
            "MessageEndedInspireShootFrenzy",
            "MessageEndedInspireTrade",
            "MessageEndedInspireRecruitment",
            "MessageEndedInspireTaming",
            "MessageEndedInspireSurgery",
            "MessageEndedInspireCreativity",
            // MentalStateDefs
            "MessageRecoveryBerserk",
            "MessageRecoveryArson",
            "MessageRecoveryEscape",
            "MessageRecoverySlaughter",
            "MessageRecoveryMurderous",
            "MessageRecoveryReturn",
            "MessageRecoveryPsyWander",
            "MessageRecoveryTantrum",
            "MessageRecoverySadistic",
            "MessageRecoveryCorpse",
            "MessageRecoveryGlutton",
            "MessageRecoverySadWander",
            "MessageRecoveryHiding",
            "MessageRecoveryInsulting",
            "MessageRecoveryConfused",
            "MessageRecoveryFleeing",
            "MessageRecoveryManhunting",
            // ThingDefs_Buildings
            "MessagePlantResting",
            "MessageNeedsNewBarrel",
            "MessageNoSlugs",
            "MessageNeedsNewReinforcedBarrel",
            "MessageNoFoam",
            "MessageNoRockets",
            // Misc_Gameplay.xml
            "BladelinkAlreadyBondedMessage",
            // Dialogs_Various.xml
            "MessageMustChooseRouteFirst",
            "MessageNoValidExitTile",
            // GameplayCommands.xml
            "MessageTargetBeyondMaximumRange",
            "MessageTargetBelowMinimumRange",
            "MessageTurretWontFireBecauseHoldFire",
            // Incidents.xml
            "MessageAnimalInsanityPulse",
            // Misc_Gameplay.xml
            "MessageColonistMarkedForExecution",	
            // FloatMenu.xml	
            "NoPrisonerBed",	
            "NoNonPrisonerBed",	
            "NoAnimalBed",
            // ROYALTY
            // Gatherings.xml
            "MessageConcertCalledOff",
            "MessageConcertFinished",
            // Hediffs_Local_Misc.xml
            "MessageCuredBloodRot",
            "MessageCuredAbasia",
            // Ritual_Behaviors.xml
            "MessageAdultsAnima",
            // Buildings_ConditionCausers.xml
            "MessageWeatherFinished",
            "MessageSupressorFinished",
            "MessageEMIFinished",
            // Plants_Wild.xml
            "MessageAnimaDied",
            // Misc_Gameplay.xml
            "MessageShuttleDestinationIsTooFar",
            // Script_BuildMonument_Worker.xml
            "MessageMonumentViolated",
            // IDEOLOGY
            // Abilities.xml
            "MessageConvertSuccess",
            "MessageConvertFailure",
            "MessageReassureSuccess",
            "MessageCounselSuccess",
            "MessageCounselSuccessNoNeg",
            "MessageCounselFailure",
            "MessageAnimalCalmSuccess",
            // Ritual_Behaviors.xml
            "MessageAdultsGauranlen",
            "RitualStageActionMessage",
            // Misc_Gameplay.xml
            "BiosculpterEnteringMessage",
            "BiosculpterLoadingStartedMessage",
            "BiosculpterCarryStartedMessage",
            "BiosculpterNoPowerEjectedMessage",
            "BiosculpterHealCompletedMessage",
            "BiosculpterHealCompletedWithCureMessage",
            "BiosculpterAgeReversalCompletedMessage",
            "BiosculpterPleasureCompletedMessage",
            // Script_ReliquaryPilgrims.xml
            "PilgrimsLeavingMessage",
            "PilgrimsLeftMessage",
            "TerminalHackedMessage",
            "TerminalHackedMultiMessage",
            "AllTerminalsHackedMessage",
            // BIOTECH
            // DamageDefs
            "MessageDeathByBeam",
            "MessageDeathByHeat",
            "MessageDeathByShock",
            // GameConditions_Misc.xml
            "MessageAcidicSmogStart",
            "MessageAcidicSmogEnd",
            // Hediffs_Global_Misc.xml
            "MessageLaborProgressing",
            "MessageInfantIllness",
            "MessageNotBreastfeeding",
            "MessageNotLactating",
            // Hediffs_Various.xml
            "MessageGeneRegrowing",
            // MentalStates_Special.xml
            "MessageRecoveryFire",
            "MessageRecoveryRage",
            // Recipes_Surgery_Misc.xml
            "MessagePerformedLitigation",
            "MessagePerformedVasectomy",
            "MessagePerformedReversal",
            "MessageImplantedIUD",
            "MessageRemovedIUD",
            // ITabs.xml
            "TryRomanceNoOptsMessage",
            "TryRomanceFailedMessage",
            // Misc_Gameplay.xml
            "ImplantFailedMessage"
        };

        public static IReadOnlyList<string> genericAlert_labels { get; } = new string[]
        {
            // CUSTOM
            "BreakRiskMinor",
            "BreakRiskMajor",
            "BreakRiskExtreme",
            "AlertTatteredApparel",
            "AlertUnhappyNudity",
            // CORE
            "ActivatorCountdown",
            "AnimalFilth",
            "AnimalPenNeeded",
            "AnimalPenNotEnclosed",
            "AnimalRoaming",
            "AwaitingMedicalOperation",
            "BilliardsTableOnWall",
            "Boredom",
            "BrawlerHasRangedWeapon",
            "CannotBeUsedRoofed",
            "CaravanIdle",
            "CasketOpening",
            "ColonistLeftUnburied",
            "ColonistNeedsRescuing",
            "ColonistNeedsTend",
            "ColonistsIdle",
            "DormancyWakeUpDelay",
            "Exhaustion",
            "FireInHomeArea",
            "FuelNodeIgnition",
            "Heatstroke",
            "HitchedAnimalHungryNoFood",
            "HunterHasShieldAndRangedWeapon",
            "HunterLacksRangedWeapon",
            "Hypothermia",
            "HypothermicAnimals",
            "ImmobileCaravan",
            "InfestationDelay",
            "JoyBuildingNoChairs",
            "LifeThreateningHediff",
            "LowFood",
            "LowMedicine",
            "MajorOrExtremeBreakRisk",
            "MinifiedTreeAboutToDie",
            "MinorBreakRisk",
            "NeedBatteries",
            "NeedColonistBeds",
            "NeedDefenses",
            "NeedDoctor",
            "NeedJoySources",
            "NeedMealSource",
            "NeedMiner",
            "NeedResearchBench",
            "NeedResearchProject",
            "NeedWarden",
            "NeedWarmClothes",
            "PasteDispenserNeedsHopper",
            "PennedAnimalHungry",
            "PredatorInPen",
            "QuestExpiresSoon",
            "ShieldUserHasRangedWeapon",
            "StarvationAnimals",
            "StarvationColonists",
            "Thought",
            "Alert",
            "Delay",
            "MoodBelow",
            "ShuttleDelay",
            "ShuttleLeaveDelay",
            // ROYALTY
            "AnimaLinkingReady",
            "BestowerWaiting",
            "DisallowedBuildingInsideMonument",
            "MonumentMarkerMissingBlueprints",
            "NeedMeditationSpot",
            "RoyalNoAcceptableFood",
            "RoyalNoThroneAssigned",
            "ShuttleLandingBeaconUnusable",
            "ThroneroomInvalidConfiguration",
            "TimedMakeFactionHostile",
            "TimedRaidsArriving",
            "TitleRequiresBedroom",
            "UndignifiedBedroom",
            "UndignifiedThroneroom",
            "UnusableMeditationFocus",
            "PermitAvailable",
            // IDEOLOGY
            "AgeReversalDemandNear",
            "ConnectedPawnNotAssignedToPlantCutting",
            "DateRitualComing",
            "GauranlenTreeWithoutProductionMode",
            "IdeoBuildingDisrespected",
            "IdeoBuildingMissing",
            "NeedSlaveBeds",
            "RolesEmpty",
            "SlaveRebellionLikely",
            "SlavesUnattended",
            "SlavesUnsuppressed",
            // BIOTECH
            "Biostarvation",
            "GenebankUnpowered",
            "LowBabyFood",
            "LowDeathrest",
            "LowHemogen",
            "MechChargerFull",
            "MechDamaged",
            "MechMissingBodyPart",
            "NeedBabyCribs",
            "NeedMechChargers",
            "NeedSlaveCribs",
            "NoBabyFeeders",
            "NoBabyFoodCaravan",
            "PollutedTerrain",
            "PsychicBondedSeparated",
            "ReimplantationAvailable",
            "SubjectHasNowOverseer",
            "ToxicBuildup",
            "ToxifierGeneratorStopped",
            "WarqueenHasLowResources"
        };
        
        public static IReadOnlyList<string> genericLetter_labels { get; } = new string[]
        {
            // Letters.xml
            // CORE
            "LetterLabelFirstSummerWarning",
            "LetterLabelAreaRevealed",
            "LetterLabelRoofCollapsed",
            "LetterLabelBirthday",
            "LetterLabelNewDisease",
            "LetterLabelAncientShrineWarning",
            "LetterLabelAnimalManhunterRevenge",
            "LetterFriendlyTrapSprungLabel",
            "LetterLeadersDeathLabel",
            "LetterLeaderChangedLabel",
            "LetterNewLeader",
            "LetterLabelNoticedRelatedPawns",
            "LetterLabelAffair",
            "LetterLabelNewLovers",
            "LetterLabelBreakup",
            "LetterLabelAcceptedProposal",
            "LetterLabelRejectedProposal",
            "LetterLabelRelationsChange_Hostile",
            "LetterLabelRelationsChange_Ally",
            "LetterLabelRelationsChange_NeutralFromHostile",
            "LetterLabelRelationsChange_NeutralFromAlly",
            "LetterLabelShortCircuit",
            "LetterLabelSuffixBondedAnimalDied",
            "LetterLabelPrisonBreak",
            "LetterLabelNewlyAddicted",
            "LetterLabelDrugBinge",
            "LetterLabelAllCaravanColonistsDied",
            "LetterLabelCaravanEnteredMap",
            "LetterLabelCaravanEnteredEnemyBase",
            "LetterLabelTransportPodsLandedInEnemyBase",
            "LetterLabelFactionBaseDefeated",
            "LetterLabelFoundPreciousLump",
            "LetterLabelDeepScannerFoundLump",
            "LetterLabelAmbushInExistingMap",
            "LetterLabelPeaceTalks_Disaster",
            "LetterLabelPeaceTalks_Backfire",
            "LetterLabelPeaceTalks_TalksFlounder",
            "LetterLabelPeaceTalks_Success",
            "LetterLabelPeaceTalks_Triumph",
            "LetterLabelAICoreOffer",
            "LetterCraftedLegendaryLabel",
            "LetterCraftedMasterworkLabel",
            "LetterLabelPawnsLostBecauseMapClosed_Caravan",
            "LetterLabelPawnsLostBecauseMapClosed_Home",
            "LetterLabelHibernateComplete",
            "LetterLabelVisitorsGaveGift",
            "LetterLabelFactionBaseProximity",
            "LetterLabelCaravansBattlefieldVictory",
            "LetterLabelRescueQuestFinished",
            "LetterLabelPredatorHuntingColonist",
            "LetterDisease_Blocked",
            "LetterLabelTraitDisease",
            "LetterLabelQuestDropPodsArrived",
            "LetterLabelQuestItemsAddedToCaravanInventory",
            "LetterLabelRefugeeJoins",
            "LetterLabelRescueeJoins",
            "LetterLabelPawnsArriveAndJoin",
            "LetterLabelPawnsArrive",
            "LetterLabelPawnLeaving",
            "LetterLabelPawnsLeaving",
            "LetterLabelPawnsKidnapped",
            "LetterTechprintAppliedLabel",
            "LetterQuestCompletedLabel",
            "LetterQuestFailedLabel",
            "LetterQuestConcludedLabel",
            "LetterBladelinkWeaponBondedLabel",
            "LetterLabelMechClusterArrived",
            "LetterLabelSiteCountdownStarted",
            "LetterLabelUnrecruitablePawnCaptured",
            "LetterJoinOfferLabel",
            // ROYALTY
            "LetterLabelLostRoyalTitle",
            "LetterLabelGainedRoyalTitle",
            "LetterLabelRewardsForNewTitle",
            "LetterTitleHeirLostLabel",
            "LetterTitleInheritance",
            "LetterLawViolationDetectedLabel",
            "LetterLabelSpeechCancelled",
            "WildDecree",
            "LetterLabelRandomDecree",
            "LetterLabelPsylinkLevelGained",
            "LetterLabelBestowingCeremonyExpired",
            "LetterLabelBestowingCeremonyTitleUpdated",
            "LetterLabelShuttleCrashed",
            "LetterLabelWandererJoinsAbasia",
            // IDEOLOGY
            "LetterLabelConvertIdeoAttempt_Success",
            "LetterLabelGrandSlaveRebellion",
            "LetterLabelLocalSlaveRebellion",
            "LetterLabelSingleSlaveRebellion",
            "LetterLabelGrandSlaveEscape",
            "LetterLabelLocalSlaveEscape",
            "LetterLabelSingleSlaveEscape",
            "LetterLabelRoleActive",
            "LetterLabelRoleInactive",
            "LetterLabelRoleLost",
            "LetterObligationRoleInactive",
            "LetterTitleObligationsActivated",
            "LetterTitleObligationsDeactivated",
            "LetterBondRemoved",
            "LetterLabelEnslavementSuccess",
            "LetterLabelNewPrimaryIdeo",
            "PawnsHaveCharitableBeliefs",
            "LetterIdeoChangedDivorce",
            "LetterLabelSurpriseReinforcements",
            "LetterWantLookChange",
            "LetterLabelSpacedroneIncoming",
            "LetterLabelRelicDestroyed",
            "LetterLabelRelicLost",
            "LetterLabelLastHackerLost",
            "LetterLabelRelicFound",
            "LetterLabelRelicsCollected",
            "LetterLabelPawnConnected",
            "LetterLabelConnectedTreeDestroyed",
            "LetterLabelArchonexusWealthReached",
            "LetterLabelArchonexusStructureResearched",
            "LetterLabelArchotechStructuresAbandoned",
            "LetterLabelReformIdeo",
            "LetterTitleObligationsActivated",
            "LetterTitleObligationsDeactivated",
            // BIOTECH
            "LetterLabelMechsFeral",
            "LetterLabelMechlinkInstalled",
            "LetterLabelBossgroupSummoned",
            "LetterLabelBossgroupArrived",
            "LetterLabelCommsConsoleSpawned",
            "LetterLabelBossgroupCallerUnlocked",
            "LetterLabelMechanitorCasketOpened",
            "LetterLabelMechanitorCasketFound",
            "LetterLabelBecameChild",
            "LetterLabelBecameAdult",
            "LetterLabelAdopted",
            "LetterLabelThirdTrimester",
            "LetterVatBirth",
            "LetterColonistPregnancyLaborLabel",
            "BabyAlreadyNamed",
            "BirthdayGrowthMoment",
            "LetterLabelMechlinkAvailable",
            "LetterLabelXenogermOrderedImplanted",
            "LetterLabelInvoluntaryDeathrest",
            "LetterLabelSanguophageWaitingToReimplant",
            "LetterLabelGenesImplanted",
            // CORE
            // Gatherings.xml 
            "LetterLabelParty",
            "LetterLabelMarriage",
            // Hediffs_Global_Misc.xml
            "LetterLabelPregnant",
            "LetterLabelOverdose",
            "LetterLabelCatatonicBreakdown",
            // HediffGiverSets.xml
            "LetterLabelSavant",
            // Inspirations.xml
            "LetterLabelInspiredWorkFrenzy",
            "LetterLabelInspiredGoFrenzy",
            "LetterLabelInspiredShootFrenzy",
            "LetterLabelInspiredTrade",
            "LetterLabelInspiredRecruitment",
            "LetterLabelInspiredTaming",
            "LetterLabelInspiredSurgery",
            "LetterLabelInspiredCreativity",
            // MentalStateDefs
            "LetterLabelBerserk",
            "LetterLabelArson",
            "LetterLabelJailbreaker",
            "LetterLabelSlaughter",
            "LetterLabelMurderous",
            "LetterLabelGaveUp",
            "LetterLabelPsyWander",
            "LetterLabelTantrum",
            "LetterLabelSadistic",
            "LetterLabelCorpse",
            "LetterLabelGlutton",
            "LetterLabelSadWander",
            "LetterLabelHiding",
            "LetterLabelInsulting",
            "LetterLabelConfused",
            "LetterLabelFleeing",
            "LetterLabelManhunting",
            // QuestScriptDefs
            "LetterLabelQuestExpired",
            "LetterLabelQuestFailed",
            "LetterLabelPaymentArrived",
            "LetterLabelChasing",
            // SurgeryOutcomeEffectDefs.xml
            "LetterLabelSurgeryFailed",
            "LetterLabelSurgeryFailedSterilized",
            // Storyteller
            "LetterLabelCaravanAmbush",
            "LetterLabelCaravanManhunter",
            "LetterLabelDiseaseFlu",
            "LetterLabelDiseasePlague",
            "LetterLabelDiseaseMalaria",
            "LetterLabelDiseaseSleeping",
            "LetterLabelDiseaseFibrous",
            "LetterLabelDiseaseSensory",
            "LetterLabelDiseaseGutWorms",
            "LetterLabelDiseaseMuscle",
            "LetterLabelDiseaseAnimalFlu",
            "LetterLabelDiseaseAnimalPlague",
            "LetterLabelAmbrosiaSprout",
            "LetterLabelRansom",
            "LetterLabelMeteorite",
            "LetterLabelAnimalMigration",
            "LetterLabelWanderer",
            "LetterLabelToxicFallout",
            "LetterLabelVolcanicWinter",
            "LetterLabelHeatWave",
            "LetterLabelColdSnap",
            "LetterLabelFlashstorm",
            "LetterLabelManInBlack",
            "LetterLabelInfestation",
            "LetterLabelDrillInfestation",
            "LetterLabelDefoliatorShip",
            "LetterLabelPsychicShip",
            "LetterLabelMechCluster",
            "LetterLabelEclipse",
            "LetterLabelPsychicDrone",
            "LetterLabelPsychicSoothe",
            "LetterLabelSolarFlare",
            "LetterLabelAurora",
            "LetterLabelQuestAvailable",
            "LetterLabelJourneyOffer",
            "LetterLabelRaidEnemy",
            "LetterLabelRaidFriendly",
            "LetterLabelRaidSiege",
            // Incidents.xml
            "LetterLabelAnimalInsanity",
            "LetterLabelCropBlight",
            "LetterLabelMiracleHeal",
            "LetterLabelRefugeePodCrash",
            "LetterLabelWandererJoins",
            "LetterLabelCargoPodCrash",
            "LetterLabelSingleVisitorArrives",
            "LetterLabelGroupVisitorsArrive",
            "LetterLabelManhunterPackArrived",
            "LetterLabelPsychicDroneLevelIncreased",
            "LetterLabelAgentRevealed",
            "LetterLabelBeaversArrived",
            "LetterLabelAnimalSelfTame",
            "LetterLabelFarmAnimalsWanderIn",
            "LetterLabelThrumboPasses",
            "LetterLabelTraderCaravanArrival",
            "EscapeShipFoundLabel",
            "LetterLabelCaravanRequest",
            "LetterLabelSiteNoLongerHostile",
            "LetterLabelSiteNoLongerHostileMulti",
            "LetterLabelQuestFailedReason",
            // Messages.xml
            "LetterLabelMessageRecruitSuccess",
            // Thoughts_Memory_Misc.xml
            "LetterLabelCatharsis",
            // ROYALTY
            // Gatherings.xml
            "LetterLabelConcert",
            // Incidents_Map_Special.xml
            "LetterLabelAnimaTree",
            // QuestScriptDefs
            "LetterLabelMonumentCompleted",
            "LetterLabelMonumentDestroyed",
            "LetterLabelMonumentMarkerArrived",
            "LetterLabelTimeExpired",
            "LetterLabelDecreeForgotten",
            "LetterLabelDecreeCancelled",
            "LetterLabelArrived",
            "LetterLabelGuestDied",
            "LetterLabelSubjectUnhappy",
            "LetterLabelGuestCaptured",
            "LetterLabelUnauthorizedSurgery",
            "LetterLabelGuestLost",
            "LetterLabelGuestUnhappy",
            "LetterLabelGuestCapture",
            "LetterLabelLoyaltySquad",
            "LetterLabelAngryAnimal",
            "LetterLabelCaptured",
            "LetterLabelChasingSubject",
            "LetterLabelSiteHarass",
            "LetterLabelSiteAppeared",
            "LetterLabelHeirFailed",
            "LetterLabelTitleHolderDied",
            "LetterLabelDesignatedHeirDied",
            "LetterLabelNewHeir",
            "LetterLabelDiedAnger",
            "LetterLabelDiedRevenge",
            "LetterLabelDiedDeparture",
            "LetterLabelArrestedAnger",
            "LetterLabelArrestedRevenge",
            "LetterLabelArrestedDeparture",
            "LetterLabelViolatedAnger",
            "LetterLabelViolatedRevenge",
            "LetterLabelViolatedDeparture",
            "LetterLabelBetrayal",
            "LetterLabelHospitalityReward",
            "LetterLabelBetrayalReward",
            "LetterLabelBetrayalRewardRetracted",
            "LetterLabelShuttleArrived",
            "LetterLabelShuttleDestroyed",
            "LetterLabelColonistsReturned",
            "LetterLabelRaidAnyone",
            "LetterLabelRescueShuttleArrived",
            "LetterLabelQuestCompleted",
            "LetterLabelLaborersArrived",
            "LetterLabelLaborerDied",
            "LetterLabelDestroyed",
            // Ritual_Behaviors.xml
            "LetterLabelThroneSpeech",
            // Incidents_Map_Disease.xml
            "LetterLabelDiseaseAbasia",
            "LetterLabelDiseaseBloodRot",
            // Incidents.xml
            "LetterLabelTributeCollectorArrival",
            // Misc_Gameplay.xml
            "LetterLabelLinkingRitualCompleted",
            // IDEOLOGY
            // Incidents_Map_Special.xml
            "LetterLabelBeggarsArrive",
            "LetterLabelPilgrimsArrive",
            "LetterLabelGauranlenPodSpawn",
            "LetterLabelInfestationJelly",
            "LetterLabelWanderersSkylantern",
            // MentalStates_Mood.xml
            "LetterLabelCrisisOfBelief",
            "LetterLabelRebel",
            // QuestScriptDefs
            "LetterLabelBeggarsBetrayed",
            "LetterLabelArchonexusDiscovered",
            // WorkSites.xml
            "LetterLabelWorkSite",
            // Incidents_World_Quests.xml
            "LetterLabelArchonexusVictory",
            // BIOTECH
            // HediffDefs
            "LetterLabelReusable",
            // Incidents_Map_Special.xml
            "LetterLabelAncientMechanitor",
            "LetterLabelPoluxTree",
            // MentalStates_Special.xml
            "LetterLabelFleeingFire",
            "LetterLabelWarcall",
            // QuestScriptDefs
            "LetterLabelBossDefeated",
            "LetterLabelMechanitorShip",
            "LetterLabelMechanoidAttack",
            "LetterLabelMechlinkAvailable",
            "LetterLabelWastepacks",
            "LetterLabelSanguophagesArrive",
            "LetterLabelSanguophagesArgument",
            "LetterLabelMeetingComplete",
            "LetterLabelSanguophagesHunters",
            "LetterLabelCrashedShuttle",
            "LetterLabelThrallReinforcements",
            "LetterLabelAssaultBegun",
            "LetterLabelAttached",
            // Incidents_Map_Threats.xml
            "LetterLabelWastepackInfestation",
            // ThingDefs_Buildings
            "LetterLabelAncientMech",
            "LetterLabelAvailable"
        };

        public bool[] genericMessage_values = new bool[genericMessage_labels.Count];
        public bool[] genericAlert_values = new bool[genericAlert_labels.Count];
        public bool[] genericLetter_values = new bool[genericLetter_labels.Count];

        public bool taintedMessagePatch = true;
        public bool idleColonistsPatch = true;
        public bool drawAutoSelectCheckboxPatch = true;

        public override void ExposeData()
        {
            for (int i = 0; i < genericMessage_labels.Count; i++)
            {
                Scribe_Values.Look(ref genericMessage_values[i], genericMessage_labels[i]);
            }
            for (int i = 0; i < genericAlert_labels.Count; i++)
            {
                Scribe_Values.Look(ref genericAlert_values[i], genericAlert_labels[i]);
            }
            for (int i = 0; i < genericLetter_labels.Count; i++)
            {
                Scribe_Values.Look(ref genericLetter_values[i], genericLetter_labels[i]);
            }

            Scribe_Values.Look(ref taintedMessagePatch, "taintedMessagePatch");
            Scribe_Values.Look(ref idleColonistsPatch, "idleColonistsPatch");
            Scribe_Values.Look(ref drawAutoSelectCheckboxPatch, "drawAutoSelectCheckboxPatch");
            base.ExposeData();
        }

        public List<string> ActiveGenericMessagePatches()
        {
            List<string> activePatches = new List<string>();

            for (int i = 0; i < genericMessage_labels.Count; i++)
            {
                if (genericMessage_values[i]) activePatches.Add(genericMessage_labels[i]);
            }

            return activePatches;
        }

        public List<string> ActiveGenericAlertPatches()
        {
            List<string> activePatches = new List<string>();

            for (int i = 0; i < genericAlert_labels.Count; i++)
            {
                if (genericAlert_values[i]) activePatches.Add(genericAlert_labels[i]);
            }

            return activePatches;
        }

        public List<string> ActiveGenericLetterPatches()
        {
            List<string> activePatches = new List<string>();

            for (int i = 0; i < genericLetter_labels.Count; i++)
            {
                if (genericLetter_values[i]) activePatches.Add(genericLetter_labels[i]);
            }

            return activePatches;
        }

        public bool GetGenericMessagePatchValue(string label)
        {
            for (int i = 0; i < genericMessage_labels.Count; i++)
            {
                if (genericMessage_labels[i] == label) return genericMessage_values[i];
            }

            throw new ArgumentException("Argument " + label + " not found in the list.");
        }

        public bool GetGenericAlertPatchValue(string label)
        {
            for (int i = 0; i < genericAlert_labels.Count; i++)
            {
                if (genericAlert_labels[i] == label) return genericAlert_values[i];
            }

            throw new ArgumentException("Argument " + label + " not found in the list.");
        }

        public bool GetGenericLetterPatchValue(string label)
        {
            for (int i = 0; i < genericLetter_labels.Count; i++)
            {
                if (genericLetter_labels[i] == label) return genericLetter_values[i];
            }

            throw new ArgumentException("Argument " + label + " not found in the list.");
        }
    }

    public class BUMMod : Mod
    {
        private enum Tab
        {
            Messages,
            Alerts,
            Letters,
            Misc
        }
        readonly BUMSettings settings;
        private static List<TabRecord> tabsList = new List<TabRecord>();
        private Tab tab = Tab.Messages;
        private Vector2 scrollPosition;
        private static string searchText = "";

        private const int LINE_MAX = 100;

        public BUMMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<BUMSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            // Create tab menu area
            Rect tabRect = inRect;
            tabRect.yMin += 30f;
            
            tabsList.Clear();
            tabsList.Add(new TabRecord("Messages", (() => tab = Tab.Messages), tab == Tab.Messages));
            tabsList.Add(new TabRecord("Alerts", (() => tab = Tab.Alerts), tab == Tab.Alerts));
            tabsList.Add(new TabRecord("Letters", (() => tab = Tab.Letters), tab == Tab.Letters));
            tabsList.Add(new TabRecord("Misc", (() => tab = Tab.Misc), tab == Tab.Misc));
            
            Widgets.DrawMenuSection(tabRect);
            TabDrawer.DrawTabs(tabRect, tabsList);
            tabsList.Clear();
            
            // Create Custom and Generic menu areas

            Rect contentRect = tabRect;
            contentRect = contentRect.ContractedBy(20f);

            Rect customRect = contentRect;
            Rect genericRect = contentRect;
            genericRect.yMin += 85f;
            Rect genericTitleRect = genericRect;
            genericTitleRect.width -= 200f;
            Rect genericBtnRect = genericRect;
            genericBtnRect.width = 95f;
            genericBtnRect.height = 25f;
            genericBtnRect.x = genericTitleRect.x + genericTitleRect.width + 10f;
            genericBtnRect.y += 29f;
            Rect genericViewRect = genericRect;
            genericViewRect.yMin += 60f;

            Listing_Standard customList = new Listing_Standard();
            customList.Begin(customRect);
            Text.Font = GameFont.Medium;
            
            switch (tab)
            {
                case Tab.Messages:
                    customList.Label("Custom Message Blockers");
                    Text.Font = GameFont.Small;
                    customList.CheckboxLabeled("MessageDeterioratedAway - But only tainted apparel", ref settings.taintedMessagePatch);
                    break;
                case Tab.Alerts:
                    customList.Label("Custom Alert Blockers");
                    Text.Font = GameFont.Small;
                    customList.CheckboxLabeled("ColonistsIdle - But only for your own colonists, not guests", ref settings.idleColonistsPatch);
                    break;
                case Tab.Letters:
                    customList.Label("Custom Letter Blockers");
                    Text.Font = GameFont.Small;
                    customList.Label("No custom letter blockers currently; requests open on the workshop.");
                    break;
                case Tab.Misc:
                    customList.Label("Miscellaneous Blockers");
                    Text.Font = GameFont.Small;
                    customList.CheckboxLabeled("Remove and disable the automatically add food to caravan checkbox", ref settings.drawAutoSelectCheckboxPatch);
                    break;
            }
            customList.Gap();
            customList.GapLine();
            customList.End();

            if ((tab == Tab.Messages) || (tab == Tab.Alerts) || (tab == Tab.Letters))
            {
                Listing_Standard genericTitle = new Listing_Standard();
                genericTitle.Begin(genericTitleRect);
                Text.Font = GameFont.Medium;
                switch (tab)
                {
                    case Tab.Messages:
                        genericTitle.Label("Generic Message Blockers");
                        break;
                    case Tab.Alerts:
                        genericTitle.Label("Generic Alert Blockers");
                        break;
                    case Tab.Letters:
                        genericTitle.Label("Generic Letter Blockers");
                        break;
                }
                Text.Font = GameFont.Small;
                searchText = genericTitle.TextEntry(searchText);
                genericTitle.End();

                if (Widgets.ButtonText(genericBtnRect, "Select All")) ChangeTabPatches(true);
                genericBtnRect.x += 105f;
                if (Widgets.ButtonText(genericBtnRect, "Deselect All")) ChangeTabPatches(false);

                Rect scrollRect = genericViewRect;
                switch (tab)
                {
                    case Tab.Messages:
                        scrollRect.height = 26.1f * BUMSettings.genericMessage_labels.Count;
                        break;
                    case Tab.Alerts:
                        scrollRect.height = 26.1f * BUMSettings.genericAlert_labels.Count;
                        break;
                    case Tab.Letters:
                        scrollRect.height = 26.1f * BUMSettings.genericLetter_labels.Count;
                        break;
                }
                scrollRect.width = genericViewRect.width - 20f;
                Widgets.BeginScrollView(genericViewRect, ref scrollPosition, scrollRect);
                Rect listRect = scrollRect;
                Listing_Standard listingStandard = new Listing_Standard();
                listingStandard.verticalSpacing = 4f;
                listingStandard.Begin(listRect);


                bool searchOn = (searchText.Length > 0);
                switch (tab)
                {
                    case Tab.Messages:
                        for (int i = 0; i < BUMSettings.genericMessage_labels.Count; i++)
                        {
                            string label = BUMSettings.genericMessage_labels[i];
                            string message = label + " - " + label.Translate();

                            if (searchOn && !message.ToLower().Contains(searchText.ToLower())) continue;
                            if (message.Length > LINE_MAX) message = (message.Substring(0, LINE_MAX) + "...");

                            listingStandard.CheckboxLabeled(message, ref settings.genericMessage_values[i]);
                        }
                        break;
                    case Tab.Alerts:
                        for (int i = 0; i < BUMSettings.genericAlert_labels.Count; i++)
                        {
                            string label = BUMSettings.genericAlert_labels[i];
                            string message = label + " - " + label.Translate();

                            if (searchOn && !message.ToLower().Contains(searchText.ToLower())) continue;
                            if (message.Length > LINE_MAX) message = (message.Substring(0, LINE_MAX) + "...");

                            listingStandard.CheckboxLabeled(message, ref settings.genericAlert_values[i]);
                        }
                        break;
                    case Tab.Letters:
                        for (int i = 0; i < BUMSettings.genericLetter_labels.Count; i++)
                        {
                            string label = BUMSettings.genericLetter_labels[i];
                            string message = label + " - " + label.Translate();

                            if (searchOn && !message.ToLower().Contains(searchText.ToLower())) continue;
                            if (message.Length > LINE_MAX) message = (message.Substring(0, LINE_MAX) + "...");

                            listingStandard.CheckboxLabeled(message, ref settings.genericLetter_values[i]);
                        }
                        break;
                }

                listingStandard.End();
                Widgets.EndScrollView();
            }
            
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "BUM: Block Unwanted Minutiae";
        }
        
        public void ChangeTabPatches(bool newState)
        {
            if (newState) SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
            else SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();

            switch (tab)
            {
                case Tab.Messages:
                    for (int i = 0; i < settings.genericMessage_values.Length; i++)
                        settings.genericMessage_values[i] = newState;
                    break;
                case Tab.Alerts:
                    for (int i = 0; i < settings.genericAlert_values.Length; i++)
                        settings.genericAlert_values[i] = newState;
                    break;
                case Tab.Letters:
                    for (int i = 0; i < settings.genericLetter_values.Length; i++)
                        settings.genericLetter_values[i] = newState;
                    break;
            }
        }
    }
}