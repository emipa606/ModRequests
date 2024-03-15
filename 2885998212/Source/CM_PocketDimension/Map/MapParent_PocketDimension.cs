using RimWorld;
using RimWorld.Planet;
using Verse;

namespace KB_PocketDimension
{
    public class MapParent_PocketDimension : MapParent
    {
        public string dimensionSeed = string.Empty;

        private bool shouldBeDeleted = false;

        public MapParent_PocketDimension()
        {

        }
        public override void SpawnSetup()
        {
            base.SpawnSetup();

            Logger.MessageFormat(this, this.dimensionSeed);

            if (this.dimensionSeed != null)
                PocketDimensionUtility.MapParents[this.dimensionSeed] = this;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<string>(ref this.dimensionSeed, "dimensionSeed", string.Empty);
        }

        protected override bool UseGenericEnterMapFloatMenuOption
        {
            get
            {
                return false;
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (this.shouldBeDeleted)
            {
                this.CheckRemoveMapNow();

                Building_PocketDimensionBox box = PocketDimensionUtility.GetBox(this.dimensionSeed);
                if (box != null && !box.BeingDestroyed && !box.GetLost)
                {
                    box.GetLost = true;
                }
            }
        }

        public void Abandon(bool lost)
        {
            if (!this.shouldBeDeleted)
            {
                TaggedString label = "KB_PocketDimensionDestroyedLetterLabel".Translate();
                TaggedString text = "KB_PocketDimensionDestroyedLetterText".Translate();

                if (lost)
                {
                    label = "KB_PocketDimensionLostLetterLabel".Translate();
                    text = "KB_PocketDimensionLostLetterText".Translate();
                }

                Building_PocketDimensionBox box = PocketDimensionUtility.GetBox(this.dimensionSeed);
                if (box != null && !box.BeingDestroyed)
                {
                    Thing thingHolder = box.SpawnedParentOrMe;
                    if (thingHolder != null)
                    {
                        LookTargets lookTarget = new LookTargets(thingHolder.Position, thingHolder.Map);
                        Find.LetterStack.ReceiveLetter(label, text, PocketDimensionDefOf.KB_PocketDimensionBreachedLetter, lookTarget);
                    }
                    else
                    {
                        Find.LetterStack.ReceiveLetter(label, text, PocketDimensionDefOf.KB_PocketDimensionBreachedLetter);
                    }
                }
                else
                {
                    Find.LetterStack.ReceiveLetter(label, text, PocketDimensionDefOf.KB_PocketDimensionBreachedLetter);
                }

                if (box != null)
                    box.GetLost = true;

                Building_PocketDimensionExit exit = PocketDimensionUtility.GetExit(this.dimensionSeed);
                if (exit != null)
                    exit.GetLost = true;
            }

            this.shouldBeDeleted = true;
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            alsoRemoveWorldObject = false;
            bool result = false;
            if (this.shouldBeDeleted)
            {
                result = true;
                alsoRemoveWorldObject = true;
            }
            return result;
        }
    }
}
