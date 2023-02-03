using System.Collections.Generic;

using Verse;
using RimWorld;

namespace ImprovedInsectoids
{
    public class CompMentalStatePusher : ThingComp
    {
        private int counter;

        private List<Pawn> pawnsAlreadyTried = new List<Pawn>();

        /// <summary>
        /// Returns this ThingComp's CompProperties.
        /// </summary>
        public CompProperties_MentalStatePusher Properties
        {
            get
            {
                return this.props as CompProperties_MentalStatePusher;
            }
        }

        /// <summary>
        /// Tries to apply a mental state to all Pawns within a certain radius.
        /// </summary>
        public override void CompTick()
        {
            base.CompTick();

            if (!ImprovedInsectoidsMod.ModSettings.fearEnabled)
            {
                return;
            }

            CompProperties_MentalStatePusher properties = this.Properties;

            this.counter += 1;
            if (this.counter >= properties.tickInterval)
            {
                this.counter = 0;

                if (!(this.parent is Pawn pawn))
                {
                    return;
                }
                if (pawn.Map is null || pawn.Dead || pawn.Downed)
                {
                    return;
                }

                foreach (Thing thing in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, properties.radius, true))
                {
                    if ((thing == this.parent) || !(thing is Pawn target) || (target.Dead || target.Downed || target.InMentalState || pawnsAlreadyTried.Contains(target)))
                    {
                        continue;
                    }
                    this.pawnsAlreadyTried.Add(target);

                    if (!properties.affectOwnFaction && target.Faction == pawn.Faction)
                    {
                        continue;
                    }

                    if (!Rand.Chance(properties.chance))
                    {
                        continue;
                    }
                    MentalStateDef mentalStateToStart = properties.mentalState;
                    if (target.Faction == Faction.OfPlayer)
                    {
                        if (!ImprovedInsectoidsMod.ModSettings.fearAffectsPlayerFaction)
                        {
                            continue;
                        }
                        if (!(properties.exceptionForPlayerFaction is null))
                        {
                            mentalStateToStart = properties.exceptionForPlayerFaction;
                        }
                    }
                    target.mindState.mentalStateHandler.TryStartMentalState(mentalStateToStart, null, false, false, null, false, false, false);
                }
            }

            if (this.counter >= 30000)
            {
                this.pawnsAlreadyTried.Clear();
            }
        }
    }
}
