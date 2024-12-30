using Verse;
using AnimalBehaviours;
using ESCP_NecromanticThralls;

namespace ESCP_NecromanticThralls_VEF
{
    class CompDraftableThrall : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (ThrallUtility.ThingIsThrall(parent) && !AnimalCollectionClass.draftable_animals.Contains(parent))
            {
                AnimalCollectionClass.AddDraftableAnimalToList(parent);
            }
        }

        public override void PostDeSpawn(Map map)
        {
            if (ThrallUtility.ThingIsThrall(parent) && AnimalCollectionClass.draftable_animals.Contains(parent))
            {
                AnimalCollectionClass.RemoveDraftableAnimalFromList(parent);
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (ThrallUtility.ThingIsThrall(parent) && AnimalCollectionClass.draftable_animals.Contains(parent))
            {
                AnimalCollectionClass.RemoveDraftableAnimalFromList(parent);
            }
        }
    }
}
