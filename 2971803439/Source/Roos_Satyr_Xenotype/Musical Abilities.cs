using RimWorld;
using Verse;

namespace Roos_Satyr_Xenotype
{

    // ---------------------------- Melodic Base ---------------------------- 
    public abstract class RBSF_CompAbilityEffect_MelodicBase : CompAbilityEffect
    {
        public static int searchRadius;
        public static HediffDef appliedHediffDef;
        public static ThoughtDef appliedThoughtDef;
        public static AbilityDef usedAbilityDef;
        public static bool affectsEnemies;

        //Constructor
        public RBSF_CompAbilityEffect_MelodicBase()
        {
            searchRadius = 6;
        }

        //Ability ticks to apply hediffs to nearby pawns when Melodic ability is warming up.
        public override void CompTick()
        {
            Pawn startPawn = parent.pawn;
            if (!startPawn.IsHashIntervalTick(250))
                return;

            if (!startPawn.abilities?.GetAbility(usedAbilityDef)?.verb?.WarmingUp == true)
                return;

            Hediff playerHediff = HediffMaker.MakeHediff(RBSF_DefOf.RBSF_PlayingTune, startPawn);
            startPawn.health.AddHediff(playerHediff);

            foreach (Pawn pawn in startPawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (pawn == null || !pawn.Position.InHorDistOf(startPawn.Position, searchRadius) || pawn == startPawn || !pawn.RaceProps.Humanlike || (pawn.HostileTo(this.parent.pawn.Faction) && !pawn.IsPrisoner && !affectsEnemies) )
                {
                    continue;
                }

                ApplySong(pawn);
            }
            
            base.CompTick();
        }

        public virtual void ApplySong(Pawn pawn)
        {

            Hediff hediff = HediffMaker.MakeHediff(appliedHediffDef, pawn);
            pawn.health.AddHediff(hediff);
            Log.Message("Pawn: " + pawn.Name + " given hediff " + hediff.Label);
            return;
        }
    }
    // ---------------------------- Melodic Healing ---------------------------- 
    public class RBSF_CompAbilityEffect_MelodicHealing : RBSF_CompAbilityEffect_MelodicBase
    {
        public override void CompTick()
        {
            appliedHediffDef = RBSF_DefOf.RBSF_HeardHealing;
            usedAbilityDef = RBSF_DefOf.RBSF_MelodicHealing;
            affectsEnemies = false;
            base.CompTick();
        }
    }

    public class RBSF_CompProperties_AbilityMelodicHealing : CompProperties_AbilityEffect
    {
        public RBSF_CompProperties_AbilityMelodicHealing()
        {
            this.compClass = typeof(RBSF_CompAbilityEffect_MelodicHealing);
        }
    }


    // ---------------------------- Melodic Warcall ---------------------------- 
    public class RBSF_CompAbilityEffect_MelodicWarcall : RBSF_CompAbilityEffect_MelodicBase
    {
        public override void CompTick()
        {
            appliedHediffDef = RBSF_DefOf.RBSF_HeardWarcall;
            usedAbilityDef = RBSF_DefOf.RBSF_MelodicWarcall;
            affectsEnemies = false;
            base.CompTick();
        }
    }

    public class RBSF_CompProperties_AbilityMelodicWarcall : CompProperties_AbilityEffect
    {
        public RBSF_CompProperties_AbilityMelodicWarcall()
        {
            this.compClass = typeof(RBSF_CompAbilityEffect_MelodicWarcall);
        }
    }

    // ---------------------------- Melodic Lullaby ---------------------------- 
    public class RBSF_CompAbilityEffect_MelodicLullaby : RBSF_CompAbilityEffect_MelodicBase
    {
        public override void CompTick()
        {
            appliedHediffDef = RBSF_DefOf.RBSF_HeardLullaby;
            usedAbilityDef = RBSF_DefOf.RBSF_MelodicLullaby;
            affectsEnemies = true;
            base.CompTick();
        }
    }

    public class RBSF_CompProperties_AbilityMelodicLullaby : CompProperties_AbilityEffect
    {
        public RBSF_CompProperties_AbilityMelodicLullaby()
        {
            this.compClass = typeof(RBSF_CompAbilityEffect_MelodicLullaby);
        }
    }

    // ---------------------------- Melodic Hymn ---------------------------- 

    public class RBSF_CompAbilityEffect_MelodicHymn : RBSF_CompAbilityEffect_MelodicBase
    {
        public override void CompTick()
        {
            appliedHediffDef = RBSF_DefOf.RBSF_HeardHymn;
            usedAbilityDef = RBSF_DefOf.RBSF_MelodicHymn;
            affectsEnemies = false;
            base.CompTick();
        }
    }

    public class RBSF_CompProperties_AbilityMelodicHymn : CompProperties_AbilityEffect
    {
        public RBSF_CompProperties_AbilityMelodicHymn()
        {
            this.compClass = typeof(RBSF_CompAbilityEffect_MelodicHymn);
        }
    }

    // ---------------------------- Melodic Sonata ---------------------------- 
    public class RBSF_CompAbilityEffect_MelodicSonata : RBSF_CompAbilityEffect_MelodicBase
    {

        public override void ApplySong(Pawn pawn)
        {
            pawn.needs.mood.thoughts.memories.TryGainMemoryFast(appliedThoughtDef);

        }

        public override void CompTick()
        {
            appliedThoughtDef = RBSF_DefOf.RBSF_HeardSonata;
            usedAbilityDef = RBSF_DefOf.RBSF_MelodicSonata;
            affectsEnemies = false;
            base.CompTick();
        }

    }

    public class RBSF_CompProperties_AbilityMelodicSonata : CompProperties_AbilityEffect
    {
        public RBSF_CompProperties_AbilityMelodicSonata()
        {
            this.compClass = typeof(RBSF_CompAbilityEffect_MelodicSonata);
        }
    }


}