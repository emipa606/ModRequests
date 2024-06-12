// Nightvision NightVision ExtractTapetum_RecipeWorker.cs
// 
// 19 07 2018
// 
// 21 07 2018

using JetBrains.Annotations;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace NightVision
{
    [UsedImplicitly]
    public class ExtractTapetum_RecipeWorker : Recipe_RemoveBodyPart
    {
        // 16.08.21 1.3RW - changed base class to Recipe_RemoveBodyPart (unsure why it wasn't)
        [NotNull]
        private static readonly ThingDef ExtractedTapetum = ThingDef.Named("NV_TapetumRaw");

        public override void ApplyOnPawn(
                        Pawn pawn,
                        BodyPartRecord part,
                        Pawn billDoer,
                        List<Thing> ingredients,
                        Bill bill
                    )
        {
            // 16.08.2021 1.3RW added some utility functions and changed layout
            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }

                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
                GenSpawn.Spawn(ExtractTapetum_RecipeWorker.ExtractedTapetum, billDoer.Position, billDoer.Map);
            }

        
            DamagePart(pawn, part);
            
            if (IsViolationOnPawn(pawn, part, Faction.OfPlayer))
            {
                ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
            }
            
        }

        public override string GetLabelWhenUsedOn(
                        Pawn pawn,
                        BodyPartRecord part
                    )
            => "HarvestOrgan".Translate() + " " + "NVTapetum".Translate();

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(
                        Pawn pawn,
                        RecipeDef recipeDef
                    )
        {
            IEnumerable<BodyPartRecord> parts =
                        pawn.health.hediffSet.GetNotMissingParts(tag: Defs_Rimworld.EyeTag);

            foreach (BodyPartRecord part in parts.DefaultIfEmpty())
            {
                if (!pawn.health.hediffSet.HasDirectlyAddedPartFor(part)
                    && MedicalRecipesUtility.IsClean(pawn, part))
                {
                    yield return part;
                }
            }
        }
    }
}