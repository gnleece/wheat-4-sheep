public class SystemRandomProvider : IRandomProvider
{
    private readonly System.Random random;

    public SystemRandomProvider()
    {
        random = new System.Random();
    }

    public SystemRandomProvider(int seed)
    {
        random = new System.Random(seed);
    }

    public int Next(int maxValue) => random.Next(maxValue);
    public int Next(int minValue, int maxValue) => random.Next(minValue, maxValue);
    public double NextDouble() => random.NextDouble();
}
