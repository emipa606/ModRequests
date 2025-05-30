﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace HediffResourceFramework
{

	public class HediffCompProperties_AdjustHediffsPerStages : HediffCompProperties
	{
        public List<List<HediffOption>> resourceSettingsPerStages;

        public string disablePostUse;
        public HediffCompProperties_AdjustHediffsPerStages()
		{
			compClass = typeof(HediffComp_AdjustHediffsPerStages);
		}
	}

	public class HediffComp_AdjustHediffsPerStages : HediffComp, IAdjustResource
	{
        public HediffCompProperties_AdjustHediffsPerStages Props => (HediffCompProperties_AdjustHediffsPerStages)this.props;
        public Thing Parent => this.Pawn;
        public List<HediffOption> ResourceSettings
        {
            get
            {
                var stageInd = this.parent.CurStageIndex;
                if (Props.resourceSettingsPerStages.Count > stageInd)
                {
                    return Props.resourceSettingsPerStages[stageInd];
                }
                return null;
            }
        }
        public string DisablePostUse => Props.disablePostUse;

        private Dictionary<HediffResource, HediffResouceDisable> postUseDelayTicks;
        public Dictionary<HediffResource, HediffResouceDisable> PostUseDelayTicks
        {
            get
            {
                if (postUseDelayTicks is null)
                {
                    postUseDelayTicks = new Dictionary<HediffResource, HediffResouceDisable>();
                }
                return postUseDelayTicks;
            }
        }
        public void Register()
        {
            var gameComp = HediffResourceUtils.HediffResourceManager;
            gameComp.RegisterAdjuster(this);
        }

        public void Deregister()
        {
            var gameComp = HediffResourceUtils.HediffResourceManager;
            gameComp.RegisterAdjuster(this);
        }
        public bool TryGetQuality(out QualityCategory qc)
        {
            qc = QualityCategory.Normal;
            return false;
        }

        public void Drop()
        {
            this.Pawn.health.RemoveHediff(this.parent);
            Notify_Removed();
        }
        public void Notify_Removed()
        {
            Deregister();
            if (this.Pawn != null)
            {
                HediffResourceUtils.RemoveExcessHediffResources(this.Pawn, this);
            }
        }

        public override void CompPostPostRemoved()
        {
            this.Notify_Removed();
            base.CompPostPostRemoved();
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            Register();
        }
        public void ResourceTick()
        {
            var pawn = this.Pawn;
            if (pawn != null)
            {
                    var resourceSetting = ResourceSettings;
                if (resourceSetting != null)
                {
                    foreach (var hediffOption in resourceSetting)
                    {
                        var hediffResource = pawn.health.hediffSet.GetFirstHediffOfDef(hediffOption.hediff) as HediffResource;
                        if (hediffResource != null && (PostUseDelayTicks.TryGetValue(hediffResource, out var disable) && (disable.delayTicks > Find.TickManager.TicksGame) 
                            || !hediffResource.CanGainResource))
                        {
                            continue;
                        }
                        else
                        {
                            float num = hediffOption.resourcePerSecond;
                            HediffResourceUtils.AdjustResourceAmount(pawn, hediffOption.hediff, num, hediffOption.addHediffIfMissing, hediffOption.applyToPart);
                        }
                    }
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Collections.Look(ref postUseDelayTicks, "postUseDelayTicks", LookMode.Reference, LookMode.Deep, ref hediffResourceKeys, ref hediffResourceDisablesValues);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                var gameComp = HediffResourceUtils.HediffResourceManager;
                gameComp.RegisterAdjuster(this);
            }
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        private List<HediffResource> hediffResourceKeys;
        private List<HediffResouceDisable> hediffResourceDisablesValues;
    }
}