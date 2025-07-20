using RimWorld;
using Verse;

namespace DRNE_NeutroamineExpansion
{
    public class HediffThatNeutrogenesisates : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();
            if ((double)Severity >= 1.0 && pawn.Map != null)
            {
                ChangePawn();
            }
        }
        private void ChangePawn()
        {
            ModExt_NeutrogenesisXMLSettings modExt = def.GetModExtension<ModExt_NeutrogenesisXMLSettings>();
            Faction PlayerFaction = Faction.OfPlayer;
            float? NewBioAge = modExt.ChangedBioAge;
            PawnKindDef generatedPawnKind = modExt.primaryPawnKind;
            PawnGenerationRequest animalGeneration = new PawnGenerationRequest(generatedPawnKind, PlayerFaction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: false, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, NewBioAge, 0f);
            if (modExt.requiredSizeForPrimary > base.pawn.BodySize && base.pawn.BodySize >= modExt.requiredSizeForSecondary && modExt.secondaryPawnKind != null)
            {
                animalGeneration.KindDef = modExt.secondaryPawnKind;
            }
            if (modExt.requiredSizeForSecondary > base.pawn.BodySize && base.pawn.BodySize >= modExt.requiredSizeForTernary && modExt.ternaryPawnKind != null)
            {
                animalGeneration.KindDef = modExt.ternaryPawnKind;
            }
            if (base.pawn.inventory != null)
            {
                base.pawn.inventory.DropAllNearPawn(base.pawn.Position);
            }
            if (base.pawn.apparel != null)
            {
                base.pawn.apparel.DropAll(base.pawn.Position);
            }
            if (base.pawn.equipment != null)
            {
                base.pawn.equipment.DropAllEquipment(base.pawn.Position);
            }
            if (base.pawn.BodySize >= modExt.requiredSizeForTernary)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(animalGeneration);
                GenSpawn.Spawn(pawn, base.pawn.Position, base.pawn.Map);
                base.pawn.Destroy();
            }
            else { base.pawn.Kill(null, this); }
        }
    }
}
