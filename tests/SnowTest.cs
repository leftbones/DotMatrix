namespace DotMatrix;

class SnowTest : Test {
    public SnowTest(Engine engine, int duration) : base(engine, duration) {

    }

    public override void Tick(Engine E) {
        var SnowPos = new Vector2i(
            Camera.Position.X + RNG.Range(-Engine.WindowSize.X / 2, Engine.WindowSize.X / 2),
            Camera.Position.Y - Engine.WindowSize.Y / 2
        ) / Global.MatrixScale;
        var SnowID = Atlas.GetIDFromName("Snow");

        E.Matrix.Set(SnowPos, new Powder(SnowID, SnowPos));

        base.Tick(E);
    }
}