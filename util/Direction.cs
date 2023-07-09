namespace DotMatrix;

public static class Direction {
    public static int Seed = Environment.TickCount;

    public static readonly Vector2i None = new Vector2i(0, 0);

    // Directionections
    public static readonly Vector2i Up = new Vector2i(0, -1);
    public static readonly Vector2i Down = new Vector2i(0, 1);
    public static readonly Vector2i Left = new Vector2i(-1, 0);
    public static readonly Vector2i Right = new Vector2i(1, 0);
    public static readonly Vector2i UpLeft = new Vector2i(-1, -1);
    public static readonly Vector2i UpRight = new Vector2i(1, -1);
    public static readonly Vector2i DownLeft = new Vector2i(-1, 1);
    public static readonly Vector2i DownRight = new Vector2i(1, 1);

    // Directionection Sets
    public static readonly List<Vector2i> Vertical = new List<Vector2i>() { Direction.Up, Direction.Down };
    public static readonly List<Vector2i> Horizontal = new List<Vector2i>() { Direction.Left, Direction.Right };

    public static readonly List<Vector2i> Upward = new List<Vector2i>() { Direction.Up, Direction.UpLeft, Direction.UpRight };
    public static readonly List<Vector2i> Downward = new List<Vector2i>() { Direction.Down, Direction.DownLeft, Direction.DownRight };

    public static readonly List<Vector2i> DiagonalUp = new List<Vector2i>() { Direction.UpLeft, Direction.UpRight };
    public static readonly List<Vector2i> DiagonalDown = new List<Vector2i>() { Direction.DownLeft, Direction.DownRight };

    public static readonly List<Vector2i> UpperHalf = new List<Vector2i>() { Direction.Left, Direction.UpLeft, Direction.Up, Direction.UpRight, Direction.Right };
    public static readonly List<Vector2i> LowerHalf = new List<Vector2i>() { Direction.Left, Direction.DownLeft, Direction.Down, Direction.DownRight, Direction.Right };

    public static readonly List<Vector2i> CardinalUp = new List<Vector2i>() { Direction.Left, Direction.Up, Direction.Right };
    public static readonly List<Vector2i> CardinalDown = new List<Vector2i>() { Direction.Left, Direction.Down, Direction.Right };

    public static readonly List<Vector2i> Cardinal = new List<Vector2i>() { Direction.Left, Direction.Right, Direction.Up, Direction.Down };
    public static readonly List<Vector2i> Diagonal = new List<Vector2i>() { Direction.UpLeft, Direction.UpRight, Direction.DownLeft, Direction.DownRight };

    public static readonly List<Vector2i> Full = new List<Vector2i>() { Direction.Up, Direction.Down, Direction.Left, Direction.Right, Direction.UpLeft, Direction.UpRight, Direction.DownLeft, Direction.DownRight };

    public static readonly List<Vector2i> Gravity = new List<Vector2i>() { Direction.Up, Direction.None, Direction.Down };

    // Directionection Sets (Shuffled Order)
    public static List<Vector2i> ShuffledVertical { get { return Vertical.OrderBy(a => RNG.Random.Next()).ToList(); } }
    public static List<Vector2i> ShuffledHorizontal { get { return Horizontal.OrderBy(a => RNG.Random.Next()).ToList(); } }
    public static List<Vector2i> ShuffledUpward { get { return Upward.OrderBy(a => RNG.Random.Next()).ToList(); } }
    public static List<Vector2i> ShuffledDownward { get { return Downward.OrderBy(a => RNG.Random.Next()).ToList(); } }
    public static List<Vector2i> ShuffledDiagonalUp { get { return DiagonalUp.OrderBy(a => RNG.Random.Next()).ToList(); } }
    public static List<Vector2i> ShuffledDiagonalDown { get { return DiagonalDown.OrderBy(a => RNG.Random.Next()).ToList(); } }
    public static List<Vector2i> ShuffledUpperHalf { get { return UpperHalf.OrderBy(a => RNG.Random.Next()).ToList(); } }
    public static List<Vector2i> ShuffledLowerHalf { get { return LowerHalf.OrderBy(a => RNG.Random.Next()).ToList(); } }
    public static List<Vector2i> ShuffledCardinalUp { get { return CardinalUp.OrderBy(a => RNG.Random.Next()).ToList(); } }
    public static List<Vector2i> ShuffledCardinalDown { get { return CardinalDown.OrderBy(a => RNG.Random.Next()).ToList(); } }
    public static List<Vector2i> ShuffledCardinal { get { return Cardinal.OrderBy(a => RNG.Random.Next()).ToList(); } }
    public static List<Vector2i> ShuffledDiagonal { get { return Diagonal.OrderBy(a => RNG.Random.Next()).ToList(); } }
    public static List<Vector2i> ShuffledFull { get { return Full.OrderBy(a => RNG.Random.Next()).ToList(); } }

    // Random Directionection from Directionection Set
    public static Vector2i RandomVertical { get { return Direction.Vertical[RNG.Range(0, 1)]; } }
    public static Vector2i RandomHorizontal { get { return Direction.Horizontal[RNG.Range(0, 1)]; } }
    public static Vector2i RandomUpward { get { return Direction.Upward[RNG.Range(0, 2)]; } }
    public static Vector2i RandomDownward { get { return Direction.Downward[RNG.Range(0, 2)]; } }
    public static Vector2i RandomDiagonalUp { get { return Direction.DiagonalUp[RNG.Range(0, 1)]; } }
    public static Vector2i RandomDiagonalDown { get { return Direction.DiagonalDown[RNG.Range(0, 1)]; } }
    public static Vector2i RandomUpperHalf { get { return Direction.UpperHalf[RNG.Range(0, 4)]; } }
    public static Vector2i RandomLowerHalf{ get { return Direction.LowerHalf[RNG.Range(0, 4)]; } }
    public static Vector2i RandomCardinalUp { get { return Direction.CardinalUp[RNG.Range(0, 2)]; } }
    public static Vector2i RandomCardinalDown { get { return Direction.CardinalDown[RNG.Range(0, 2)]; } }
    public static Vector2i RandomCardinal { get { return Direction.Cardinal[RNG.Range(0, 3)]; } }
    public static Vector2i RandomDiagonal { get { return Direction.Diagonal[RNG.Range(0, 3)]; } }
    public static Vector2i RandomFull { get { return Direction.Full[RNG.Range(0, 7)]; } }

    // Returns the given direction mirrored vertically (UpLeft -> DownLeft)
    public static Vector2i MirrorVertical(Vector2i direction) {
        if (direction == Up) return Down;
        if (direction == Down) return Up;
        if (direction == UpLeft) return DownLeft;
        if (direction == UpRight) return DownRight;
        if (direction == DownLeft) return UpLeft;
        if (direction == DownRight) return UpRight;
        return None;
    }

	// Returns the given direction mirrored horizontally (UpLeft -> UpRight)
    public static Vector2i MirrorHorizontal(Vector2i direction) {
        if (direction == Left) return Right;
        if (direction == Right) return Left;
        if (direction == UpLeft) return UpRight;
        if (direction == UpRight) return UpLeft;
        if (direction == DownLeft) return DownRight;
        if (direction == DownRight) return DownLeft;
        return None;
    }

    // Returns the given direction reversed (UpLeft -> DownRight)
    public static Vector2i Reverse(Vector2i direction) {
        if (direction == Up) return Down;
        if (direction == Down) return Up;
        if (direction == Left) return Right;
        if (direction == Right) return Left;
        if (direction == UpLeft) return DownRight;
        if (direction == UpRight) return DownLeft;
        if (direction == DownLeft) return UpRight;
        if (direction == DownRight) return UpLeft;
        return None;
    }

	// Find the direction of movement between two points (Vector2i)
    public static Vector2i GetMovementDirection(Vector2i a, Vector2i b) {
        return new Vector2i(Math.Sign(b.X - a.X), Math.Sign(b.Y - a.Y));
    }
}