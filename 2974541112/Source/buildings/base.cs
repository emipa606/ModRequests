using System.Collections.Generic;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace zed_0xff.CPS;

public abstract class Building_Base : Building_Bed {
    public bool IsDespawning = false;

    public abstract int MaxSlots { get; }

    public virtual bool FixSleepingPawnHeadPos(ref Pawn pawn, ref Vector3 pos){
        return false;
    }

    public bool FixSleepingPawnFramePos(ref Pawn pawn, ref Vector3 pos){
        if( FixSleepingPawnHeadPos(ref pawn, ref pos) ){
            pos.z += 0.5f;
            return true;
        }
        return false;
    }

    protected CompAssignableToPawn compAssignableToPawn = null;
    protected CompPowerTrader      compPowerTrader = null;
    protected CompFlickable        compFlickable = null;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        IsDespawning = false;
        // Cache.Add() should be called before base.SpawnSetup() or newly built buildings will not be rooms until save-load cycle
        // this.Map is not yet set at this point
        Cache.Add(this, map);

        compAssignableToPawn = GetComp<CompAssignableToPawn>();
        compPowerTrader      = GetComp<CompPowerTrader>();
        compFlickable        = GetComp<CompFlickable>();

        // fix number of sleeping slots
        compAssignableToPawn.Props.maxAssignedPawnsCount = MaxSlots;
        base.SpawnSetup(map, respawningAfterLoad);
        compAssignableToPawn.Props.maxAssignedPawnsCount = MaxSlots;
    }

    public bool IsPowerOn(){
        if( compFlickable != null && !compFlickable.SwitchIsOn )
            return false;

        return (compPowerTrader != null) && compPowerTrader.PowerOn;
    }

    // fix base NullReferenceException when CPS is a room for itself 
    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        Cache.Remove(this);
        if( def.building.isFence ){
            // CPS is a room for itself
            District district = this.GetDistrict();
            IsDespawning = true; // see Patch_RegionAndRoomQuery.cs
            base.DeSpawn(mode);
            if (district != null) {
                district.Notify_RoomShapeOrContainedBedsChanged();
//                if( district.Room != null ){ // <-- in fact, only this check is important here
//                    district.Room.Notify_RoomShapeChanged();
//                }
            }
        } else {
            // regular building
            base.DeSpawn(mode);
        }
    }

    // fix pawn in 5th slot unable to lay down
    new public Pawn GetCurOccupant(int slotIndex)
    {
        if (!base.Spawned) return null;

        IntVec3 sleepingSlotPos = GetSleepingSlotPos(slotIndex);
        List<Thing> list = Map.thingGrid.ThingsListAt(sleepingSlotPos);
        if( slotIndex >= OwnersForReading.Count )
            return null;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] is Pawn pawn && pawn.CurJob != null && pawn.GetPosture().InBed() && OwnersForReading[slotIndex] == pawn)
            {
                return pawn;
            }
        }
        return null;
    }

    // fix pawn in 5th slot unable to lay down
    new public Pawn GetCurOccupantAt(IntVec3 pos)
    {
        for (int i = 0; i < MaxSlots; i++)
        {
            if (GetSleepingSlotPos(i) == pos)
            {
                Pawn pawn = GetCurOccupant(i);
                if( pawn != null )
                    return pawn;
            }
        }
        return null;
    }

    private Dictionary<Pawn, int> pawnSlotsCache = new Dictionary<Pawn, int>();

    public int GetCurOccupantSlotIndexFast(Pawn p)
    {
        int r = -1;
        if (pawnSlotsCache.TryGetValue(p, out r)){
            return r;
        }

        // slower
        return OwnersForReading.IndexOf(p);
    }

    // 1. update pawnSlotsCache
    // 2. move pawns to their slots if they're sleeping on a wrong one
    public override void TickRare() {
        pawnSlotsCache.Clear();
        int i = 0;
        var rect = this.OccupiedRect();
        foreach( Pawn pawn in OwnersForReading){
            if( pawn != null ){
                pawnSlotsCache[pawn] = i;
                if( pawn.GetPosture().InBed() && rect.Contains(pawn.Position) ){
                    var pos = GetSleepingSlotPos(i);
                    if( pawn.Position != pos ){
                        if( Prefs.DevMode )
                            Log.Message("[d] CPS: teleporting " + pawn + " from " + pawn.Position + " to their slot #" + i + " at " + pos);
                        pawn.Position = pos;
                        //pawn.Notify_Teleported(endCurrentJob: false, resetTweenedPos: false);
                    }
                }
            }
            i++;
        }
        base.TickRare();
    }

    public override string GetInspectString(){
        if( !Prefs.DevMode )
            return base.GetInspectString();

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        for( int i=0; i<MaxSlots; i++){
            stringBuilder.AppendInNewLine("GetCurOccupant("+i+"): " + GetCurOccupant(i));
        }
        for( int i=0; i<MaxSlots; i++){
            stringBuilder.AppendInNewLine("GetSleepingSlotPos("+i+"): " + GetSleepingSlotPos(i));
        }
        int j = 0;
        foreach( Pawn p in OwnersForReading){
            stringBuilder.AppendInNewLine("OwnersForReading["+j+"]: " + p);
            j++;
        }
        foreach( var pos in this.OccupiedRect() ){
            stringBuilder.AppendInNewLine("GetCurOccupantAt("+pos+"): " + GetCurOccupantAt(pos));
        }
        return stringBuilder.ToString();
    }

    // draw assigned/total slots count
    public override void DrawGUIOverlay()
    {
        if ((int)Find.CameraDriver.CurrentZoom == 0){
            GenMapUI.DrawThingLabel(this, OwnersForReading.Count + "/" + MaxSlots);
        }
    }
}

