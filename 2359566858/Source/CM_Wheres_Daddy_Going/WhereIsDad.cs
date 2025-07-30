using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CM_Wheres_Daddy_Going
{
    public class WhereIsDad : WorldComponent
    {
        List<ParentalMemory> parentalMemories = new List<ParentalMemory>();

        public WhereIsDad(World world) : base(world)
        {

        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref parentalMemories, "parentalMemories", LookMode.Deep);
        }

        private ParentalMemory GetMemory(Pawn parent)
        {
            return parentalMemories.Find(pm => pm.pawn == parent);
        }

        public ParentalMemory GetOrCreateMemory(Pawn parent)
        {
            ParentalMemory memory = GetMemory(parent);
            if (memory != null)
                return memory;

            memory = new ParentalMemory(parent);
            parentalMemories.Add(memory);

            return memory;
        }

        public bool SetGotTheThing(Pawn parent, bool gotIt)
        {
            ParentalMemory memory = GetMemory(parent);
            if (memory != null)
            {
                memory.gotIt = gotIt;
                return true;
            }

            return false;
        }

        public ThingDef WhatIsParentGetting(Pawn parent)
        {
            ParentalMemory memory = GetMemory(parent);
            if (memory != null)
                return memory.thingDef;

            return null;
        }

        public bool? DidParentGetTheThing(Pawn parent)
        {
            ParentalMemory memory = GetMemory(parent);
            if (memory == null)
                return null;

            return memory.gotIt;
        }

        public Pawn RandomParent()
        {
            if (parentalMemories.Count > 0)
            {
                ParentalMemory memory = parentalMemories.RandomElement();
                if (memory != null)
                    return memory.pawn;
            }

            return null;
        }

        public void RemoveParent(Pawn parent)
        {
            parentalMemories.RemoveAll(pm => pm.pawn == parent);
        }
    }
}
