using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using RimWorld;
using Unity;
using Verse;

namespace PorousBoat.BloodAndBones
{
    [StaticConstructorOnStartup]
    public class BloodAndBones
    {
        static BloodAndBones()
        {
            Log.Message("BLOOD & BONES: Running...");
        }
    }

    public class CompProperties_BloodlinkWeapon : CompProperties_Biocodable
    {
        public CompProperties_BloodlinkWeapon()
        {
            compClass = typeof(CompBloodlinkWeapon);
        }
    }

    public class RitualAttachableOutcomeEffectWorker_BloodlinkBestowal : RitualAttachableOutcomeEffectWorker
    {
        public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, OutcomeChance outcome, out string extraOutcomeDesc,
            ref LookTargets letterLookTargets)
        {
            ListerThings li = new ListerThings(ListerThingsUse.Region);
            List<Thing> boneWeps = li.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("BaseBoneWeapon"));
            if (boneWeps.Count >= 0)
            {
                Thing chosenWep = boneWeps.RandomElement();
                BestowBloodlink(chosenWep, jobRitual);
            }
            else
            {
                Messages.Message("No suitable weapon found for bestowal", MessageTypeDefOf.NegativeEvent);
            }
            extraOutcomeDesc = def.letterInfoText;
        }

        private void BestowBloodlink(Thing wep, LordJob_Ritual r)
        {
            // Terribly unsafe check for weapon type
            switch (wep.Label)
            {
                case "dagger":
                    GenSpawn.Spawn(DefDatabase<ThingDef>.GetNamed("BoneWeapon_DaggerBloodlink"), r.Spot, r.Map);
                    break;
                case "sword":
                    GenSpawn.Spawn(DefDatabase<ThingDef>.GetNamed("BoneWeapon_SwordBloodlink"), r.Spot, r.Map);
                    break;
                case "scythe":
                    GenSpawn.Spawn(DefDatabase<ThingDef>.GetNamed("BoneWeapon_ScytheBloodlink"), r.Spot, r.Map);
                    break;
                default:
                    break;
            }
            wep.Destroy();
        }
    }
}