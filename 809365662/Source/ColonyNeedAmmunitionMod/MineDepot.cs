using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.Sound;
using UnityEngine;

namespace ColonyNeedAmmunitionMod
{
    public class MineDepot : Building
    {
        private float miningProgress = 0;

        private DepletionOfMine depletion {
            get{
                DepletionOfMine trg = null;
                foreach (Thing mono in this.Position.GetThingList()) {
                    if (mono.def.defName == "DepletionOfMine") {
                        trg = (DepletionOfMine)mono;
                        break;
                    }
                }

                if (trg == null) {
                    trg = (DepletionOfMine)ThingMaker.MakeThing(ThingDef.Named("DepletionOfMine"));
                    GenPlace.TryPlaceThing(trg, this.Position, ThingPlaceMode.Direct);
                }

                if (trg == null) {
                    Log.Error("ColonyNeedAmmunitionMod.MinDepot.depletion.get:Can't place DepletionOfMine directly at " + this.Position);
                }

                return trg;
            }
        }
        
        private float MiningDifficulty {
            get {
                if (1f - depletion.Depletion < Math.Pow(0.5f, 3))
                {
                    return 8.0f;
                }
                else if (1f - depletion.Depletion < Math.Pow(0.5f, 2)) {
                    return 4.0f;
                }
                else if (1f - depletion.Depletion < 0.5f){
                    return 2.0f;
                }

                return 1.0f;
            }
        }    

        
        public override void PreApplyDamage(DamageInfo damage, out bool absorbed)
        {
            base.PreApplyDamage(damage, out absorbed);
            if (absorbed)
            {
                return;
            }
            if (damage.Def.harmsHealth && damage.Def == DamageDefOf.Mining)
            {
                if (damage.Instigator as Pawn != null) {
                    int level = (damage.Instigator as Pawn).skills.GetSkill(SkillDefOf.Mining).level;
                    int val = level / 6;
                    if (level == 20) val = 9;
                    damage.SetAmount(damage.Amount * (1 + val));
                }
                miningProgress += ((float)damage.Amount / (float)this.MaxHitPoints) / this.MiningDifficulty;
                if (1 <= miningProgress) {
                    miningProgress -= 1.0f;
                    GenerateItems();
                    if (damage.Instigator != null && damage.Instigator as Pawn != null) {
                        Pawn worker = damage.Instigator as Pawn;
                        worker.jobs.EndCurrentJob(Verse.AI.JobCondition.Succeeded);
                    }
                }
                absorbed = true;
            }
            else
            {
                absorbed = false;
            }
        }
        private void GenerateItems() {
            CompThingMineDepot prop = this.TryGetComp<CompThingMineDepot>();
            if (prop != null) {
                ThingDef monodef = ThingDef.Named(prop.Props.ProduceDefName);
                Thing mono = ThingMaker.MakeThing(monodef);
                mono.stackCount = prop.Props.ProduceAmount;
                GenPlace.TryPlaceThing(mono, this.Position, ThingPlaceMode.Near);

                this.depletion.addDepletion((float)prop.Props.ProduceAmount/(float)prop.Props.MaxAmount);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<float>(ref this.miningProgress, "miningProgress", 0f, false);
        }

        public override string GetInspectString()
        {
            CompThingMineDepot prop = this.TryGetComp<CompThingMineDepot>();
            StringBuilder builder = new StringBuilder(base.GetInspectString());
            string text = "MiningProgress".Translate() + ": " + this.miningProgress.ToStringPercent();
            builder.AppendLine(text);
            if (prop != null)
            {
                text = "Remaining".Translate() + ": " + (prop.Props.MaxAmount - (int)(depletion.Depletion * prop.Props.MaxAmount)).ToString();
                builder.AppendLine(text);
                text = "MiningDifficulty".Translate() + ": " + this.MiningDifficulty.ToStringPercent();
                builder.AppendLine(text);
            }
            return builder.ToString();
        }

    }
}
