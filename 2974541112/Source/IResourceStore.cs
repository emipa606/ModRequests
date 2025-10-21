using System.Collections.Generic;

namespace zed_0xff.CPS;

public interface IResourceStore {
    public string ResourceLabel { get; }
    public float TargetValue { get; }
    public float Max { get; }
    public int ValueForDisplay { get; }
    public int MaxForDisplay { get; }
    public float MaxLevelOffset { get; }
    public List<float> resourceGizmoThresholds { get; }

    public float ValuePercent { get; }
    public float Value { get; }
    public bool ResourceAllowed { get; set; }

    public void SetTargetValuePct(float val);
    public int PostProcessValue(float value);
}
