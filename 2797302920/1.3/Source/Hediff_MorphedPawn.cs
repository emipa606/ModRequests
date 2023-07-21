using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace Renamon
{
    public class Hediff_MorphedPawn : HediffWithComps
    {
        public Pawn oldPawn;
        public override void PostRemoved()
        {
            base.PostRemoved();
            RemoveDeduplication();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            if (this.pawn.Faction == Faction.OfPlayer)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Renamon.CancelDuplication".Translate(),
                    defaultDesc = "Renamon.CancelDuplicationDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Abilities/CancelDupe_body"),
                    action = RemoveDeduplication
                };
            }
        }
        public void RemoveDeduplication()
        {
            SoundDef.Named("Morph_Fade").PlayOneShot(this.pawn);
            var drafted = pawn.drafter != null ? pawn.drafter.Drafted : false;
            var selected = Find.Selector.IsSelected(pawn);
            foreach (var injury in pawn.health.hediffSet.hediffs.Where(x => x is Hediff_Injury || x is Hediff_MissingPart).ToList())
            {
                if (injury is Hediff_Injury)
                {
                    Hediff firstHediffOfDef = oldPawn.health.hediffSet.hediffs.FirstOrDefault(x => x.def == injury.def && x.Part == injury.Part);
                    if (firstHediffOfDef != null)
                    {
                        firstHediffOfDef.Severity += injury.Severity;
                    }
                    else if (injury.Severity > 0f)
                    {
                        var part = oldPawn.health.hediffSet.GetNotMissingParts().Where(x => x.def == injury.Part.def)
                            .TryRandomElement(out var bodyPart) ? bodyPart : null;
                        firstHediffOfDef = HediffMaker.MakeHediff(injury.def, oldPawn, part);
                        firstHediffOfDef.Severity = injury.Severity;
                        oldPawn.health.AddHediff(firstHediffOfDef);
                    }
                }
                else if (oldPawn.health.hediffSet.GetNotMissingParts().Where(x => x.def == injury.Part.def).TryRandomElement(out var bodyPart))
                {
                    oldPawn.health.AddHediff(HediffMaker.MakeHediff(injury.def, oldPawn, bodyPart));
                }
            }

            if (pawn.apparel != null)
            {
                oldPawn.apparel = pawn.apparel;
                pawn.apparel = new Pawn_ApparelTracker(pawn);
                oldPawn.apparel.pawn = oldPawn;
                foreach (var verb in oldPawn.apparel.AllApparelVerbs)
                {
                    verb.caster = oldPawn;
                }
            }
            
            if (pawn.equipment != null)
            {
                oldPawn.equipment = pawn.equipment;
                pawn.equipment = new Pawn_EquipmentTracker(pawn);
                oldPawn.equipment.pawn = oldPawn;
                foreach (var verb in oldPawn.equipment.AllEquipmentVerbs)
                {
                    verb.caster = oldPawn;
                }
            }
            
            if (pawn.inventory != null)
            {
                oldPawn.inventory = pawn.inventory;
                pawn.inventory = new Pawn_InventoryTracker(pawn);
                oldPawn.inventory.pawn = oldPawn;
            }

            if (oldPawn.drafter != null)
            {
                oldPawn.drafter.Drafted = drafted;
            }
            var lord = this.pawn.GetLord();
            if (lord != null)
            {
                lord.AddPawn(oldPawn);
                lord.RemovePawn(this.pawn);
            }
            if (oldPawn.Dead)
            {
                var corpse = oldPawn.Corpse ?? oldPawn.MakeCorpse(null, false, 0);
                GenSpawn.Spawn(corpse, pawn.PositionHeld, pawn.MapHeld);
            }
            else
            {
                GenSpawn.Spawn(oldPawn, pawn.PositionHeld, pawn.MapHeld);
                oldPawn.Rotation = pawn.Rotation;
                oldPawn.guest.SetGuestStatus(pawn.HostFaction, pawn.guest.GuestStatus);
            }

            if (selected)
            {
                Find.Selector.Select(oldPawn);
            }
            
            if (oldPawn.WorkTagIsDisabled(WorkTags.Violent) && oldPawn.equipment.Primary != null)
            {
                oldPawn.equipment.DropAllEquipment(oldPawn.Position);
            }
            oldPawn.GetComp<CompPawnMorpher>().lastDuplicateTicks = Find.TickManager.TicksGame;
            CompPawnMorpher.ThrowSmokePuffThick(oldPawn.DrawPos, oldPawn.MapHeld, 2.3f, Color.white);
            if (pawn.Dead)
            {
                pawn.Corpse.Destroy();
            }
            else
            {
                pawn.Destroy();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref oldPawn, "oldPawn");
        }
    }
}
