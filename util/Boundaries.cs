using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;


// Static class used to run the marching squares algorithm and Douglas Puecker line simplification algorithm on a list of points
static class Boundaries {
    public static int MinPointsToSimplify = 5;      // Minimum number of points in a line required to run the simplification algorithm


    // 
    // Marching Squares Algorithm

    // Find the state of a cell based on it's contents and the contents of it's neighbors, then add the appropriate points to a pair in a list of pairs which make up boundaries
    public static List<Tuple<Vector2, Vector2>> Calculate(Matrix M, Pixel[,] Pixels, int X1, int Y1, int X2, int Y2) {
        var Lines = new List<Tuple<Vector2, Vector2>>();

        for (int x = X1; x < X2 - 1; x++) {
            for (int y = Y1; y < Y2 - 1; y++) {
                // if (!Pixels[x, y].Settled) {
                //     continue;
                // }

                var A = new Vector2(x + 0.5f, y);
                var B = new Vector2(x + 1, y + 0.5f);
                var C = new Vector2(x + 0.5f, y + 1);
                var D = new Vector2(x, y + 0.5f);

                var State = GetState(
                    M.InBoundsAndEmpty(x, y) ? 1 : 0,
                    M.InBoundsAndEmpty(x + 1, y) ? 1 : 0,
                    M.InBoundsAndEmpty(x + 1, y + 1) ? 1 : 0,
                    M.InBoundsAndEmpty(x, y + 1) ? 1 : 0
                );

                switch (State) {
                    case 1:
                        Lines.Add(new Tuple<Vector2, Vector2>(D, C));
                        break;
                    case 2:
                        Lines.Add(new Tuple<Vector2, Vector2>(C, B));
                        break;
                    case 3:
                        Lines.Add(new Tuple<Vector2, Vector2>(D, B));
                        break;
                    case 4:
                        Lines.Add(new Tuple<Vector2, Vector2>(B, A));
                        break;
                    case 5:
                        Lines.Add(new Tuple<Vector2, Vector2>(D, A));
                        Lines.Add(new Tuple<Vector2, Vector2>(C, B));
                        break;
                    case 6:
                        Lines.Add(new Tuple<Vector2, Vector2>(C, A));
                        break;
                    case 7:
                        Lines.Add(new Tuple<Vector2, Vector2>(D, A));
                        break;
                    case 8:
                        Lines.Add(new Tuple<Vector2, Vector2>(A, D));
                        break;
                    case 9:
                        Lines.Add(new Tuple<Vector2, Vector2>(A, C));
                        break;
                    case 10:
                        Lines.Add(new Tuple<Vector2, Vector2>(D, C));
                        Lines.Add(new Tuple<Vector2, Vector2>(A, B));
                        break;
                    case 11:
                        Lines.Add(new Tuple<Vector2, Vector2>(A, B));
                        break;
                    case 12:
                        Lines.Add(new Tuple<Vector2, Vector2>(B, D));
                        break;
                    case 13:
                        Lines.Add(new Tuple<Vector2, Vector2>(B, C));
                        break;
                    case 14:
                        Lines.Add(new Tuple<Vector2, Vector2>(C, D));
                        break;
                    case 15:
                        break;
                }
            }
        }

        return Lines;
    }

    // Get the state of a subgrid square based on the value of it's four corners
    public static int GetState(int A, int B, int C, int D) {
        // return A * 8 + B * 4 + C * 2 + D * 1;
        var Binary = $"{A}{B}{C}{D}";
        return Convert.ToInt32(Binary, 2);
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
    private static Segment CreateSegment(int Start, int End, List<Vector2> Points) {
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
    private static bool IsValid(Vector2 Point) {
        return Point.X >= 0 && Point.Y >= 0;
    }

    // Split a segment at the furthest (perpendicular) index and return the two resulting segments
    private static IEnumerable<Segment> SplitSegment(Segment Segment, List<Vector2> Points) {
        return new[] {
            CreateSegment(Segment.Start, Segment.Furthest, Points),
            CreateSegment(Segment.Furthest, Segment.End, Points)
        };
    }

    // Returns the initial segment for the algorithm, if any points contain invalid values, returns multiple segments for each side of the null point
    private static IEnumerable<Segment> GetSegments(List<Vector2> Points) {
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
    private static float GetDistance(Vector2 Start, Vector2 End, Vector2 Point) {
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
    private static IEnumerable<Vector2> GetPoints(Segment Segment, int Count, int Index, List<Vector2> Points) {
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
    private static void Reduce(ref List<Segment> Segments, List<Vector2> Points, int max, float tolerance) {
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
    public static IEnumerable<Vector2> Simplify(List<Vector2> Points, int max, float tolerance=0.0f) {
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