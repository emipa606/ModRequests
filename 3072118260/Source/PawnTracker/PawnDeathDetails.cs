using System.Collections.Generic;
using Verse;

namespace PawnTrackerMain
{
    public class PawnDeathDetails : IExposable
    { 
        public DamageInfo damageInfo;
        public DamageDef damageDef;
        public string damageType;
        public Thing instigator;
        public string bodyPart;
        public Hediff exactCulprit;
        //public Pawn_EquipmentTracker instigatorEquipment;
        public string combatLogText;
        public HediffDef hediffDef;
        public ThingDef hediffSource;
        
        //public ThingDef weapon;

        public static PawnDeathDetails ExtractDeathDetails(DamageInfo? dinfo, Hediff exactCulprit, bool spawned)
        {
            var details = new PawnDeathDetails();
            DamageInfo damageInfo;
            if (dinfo != null && dinfo.HasValue)
            {
                damageInfo = dinfo.Value;
                
                details.damageDef = damageInfo.Def;
                details.damageInfo = damageInfo;
                details.damageType = damageInfo.Def != null ? damageInfo.Def.defName : null;
                //details.weapon = damageInfo.Weapon;
                details.instigator = damageInfo.Instigator ?? null;

                if (exactCulprit != null)
                {
                    details.combatLogText = exactCulprit.combatLogText ?? null;
                    details.hediffDef = exactCulprit.def ?? null;
                    details.hediffSource = exactCulprit.source ?? null;
                    if (exactCulprit.Part != null)
                        details.bodyPart = exactCulprit.Part.Label;
                }                
            }

            if (exactCulprit != null)
                details.exactCulprit = exactCulprit;
            return details;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref this.damageDef, "damageDef");
            Scribe_Values.Look(ref this.damageType, "damageType");
            //Scribe_Defs.Look(ref this.weapon, "weapon");
            Scribe_References.Look(ref this.instigator, "instigator");
            //Scribe_Deep.Look<Pawn_EquipmentTracker>(ref this.instigatorEquipment, "InstigatorEquipment", (object) this);
            Scribe_Values.Look(ref this.bodyPart, "bodyPart");
            Scribe_Values.Look(ref this.combatLogText, "combatLogText");
            Scribe_References.Look(ref this.exactCulprit, "exactCulprit");
            Scribe_Defs.Look<ThingDef>(ref this.hediffSource, "source");
            Scribe_Defs.Look<HediffDef>(ref this.hediffDef, "def");
        }
    }
}