using GasNetwork;
using RimWorld;

namespace AnaerobicDigester
{
    public class Building_WorkTable_GasPerRecipe : Building_WorkTable_Gas, IBillGiver, IBillGiverWithTickAction
    {
        public new bool CurrentlyUsableForBills()
        {
            if (!base.CurrentlyUsableForBills())
            {
                return false;
            }

            return this.gasComp.Props.gasConsumptionWhenUsed == 0f ||
                   this.gasComp.Network.Stored >= this.gasComp.Props.gasConsumptionWhenUsed;
        }

        public new bool UsableForBillsAfterFueling()
        {
            if (!base.UsableForBillsAfterFueling())
            {
                return false;
            }

            return CurrentlyUsableForBills();
        }

    }
}