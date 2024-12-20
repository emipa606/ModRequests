﻿using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace WhatTheHack.Buildings;

public abstract class Building_BaseMechanoidPlatform : Building_Bed
{
    public const int SLOTINDEX = 1;
    public CompRefuelable refuelableComp;
    protected bool regenerateActive = true;
    protected bool repairActive = true;

    public virtual bool RegenerateActive => regenerateActive;
    public virtual bool RepairActive => repairActive;

    public abstract bool CanHealNow();
    public abstract bool HasPowerNow();

    //make sure if you place the platform on a downed mech, the mech will get an opportunity to start the rest job again. 
    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (!respawningAfterLoad)
        {
            var curOccupant = GetCurOccupant(SLOTINDEX);
            if (curOccupant != null && curOccupant.IsHacked())
            {
                curOccupant.jobs.StartJob(new Job(WTH_DefOf.WTH_Mechanoid_Rest, this) { count = 1 },
                    JobCondition.InterruptForced);
            }
        }

        if (Medical)
        {
            medicalInt = false;
            //Traverse.Create(this).Field("medicalInt").SetValue(false);
        }

        //Medical = false;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        medicalInt = false;
        //Traverse.Create(this).Field("medicalInt").SetValue(false);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            if (!(gizmo is Command_Toggle toggleCommand && toggleCommand.icon.name == "AsMedical"))
            {
                yield return gizmo;
            }
        }
        //yield return new Command_Action
        //{
        //    defaultLabel = "CommandThingSetOwnerLabel".Translate(),
        //    icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner", true),
        //    defaultDesc = "WTH_Gizmo_SetMechanoidOwner_Description".Translate(),
        //    action = delegate
        //    {
        //        Find.WindowStack.Add(new Dialog_AssignBuildingOwner(this.TryGetComp<CompAssignableToPawn_Bed>()));
        //    },
        //    hotKey = KeyBindingDefOf.Misc3
        //};
    }
}