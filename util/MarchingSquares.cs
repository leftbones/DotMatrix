namespace DotMatrix;

static class MarchingSquares {
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
}