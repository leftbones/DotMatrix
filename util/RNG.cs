namespace DotMatrix;

public static class RNG {
    public static Random Random = new Random(Guid.NewGuid().GetHashCode());

    // Return true or false based on a roll using a float
    public static bool Roll(float n) {
        return Random.NextDouble() <= n;
    }

	public static bool Rollf(float n) { return Roll(n); }

    // Return true or false based on a roll using a double
    public static bool Roll(double n) {
        return Random.NextDouble() <= n;
    }

	public static bool Rolld(double n) { return Roll(n); }

    // Return true or false based on a roll using an integer
    public static bool Roll(int n) {
        return Random.Next(1, 100) <= n;
    }

	public static bool Rolli(int n) { return Roll(n); }

    // Return true or false based on 1:n odds
    public static bool Odds(int n) {
        return Range(1, n) == 1;
    }

	// Return true or false based on a percent chance out of 100
	[Obsolete("Deprecated, use `Roll(int n)` instead")]
    public static bool Chance(int chance) {
        return Random.Next(1, 100) <= chance;
    }

    // Return the result of a coin flip
    public static bool CoinFlip() {
        return Roll(50);
    }

    // Return a random int from an inclusive range
    public static int Range(int min, int max) {
        return Random.Next(min, max+1);
    }

	// Return the given list with its contents shuffled
    public static IEnumerable<object> Shuffle(IEnumerable<object> e) {
        return e.OrderBy(a => Random.Next()).ToList();
    }
}