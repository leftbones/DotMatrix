using System.Linq;

namespace DotMatrix;


// Static class used to run the marching squares algorithm and Douglas Puecker line simplification algorithm on a list of points
static class Boundaries {
    public static int MinPointsToSimplify = 5;      // Minimum number of points in a line required to run the simplification algorithm


    // 
    // Marching Squares Algorithm

    // Use marching squares to find the boundaries of a chunk based on the pixels and how many neighbors they have
    public static List<Vector2i> Calculate(Pixel[,] Pixels, int X1, int Y1, int X2, int Y2) {
        var Points = new List<Vector2i>();

        for (int x = X1; x < X2; x++) {
            for (int y = Y1; y < Y2; y++) {
                var Pixel = Pixels[x, y];
                if (Pixel.ID > 1 && Pixel.Settled) {
                    var EmptyCount = 0;
                    if (x - 1 >= 0 && Pixels[x - 1, y].ID > 0) { EmptyCount++; } else if (x - 1 <= X1) { EmptyCount++; }
                    if (x + 1 < X2 && Pixels[x + 1, y].ID > 0) { EmptyCount++; } else if (x + 1 >= X2) { EmptyCount++; }
                    if (y - 1 >= 0 && Pixels[x, y - 1].ID > 0) { EmptyCount++; } else if (y - 1 <= Y1) { EmptyCount++; }
                    if (y + 1 < Y2 && Pixels[x, y + 1].ID > 0) { EmptyCount++; } else if (y + 1 >= Y2) { EmptyCount++; }

                    if (EmptyCount == 2 || EmptyCount == 3) {
                        Points.Add(new Vector2i(x, y));
                    }
                }
            }
        }

        return Points;
    }


    //
    // Douglas Puecker Line Simplification Algorithm
    // Reference: https://gist.github.com/ahancock1/0d99b43c4c01ef9b4fe4a5e7ad1e9029

    // Segment class used for the Douglas Puecker line simplification algorithm
    private class Segment {
        public int Start { get; set; }          // Start point of the line segment
        public int End { get; set; }            // End poitn of the line segment
        public int Furthest { get; set; }       // The index of the furthest perpendicular point on the line
        public float Distance { get; set; }     // The distance of the furthest perpendicular point on the line
    }

    // Create a new line segment with the given start and end indices
    private static Segment CreateSegment(int Start, int End, List<Vector2i> Points) {
        var Count = End - Start;
        
        if (Count >= MinPointsToSimplify - 1) {
            var First = Points[Start];
            var Last = Points[End];

            var Max = Points.GetRange(Start + 1, Count - 1).Select(
                (Point, Index) => new {
                    Index = Start + 1 + Index,
                    Distance = GetDistance(First, Last, Point)
                }).OrderByDescending(P => P.Distance).First();

            return new Segment {
                Start = Start,
                End = End,
                Furthest = Max.Index,
                Distance = Max.Distance
            };
        }

        return new Segment {
            Start = Start,
            End = End,
            Furthest = -1
        };
    }

    // Check if the values of a point are valid, returns false if not (conditions may change in the future)
    private static bool IsValid(Vector2i Point) {
        return Point.X >= 0 && Point.Y >= 0;
    }

    // Split a segment at the furthest (perpendicular) index and return the two resulting segments
    private static IEnumerable<Segment> SplitSegment(Segment Segment, List<Vector2i> Points) {
        return new[] {
            CreateSegment(Segment.Start, Segment.Furthest, Points),
            CreateSegment(Segment.Furthest, Segment.End, Points)
        };
    }

    // Returns the initial segment for the algorithm, if any points contain invalid values, returns multiple segments for each side of the null point
    private static IEnumerable<Segment> GetSegments(List<Vector2i> Points) {
        var Previous = 0;

        foreach (var P in Points.Select((P, I) => new {
            Point = P,
            Index = I
        }).Where(P => !IsValid(P.Point))) {
            yield return CreateSegment(Previous, P.Index - 1, Points);
            Previous = P.Index + 1;
        }

        yield return CreateSegment(Previous, Points.Count - 1, Points);
    }

    // Calculate the perpendicular distance of a point relative to two other points
    private static float GetDistance(Vector2i Start, Vector2i End, Vector2i Point) {
        var X = End.X - Start.X;
        var Y = End.Y - Start.Y;

        var M = X * X + Y * Y;
        var U = ((Point.X - Start.X) * X + (Point.Y - Start.Y) * Y) / M;

        if (U < 0) {
            X = Start.X;
            Y = Start.Y;
        } else if (U > 1) {
            X = End.X;
            Y = End.Y;
        } else {
            X = Start.X + U * X;
            Y = Start.Y + U * Y;
        }

        X = Point.X - X;
        Y = Point.Y - Y;

        return (float)Math.Sqrt(X * X + Y * Y); // Math.Sqrt can be slow, maybe use something else (or maybe this should just return double instead of casting to float?)
    }

    // Returns the reduced points from the given segment
    private static IEnumerable<Vector2i> GetPoints(Segment Segment, int Count, int Index, List<Vector2i> Points) {
        yield return Points[Segment.Start];

        var Next = Segment.End + 1;

        var IsGap = Next < Points.Count && !IsValid(Points[Next]);

        if (Index == Count - 1 || IsGap) {
            yield return Points[Segment.End];

            if (IsGap) {
                yield return Points[Next];
            }
        }
    }

    // Reduce the segments until the specified maximum, the tolerance has been met, or the points can no longer be reduced
    private static void Reduce(ref List<Segment> Segments, List<Vector2i> Points, int max, float tolerance) {
        var Gaps = Points.Count(P => !IsValid(P));

        while (Segments.Count + Gaps < max - 1) {
            // Get the largest perpendicular distance segment
            var Current = Segments.OrderByDescending(S => S.Distance).First();

            // Check if the tolerance has been met or the segment can no longer be reduced
            if (Current.Distance <= tolerance) {
                break;
            }

            Segments.Remove(Current);

            var Split = SplitSegment(Current, Points);

            Segments.AddRange(Split);
        }
    }

    // Use the Douglas Puecker line simplification algorithm to reduce the number of vertices in a bounding area calculatede above
    public static IEnumerable<Vector2i> Simplify(List<Vector2i> Points, int max, float tolerance=0.0f) {
        if (max < MinPointsToSimplify || Points.Count < max) {
            return Points;
        }

        var Segments = GetSegments(Points).ToList();

        Reduce(ref Segments, Points, max, tolerance);

        return Segments
            .OrderBy(P => P.Start)
            .SelectMany((S, I) => GetPoints(S, Segments.Count, I, Points));
    }
}