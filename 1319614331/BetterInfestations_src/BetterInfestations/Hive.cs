using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace BetterInfestations
{
    public class Hive : ThingWithComps, IAttackTarget, ILoadReferenceable
    {
        public static readonly string MemoAttackedByEnemy = "HiveAttacked";
        public static readonly string MemoDeSpawned = "HiveDeSpawned";
        public static readonly string MemoDestroyedNonRoofCollapse = "HiveDestroyedNonRoofCollapse";
        public CompCanBeDormant CompDormant => GetComp<CompCanBeDormant>();
        Thing IAttackTarget.Thing => this;
        public float TargetPriorityFactor => 0.4f;
        public LocalTargetInfo TargetCurrentlyAimingAt => LocalTargetInfo.Invalid;
        public CompSpawnerPawns CompSpawnerPawns => GetComp<CompSpawnerPawns>();

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            if (!Spawned)
            {
                return true;
            }
            CompCanBeDormant comp = GetComp<CompCanBeDormant>();
            return comp != null && !comp.Awake;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (Faction == null)
            {
                SetFaction(Faction.OfInsects);
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (Spawned && !CompDormant.Awake && !Position.Fogged(Map))
            {
                CompDormant.WakeUp();
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            for (int i = 0; i < 4; i++)
            {
                Lord lord = CompSpawnerPawns.Lord[i];
                if (lord != null)
                {
                    lord.ReceiveMemo(MemoDeSpawned);
                }
            }
            base.DeSpawn(mode);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            for (int i = 0; i < 4; i++)
            {
                Lord lord = CompSpawnerPawns.Lord[i];
                if (lord != null)
                {
                    Map.lordManager.RemoveLord(lord);
                }
            }

            bool flag = false;
            List<Thing> list = Map.listerThings.ThingsOfDef(def);
            for (int i = 0; i < list.Count; i++)
            {
                Hive hive;
                if ((hive = list[i] as Hive) != null && hive != this && hive.CompDormant.Awake)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                QuestUtility.SendQuestTargetSignals(questTags, "AllHivesDestroyed");
            }
            base.Destroy(mode);
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (dinfo.Def.ExternalViolenceFor(this) && dinfo.Instigator != null && dinfo.Instigator.Faction != null)
            {
                Lord lord = CompSpawnerPawns.Lord[0];
                if (lord != null)
                {
                    HiveUtility.NotifyAttackedNearbyHives(this, lord, MemoAttackedByEnemy);
                }
            }
            if (dinfo.Def == DamageDefOf.Flame && HitPoints < MaxHitPoints * 0.3f)
            {
                Lord lord = CompSpawnerPawns.Lord[0];
                if (lord != null)
                {
                    HiveUtility.NotifyAttackedNearbyHives(this, lord, MemoAttackedByEnemy);
                }
            }
            base.PostApplyDamage(dinfo, totalDamageDealt);
        }

        public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
        {
            if (Spawned && (!dinfo.HasValue || dinfo.Value.Category != DamageInfo.SourceCategory.Collapse))
            {
                for (int i = 0; i < 4; i++)
                {
                    Lord lord = CompSpawnerPawns.Lord[i];
                    if (lord != null)
                    {
                        lord.ReceiveMemo(MemoDestroyedNonRoofCollapse);
                    }
                }
            }
            base.Kill(dinfo, exactCulprit);
        }

        public override bool PreventPlayerSellingThingsNearby(out string reason)
        {
            List<Pawn> pawns = HiveUtility.AllHivePawns(this);
            if (pawns.Count > 0 && pawns.Any((Pawn p) => !p.Downed))
            {
                reason = def.label;
                return true;
            }
            reason = null;
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode != LoadSaveMode.Saving)
            {
                bool value = false;
                Scribe_Values.Look(ref value, "active", defaultValue: false);
                if (value)
                {
                    CompDormant.WakeUp();
                }
            }
        }
    }
}