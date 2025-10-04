using Verse;
using RimWorld;
using Verse.Sound;
using System.Linq;
using System.Collections.Generic;

namespace TeleportationTrap
{
    public class Building_TrapTeleporter : Building_Trap
    {
        // Ensure this Building has a Comp_Linkable component
        public Comp_Linkable LinkableComp => this.TryGetComp<Comp_Linkable>();
        private bool isTrapEnabled = true; // Tracks whether the trap is enabled
        private bool autoRearm; // Tracks the autoRearm state (accessed via reflection)

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            // Access the private 'autoRearm' field using reflection
            var autoRearmField = typeof(Building_Trap).GetField("autoRearm", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (autoRearmField != null)
            {
                autoRearmField.SetValue(this, false);
            }

            if (!respawningAfterLoad)
            {
                SoundDefOf.TrapArm.PlayOneShot(new TargetInfo(Position, map));
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            // Save and load the isTrapEnabled field
            Scribe_Values.Look(ref isTrapEnabled, "isTrapEnabled", defaultValue: true);

            // Save and load the autoRearm field
            Scribe_Values.Look(ref autoRearm, "autoRearm", defaultValue: false);
        }

        /// <summary>
        /// Override the spring chance to return 0 only if the trap is disabled and has linked connections.
        /// </summary>
        protected override float SpringChance(Pawn p)
        {
            if (!isTrapEnabled && LinkableComp?.LinkedThings.Any() == true)
            {
                return 0f; // Disable springing if the trap is disabled and linked
            }

            // Call the base logic for normal behavior
            return base.SpringChance(p);
        }



        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            // Access the Comp_Linkable and invoke DrawLinks
            var linkableComp = this.TryGetComp<Comp_Linkable>();
            linkableComp?.DrawLinks();
        }

        protected override void SpringSub(Pawn p)
        {

            // Default behavior if trap is disabled but no linked connections
            if (!isTrapEnabled && (LinkableComp == null || !LinkableComp.LinkedThings.Any()))
            {
                // Default behavior: Damage or trap effect
                if (p != null)
                {
                    var damage = 10f; // Example damage value
                    var damageInfo = new DamageInfo(DamageDefOf.Stab, damage, 0, -1f, this);
                    p.TakeDamage(damageInfo);

                    // Play a generic spring sound
                    SoundStarter.PlayOneShot(SoundDefOf.TrapSpring, new TargetInfo(Position, Map));
                }
                return;
            }

            // Skip the trap logic entirely if disabled and linked
            if (!isTrapEnabled) return;

            // Validate linked objects before proceeding
            LinkableComp?.ValidateLinkedObjects();

            // Play the trap spring sound
            SoundStarter.PlayOneShot(SoundDefOf.TrapSpring, new TargetInfo(Position, Map));

            if (p == null || p.Map == null)
            {
                return;
            }

            // Trigger the CustomBlastEMPEffect at the trap's position
            TriggerEffect(Position);

            // Ensure there are linked objects
            if (LinkableComp != null && LinkableComp.LinkedThings.Any())
            {
                // Find the first valid linked object that is not this trap
                var target = LinkableComp.LinkedThings.FirstOrDefault(t => t != this && t != null && !t.Destroyed && t.Position.IsValid);
                if (target != null && target.Position.Walkable(p.Map))
                {
                    // Trigger the CustomBlastEMPEffect at the linked building's position
                    TriggerEffect(target.Position);

                    // Safely teleport the pawn
                    p.Position = target.Position;
                    p.Notify_Teleported();
                    Messages.Message($"{p.LabelShort} has been teleported to {target.LabelShort}.", MessageTypeDefOf.PositiveEvent, false);
                    return; // Exit the method after successful teleportation
                }
            }

            // Notify that teleportation failed due to no linked object
            Messages.Message("", MessageTypeDefOf.RejectInput, false);
        }


        /// <summary>
        /// Triggers the CustomBlastEMPEffect at the given position.
        /// </summary>
        /// <param name="position">The position where the effect should play.</param>
        private void TriggerEffect(IntVec3 position)
        {
            if (Map == null || position == IntVec3.Invalid)
            {
                return;
            }

            // Create an effecter using the defined EffecterDef
            Effecter effecter = EffecterDefOf.CustomBlastEMPEffect.Spawn();
            effecter.Trigger(new TargetInfo(position, Map), null);
            effecter.Cleanup();
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            // Add the toggle command for enabling/disabling the trap
            yield return new Command_Toggle
            {
                defaultLabel = "Teleportation mode",
                defaultDesc = "<color=red>Toggle on to allow teleporting enemies to and from a linked trap, and toggle off to allow only receiving teleported enemies</color>. Enabled + Linked: Teleports enemies to its linked trap and receives from it; both traps are consumed. Disabled + Linked: Only receives teleported enemies; won't trigger on contact; both traps are consumed.Unlinked: Functions as a single-use spike trap with no teleportation.",
                icon = TexCommand.ForbidOff, // Use an appropriate icon
                isActive = () => isTrapEnabled,
                toggleAction = () => isTrapEnabled = !isTrapEnabled
            };
        }

        public override void Tick()
        {
            base.Tick();

            // Validate linked objects to ensure integrity
            LinkableComp?.ValidateLinkedObjects();
        }




    }

    [DefOf]
    public static class EffecterDefOf
    {
        public static EffecterDef CustomBlastEMPEffect;

        static EffecterDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(EffecterDefOf));
        }
    }
}