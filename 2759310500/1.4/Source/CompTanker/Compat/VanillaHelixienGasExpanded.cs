using PipeSystem;
using UnityEngine;

namespace CompTanker.Compat
{
    [HotSwappable]
    public static class VanillaHelixienGasExpanded
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
            PipeNet net = tanker.parent.GetComp<CompResource>()?.PipeNet;
            if (net == null) return;

            if (tanker.isDraining)
            {
                if (tanker.storedAmount <= 0)
                {
                    tanker.isDraining = false;
                    return;
                }

                float num = Mathf.Clamp((float)tanker.storedAmount, net.AvailableCapacity, (float)tanker.Props.drainAmount);
                if (num <= 0) return;
                net.DistributeAmongStorage(num, out float actuallyStored);
                tanker.storedAmount -= actuallyStored;
            }
            else if (tanker.isFilling)
            {
                if (tanker.storedAmount >= tanker.Props.storageCap)
                {
                    tanker.isFilling = false;
                    return;
                }

                float num = Mathf.Clamp(net.Stored, (float)(tanker.Props.storageCap - tanker.storedAmount), (float)tanker.Props.fillAmount);
                if (num <= 0) return;
                net.DrawAmongStorage(num, net.storages);
                net.receiversDirty = true;
                tanker.storedAmount += num;
            }
        }
    }
}
