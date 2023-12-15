namespace DotMatrix;

/// <summary>
/// For each cell in the Matrix, have a 50% chance to set a Smoke pixel there. Gases are the worst in terms of performance, so this is a good way to test the FPS.
/// </summary>

class SmokeTest : Test {
    public SmokeTest(Engine engine, int duration=0) : base(engine, duration) {
        for (int x = 0; x < Matrix.Size.X; x++) {
            for (int y = 0; y < Matrix.Size.Y; y++) {
                if (RNG.CoinFlip()) {
                    var Pos = new Vector2i(x, y);
                    Matrix.Set(Pos, new Gas(Atlas.GetIDFromName("Smoke"), Pos));
                }
            }
        }
    }
}