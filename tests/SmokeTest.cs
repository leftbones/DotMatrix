namespace DotMatrix;

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