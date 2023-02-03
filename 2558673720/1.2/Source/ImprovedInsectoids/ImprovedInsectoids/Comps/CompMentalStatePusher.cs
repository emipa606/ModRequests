using System.Collections.Generic;

using Verse;
using RimWorld;

namespace ImprovedInsectoids
{
    public class CompMentalStatePusher : ThingComp
    {
        public int counter = 0;

        private List<Pawn> pawnsAlreadyTried = new List<Pawn>();

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }

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

            if (!ImprovedInsectoidsMod.modSettings.fearEnabled)
            {
                return;
            }

            CompProperties_MentalStatePusher properties = this.Properties;

            counter += 1;
            if (counter >= properties.tickInterval)
            {
                counter = 0;

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
                    if (thing != this.parent && thing is Pawn target && !(target.Dead || target.Downed || target.InMentalState || pawnsAlreadyTried.Contains(target)))
                    {
                        pawnsAlreadyTried.Add(target);

                        if (!properties.affectOwnFaction && target.Faction == pawn.Faction)
                        {
                            continue;
                        }

                        if (Rand.Chance(properties.chance))
                        {
                            MentalStateDef mentalStateToStart = properties.mentalState;
                            if (!(properties.exceptionForPlayerFaction is null) && target.Faction == Faction.OfPlayer)
                            {
                                mentalStateToStart = properties.exceptionForPlayerFaction;
                            }
                            target.mindState.mentalStateHandler.TryStartMentalState(mentalStateToStart, null, false, false, null, false);
                        }
                    }
                }
            }

            if (counter >= 30000)
            {
                pawnsAlreadyTried.Clear();
            }
        }
    }
}
