using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace VVO_Obliterator
{
    public class Projectile_ObliteratorBullet : Bullet
    {
        public ModExtension_ObliteratorBullet Props => base.def.GetModExtension<ModExtension_ObliteratorBullet>();
        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            if (Props != null && hitThing != null && hitThing is Pawn hitPawn) 
            {
                float randomPercent = Rand.Value * 100;
                bool enableAlert = LoadedModManager.GetMod<ObliteratorMod>().GetSettings<ObliteratorSettings>().enableAlert;
                float destroyBodyPartChance = LoadedModManager.GetMod<ObliteratorMod>().GetSettings<ObliteratorSettings>().destroyBodyPartChance;

                // See if we apply the hediff 
                if (randomPercent <= destroyBodyPartChance)
                {
                    // Get list of available body parts
                    IEnumerable<BodyPartRecord> parts = hitPawn.health?.hediffSet?.GetNotMissingParts();
                    List<BodyPartRecord> partsList = parts.ToList();

                    // Get a random index
                    Random rnd = new Random();
                    int randomNumber = rnd.Next(0, parts.Count());   

                    // Destroy a body part of a given pawn
                    DestroyPart(hitPawn, partsList[randomNumber]);
                    if (enableAlert)
                    {
                        Messages.Message("VVO_Obliterator_SuccessMessage".Translate(
                            this.launcher.Label, partsList[randomNumber].Label, hitPawn.Label), MessageTypeDefOf.NeutralEvent);
                    }
                }
            }
        }

        public virtual void DestroyPart(Pawn pawn, BodyPartRecord part)
        {
            pawn.TakeDamage(new DamageInfo(DamageDefOf.Bullet, 99999f, 999f, -1f, null, part, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true));
        }
    }
}
