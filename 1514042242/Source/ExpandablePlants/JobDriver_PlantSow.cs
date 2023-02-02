using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace ExpandablePlants
{
    class JobDriver_PlantSow : JobDriver
    {
        private const TargetIndex plantTargetIndex = TargetIndex.A;

        private RimWorld.Plant Plant => job.GetTarget(plantTargetIndex).Thing as RimWorld.Plant;
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref sowWorkDone, "sowWorkDone");
        }
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            LocalTargetInfo plantTarget = job.targetA;
            return pawn.Reserve(plantTarget, job);
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch)
                .FailOn(() => PlantUtility.AdjacentSowBlocker(job.plantDefToSow, TargetA.Cell, Map) != null)
                .FailOn(() => !job.plantDefToSow.CanEverPlantAt(TargetLocA, Map));

            Toil sowToil = new Toil();
            sowToil.initAction = delegate ()
            {
                TargetThingA = GenSpawn.Spawn(job.plantDefToSow, TargetLocA, Map);
                pawn.Reserve(TargetThingA, sowToil.actor.CurJob);
                RimWorld.Plant plant = (RimWorld.Plant)TargetThingA;
                plant.Growth = 0f;
                plant.sown = true;
            };
            sowToil.tickAction = delegate ()
            {
                Pawn actor = sowToil.actor;
                if (actor.skills != null) actor.skills.Learn(SkillDefOf.Plants, 0.085f);
                float workSpeedStat = actor.GetStatValue(StatDefOf.PlantWorkSpeed);

                RimWorld.Plant plant = Plant;
                if (plant.LifeStage != PlantLifeStage.Sowing) Log.Error(this + " getting sowing work while not in Sowing life stage.");

                sowWorkDone += workSpeedStat;
                if (sowWorkDone >= plant.def.plant.sowWork)
                {
                    plant.Growth = 0.05f;
                    Map.mapDrawer.MapMeshDirty(plant.Position, MapMeshFlag.Things);
                    actor.records.Increment(RecordDefOf.PlantsSown);
                    ReadyForNextToil();
                    return;
                }
            };

            sowToil.defaultCompleteMode = ToilCompleteMode.Never;
            sowToil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            sowToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            sowToil.WithEffect(EffecterDefOf.Sow, TargetIndex.A);
            sowToil.WithProgressBar(TargetIndex.A, () => sowWorkDone / Plant.def.plant.sowWork, true);
            sowToil.PlaySustainerOrSound(() => SoundDefOf.Interact_Sow);

            sowToil.AddFinishAction(delegate
            {
                if (TargetThingA != null)
                {
                    RimWorld.Plant plant = (RimWorld.Plant)sowToil.actor.CurJob.GetTarget(TargetIndex.A).Thing;
                    if (sowWorkDone < plant.def.plant.sowWork && !TargetThingA.Destroyed)
                    {
                        TargetThingA.Destroy(DestroyMode.Vanish);
                    }
                }
            });
            sowToil.activeSkill = (() => SkillDefOf.Plants);
            yield return sowToil;
            yield break;
        }

        // Token: 0x0400024D RID: 589
        private float sowWorkDone = 0f;
    }
}
