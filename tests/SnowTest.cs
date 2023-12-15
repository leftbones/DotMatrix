namespace DotMatrix;

/// <summary>
/// Simulates snowfall!
/// </summary>

class SnowTest : Test {
    private int Intensity;

    public SnowTest(Engine engine, int duration, int intensity=1) : base(engine, duration) {
        Intensity = intensity;
    }

    public override void Tick() {
        for (int i = 0; i < Intensity; i++) {
            var Pos = new Vector2i(
                Camera.Position.X + RNG.Range(-Engine.WindowSize.X / 2, Engine.WindowSize.X / 2),
                Camera.Position.Y - Engine.WindowSize.Y / 2
            ) / Global.MatrixScale;
            var ID = Atlas.GetIDFromName("Snow");

            if (Matrix.InBoundsAndEmpty(Pos)) {
                Matrix.Set(Pos, new Powder(ID, Pos));
            }
        }

        base.Tick();
    }
}