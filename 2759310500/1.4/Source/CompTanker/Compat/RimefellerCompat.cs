using Rimefeller;
using System;
using Verse;

namespace CompTanker.Compat
{
    [HotSwappable]
    public static class RimefellerCompat
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
            PipelineNet net = tanker.parent.GetComp<CompPipe>()?.pipeNetRef;
            if (net == null) return;

            if (tanker.isDraining)
            {
                if (tanker.storedAmount <= 0)
                {
                    tanker.isDraining = false;
                    return;
                }

                var num = Math.Min(tanker.storedAmount, tanker.Props.drainAmount);
                if (num <= 0) return;

                switch (tanker.Props.contents)
                {
                    case TankType.Oil:
                        num -= net.PushCrude(num);
                        break;
                    case TankType.Fuel:
                        num -= net.PushFuel((float)num);
                        break;
                    default:
                        return;
                }

                tanker.storedAmount -= num;
            }

            else if (tanker.isFilling)
            {
                if (tanker.storedAmount >= tanker.Props.storageCap)
                {
                    tanker.isFilling = false;
                    return;
                }

                var num = Math.Min(tanker.Props.fillAmount, tanker.Props.storageCap - tanker.storedAmount);
                if (num <= 0) return;
                bool success;

                switch (tanker.Props.contents)
                {
                    case TankType.Oil:
                        success = net.PullOil(num);
                        break;
                    case TankType.Fuel:
                        success = net.PullFuel(num);
                        break;
                    default:
                        return;
                }

                if (success) tanker.storedAmount += num;
            }
        }

        public static void MarkForDrawing(Map map)
        {
            map.GetComponent<MapComponent_Rimefeller>().MarkTowersForDraw = true;
        }
    }
}
