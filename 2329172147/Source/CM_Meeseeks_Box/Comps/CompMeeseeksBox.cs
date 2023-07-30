using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public class CompMeeseeksBox : ThingComp
    {
        public CompProperties_MeeseeksBox Props => props as CompProperties_MeeseeksBox;

        public int cooldownTicksRemaining = 0;

        public int cooldownTicksTotal = 0;

        private Effecter progressBar = null;

        public bool Coolingdown => cooldownTicksRemaining > 0;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            QualityCategory boxQuality = QualityCategory.Normal;
            parent.TryGetQuality(out boxQuality);
            int qualityInt = (int)boxQuality;

            float cooldownMultiplier = ((float)(qualityInt + 1)) / ((int)QualityCategory.Legendary + 1);

            cooldownTicksTotal = (int)(Props.cooldownTicksBase * cooldownMultiplier * cooldownMultiplier);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref cooldownTicksRemaining, "cooldownTicksRemaining", 0);
        }


        public override void CompTick()
        {
            base.CompTick();

            if (Coolingdown)
                cooldownTicksRemaining -= 1;

            if (!Coolingdown)
            {
                if (progressBar != null)
                {
                    progressBar.Cleanup();
                    progressBar = null;
                }
            }
            else
            {
                if (progressBar == null)
                {
                    EffecterDef progressBarDef = MeeseeksDefOf.CM_Meeseeks_Box_Effecter_Progress_Bar;
                    progressBar = progressBarDef.Spawn();
                }
                else
                {
                    progressBar.EffectTick(this.parent, TargetInfo.Invalid);

                    MoteProgressBar_Colored mote = ((SubEffecter_ProgressBar_Colored)progressBar.children[0]).mote;
                    if (mote != null)
                    {
                        mote.SetFilledColor(new Color(0.95f, 0.10f, 0.15f));
                        mote.progress = Mathf.Clamp01(((float)cooldownTicksRemaining / cooldownTicksTotal));
                        mote.offsetZ = -0.5f;
                    }
                }
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);

            if (Coolingdown)
                return;

            if (signal.StartsWith("CM_Meeseeks_Box_Button_Presser:"))
            {
                Logger.MessageFormat(this, "Got pressed");
                int colon = signal.IndexOf(":") + 1;
                bool validPresserSignal = (colon >= 0 && colon < signal.Length);
                if (validPresserSignal)
                {
                    string presserID = signal.Substring(colon);

                    Pawn presser = parent.MapHeld.mapPawns.AllPawns.Where(pawn => pawn.ThingID == presserID).FirstOrDefault();

                    if (presser != null)
                    {
                        Logger.MessageFormat(this, "Make Meeseeks");
                        SpawnMeeseeks(presser);
                    }
                }
            }
        }

        private void SpawnMeeseeks(Pawn creator)
        {
            if (!Prefs.DevMode || !MeeseeksMod.settings.screenShotDebug)
                cooldownTicksRemaining = cooldownTicksTotal;

            MeeseeksUtility.SpawnMeeseeks(creator, parent, creator.MapHeld);
        }
    }
}
