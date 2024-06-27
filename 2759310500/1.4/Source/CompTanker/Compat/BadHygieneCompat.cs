using DubsBadHygiene;
using System;
using Verse;

namespace CompTanker.Compat
{
    [HotSwappable]
    public static class BadHygieneCompat
    {
        public static bool IsActive { get; private set; }

        public static void Init()
        {
            if (IsActive) return;
            IsActive = true;
        }

        public static void HandleTick(CompTanker tanker)
        {
            if (IsActive) HandleTickPrivate(tanker);
        }

        private static void HandleTickPrivate(CompTanker tanker)
        {
            PlumbingNet net = tanker.parent.GetComp<CompPipe>()?.pipeNetRef;
            if (net == null) return;
            if (net.HasFilter) tanker.contaminationLevel = 0;

            if (tanker.isDraining)
            {
                if (tanker.storedAmount <= 0)
                {
                    tanker.isDraining = false;
                    return;
                }

                var num = Math.Min(tanker.storedAmount, tanker.Props.drainAmount);
                if (num <= 0) return;
                ContaminateNet((ContaminationLevel)tanker.contaminationLevel, net);
                num -= net.PushWater((float)num);
                tanker.storedAmount -= num;
            }
            else if (tanker.isFilling)
            {
                if (tanker.storedAmount >= tanker.Props.storageCap)
                {
                    tanker.isFilling = false;
                    return;
                }

                var num = Math.Min(tanker.Props.storageCap - tanker.storedAmount, tanker.Props.fillAmount);
                num = Math.Max(num, 0);

                if (!net.PullWater((float)num, out ContaminationLevel lvl)) return;
                tanker.contaminationLevel = (int)lvl;
                tanker.storedAmount += num;
            }
        }

        public static void MarkForDrawing(Map map)
        {
            map.GetComponent<MapComponent_Hygiene>().MarkTowersForDraw = true;
        }

        public static void ContaminateNet(ContaminationLevel level, PlumbingNet net)
        {
            foreach (var tower in net.WaterTowers)
            {
                //The tank is empty, set its contamination level to that of the tanker
                if (tower.WaterQuality >= level && tower.WaterStorage == 0)
                {
                    tower.WaterQuality = level;
                }
                //The tank has some water in it, contaminate it if the tanker is contaminated 
                else if (tower.WaterQuality < level) tower.WaterQuality = level;
            }
        }
    }
}
