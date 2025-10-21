using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

namespace zed_0xff.CPS;

// fixes 'Tried to get a resource "UI/Commands/ForPrisoners" from a different thread. All resources must be loaded in the main thread.'
[StaticConstructorOnStartup]
public partial class Building_TSS : Building_MultiEnterable, IStoreSettingsParent, IThingHolderWithDrawnPawn, IBillTab, IBillGiver {
    public /*override*/ int MaxSlots => 16;

    private int lastTickWithPower = 0;
    private int lastRotate = 0;
    private int curOffset = 0;
    private int nPawns = 0;

    private const float BasePawnConsumedNutritionPerDay = 1.6f;
    public const float NutritionBuffer = BasePawnConsumedNutritionPerDay * 10; // should be MaxSlots here?
    public float restEffectiveness = StatDefOf.BedRestEffectiveness.valueIfMissing;
    public float comfort = 0;
    public float beauty = 0;

    [Unsaved(false)]
    private CompPowerTrader cachedPowerComp;
    private CompPowerTrader PowerTraderComp {
        get {
            if (cachedPowerComp == null) {
                cachedPowerComp = this.TryGetComp<CompPowerTrader>();
            }
            return cachedPowerComp;
        }
    }

    public bool PowerOn => PowerTraderComp.PowerOn;

    [Unsaved(false)]
    private CompAffectedByFacilities cachedAffectedByFacilitiesComp;
    public CompAffectedByFacilities AffectedByFacilitiesComp {
        get {
            if (cachedAffectedByFacilitiesComp == null) {
                cachedAffectedByFacilitiesComp = this.TryGetComp<CompAffectedByFacilities>();
            }
            return cachedAffectedByFacilitiesComp;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////
    // <forOwnerType>

    public BedOwnerType ForOwnerType
    {
        get
        {
            return forOwnerType;
        }
        set
        {
            if (value != forOwnerType)
            {
                EjectAll();
                forOwnerType = value;
                cachedTopGraphic = null;
                Notify_ColorChanged();
                NotifyRoomBedTypeChanged();
            }
        }
    }

    private BedOwnerType forOwnerType;
    public bool ForPrisoners
    {
        get
        {
            return forOwnerType == BedOwnerType.Prisoner;
        }
        set
        {
            if (value == ForPrisoners)
            {
                return;
            }
            if (Current.ProgramState != ProgramState.Playing && Scribe.mode != 0)
            {
                Log.Error("TSS: Tried to set ForPrisoners while game mode was " + Current.ProgramState);
                return;
            }
            EjectAll();
            if (value)
            {
                forOwnerType = BedOwnerType.Prisoner;
            }
            else
            {
                forOwnerType = BedOwnerType.Colonist;
                Log.Error("Bed ForPrisoners=false, but should it be for for colonists or slaves?  Set ForOwnerType instead.");
            }
            cachedTopGraphic = null;
            Notify_ColorChanged();
            NotifyRoomBedTypeChanged();
        }
    }

    private void NotifyRoomBedTypeChanged()
    {
        this.GetRoom()?.Notify_BedTypeChanged();
    }

    private static int lastBedOwnerSetChangeFrame = -1;
    public void SetBedOwnerTypeByInterface(BedOwnerType ownerType)
    {
        if (lastBedOwnerSetChangeFrame == Time.frameCount) {
            return;
        }
        lastBedOwnerSetChangeFrame = Time.frameCount;

        ((ForOwnerType != ownerType) ? SoundDefOf.Checkbox_TurnedOn : SoundDefOf.Checkbox_TurnedOff).PlayOneShotOnCamera();
        ForOwnerType = ownerType;
    }

    // </forOwnerType>
    ///////////////////////////////////////////////////////////////////////////////////////////

    public override void PostMake()
    {
        base.PostMake();
        allowedNutritionSettings = new StorageSettings(this);
        if (def.building.defaultStorageSettings != null)
        {
            allowedNutritionSettings.CopyFrom(def.building.defaultStorageSettings);
        }
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        if( !respawningAfterLoad ){
            // from Building_Bed, works good before base.SpawnSetup()
            Region region = map.regionGrid.GetValidRegionAt_NoRebuild(InteractionCell);
            if( region != null && region.Room != null && region.Room.IsPrisonCell ){
                ForPrisoners = true;
            }
        }

        base.SpawnSetup(map, respawningAfterLoad);

        if( ai == null ){
            ai = new AI(this);
        }

        restEffectiveness = 
            def.statBases.StatListContains(StatDefOf.BedRestEffectiveness)
            ? this.GetStatValue(StatDefOf.BedRestEffectiveness) 
            : StatDefOf.BedRestEffectiveness.valueIfMissing;

        comfort = 
            def.statBases.StatListContains(StatDefOf.Comfort)
            ? this.GetStatValue(StatDefOf.Comfort) 
            : StatDefOf.Comfort.valueIfMissing;

        beauty = 
            def.statBases.StatListContains(StatDefOf.Beauty)
            ? this.GetStatValue(StatDefOf.Beauty) 
            : StatDefOf.Beauty.valueIfMissing;

        if (ModLister.HasActiveModWithName("Vanilla Nutrient Paste Expanded")) {
            Type t = GenTypes.GetTypeInAnyAssembly("zed_0xff.CPS.Dispenser_VNPE");
            if( t != null ){
                dispensers.Add((IDispenser)Activator.CreateInstance(t, new object[] { this }));
            }
        }
        dispensers.Add(new Dispenser_Internal(this));

        if (ModLister.HasActiveModWithName("Vanilla Races Expanded - Sanguophage")){
            Type t = GenTypes.GetTypeInAnyAssembly("zed_0xff.CPS.HemogenNetAdapter");
            if( t != null ){
                hemogenNetAdapter = (IPipeNetAdapter)Activator.CreateInstance(t, new object[] { this });
            }
        }

        if (ModLister.HasActiveModWithName("Dubs Bad Hygiene")) {
            Type t = GenTypes.GetTypeInAnyAssembly("zed_0xff.CPS.Plugin_DBH");
            if( t != null ){
                dbh = (IPlugin)Activator.CreateInstance(t, new object[] { this });
            }
        }

        if( recipeMap.Count == 0 && ModsConfig.BiotechActive ){
            recipeMap[DefDatabase<RecipeDef>.GetNamed("CPS_DrawBlood_HemogenFarmPrisoners")] = PatientType.Donor_HemogenFarmPrisoner;
            recipeMap[DefDatabase<RecipeDef>.GetNamed("CPS_DrawBlood_AllPrisoners")]         = PatientType.Donor_Prisoner;
            recipeMap[DefDatabase<RecipeDef>.GetNamed("CPS_DrawBlood_Slaves")]               = PatientType.Donor_Slave;
            recipeMap[DefDatabase<RecipeDef>.GetNamed("CPS_DrawBlood_All")]                  = PatientType.Donor_Any;

            recipeMap[DefDatabase<RecipeDef>.GetNamed("CPS_BloodTransfusion_Colonists_50")]  = PatientType.Recipient_Colonist_50;
            recipeMap[DefDatabase<RecipeDef>.GetNamed("CPS_BloodTransfusion_Colonists_100")] = PatientType.Recipient_Colonist_100;
            recipeMap[DefDatabase<RecipeDef>.GetNamed("CPS_BloodTransfusion_All_50")]        = PatientType.Recipient_All_50;
        }

        rotate();
    }

    private IPlugin dbh = null;

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish) {
        sustainerWorking = null;
        // destroy all add-ons on building destroy
        List<Thing> facilities = new List<Thing>();
        facilities.AddRange(AffectedByFacilitiesComp.LinkedFacilitiesListForReading);
        foreach( Thing t in facilities ){
            t.DeSpawn(mode);
        }
        base.DeSpawn(mode);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////
    // <IThingHolderWithDrawnPawn>

    public float HeldPawnDrawPos_Y => DrawPos.y + 3f / 74f;
    public float HeldPawnBodyAngle => base.Rotation.AsAngle;
    public PawnPosture HeldPawnPosture => PawnPosture.LayingOnGroundFaceUp;

    // </IThingHolderWithDrawnPawn>
    ///////////////////////////////////////////////////////////////////////////////////////////
    // <IStoreSettingsParent>
    
    private StorageSettings allowedNutritionSettings;
    public bool StorageTabVisible => true;

    public bool CanAcceptNutrition(Thing thing) {
        return allowedNutritionSettings.AllowedToAccept(thing);
    }

    public StorageSettings GetStoreSettings() {
        return allowedNutritionSettings;
    }

    public StorageSettings GetParentStoreSettings() {
        return def.building.fixedStorageSettings;
    }

    public void Notify_SettingsChanged() {
    }

    // </IStoreSettingsParent>
    ///////////////////////////////////////////////////////////////////////////////////////////
    // <Building_Enterable>

    private float timeSpeed = 1;
    public override bool IsContentsSuspended => (timeSpeed == 0);

    public override Vector3 PawnDrawOffset => Vector3.zero;

    public override AcceptanceReport CanAcceptPawn(Pawn pawn){
        return CanAcceptPawn(pawn, false);
    }

    public AcceptanceReport CanAcceptPawn(Pawn pawn, bool forcePrisoner) {
        bool pawnIsPrisoner = pawn.IsPrisonerOfColony || forcePrisoner;

        if (!pawn.IsColonist && !pawn.IsSlaveOfColony && !pawnIsPrisoner)
        {
            return false;
        }
        if (!pawn.RaceProps.Humanlike || pawn.IsQuestLodger())
        {
            return false;
        }
        if (nPawns >= MaxSlots)
        {
            return "Occupied".Translate();
        }
        if (!PowerOn)
        {
            return "NoPower".Translate().CapitalizeFirst();
        }
        if( ForPrisoners && !pawnIsPrisoner ){
            return "ForPrisonerUse".Translate().CapitalizeFirst();
        }
        if( !ForPrisoners && pawnIsPrisoner ){
            return "ForColonistUse".Translate().CapitalizeFirst();
        }
        return true;
    }

    public override void TryAcceptPawn(Pawn pawn) {
        if ( !CanAcceptPawn(pawn) ) {
            return;
        }
        selectedPawn = null;
        bool num = pawn.DeSpawnOrDeselect();
        if (innerContainer.TryAddOrTransfer(pawn)) {
            SoundDefOf.CryptosleepCasket_Accept.PlayOneShot(SoundInfo.InMap(this));
            SelectedPawns.Remove(pawn);
            if( pawn.IsPrisonerOfColony ){
                if( CPSMod.Settings.tss.unassignPrisonerBeds ) pawn.ownership.UnclaimBed();
            } else if( pawn.IsSlaveOfColony ) {
                if( CPSMod.Settings.tss.unassignSlaveBeds ) pawn.ownership.UnclaimBed();
            } else {
                if( CPSMod.Settings.tss.unassignColonistBeds ) pawn.ownership.UnclaimBed();
            }
            rotate();
        }
        if (num) {
            Find.Selector.Select(pawn, playSound: false, forceDesignatorDeselect: false);
        }
    }

    public void Eject(Thing t){
        innerContainer.TryDrop(t, InteractionCell, Map, ThingPlaceMode.Near, out _);
        if( t is Pawn || t is Corpse ){
            SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(this));
            if( topPawns != null ){
                topPawns.Remove(t as Pawn);
            }
            rotate(); // should be called after TryDrop()
        }
    }

    // </Building_Enterable>
    ///////////////////////////////////////////////////////////////////////////////////////////
    // <Sound>

    [Unsaved(false)]
    private Sustainer sustainerWorking = null;

    // </Sound>
    ///////////////////////////////////////////////////////////////////////////////////////////

    private void rotate(){
       lastRotate = Find.TickManager.TicksGame;

       if ( topPawns == null ){
           topPawns = new List<Pawn>();
       } else {
           topPawns.Clear();
       }

       Thing[] allPawns = innerContainer.Where((Thing t) => t is Pawn pawn && !pawn.Dead).ToArray();
       nPawns = allPawns.Length;

       if( nPawns <= 4 ){
           foreach( Thing t in allPawns ){
               topPawns.Add(t as Pawn);
           }
       } else {
           curOffset++;
           if( curOffset >= nPawns )
               curOffset = 0;

           int i = curOffset;
           while( topPawns.Count < 4 ){
               if( i >= nPawns )
                   i = 0;
               topPawns.Add(allPawns[i] as Pawn);
               i++;
           }
       }
    }

    // 1. play recipeSound (does not play if called in rare tick)
    // 2. tick contained pawns
    // 3. make them rest
    // 4. do bills
    // 5. tick effects
    // 6. call TickRare()
    public override void Tick()
    {
        if( PowerOn ){
            if ( nPawns > 0 ){
                if( CPSMod.Settings.tss.sounds ){
                    if (sustainerWorking == null || sustainerWorking.Ended) {
                        sustainerWorking = SoundDefOf.GrowthVat_Working.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
                    } else {
                        sustainerWorking.Maintain();
                    }
                }
            }

            if( !IsContentsSuspended ){
                // tick resting
                foreach( Thing t in innerContainer ) {
                    if( t is Pawn pawn && !pawn.Dead && pawn.needs.rest != null) {
                        pawn.needs.rest.TickResting(restEffectiveness);
                    }
                }
            }
        }

        base.Tick();
        switch( timeSpeed ){
            case 2:
                innerContainer.ThingOwnerTick();
                innerContainer.ThingOwnerTick();
                break;
            case 0.5f:
                if( Find.TickManager.TicksGame%2 == 0 ){
                    innerContainer.ThingOwnerTick();
                }
                break;
            default:
                // time is handled by IsContentsSuspended
                innerContainer.ThingOwnerTick();
                break;
        }

        Tick_Bills();

        if( this.IsHashIntervalTick(250) ){
            TickRare();
        }
    }

    // 1. count pawns
    // 2. eject all on power failure check
    // 3. rotate pawns
    // 4. feed pawns
    // 5. check DBH needs
    // 6. update time speed
    public override void TickRare() {
        timeSpeed = 1;
        foreach( Thing t in AffectedByFacilitiesComp.LinkedFacilitiesListForReading ){
            if( !t.TryGetComp<CompPowerTrader>().PowerOn ) continue;

            CompProperties_TimeSpeed compTimespeed = t.def.GetCompProperties<CompProperties_TimeSpeed>();
            if( compTimespeed == null ) continue;

            timeSpeed = compTimespeed.ratio;
            break;
        }

        if( topPawns == null )
            rotate();

        if( PowerOn ){
            lastTickWithPower = Find.TickManager.TicksGame;
            feedOccupants();

            Thing thingToEject = null;
            foreach( Thing t in innerContainer ){
                if( t is Pawn pawn ){
                    if( pawn.Dead ){
                        thingToEject = t;
                    } else {
                        pawn.health.AddHediff(HediffDefOf.CryptosleepSickness);
                        pawn.needs?.mood?.thoughts?.memories?.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
                    }
                } else if( t is Corpse ){
                    thingToEject = t;
                }
            }
            if( thingToEject != null ){
                Eject(thingToEject);
            }

            if( Math.Abs(Find.TickManager.TicksGame - lastRotate) > 1200 ){
                ai.Work();
                rotate();
            }

            if( dbh != null ){
                foreach( Thing t in innerContainer ){
                    if( t is Pawn pawn ){
                        dbh.ProcessPawn(pawn);
                    }
                }
            }
        } else {
            if( IsInternalBatteryEmpty() ){
                LocalTargetInfo target = this;
                foreach( Thing t in innerContainer ){
                    if( t is Pawn pawn && pawn.InAggroMentalState ){
                        var verb = pawn.TryGetAttackVerb(this);
                        if( verb == null ) continue;

                        verb.TryStartCastOn(target);
                        if( verb is Verb_MeleeAttackDamage dmg ){
                            // from Verb_MeleeAttackDamage.DamageInfosToApply
                            float num = dmg.verbProps.AdjustedMeleeDamageAmount(dmg, pawn);
                            float armorPenetration = dmg.verbProps.AdjustedArmorPenetration(dmg, pawn);
                            num = Rand.Range(num * 0.8f, num * 1.2f);
                            var source = pawn.def;
                            bool instigatorGuilty = true;
                            DamageInfo damageInfo = new DamageInfo(DamageDefOf.Blunt, num, armorPenetration, -1f, pawn, null, source, DamageInfo.SourceCategory.ThingOrUnknown, null, instigatorGuilty);
                            this.TakeDamage(damageInfo);
                            damageInfo.SetAmount(num/4);
                            pawn.TakeDamage(damageInfo);
                        }
                    }
                }
            }
        }
        //base.TickRare(); // don't call base because we call TickRare() from Tick() ourselves (questionable)
    }

    public bool IsInternalBatteryEmpty(){
        return lastTickWithPower != 0 && Find.TickManager.TicksGame - lastTickWithPower > GenDate.TicksPerHour*2;
    }

    void EjectAll(){
        List<Thing> things = new List<Thing>( innerContainer ); // original list will be modified in process
        foreach( Thing t in things ){
            if( t is Pawn ){
                Eject(t);
            }
        }
        topPawns = null;
        nPawns = 0;
        selectedPawn = null;
        SelectedPawns.Clear();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref allowedNutritionSettings, "allowedNutritionSettings");
        Scribe_Values.Look(ref forOwnerType, "forOwnerType", BedOwnerType.Colonist);
        Scribe_Values.Look(ref lastTickWithPower, "lastTickWithPower", 0);
        Scribe_Values.Look(ref timeSpeed, "timeSpeed", 1);

        Scribe_Deep.Look(ref billStack, "bills", this);
        Scribe_Deep.Look(ref currentBillReport, "currentBillReport");

        Scribe_Deep.Look(ref ai, "ai", this);

        if (allowedNutritionSettings == null) {
            allowedNutritionSettings = new StorageSettings(this);
            if (def.building.defaultStorageSettings != null)
            {
                allowedNutritionSettings.CopyFrom(def.building.defaultStorageSettings);
            }
        }
    }
}
