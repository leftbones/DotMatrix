namespace DotMatrix;

/// <summary>
/// This test is just for fun. It mimics the sand/water/salt/oil spouts from falling sand games I played as a kid.
/// </summary>

class SpoutTest : Test {
    private Vector2i StartPos;

    private Vector2i SandPos;
    private Vector2i WaterPos;
    private Vector2i SaltPos;
    private Vector2i OilPos;

    private int SandID;
    private int WaterID;
    private int SaltID;
    private int OilID;

    public SpoutTest(Engine engine, int duration) : base(engine, duration) {
        StartPos = new Vector2i(Camera.Position.X - Engine.WindowSize.X / 2, Camera.Position.Y - Engine.WindowSize.Y / 2);

        var Q = Engine.WindowSize.X / 4;
        SandPos = new Vector2i(StartPos.X - (Q / 2) + Q, StartPos.Y) / Global.MatrixScale;
        WaterPos = new Vector2i(StartPos.X - (Q / 2) + (Q * 2), StartPos.Y) / Global.MatrixScale;
        SaltPos = new Vector2i(StartPos.X - (Q / 2) + (Q * 3), StartPos.Y) / Global.MatrixScale;
        OilPos = new Vector2i(StartPos.X - (Q / 2) + (Q * 4), StartPos.Y) / Global.MatrixScale;

        SandID = Atlas.GetIDFromName("Sand");
        WaterID = Atlas.GetIDFromName("Water");
        SaltID = Atlas.GetIDFromName("Salt");
        OilID = Atlas.GetIDFromName("Oil");
    }

    public override void Tick() {
        Matrix.Set(SandPos, new Powder(SandID, SandPos));
        Matrix.Set(WaterPos, new Liquid(WaterID, WaterPos));
        Matrix.Set(SaltPos, new Powder(SaltID, SaltPos));
        Matrix.Set(OilPos, new Liquid(OilID, OilPos));

        base.Tick();
    }
}