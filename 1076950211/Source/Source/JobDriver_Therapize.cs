using System.Collections.Generic;
using RimWorld;
using Therapy.Patches;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Therapy;

public class JobDriver_Therapize : JobDriver
{
    public static readonly SimpleCurve beautyEffectCurve = new()
    {
        new CurvePoint(0f, 0f),
        new CurvePoint(0.1f, 0.15f),
        new CurvePoint(0.15f, 0.5f),
        new CurvePoint(1f, 1f)
    };

    //public JobDriver_Therapize()
    //{
    //    float count = RoomStatDefOf.Beauty.scoreStages.Count;
    //    for (var i = 0; i < count; i++)
    //    {
    //        var stage = RoomStatDefOf.Beauty.scoreStages[i];
    //        Log.Message($"Beauty effect: {stage.label} - {BeautyEffectCurve.Evaluate(i/count)}");
    //    }
    //}

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.GetTarget(TargetIndex.A), job) &&
               pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, null, errorOnFailed);
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.EndOnDespawnedOrNull(TargetIndex.A); // Chair
        this.EndOnDespawnedOrNull(TargetIndex.B); // Patient
        this.FailOnMentalState(TargetIndex.B);
        this.FailOnDowned(TargetIndex.B);
        this.FailOn(() => PatientNotOnCouch());

        //yield return Toils_Reserve.Reserve(TargetIndex.A);
        //yield return Toils_Reserve.Reserve(TargetIndex.B);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
        var counsel = new Toil();
        counsel.tickAction = delegate
        {
            rotateToFace = TargetIndex.C; // Nothing, so rotator doesn't get overwritten
            pawn.rotationTracker.FaceCell(TargetLocA + TargetThingA.Rotation.FacingCell);
            //float statValue = TargetThingA.GetStatValue(StatDefOf.EntertainmentStrengthFactor);
            //float extraJoyGainFactor = statValue;
            //JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);

            pawn.skills.Learn(SkillDefOf.Social, 0.11f);
            pawn.GainComfortFromCellIfPossible();

            CounselPatient((Pawn)TargetThingB);
        };
        counsel.defaultCompleteMode = ToilCompleteMode.Delay;
        counsel.defaultDuration = 8000;
        counsel.socialMode = RandomSocialMode.Off;
        counsel.FailOnDowned(TargetIndex.B);
        counsel.FailOnMentalState(TargetIndex.B);
        counsel.FailOn(PatientNotOnCouch);
        counsel.AddFinishAction(() =>
        {
            if (TargetB.HasThing) Dismiss((Pawn)TargetThingB);
        });
        yield return counsel;
    }

    private static void Dismiss(Pawn patient)
    {
        if (patient.jobs.curDriver is JobDriver_ReceiveTherapy receive) receive.Dismiss();
    }

    private bool PatientNotOnCouch(Toil toil = null)
    {
        if (TargetThingA == null) return true;
        if (TargetThingB == null) return true;
        if (!TargetThingA.Spawned) return true;
        if (!TargetThingB.Spawned) return true;
        var couch = TargetThingB.Position.GetEdifice(TargetThingB.Map);
        if (couch == null) return true;
        if (TargetThingA.Position.DistanceTo(couch.Position) > 4) return true;
        return false;
    }

    private void CounselPatient(Pawn patient)
    {
        if (patient.IsHashIntervalTick(150))
        {
            //Log.Message(pawn.NameStringShort+ " is therapizing "+patient.NameStringShort);
            var room = pawn.GetRoom();
            var couch = patient.CurrentCouch();

            if (room != null && couch != null)
            {
                var effect = Mathf.Max(0, pawn.GetStatValue(StatDefOf.SocialImpact)); // 0-1ish
                effect *= Mathf.Max(0, couch.GetStatValue(StatDefOf.Comfort)); // 0-1ish
                effect *= Mathf.Max(0, GetRoomEffect(room)); // 0-1ish
                effect *= Mathf.Max(0, pawn.skills.AverageOfRelevantSkillsFor(MainUtility.therapyWorkTypeDef)); // 0-20

                var amount = (int)(effect * 1000) - 100;
                //Log.Message($"Therapy by {pawn.NameShortColored}: {amount} = Social Impact ({pawn.GetStatValue(StatDefOf.SocialImpact)}) * Couch ({couch.GetStatValue(StatDefOf.Comfort)}) * Room ({GetRoomEffect(room)}) * Therapy Skill ({pawn.skills.AverageOfRelevantSkillsFor(MainUtility.therapyWorkTypeDef)})");

                if (patient.jobs.curDriver is JobDriver_ReceiveTherapy driver)
                    driver.HealMemories(amount);
                else
                    Log.Message("Patient " + patient.Name.ToStringShort + " has wrong JobDriver: " +
                                patient.jobs.curDriver);
            }
        }

        if (patient.IsHashIntervalTick(350)) ShowInteractionBubble(patient);
    }

    private static float GetRoomEffect(Room room)
    {
        // Higher when neutral, then tapering off
        float indexBeauty = RoomStatDefOf.Beauty.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Beauty));
        var count = RoomStatDefOf.Beauty.scoreStages.Count;
        return beautyEffectCurve.Evaluate(indexBeauty / count);
    }

    private void ShowInteractionBubble(Pawn patient)
    {
        switch (Rand.RangeInclusive(0, 2))
        {
            case 0:
                return;
            case 1:
                TryCreateBubble(patient, pawn,
                    Rand.Element(Mod_Therapy.symbolPatient1, Mod_Therapy.symbolPatient2));
                return;
            case 2:
                TryCreateBubble(pawn, patient,
                    Rand.Element(Mod_Therapy.symbolTherapist1, Mod_Therapy.symbolTherapist2));
                return;
        }
    }

    private static void TryCreateBubble(Pawn pawn1, Pawn pawn2, Texture2D symbol)
    {
        if (pawn1.interactions.InteractedTooRecentlyToInteract()) return;
        MoteMaker.MakeInteractionBubble(pawn1, pawn2, ThingDefOf.Mote_Speech, symbol);
    }
}