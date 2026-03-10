public class SystemRandomProvider : IRandomProvider
{
    private readonly System.Random _random;

    public SystemRandomProvider()
    {
        _random = new System.Random();
    }

    public SystemRandomProvider(int seed)
    {
        _random = new System.Random(seed);
    }

    public int Next(int maxValue) => _random.Next(maxValue);
    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);
    public double NextDouble() => _random.NextDouble();
}
