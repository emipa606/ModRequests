using System;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace LobotomyCorp
{
    public abstract class Comp_TickWhileHeld : ThingComp
    {

        public abstract void CompTick_Held();

    }

    public static class LCThingCompReferences
    {

        public static List<Comp_TickWhileHeld> activeLCThingComps = new List<Comp_TickWhileHeld>();

    }


    public class Comp_Sanguine_Desire_Weapon : ThingComp
    {
        public override void Notify_UsedWeapon(Pawn pawn)
        {
            Log.Message("Used Weapon");
            if (pawn.skills.GetSkill(SkillDefOf.Social).levelInt <= 8) // If social skill <= 8. Using social until we get temperence in
            {
                //dinfo.SetAmount(dinfo.Amount + 11);
                DamageInfo selfDamage = new DamageInfo();
                selfDamage.SetAmount(10);
                DamageWorker_LCWhite.applyWhitePostResist(selfDamage, pawn);
                Log.Message("Ouch!");
            }
        }
    }
    public class Comp_Solemn_Vow : ThingComp
    {
        public bool whiteAttack = true;
        public override void Notify_UsedWeapon(Pawn pawn)
        {
            whiteAttack = !whiteAttack;
        }
    }

    public class Comp_MeleeMultiHit : Comp_TickWhileHeld
    {
        public CompProperties_MeleeMultiHit Props => (CompProperties_MeleeMultiHit)this.props;

        private bool isBursting = false;
        private int burstsLeft = 0;

        private int hitCount => Props.hitCount;
        private int ticksBetweenBurstHits => (int)(60 * Props.secondsBetweenBurstHits);
        private int ticksBetweenBursts => (int)(60 * Props.secondsBetweenBursts);
        private SoundDef startBurstSound => Props.startBurstSound;

        private int tick = 0;
        private bool setStanceNow = false;
        private Pawn wielder = null;
        private bool burstOnDowned = false;
        public override void CompTick_Held()
        {
               tick++;
            
            if (setStanceNow && tick >= 2)
            {
                if (burstsLeft > 1)
                {
                    //Log.Message("burstsLeft: " + burstsLeft.ToString());
                    wielder.stances.SetStance(new Stance_Cooldown(ticksBetweenBurstHits, wielder.LastAttackedTarget, null));
                }
                else
                {
                    wielder.stances.SetStance(new Stance_Cooldown(ticksBetweenBursts, wielder.LastAttackedTarget, null));
                    isBursting = false;
                    //Log.Message("Not Bursting");
                }
                burstsLeft--;
                setStanceNow = false;
            }

            if (isBursting && wielder != null) // Burst ending conditions
            {
                if (burstOnDowned == false && wielder.LastAttackedTarget.Pawn != null && wielder.LastAttackedTarget.Pawn.Downed)
                {
                    isBursting = false;
                    burstOnDowned = true;
                    //Log.Message("Target downed. Burst ended.");
                }
                if (wielder.LastAttackedTarget.Pawn != null && wielder.LastAttackedTarget.Pawn.Dead)
                {
                    isBursting = false;
                    //Log.Message("Target dead. Burst ended.");
                }
                if (wielder.Downed)
                {
                    isBursting = false;
                    //Log.Message("Wielder downed. Burst ended.");
                }
            }
            if (tick > (ticksBetweenBurstHits + 180))
            {
                tick = 0;
                isBursting = false;
            }
        }

        public override void Notify_UsedWeapon(Pawn pawn)
        {
            wielder = pawn;
            if (pawn.LastAttackedTarget.Pawn != null && !pawn.LastAttackedTarget.Pawn.Downed)
            {
                burstOnDowned = false;
            }
            if (LCThingCompReferences.activeLCThingComps.Contains(this) == false)
            {
                LCThingCompReferences.activeLCThingComps.Add(this);
            }

            if (!isBursting)
            {
                
                isBursting = true;
                //Log.Message("Bursting");
                burstsLeft = hitCount;
                if (startBurstSound != null)
                {
                    startBurstSound.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                    //Log.Message("Playing burst sound");
                }
            }
            tick = 0;
            setStanceNow = true;
        }

    }

    public class CompProperties_MeleeMultiHit : CompProperties
    {
        public int hitCount = 3;
        public float secondsBetweenBurstHits = 0.5f;
        public float secondsBetweenBursts = 2;
        public SoundDef startBurstSound = null;


        public CompProperties_MeleeMultiHit()
        {
            this.compClass = typeof(Comp_MeleeMultiHit);
        }

        public CompProperties_MeleeMultiHit(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }

    }



    // EGO SUITS



    public class Comp_EGOSuitBase : ThingComp
    {

        private float redMultiplier;
        private float whiteMultiplier;
        private float blackMultiplier;
        private float paleMultiplier;

        private int suitGrade;

        private DamageDef whiteDamageDef;

        private DamageDef blackDamageDef;
        private DamageDef blackDamageDefCut;
        private DamageDef blackDamageDefBlunt;
        private DamageDef blackDamageDefStab;
        private DamageDef blackDamageDefBullet;

        private DamageDef paleDamageDef;

        private SoundDef whiteSoundDef;
        private SoundDef blackSoundDef;
        private SoundDef paleSoundDef;

        private Pawn thisPawn;
        public override void Initialize(CompProperties props)
        {
            CompProperties_EGOSuitBase Props = props as CompProperties_EGOSuitBase;
            redMultiplier = Props.redMultiplier;
            whiteMultiplier = Props.whiteMultiplier;
            blackMultiplier = Props.blackMultiplier;
            paleMultiplier = Props.paleMultiplier;
            suitGrade = Props.suitGrade;

            whiteDamageDef = DefDatabase<DamageDef>.AllDefsListForReading.Find(d => d.defName == "WhiteDmg");

            blackDamageDef = DefDatabase<DamageDef>.AllDefsListForReading.Find(d => d.defName == "BlackDmg");
            blackDamageDefCut = DefDatabase<DamageDef>.AllDefsListForReading.Find(d => d.defName == "CutBlack");
            blackDamageDefBlunt = DefDatabase<DamageDef>.AllDefsListForReading.Find(d => d.defName == "BluntBlack");
            blackDamageDefStab = DefDatabase<DamageDef>.AllDefsListForReading.Find(d => d.defName == "StabBlack");
            blackDamageDefBullet = DefDatabase<DamageDef>.AllDefsListForReading.Find(d => d.defName == "BulletBlack");

            paleDamageDef = DefDatabase<DamageDef>.AllDefsListForReading.Find(d => d.defName == "PaleDmg");

            whiteSoundDef = DefDatabase<SoundDef>.AllDefsListForReading.Find(d => d.defName == "White_Weak");
            blackSoundDef = DefDatabase<SoundDef>.AllDefsListForReading.Find(d => d.defName == "Black_Weak");
            paleSoundDef = DefDatabase<SoundDef>.AllDefsListForReading.Find(d => d.defName == "Pale_Weak");
            //Log.Message("Suit Initialized");
        }

        public float redMult
        {
            get
            {
                return this.redMultiplier;
            }
        }

        public float whiteMult
        {
            get
            {
                return this.whiteMultiplier;
            }
        }

        public float blackMult
        {
            get
            {
                return this.blackMultiplier;
            }
        }

        public float paleMult
        {
            get
            {
                return this.paleMultiplier;
            }
        }

        public int grade
        {
            get
            {
                return this.suitGrade;
            }
        }

        public void ApplyEGOResistance(ref DamageInfo dinfo)
        {

            //Log.Message("ApplyEGOResistance called");
            thisPawn = ((Apparel)this.parent).Wearer;
            if (thisPawn == null)
            {
                Log.Message("Pawn is null");
                return;
            }

            //float initialDamage = dinfo.Amount;

            if (dinfo.Def == whiteDamageDef)
            {
                //Log.Message("Is White damage");
                float newWhiteMultiplier = whiteMultiplier;

                if (newWhiteMultiplier < 1)
                {
                    newWhiteMultiplier = Math.Min(newWhiteMultiplier + (dinfo.ArmorPenetrationInt / 2), 1.0f); // Armor penetration reduces reduces Pale resistance by half of its value
                }

                dinfo.SetAmount(dinfo.Amount * newWhiteMultiplier); // Incoming Pale damage multiplied by armor multiplier
            }
            else if (dinfo.Def == blackDamageDef || dinfo.Def == blackDamageDefCut || dinfo.Def == blackDamageDefBlunt || dinfo.Def == blackDamageDefStab || dinfo.Def == blackDamageDefBullet)
            {
                //Log.Message("Is Black damage");
                float newBlackMultiplier = blackMultiplier;

                if (newBlackMultiplier < 1.5)
                {
                    newBlackMultiplier = Math.Min(newBlackMultiplier + (dinfo.ArmorPenetrationInt / 2), 1.5f); // Armor penetration reduces reduces Black resistance by half of its value
                }

                dinfo.SetAmount(dinfo.Amount * newBlackMultiplier); // Incoming Black damage multiplied by armor multiplier
            }
            else if (dinfo.Def == paleDamageDef)
            {
                //Log.Message("Is Pale damage");
                float newPaleMultiplier = paleMultiplier;

                if (newPaleMultiplier < 2)
                {
                    newPaleMultiplier = Math.Min(newPaleMultiplier + (dinfo.ArmorPenetrationInt / 2), 2.0f); // Armor penetration reduces reduces Pale resistance by half of its value
                }

                dinfo.SetAmount(dinfo.Amount * newPaleMultiplier); // Incoming Pale damage multiplied by armor multiplier
            }
            else
            {
                //Log.Message("Is other damage");
            }

            dinfo.SetAmount(dinfo.Amount / (0.9f + (suitGrade / 10.0f))); // Grade multiplier
        }

    }

    public class CompProperties_EGOSuitBase : CompProperties
    {
        public float redMultiplier = 1.0f; // May or may not be used
        public float whiteMultiplier = 1.0f;
        public float blackMultiplier = 1.5f;
        public float paleMultiplier = 2.0f;

        public int suitGrade = 1; // Zayin-Aleph : 1-5


        public CompProperties_EGOSuitBase()
        {
            this.compClass = typeof(Comp_EGOSuitBase);
        }

        public CompProperties_EGOSuitBase(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }

    }



    // Misc



    public class LCCompTickManager : MapComponent
    {

        public LCCompTickManager(Map map) : base(map)
        {
        }

        public override void MapGenerated()
        {
            base.MapGenerated();
        }

        public override void MapComponentTick()
        {
            if (LCThingCompReferences.activeLCThingComps.Count != 0)
            {
                foreach (Comp_TickWhileHeld thingComp in LCThingCompReferences.activeLCThingComps)
                {
                    thingComp.CompTick_Held();
                }
            }
        }

    }

}