using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using Verse.Sound;

namespace LobotomyCorp.Abnos
{
    public class CompAbnos_WeCanChangeAnything : CompAbnos_Base, IThingHolder
    {
        private ThingOwner innerContainer;

        private SoundDef sound;

        public CompAbnos_WeCanChangeAnything(){
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            Log.Message("posted");
            base.PostSpawnSetup(respawningAfterLoad);

            action = new Command_Target()
            {
                targetingParams = new TargetingParameters()
                {
                    validator = t =>
                    {
                        if (t == null || !t.IsValid ||
                            !t.HasThing || !(t.Thing is Pawn) ||
                            !(t.Thing as Pawn).RaceProps.Humanlike) return false;
                        return true;
                    },
                },
                //defaultLabel = parent.Label + "を使用する",
                icon = parent.def.uiIcon,
                action = t => {
                    if (actor != null) return;

                    Pawn p = t.Thing as Pawn;
                    Thing abnos = Cache.GetCoreAbnos();
                    if (abnos == null) return;

                    Verse.AI.Job tmpjob = JobMaker.MakeJob(LCDefOf.LCJob_ApproachAbnos, abnos);
                    p.jobs.TryTakeOrderedJob(tmpjob);
                },
            };
        }


        public override void CompTick()
        {
            if (innerContainer != null && innerContainer.HasData()) innerContainer.ThingOwnerTick();

            int times = TICK_TIME / Math.Min(jobTick / 5 + 1, 10);

            count = count > times ? 0 : count + 1;
            if (count % times != times - 1) return;

            if (finished || actor == null)
            {
                jobTick = TIME_NOJOB;
                finished = false;
            }
            else if (approached == true)
            {
                jobTick = TIME_ACTJOB;
                innerContainer = new ThingOwner<Thing>(this);
                //innerContainer.TryAddOrTransfer(actor.SplitOff(1));

                approached = false;
            }
            else if (jobTick != TIME_NOJOB)
            {
                jobTick++;

                
                BodyPartRecord body = actor.health.hediffSet
                        .GetRandomNotMissingPart(DamageDefOf.Stab);
                DamageInfo info = new DamageInfo(DamageDefOf.Stab, 1, 5,
                    instigator: parent, hitPart: body);
                actor.TakeDamage(info);

                if (sound == null) sound = parent.def.GetCompProperties<CompProperties_AbnosProperties>().sound;
                if (sound != null) sound.PlayOneShot(SoundInfo.InMap(new TargetInfo(parent)));
                
                Log.Message(actor.Name.ToString());

                if (actor != null && actor.Dead)
                {
                    innerContainer.Remove(actor);
                    Pawn pawn = actor;
                    finished = true;
                    actor = null;
                    IntVec3 pos = parent.Position + IntVec3.South;
                    for (int i = 0; i < pawn.GetStatValue(StatDefOf.MeatAmount); i++)
                    {
                        GenSpawn.Spawn(pawn.RaceProps.meatDef, pos, parent.Map);
                    }
                    if(pawn.Corpse != null) pawn.Corpse.Destroy();
                    Log.Message(jobTick + " times damage");

                    
                }

            }


        }
        

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(
                outChildren, GetDirectlyHeldThings()
                );
        }

        public override void PostDeSpawn(Map map)
        {
            if (innerContainer != null && innerContainer.HasData())
            {
                innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Direct);
                actor.Kill(null);
            }
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_References.Look(ref actor, "actor");
            Scribe_Deep.Look(ref innerContainer, "abnosInnerContainer", new object[] { this });
            Scribe_Values.Look(ref jobTick, "jobTick", TIME_NOJOB);

        }

    }
}
