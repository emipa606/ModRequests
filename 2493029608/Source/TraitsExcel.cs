using System;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TraitsExcel
{
    [DefOf]
    public static class TraitDefOf
    {
        public static TraitDef Archer;
        public static TraitDef DemoExpert;
        public static TraitDef anxietyBomb;

        static TraitDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TraitDefOf));
        }
    }
    [DefOf]

    public static class InspirationDefOf
    {
        public static InspirationDef Frenzy_Work;

        static InspirationDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(InspirationDefOf));
        }
    }
    public class ThoughtWorker_IsCarryingMeleeWeapon : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return p.equipment.Primary != null && p.equipment.Primary.def.IsRangedWeapon != true;
        }
    }
    public class ThoughtWorker_IsCarryingNonExplosive : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return p.equipment.Primary != null && p.equipment.Primary.def.defName != "Weapon_GrenadeFrag" && p.equipment.Primary.def.defName != "Weapon_GrenadeMolotov" && p.equipment.Primary.def.defName != "Weapon_GrenadeEMP" && p.equipment.Primary.def.defName != "Gun_IncendiaryLauncher" && p.equipment.Primary.def.defName != "Gun_SmokeLauncher" && p.equipment.Primary.def.defName != "Gun_EmpLauncher" && p.equipment.Primary.def.defName != "Gun_TripleRocket" && p.equipment.Primary.def.defName != "Gun_DoomsdayRocket";
        }
    }
    public class ThoughtWorker_IsCarryingExplosive : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return p.equipment.Primary != null && p.equipment.Primary.def.defName == "Weapon_GrenadeFrag" | p.equipment.Primary.def.defName == "Weapon_GrenadeMolotov" | p.equipment.Primary.def.defName == "Weapon_GrenadeEMP" | p.equipment.Primary.def.defName == "Gun_IncendiaryLauncher" | p.equipment.Primary.def.defName == "Gun_SmokeLauncher" | p.equipment.Primary.def.defName == "Gun_EmpLauncher" | p.equipment.Primary.def.defName == "Gun_TripleRocket" | p.equipment.Primary.def.defName == "Gun_DoomsdayRocket";
        }
    }
    public class Alert_ArcherHasMeleeWeapon : Alert
    {
        public List<Pawn> ArchersWithMeleeWeapons
        {
            get
            {
                archersWithMeleeWeaponResult.Clear();
                foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsSpawned)
                {
                    if (pawn.story.traits.HasTrait(TraitDefOf.Archer) && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon != true)
                    {
                        archersWithMeleeWeaponResult.Add(pawn);
                    }
                }
                return archersWithMeleeWeaponResult;
            }
        }

        public Alert_ArcherHasMeleeWeapon()
        {
            this.defaultLabel = "ArcherHasMeleeWeapon".Translate();
            this.defaultExplanation = "ArcherHasMeleeWeaponDesc".Translate();
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(ArchersWithMeleeWeapons);
        }

        public List<Pawn> archersWithMeleeWeaponResult = new List<Pawn>();
    }
    public class Alert_DemoExpertHasNonExplosive : Alert
    {
        public List<Pawn> DemoExpertsWithNonExplosiveWeapons
        {
            get
            {
                demoExpertsWithNonExplosiveWeaponResult.Clear();
                foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsSpawned)
                {
                    if (pawn.story.traits.HasTrait(TraitDefOf.DemoExpert) && pawn.equipment.Primary != null && pawn.equipment.Primary.def.defName != "Weapon_GrenadeFrag" && pawn.equipment.Primary.def.defName != "Weapon_GrenadeMolotov" && pawn.equipment.Primary.def.defName != "Weapon_GrenadeEMP" && pawn.equipment.Primary.def.defName != "Gun_IncendiaryLauncher" && pawn.equipment.Primary.def.defName != "Gun_SmokeLauncher" && pawn.equipment.Primary.def.defName != "Gun_EmpLauncher" && pawn.equipment.Primary.def.defName != "Gun_TripleRocket" && pawn.equipment.Primary.def.defName != "Gun_DoomsdayRocket")
                    {
                        demoExpertsWithNonExplosiveWeaponResult.Add(pawn);
                    }
                }
                return demoExpertsWithNonExplosiveWeaponResult;
            }
        }

        public Alert_DemoExpertHasNonExplosive()
        {
            this.defaultLabel = "DemoExpertHasNonExplosive".Translate();
            this.defaultExplanation = "DemoExpertHasNonExplosiveDesc".Translate();
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(DemoExpertsWithNonExplosiveWeapons);
        }

        public List<Pawn> demoExpertsWithNonExplosiveWeaponResult = new List<Pawn>();
    }
    public class Alert_EkrixiphobiaHasExplosive : Alert
    {
        public List<Pawn> EkrixipobiaWithExplosives
        {
            get
            {
                EkrixiphobiaWithExplosiveResult.Clear();
                foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsSpawned)
                {
                    if (pawn.story.traits.HasTrait(TraitDefOf.anxietyBomb) && pawn.equipment.Primary != null && pawn.equipment.Primary.def.defName == "Weapon_GrenadeFrag" | pawn.equipment.Primary.def.defName == "Weapon_GrenadeMolotov" | pawn.equipment.Primary.def.defName == "Weapon_GrenadeEMP" | pawn.equipment.Primary.def.defName == "Gun_IncendiaryLauncher" | pawn.equipment.Primary.def.defName == "Gun_SmokeLauncher" | pawn.equipment.Primary.def.defName == "Gun_EmpLauncher" | pawn.equipment.Primary.def.defName == "Gun_TripleRocket" | pawn.equipment.Primary.def.defName == "Gun_DoomsdayRocket")
                    {
                        EkrixiphobiaWithExplosiveResult.Add(pawn);
                    }
                }
                return EkrixiphobiaWithExplosiveResult;
            }
        }

        public Alert_EkrixiphobiaHasExplosive()
        {
            this.defaultLabel = "EkrixiphobiaHasExplosive".Translate();
            this.defaultExplanation = "EkrixiphobiaHasExplosiveDesc".Translate();
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(EkrixipobiaWithExplosives);
        }

        public List<Pawn> EkrixiphobiaWithExplosiveResult = new List<Pawn>();
    }
}