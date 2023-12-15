namespace DotMatrix;

/// <summary>
/// Simulates rainfall!
/// </summary>

class RainTest : Test {
    private int Intensity;

    public RainTest(Engine engine, int duration, int intensity=1) : base(engine, duration) {
        Intensity = intensity;
    }

    public override void Tick() {
        for (int i = 0; i < Intensity; i++) {
            var Pos = new Vector2i(
                Camera.Position.X + RNG.Range(-Engine.WindowSize.X / 2, Engine.WindowSize.X / 2),
                Camera.Position.Y - Engine.WindowSize.Y / 2
            ) / Global.MatrixScale;
            var ID = Atlas.GetIDFromName("Water");

            if (Matrix.InBoundsAndEmpty(Pos)) {
                Matrix.Set(Pos, new Liquid(ID, Pos));
            }
        }

        base.Tick();
    }
}