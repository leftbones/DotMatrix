namespace DotMatrix;

public class RNG {
    public Random Random { get; private set; }
    public int Seed { get; private set; }

    public RNG(int? seed=null) {
        Seed = seed ?? Environment.TickCount;
        Random = new Random(Seed);
    }

    // Return true or false based on a roll using a float
    public bool Roll(float n) {
        return Random.NextDouble() <= n;
    }

	public bool Rollf(float n) { return Roll(n); }

    // Return true or false based on a roll using a double
    public bool Roll(double n) {
        return Random.NextDouble() <= n;
    }

	public bool Rolld(double n) { return Roll(n); }

    // Return true or false based on a roll using an integer
    public bool Roll(int n) {
        return Random.Next(1, 100) <= n;
    }

	public bool Rolli(int n) { return Roll(n); }

    // Return true or false based on 1:n odds
    public bool Odds(int n) {
        return Range(1, n) == 1;
    }

	// Return true or false based on a percent chance out of 100
	[Obsolete("Deprecated, use `Roll(int n)` instead")]
    public bool Chance(int chance) {
        return Random.Next(1, 100) <= chance;
    }

    // Return the result of a coin flip
    public bool CoinFlip() {
        return Roll(50);
    }

    // Return a random int from an inclusive range
    public int Range(int min, int max) {
        return Random.Next(min, max+1);
    }

	// Return the given list with its contents shuffled
    public IEnumerable<object> Shuffle(IEnumerable<object> e) {
        return e.OrderBy(a => Random.Next()).ToList();
    }
}