using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

//Made by Vendan
namespace Clutter_Furniture
{
    public class CompBetterRottable : CompRottable
    {
        private CompProperties_Rottable PropsRot
        {
            get
            {
                return (CompProperties_Rottable)this.props;
            }
        }
        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            switch (base.Stage)
            {
                case RotStage.Fresh:
                    stringBuilder.AppendLine("RotStateFresh".Translate() + ".");
                    break;
                case RotStage.Rotting:
                    stringBuilder.AppendLine("RotStateRotting".Translate() + ".");
                    break;
                case RotStage.Dessicated:
                    stringBuilder.AppendLine("RotStateDessicated".Translate() + ".");
                    break;
            }
            float num = (float)this.PropsRot.TicksToRotStart - base.RotProgress;
            bool flag = num > 0f;
            if (flag)
            {
                float num2 = GenTemperature.GetTemperatureForCell(this.parent.PositionHeld, this.parent.Map);
                List<Thing> thingList = this.parent.PositionHeld.GetThingList(this.parent.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    bool flag2 = thingList[i] is Freezer;
                    if (flag2)
                    {
                        Freezer building_Refrigerator = thingList[i] as Freezer;
                        num2 = building_Refrigerator.Temp;
                        break;
                    }
                }
                num2 = (float)Mathf.RoundToInt(num2);
                float num3 = GenTemperature.RotRateAtTemperature(num2);
                int ticksUntilRotAtCurrentTemp = base.TicksUntilRotAtCurrentTemp;
                bool flag3 = num3 < 0.001f;
                if (flag3)
                {
                    stringBuilder.Append("CurrentlyFrozen".Translate() + ".");
                }
                else
                {
                    bool flag4 = num3 < 0.999f;
                    if (flag4)
                    {
                        stringBuilder.Append("CurrentlyRefrigerated".Translate(new object[]
						{
							ticksUntilRotAtCurrentTemp.ToStringTicksToPeriodVagueMax()
						}) + ".");
                    }
                    else
                    {
                        stringBuilder.Append("NotRefrigerated".Translate(new object[]
						{
							ticksUntilRotAtCurrentTemp.ToStringTicksToPeriodVagueMax()
						}) + ".");
                    }
                }
            }
            return stringBuilder.ToString();
        }
        public override void CompTickRare()
        {
            float rotProgress = base.RotProgress;
            float num = 1f;
            float temperature = GenTemperature.GetTemperatureForCell(this.parent.PositionHeld, this.parent.Map);
            List<Thing> thingList = this.parent.PositionHeld.GetThingList(this.parent.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                bool flag = thingList[i] is Freezer;
                if (flag)
                {
                    Freezer building_Refrigerator = thingList[i] as Freezer;
                    temperature = building_Refrigerator.Temp;
                    break;
                }
            }
            num *= GenTemperature.RotRateAtTemperature(temperature);
            base.RotProgress += Mathf.Round(num * 250f);
            bool flag2 = base.Stage == RotStage.Rotting && this.PropsRot.rotDestroys;
            if (flag2)
            {
                bool flag3 = this.parent.Position.GetSlotGroup(this.parent.Map) != null;
                if (flag3)
                {
                    Messages.Message("MessageRottedAwayInStorage".Translate(new object[]
					{
						this.parent.Label
					}).CapitalizeFirst(), MessageSound.Silent);
                    LessonAutoActivator.TeachOpportunity(ConceptDefOf.SpoilageAndFreezers, OpportunityType.GoodToKnow);
                }
                this.parent.Destroy(DestroyMode.Vanish);
            }
            else
            {
                bool flag4 = Mathf.FloorToInt(rotProgress / 60000f) != Mathf.FloorToInt(base.RotProgress / 60000f);
                bool flag5 = flag4;
                if (flag5)
                {
                    bool flag6 = base.Stage == RotStage.Rotting && this.PropsRot.rotDamagePerDay > 0f;
                    if (flag6)
                    {
                        this.parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting, GenMath.RoundRandom(this.PropsRot.rotDamagePerDay), (float)-1, null, null, null));
                    }
                    else
                    {
                        bool flag7 = base.Stage == RotStage.Dessicated && this.PropsRot.dessicatedDamagePerDay > 0f && this.ShouldTakeDessicateDamage();
                        if (flag7)
                        {
                            this.parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting, GenMath.RoundRandom(this.PropsRot.dessicatedDamagePerDay), (float)-1, null, null, null));
                        }
                    }
                }
            }
        }
        private bool ShouldTakeDessicateDamage()
        {
            bool flag = this.parent.holdingOwner != null;
            bool result;
            if (flag)
            {
                Thing thing = this.parent.holdingOwner.Owner as Thing;
                bool flag2 = thing != null && thing.def.category == ThingCategory.Building && thing.def.building.preventDeterioration;
                if (flag2)
                {
                    result = false;
                    return result;
                }
            }
            result = true;
            return result;
        }
        private void StageChanged()
        {
            Corpse corpse = this.parent as Corpse;
            bool flag = corpse != null;
            if (flag)
            {
                corpse.RotStageChanged();
            }
        }
    }
}
