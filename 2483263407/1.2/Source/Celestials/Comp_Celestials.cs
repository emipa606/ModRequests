using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using RimWorld;
using Verse;

namespace Celestials
{
    public class Comp_Celestials : ThingComp
    {
        public bool wasDeadLastTick = false;

        public bool alreadyChecked = false;

        public bool isCelestial = false;

        public int resTick = 0;

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref wasDeadLastTick, "wasDeadLastTick", false);
            Scribe_Values.Look(ref alreadyChecked, "alreadyChecked", false);
            Scribe_Values.Look(ref isCelestial, "isCelestial", false);
            Scribe_Values.Look(ref resTick, "resTick", 0);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            // Legacy preservation code, should prevent a total refresh.
            if (respawningAfterLoad)
            {
                alreadyChecked = true;
            }

            // Check if not already done.
            if (!alreadyChecked)
            {
                isCelestial = ViableCandidate() && Rand.Chance(CelestialsMod.settings.celestialChance);

                if((parent as Pawn).health.hediffSet.HasHediff(CelestialsDefOf.O21_CelestialHediff))
                {
                    isCelestial = true;
                }

                AddCelestialHediff();

                alreadyChecked = true;
            }

            // Blacklist Backwards Compatibility
            if (!ViableCandidate())
            {
                if ((parent as Pawn).health.hediffSet.HasHediff(CelestialsDefOf.O21_CelestialHediff))
                {
                    RemoveCelestialHediff();
                }

                isCelestial = false;
            }
        }

        public void AddCelestialHediff()
        {
            if (isCelestial)
            {
                Hediff hediff = HediffMaker.MakeHediff(CelestialsDefOf.O21_CelestialHediff, parent as Pawn);
                (parent as Pawn).health.AddHediff(hediff);
            }
            else
            {
                Log.Warning("Attempted to add Celestial hediff to a pawn who is not a Celestial.");
            }
        }

        public void RemoveCelestialHediff()
        {
            if ((parent as Pawn).health.hediffSet.HasHediff(CelestialsDefOf.O21_CelestialHediff))
            {
                Hediff hediff = (parent as Pawn).health.hediffSet.hediffs.Find(h => h.def == CelestialsDefOf.O21_CelestialHediff);
                (parent as Pawn).health.RemoveHediff(hediff);
            }
            else
            {
                Log.Warning("Attempted to remove Celestial hediff from a pawn who already does not have it.");
            }
        }

        public bool ViableCandidate()
        {
            Pawn pawn = parent as Pawn;
            if(pawn.RaceProps.FleshType == FleshTypeDefOf.Mechanoid
            || pawn.RaceProps.FleshType.defName == "Android"
            || pawn.RaceProps.FleshType.defName == "Artificial")
            {
                return false;
            }
            return true;
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (!isCelestial)
            {
                return;
            }

            Pawn pawn = parent as Pawn;

            if (pawn.Dead)
            {
                if (!wasDeadLastTick)
                {
                    wasDeadLastTick = true;
                    ResetResTicks();
                }

                if(Current.Game.tickManager.TicksGame >= resTick)
                {
                    try
                    {
                        if (!pawn.Corpse.Spawned)
                        {
                            if (pawn.Corpse.StoringThing() != null)
                            {
                                GenSpawn.Spawn(pawn.Corpse, pawn.PositionHeld, pawn.MapHeld);
                            }
                        }

                        if (CelestialsMod.settings.resurrectionSideEffects)
                        {
                            ResurrectionUtility.ResurrectWithSideEffects(pawn);
                        }
                        else
                        {
                            ResurrectionUtility.Resurrect(pawn);
                        }
                    }
                    catch { }
                    if (CelestialsMod.settings.empExplosion)
                    {
                        GenExplosion.DoExplosion(parent.Position, parent.Map, 8.9f, DamageDefOf.EMP, pawn);
                    }
                    if (pawn.Faction.IsPlayer)
                    {
                        Messages.Message(pawn.Name + " has resurrected.", MessageTypeDefOf.PositiveEvent, false);
                    }
                }
            }
            else
            {
                wasDeadLastTick = false;
            }
        }

        public void ResetResTicks()
        {
            resTick = Current.Game.tickManager.TicksGame + CelestialsMod.settings.resurrectionTicks.RandomInRange;
        }
    }
}
