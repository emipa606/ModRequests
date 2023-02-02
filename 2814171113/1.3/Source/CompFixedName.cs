using RimWorld;
using Verse;

namespace RenameGun
{
    public class CompProperties_FixedName : CompProperties
    {
        public CompProperties_FixedName()
        {
            compClass = typeof(CompFixedName);
        }
    }
    public class CompFixedName : ThingComp
    {
        public CompProperties_FixedName Props => base.props as CompProperties_FixedName;
        public string fixedName;
        public string colonistSetName;
        public Pawn curPawnHolder;
        public int holdingCounter;

        public bool includeHP = true;
        public bool includeQuality = true;
        public bool includeStuff = true;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            GameComponent_RenameManager.Instance.TryAddThing(this);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            GameComponent_RenameManager.Instance.RemoveThing(this);
        }
        public Pawn HoldingPawn
        {
            get
            {
                if (this.parent.ParentHolder is Pawn_EquipmentTracker eq)
                    return eq.pawn;
                if (this.parent.ParentHolder is Pawn_InventoryTracker inv)
                    return inv.pawn;
                return null;
            }
        }
        public override string TransformLabel(string label)
        {
            if (!colonistSetName.NullOrEmpty())
            {
                return colonistSetName;
            }
            if (!fixedName.NullOrEmpty())
            {
                return GenLabelFixed.ThingLabel(this.fixedName, parent, parent.stackCount, this.includeStuff, this.includeQuality, this.includeHP);
            }
            return base.TransformLabel(label);
        }

        public void AutoRename()
        {
            var pawn = HoldingPawn;
            var taleRef = Find.TaleManager.GetRandomTaleReferenceForArtConcerning(this.parent);
            var oldName = GenLabelFixed.ThingLabel(this.fixedName ?? this.parent.def.label, parent, parent.stackCount, this.includeStuff, false, false);
            this.colonistSetName = GenText.CapitalizeAsTitle(taleRef.GenerateText(TextGenerationPurpose.ArtName, RG_DefOf.NamerArtWeaponGun));
            if (PawnUtility.ShouldSendNotificationAbout(pawn))
            {
                var newName = GenLabelFixed.ThingLabel(this.colonistSetName, parent, parent.stackCount, this.includeStuff, false, false);
                Messages.Message("RG.PawnRenamedGunMessage".Translate(pawn.Named("PAWN"), oldName, newName), pawn, MessageTypeDefOf.PositiveEvent);
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref fixedName, "fixedName");
            Scribe_Values.Look(ref colonistSetName, "colonistSetName");
            Scribe_Values.Look(ref includeHP, "includeHP", true);
            Scribe_Values.Look(ref includeQuality, "includeQuality", true);
            Scribe_Values.Look(ref includeStuff, "includeStuff", true);
            Scribe_Values.Look(ref holdingCounter, "holdingCounter", 0);
            Scribe_References.Look(ref curPawnHolder, "curPawnHolder");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                GameComponent_RenameManager.Instance.TryAddThing(this);
            }
        }
    }
}
