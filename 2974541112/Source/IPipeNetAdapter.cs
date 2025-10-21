public interface IPipeNetAdapter {
    public float AvailableCapacity { get; }
    public float Stored { get; }

    public void Draw(float amount);
    public void Push(float amount);
}
