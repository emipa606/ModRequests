using RimWorld;
using System;
using Verse;

namespace ReworkedTemperatureEvents
{
    public class GameCondition_ColdSnap : GameCondition
    {
        private static int seed = Find.TickManager.TicksGame;

        private Random random = new Random(GameCondition_ColdSnap.seed);

        private float MaxTempOffsetC = 0f;

        private float MaxTempOffsetF = 0f;

        private string DisplayTempOffsetC;

        private string DisplayTempOffsetF;

        private const int LerpTicks = 12000;

        public override float TemperatureOffset()
        {
            if (this.MaxTempOffsetC == 0f)
            {
                this.MaxTempOffsetC = (float)this.random.Next(-23, -12);
                this.MaxTempOffsetF = this.MaxTempOffsetC * 1.8f;
                this.DisplayTempOffsetC = " " + this.MaxTempOffsetC.ToString() + "C";
                this.DisplayTempOffsetF = " (" + this.MaxTempOffsetF.ToString() + "F)";
                Log.Message("This cold snap resulted in the following temperature offset:" + this.DisplayTempOffsetC + this.DisplayTempOffsetF);
            }
            return GameConditionUtility.LerpInOutValue(this, 12000f, this.MaxTempOffsetC);
        }
    }
}