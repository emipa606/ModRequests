using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CM_Grab_Your_Tool
{
    public class ToolMemoryTracker : WorldComponent
    {
        private List<ToolMemory> toolMemories = new List<ToolMemory>();

        public ToolMemoryTracker(World world) : base(world)
        {

        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                toolMemories = toolMemories.Where(memory => memory != null && memory.pawn != null && !memory.pawn.Dead && !memory.pawn.Destroyed && memory.pawn.Spawned).ToList();
            }

            Scribe_Collections.Look(ref toolMemories, "toolMemories", LookMode.Deep);

            CheckToolMemories();
        }

        // Users somehow ending up with null memory and its unfeasible to test get their save
        private void CheckToolMemories()
        {
            if (toolMemories == null)
                toolMemories = new List<ToolMemory>();
        }

        public ToolMemory GetMemory(Pawn pawn)
        {
            CheckToolMemories();

            ToolMemory toolMemory = toolMemories.Find(tm => tm != null && tm.pawn == pawn);
            if (toolMemory == null)
            {
                toolMemory = new ToolMemory();
                toolMemory.pawn = pawn;

                toolMemories.Add(toolMemory);
            }

            return toolMemory;
        }

        public void ClearMemory(Pawn pawn)
        {
            CheckToolMemories();

            ToolMemory toolMemory = toolMemories.Find(tm => tm != null && tm.pawn == pawn);
            if (toolMemory != null)
            {
                Thing previouslyEquipped = toolMemory.PreviousEquipped;
                if (previouslyEquipped != null && pawn.inventory != null && pawn.inventory.GetDirectlyHeldThings() != null && pawn.inventory.GetDirectlyHeldThings().Contains(previouslyEquipped))
                    TryEquipWeapon(pawn, previouslyEquipped as ThingWithComps, false);

                toolMemories.Remove(toolMemory);
            }
        }


        public static bool EquipAppropriateWeapon(Pawn pawn, SkillDef skill)
        {
            if (pawn == null || skill == null)
                return false;

            //Log.Message("TryActuallyStartNextToil - EquipAppropriateWeapon");
            ThingOwner heldThingsOwner = pawn.inventory.GetDirectlyHeldThings();
            List<Thing> weaponsHeld = heldThingsOwner.Where(thing => thing.def.IsWeapon).ToList();

            foreach (Thing weapon in weaponsHeld)
            {
                if (HasReleventStatModifiers(weapon, skill))
                {
                    return TryEquipWeapon(pawn, weapon as ThingWithComps);
                }
            }

            return false;
        }

        public static bool HasReleventStatModifiers(Thing weapon, SkillDef skill)
        {
            if (weapon == null)
                return false;

            List<StatModifier> statModifiers = weapon.def.equippedStatOffsets;
            if (skill != null && statModifiers != null)
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
                            if (skill == skillNeed.skill)
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
                            if (skill == skillNeed.skill)
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

        public static bool TryEquipWeapon(Pawn pawn, ThingWithComps weapon, bool makeSound = true)
        {
            if (pawn == null || weapon == null)
                return false;

            ThingWithComps currentWeapon = pawn.equipment.Primary;

            bool transferSuccess = true;

            if (currentWeapon != null)
                transferSuccess = pawn.inventory.innerContainer.TryAddOrTransfer(currentWeapon);

            if (transferSuccess)
            {
                if (weapon.stackCount > 1)
                {
                    weapon = (ThingWithComps)weapon.SplitOff(1);
                }
                if (weapon.holdingOwner != null)
                {
                    weapon.holdingOwner.Remove(weapon);
                }
                pawn.equipment.AddEquipment(weapon);
                if (makeSound)
                    weapon.def.soundInteract?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                return true;
            }
            else
            {
                Log.Warning("CM_Grab_Your_Tool: Unable to transfer equipped weapon to inventory");
            }

            return false;
        }
    }
}
