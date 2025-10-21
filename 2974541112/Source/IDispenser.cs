using Verse;

public interface IDispenser {
    public Thing TryDispenseFood();
    public bool  CanDispenseNow  { get; }
    public float NutritionStored { get; }
}
