namespace DotMatrix;

static class MarchingSquares {
    public static List<Vector2i> Calculate(Pixel[,] Pixels, int X1, int Y1, int X2, int Y2) {
        var Points = new List<Vector2i>();

        for (int x = X1; x < X2; x++) {
            for (int y = Y1; y < Y2; y++) {
                var Pixel = Pixels[x, y];
                if (Pixel.ID > 0) {
                    Points.Add(new Vector2i(x, y));
                }
            }
        }

        return Points;
    }
}