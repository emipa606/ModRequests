using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    public class CompProximityExplosive : CompExplosive
    {
        protected bool shouldStartWick = false;
        protected List<Pawn> closingPawns = new List<Pawn>();
        protected WaitCallback Callback_TryFindEnemyInRange;
        protected int selfExplodeTimer = 0;
        protected int tickToExplode = 0;
        protected bool willSelfExplode = false;
        protected int EMPstunTimer = 0;
        protected StunHandler stunner;
        protected Effecter empEffecter;

        protected static readonly List<Thing> cachedMine = new List<Thing>();
        public new CompProperties_ProximityExplosive Props
        {
            get { return (CompProperties_ProximityExplosive)this.props; }
        }
        static CompProximityExplosive()
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(6, 6);
        }

        public CompProximityExplosive()
        {
            Callback_TryFindEnemyInRange = new WaitCallback(TryFindEnemyInRange);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                willSelfExplode = Rand.Chance(this.Props.selfExplodeChance);
                tickToExplode = Rand.Range(this.Props.minSelfExplodeTick, this.Props.maxSelfExplodeTick);
            }
            cachedMine.Add(this.parent);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            cachedMine.Remove(this.parent);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            cachedMine.Remove(this.parent);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref willSelfExplode, "willSelfExplode", false);
            Scribe_Values.Look<int>(ref tickToExplode, "tickToExplode", 0);
            Scribe_Values.Look<int>(ref EMPstunTimer, "EMPstunTimer", 0);
            Scribe_Values.Look<int>(ref selfExplodeTimer, "selfExplodeTimer", 0);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                cachedMine.Clear();
            }
        }

        public static IEnumerable<Thing> GetCachedMine(Map map)
        {
            return from x in cachedMine where x.Map == map select x;
        }

        public override void CompTick()
        {
            if (willSelfExplode)
            {
                if (selfExplodeTimer > tickToExplode)
                {
                    if (Props.selfDestructHarmlessly)
                    {
                        parent.Destroy();
                    }
                    else
                    {
                        shouldStartWick = true;
                    }
                }
                else
                {
                    selfExplodeTimer++;
                }
            }

            if (shouldStartWick)
            {
                StartWick(null);
            }

            base.CompTick();
            if (EMPstunTimer > 0)
            {
                EMPstunTimer--;
                if (empEffecter == null)
                {
                    empEffecter = RimWorld.EffecterDefOf.DisabledByEMP.Spawn();
                }
                empEffecter.EffectTick(parent, parent);
            }
            else
            {
                if (empEffecter != null)
                {
                    empEffecter.Cleanup();
                    empEffecter = null;
                }
                if (this.parent.IsHashIntervalTick(20) && !shouldStartWick)
                {
                    ThreadPool.QueueUserWorkItem(Callback_TryFindEnemyInRange);
                }
            }
        }

        public void CheckSring(Pawn pawn)
        {
            if (Rand.Chance(this.SpringChance(pawn))) shouldStartWick = true;
        }

        public float SpringChance(Pawn pawn)
        {
            if (pawn == null) return 0;
            if (pawn.Faction == null && pawn.def.race.Animal) return this.Props.wildAnimalSpringChance;
            if (pawn.Faction == this.parent.Faction) return Props.colonistSpringChance;
            return 0;
        }

        public void TryDisarming(Pawn actor)
        {
            if (actor == null) return;
            if (this.Props.disarmSuccessRateCurve == null) return;


            float successRate = GetDisarmSuccessRate(actor);
            if (!Rand.Chance(successRate))
            {
                this.shouldStartWick = true;
                return;
            }
            if (this.Props.disarmLeavings != null)
            {
                foreach (var item in this.Props.disarmLeavings)
                {
                    Thing thing = ThingMaker.MakeThing(item.thingDef);
                    thing.stackCount = item.count;
                    GenPlace.TryPlaceThing(thing, this.parent.Position, this.parent.Map, ThingPlaceMode.Near);
                }
            }
            this.parent.Destroy();
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            if (Props.disableByEMP && dinfo.Def == DamageDefOf.EMP)
            {
                EMPstunTimer = Props.EMPstunTick;
            }
            base.PostPreApplyDamage(ref dinfo, out absorbed);
        }

        public float GetDisarmSuccessRate(Pawn actor)
        {
            float capacityManipulation = actor.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation);
            float capacitySight = actor.health.capacities.GetLevel(PawnCapacityDefOf.Sight);
            int skillIntellectual = actor.skills.GetSkill(SkillDefOf.Intellectual).levelInt;
            int skillCrafting = actor.skills.GetSkill(SkillDefOf.Crafting).levelInt;

            float skill = (skillCrafting - 4) * (skillIntellectual - 4) * capacityManipulation * capacitySight;
            float successRate = this.Props.disarmSuccessRateCurve.Evaluate(skill);
            successRate = successRate > 0 ? successRate : 0;
            successRate = successRate > 1 ? 1 : successRate;
            return successRate;
        }

        public virtual void TryFindEnemyInRange(object a)
        {
            try
            {
                if (this.parent?.Map == null)
                {
                    return;
                }

                var pawnInRange = this.parent.Map.mapPawns.AllPawnsSpawned.AsParallel().Where(x => InRange(x.Position, this.Props.detectionRadius));
                StringBuilder stb = new StringBuilder();
                //this.parent.Position.PawnInRange(this.parent.Map, this.Props.detectionRadius)
                stb.Append($"pawn in range check for {parent}");
                bool log = false;
                foreach (var pawn in pawnInRange)
                {
                    log = true;
                    stb.Append($",{pawn}");
                    if (!closingPawns.Contains(pawn))
                    {
                        stb.AppendLine($"adding {pawn} to closing pawn");
                        closingPawns.Add(pawn);
                        CheckSring(pawn);
                    }

                    if (pawn.Faction.HostileTo(this.parent.Faction))
                    {
                        stb.AppendLine($"{pawn} triggered mine");
                        this.shouldStartWick = true;
                        Log.Message(stb.ToString());
                        return;
                    }
                }

                int i = 0;
                if (closingPawns.Count > 0)
                {
                    while (i < closingPawns.Count)
                    {
                        if (closingPawns[i].Position.DistanceTo(this.parent.Position) > this.Props.detectionRadius)
                        {
                            log = true;
                            stb.AppendLine($"removing {closingPawns[i]} from closing pawn");
                            closingPawns.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
                if (log) Log.Message(stb.ToString());
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        public bool InRange(IntVec3 target, float distance)
        {
            return distance * distance >= this.parent.Position.DistanceToSquared(target);
        }

        public void TryRemoveByPawn(Pawn pawn)
        {

        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Props.canManualDetonate)
            {
                Texture manualDetonateIconTex;
                if (Props.manualDetonateIcon == null)
                {
                    manualDetonateIconTex = parent.def.uiIcon;
                }
                else
                {
                    manualDetonateIconTex = ContentFinder<Texture2D>.Get(Props.manualDetonateIcon, false);
                }
                yield return new Command_Action
                {
                    action = delegate
                    {
                        StartWick();
                    },
                    icon = manualDetonateIconTex,
                    defaultLabel = Props.manualDetonateLabel,
                };
            }
        }
    }

    public class CompProperties_ProximityExplosive : CompProperties_Explosive
    {
        public CompProperties_ProximityExplosive()
        {
            this.compClass = typeof(CompProximityExplosive);
        }

        public float detectionRadius = 5f;
        public float wildAnimalSpringChance = 0.05f;
        public float colonistSpringChance = 0.05f;
        public float selfExplodeChance = 0.05f;
        public int minSelfExplodeTick = 0;
        public int maxSelfExplodeTick = 3600;
        public float disarmTime = 6f;
        public List<ThingDefCountClass> disarmLeavings;
        public SimpleCurve disarmSuccessRateCurve;
        public bool disarmEnabled = true;
        public bool disableByEMP = true;
        public int EMPstunTick = 30000;
        public bool selfDestructHarmlessly = false;
        public bool canManualDetonate = false;
        public string manualDetonateIcon;
        public string manualDetonateLabel;
        public bool detailedHostilityCheck;

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            if (manualDetonateLabel == null)
            {
                manualDetonateLabel = parentDef.label;
            }
        }
    }
}
