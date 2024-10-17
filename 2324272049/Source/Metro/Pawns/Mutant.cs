using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Metro
{
	public class Mutant : Pawn
    {
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            try
            {
                if (dinfo.Instigator != null && this.Map != null && (dinfo.Instigator is Pawn || dinfo.Instigator is Building))
                {
        
                    this.Map.GetComponent<HiveAIManager>().Notify_MutantAttacked(this, dinfo.Instigator);
                }
            }
            catch { }
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                if (this.Faction?.def != this.kindDef.defaultFactionType)
                {
                    var faction = Find.FactionManager.FirstFactionOfDef(this.kindDef.defaultFactionType);
                    if (faction != null)
                    {
                        this.SetFaction(faction);
                    }
                    else
                    {
                        faction = FactionGenerator.NewGeneratedFaction(this.kindDef.defaultFactionType);
                        faction.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile, false, null, null);
                        Find.FactionManager.Add(faction);
                        this.SetFaction(faction);
                    }
                }
            }
        }
    }
}

