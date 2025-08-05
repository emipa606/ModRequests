using System.Collections.Generic;
using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;
using System;
using static HarmonyLib.Code;
using System.Drawing;
using CombatExtended;
using Verse.Grammar;
using static System.Net.Mime.MediaTypeNames;

namespace BDsPlasmaWeapon
{
    public class LizionHeatShield : Apparel
    {
        private Vector3 impactAngleVect;

        private float efficiency => this.GetStatValue(StatDefOf.BDP_LizionHeatShieldEfficiency);

        private float hiccupChance => this.GetStatValue(StatDefOf.BDP_LizionHeatShieldHiccupChance);

        private int tickNextCheck = 0;

        protected CompReloadableFromFiller compReloadableFromFiller
        {
            get
            {
                return GetComp<CompReloadableFromFiller>();
            }
        }

        protected DefModExtension_LizionDeflector compGizmo
        {
            get
            {
                return def.GetModExtension<DefModExtension_LizionDeflector>();
            }
        }

        public FloatRange hiccupDamageMultiplierRange = new FloatRange(0, 0.5f);

        private bool currentMode;

        public override void PostPostMake()
        {
            base.PostPostMake();
            currentMode = true;
            if (compReloadableFromFiller == null)
            {
                Log.Warning("LizionHeatShield is meant to be used in conjunction with CompReloadableFromFiller!");
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                currentMode = true;
            }
            if (compReloadableFromFiller == null)
            {
                Log.Warning("LizionHeatShield is meant to be used in conjunction with CompReloadableFromFiller!");
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentMode, "currentMode", true);
            Scribe_Values.Look(ref tickNextCheck, "tickNextCheck", 0);
        }

        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            foreach (Gizmo wornGizmo in base.GetWornGizmos())
            {
                yield return wornGizmo;
            }
            if (Wearer == null)
            {
                yield break;
            }
            if (compReloadableFromFiller == null)
            {
                yield break;
            }
            if (compGizmo == null)
            {
                yield break;
            }
            if (Wearer.Faction.Equals(Faction.OfPlayer))
            {
                string commandIcon = currentMode ? compGizmo.onIcon : compGizmo.offIcon;

                if (commandIcon == "")
                {
                    commandIcon = "UI/Buttons/Reload";
                }

                Command_Action switchSecondaryLauncher = new Command_Action
                {
                    action = new Action(toggle),
                    defaultLabel = (currentMode ? compGizmo.onLabel : compGizmo.offLabel).Translate(),
                    defaultDesc = compGizmo.description.Translate(),
                    icon = ContentFinder<Texture2D>.Get(commandIcon, false),
                };
                yield return switchSecondaryLauncher;
            }
        }

        public void toggle()
        {
            currentMode = !currentMode;
        }

        public override void Tick()
        {
            base.Tick();

            int tick = Find.TickManager.TicksGame;

            if (tick > tickNextCheck && Wearer != null)
            {
                tickNextCheck = tick + 240;
                Fire fire = (Fire)Wearer.GetAttachment(RimWorld.ThingDefOf.Fire);
                if (fire != null && currentMode && compReloadableFromFiller != null)
                {
                    compReloadableFromFiller.DrawGas(Math.Max(fire.fireSize / efficiency, 1));
                    fire.Destroy();
                    String text = "BDP_FireExtinguished".Translate();
                    MoteMakerCE.ThrowText(Wearer.Position.ToVector3Shifted(), Wearer.Map, text);
                }
                Hediff heatstroke = Wearer.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Heatstroke);

                if (heatstroke != null)
                {
                    float estimatedGasConsumption = heatstroke.Severity * compGizmo.heatstrokeMitigationConstant * efficiency;
                    if (estimatedGasConsumption > 1)
                    {
                        compReloadableFromFiller.DrawGas(estimatedGasConsumption);
                        Wearer.health.RemoveHediff(heatstroke);
                        String text = "BDP_heatstrokeMitigated".Translate();
                        MoteMakerCE.ThrowText(Wearer.Position.ToVector3Shifted(), Wearer.Map, text);
                    }
                    else if (heatstroke.Severity > 0.2)
                    {
                        compReloadableFromFiller.DrawGas(Math.Max(estimatedGasConsumption, 1));
                        Wearer.health.RemoveHediff(heatstroke);
                        String text = "BDP_heatstrokeMitigated".Translate();
                        MoteMakerCE.ThrowText(Wearer.Position.ToVector3Shifted(), Wearer.Map, text);
                    }
                }
            }
        }

        public override void TickRare()
        {
            base.TickRare();

            int tick = Find.TickManager.TicksGame;

            if (tick > tickNextCheck && Wearer != null)
            {
                tickNextCheck = tick + 240;
                Fire fire = (Fire)Wearer.GetAttachment(RimWorld.ThingDefOf.Fire);
                if (fire != null && currentMode && compReloadableFromFiller != null)
                {
                    compReloadableFromFiller.DrawGas(Math.Max(fire.fireSize / efficiency, 1));
                    fire.Destroy();
                    String text = "BDP_FireExtinguished".Translate();
                    MoteMakerCE.ThrowText(Wearer.Position.ToVector3Shifted(), Wearer.Map, text);
                }
                Hediff heatstroke = Wearer.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Heatstroke);

                if (heatstroke != null)
                {
                    float estimatedGasConsumption = heatstroke.Severity * compGizmo.heatstrokeMitigationConstant * efficiency;
                    if (estimatedGasConsumption > 1)
                    {
                        compReloadableFromFiller.DrawGas(estimatedGasConsumption);
                        Wearer.health.RemoveHediff(heatstroke);
                        String text = "BDP_heatstrokeMitigated".Translate();
                        MoteMakerCE.ThrowText(Wearer.Position.ToVector3Shifted(), Wearer.Map, text);
                    }
                    else if (heatstroke.Severity > 0.2)
                    {
                        compReloadableFromFiller.DrawGas(Math.Max(estimatedGasConsumption, 1));
                        Wearer.health.RemoveHediff(heatstroke);
                        String text = "BDP_heatstrokeMitigated".Translate();
                        MoteMakerCE.ThrowText(Wearer.Position.ToVector3Shifted(), Wearer.Map, text);
                    }
                }
            }
        }

        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            Log.Message(dinfo.ToString());
            if (dinfo.Def.armorCategory != null)
            {
                Log.Message(dinfo.Def.armorCategory.ToString());
            }

            if (currentMode && dinfo.Def.armorCategory == DamageArmorCategoryDefOf.Heat && compReloadableFromFiller != null && dinfo.Def != DamageDefOf.LizionExplosion)
            {
                float damageCache = dinfo.Amount;
                float equivalentDamageAbsorbtion = compReloadableFromFiller.remainingCharges * efficiency;
                if (equivalentDamageAbsorbtion > 0)
                {
                    if (equivalentDamageAbsorbtion >= damageCache)
                    {
                        compReloadableFromFiller.DrawGas(Math.Max(damageCache / efficiency, 1));
                        if (hiccupChance >= 1 || (hiccupChance > 0 && Rand.Chance(hiccupChance)))
                        {
                            dinfo.SetAmount(damageCache * hiccupDamageMultiplierRange.RandomInRange);
                            MoteMakerCE.ThrowText(Wearer.Position.ToVector3Shifted(), Wearer.Map, "BDP_hiccupOccured".Translate(damageCache - dinfo.Amount), damageCache);
                            return false;
                        }
                        MoteMakerCE.ThrowText(Wearer.Position.ToVector3Shifted(), Wearer.Map, "BDP_damageMitigatedFully".Translate(damageCache));
                        return true;

                    }
                    else
                    {
                        dinfo.SetAmount(damageCache - (compReloadableFromFiller.remainingCharges * efficiency));
                        MoteMakerCE.ThrowText(Wearer.Position.ToVector3Shifted(), Wearer.Map, "BDP_partialDamageMitigation".Translate(damageCache - dinfo.Amount), damageCache);
                        compReloadableFromFiller.Empty();
                    }
                }
            }
            return false;
        }
    }
    public class DefModExtension_LizionDeflector : DefModExtension
    {
        public string onIcon = "UI/Commands/DesirePower";
        public string onLabel = "BDP_ShieldOn";
        public string offIcon = "UI/Commands/DesirePower";
        public string offLabel = "BDP_ShieldOff";
        public float heatstrokeMitigationConstant = 1;
        public string description = "BDP_ShieldDesc";
    }
}
