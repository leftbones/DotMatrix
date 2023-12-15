namespace DotMatrix;

class SnowTest : Test {
    public SnowTest(Engine engine, int duration) : base(engine, duration) {

    }

    public override void Tick(Engine E) {
        var SnowPos = new Vector2i(
            Camera.Position.X + RNG.Range(-Engine.WindowSize.X / 2, Engine.WindowSize.X / 2),
            Camera.Position.Y - Engine.WindowSize.Y / 2
        ) / Global.MatrixScale;
        var SnowID = from Element in Atlas.Powder where Element.Name == "Snow" select Element.ID;

        E.Matrix.Set(SnowPos, new Powder(SnowID.First(), SnowPos));

        base.Tick(E);
    }
}