using RimWorld;
using Verse;
using PipeSystem; // CompResource

namespace zed_0xff.CPS;

public class HemogenNetAdapter : IPipeNetAdapter {
    private CompResource hemogenComp = null;

    public float AvailableCapacity => hemogenComp.PipeNet.AvailableCapacity;
    public float Stored            => hemogenComp.PipeNet.Stored;

    public void Draw(float amount){
        hemogenComp.PipeNet.DrawAmongStorage(amount, hemogenComp.PipeNet.storages);
    }

    public void Push(float amount){
        hemogenComp.PipeNet.DistributeAmongStorage(amount);
    }

    public HemogenNetAdapter(ThingWithComps parent){
        foreach (CompResource comp in parent.GetComps<CompResource>()) {
            if (comp?.Props?.pipeNet?.defName == "VRE_HemogenNet") {
                hemogenComp = comp;
            }
        }
    }
}
