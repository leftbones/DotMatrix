using System.Numerics;
using Box2D.NetStandard.Common;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Physics {
    // Core
    public Engine Engine { get; private set; }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Canvas Canvas { get { return Engine.Canvas; } }
    public Interface Interface { get { return Engine.Interface; } }
    public Pepper Pepper { get { return Engine.Pepper; } }

    // Settings
    private bool Active = true;
    private bool DebugDraw = false;

    // Testing
    public Vector2 Gravity { get; private set; }
    public World World { get; private set; }

    private float TimeStep = 1.0f / 240.0f;
    private int VelocityIterations = 6;
    private int PositionIterations = 2;

    private Body TestBody;
    private PolygonShape TestShape;
    private Body GroundBody;
    private PolygonShape GroundShape;

    private float PTM = 16;

    public Physics(Engine engine) {
        Engine = engine;

        // Testing
        Gravity = new Vector2(0.0f, 10.0f);
        World = new World(Gravity);

        // Ground
        var GroundBodyDef = new BodyDef();
        GroundBodyDef.type = BodyType.Static;
        GroundBodyDef.position = new Vector2(50.0f, 50.0f);
        GroundBody = World.CreateBody(GroundBodyDef);

        GroundShape = new PolygonShape();
        GroundShape.SetAsBox(5.0f, 1.0f);

        var GroundFixtureDef = new FixtureDef();
        GroundFixtureDef.shape = GroundShape;

        GroundBody.CreateFixture(GroundFixtureDef);

        // Body
        var TestBodyDef = new BodyDef();
        TestBodyDef.type = BodyType.Dynamic;
        TestBodyDef.allowSleep = true;
        TestBodyDef.awake = true;
        TestBodyDef.position = new Vector2(50.0f, 30.0f);
        TestBody = World.CreateBody(TestBodyDef);

        TestShape = new PolygonShape();
        TestShape.SetAsBox(0.5f, 0.5f);

        var TestFixtureDef = new FixtureDef();
        TestFixtureDef.shape = TestShape;
        TestFixtureDef.density = 1.0f;
        TestFixtureDef.friction = 0.3f;

        TestBody.CreateFixture(TestFixtureDef);

        // Finish
        Pepper.Log("Simulaton initialized", LogType.SIMULATION);
    }

    public void SetActive(bool flag) {
        Active = flag;
        var FlagStr = Active ? "active" : "inactive";
        Pepper.Log($"Physics simulation is now {FlagStr}", LogType.SIMULATION);
    }

    public void Update() {
        if (!Active) return;

        if (IsKeyDown(KeyboardKey.KEY_P))
            TestBody.ApplyLinearImpulseToCenter(new Vector2(0.0f, -1.0f));

        World.Step(TimeStep, VelocityIterations, PositionIterations);
    }

    public void Draw() {
        if (!Active) return;

        var BodyPos = TestBody.GetPosition();
        var BodyRect = new Rectangle(BodyPos.X * PTM, BodyPos.Y * PTM, 1.0f * PTM, 1.0f * PTM);
        DrawRectanglePro(BodyRect, new Vector2(0.5f * PTM, 0.5f * PTM), TestBody.GetAngle(), new Raylib_cs.Color(0, 255, 255, 100));
        // DrawRectangleLines((int)BodyPos.X - 2, (int)BodyPos.Y - 2, 4, 4, new Raylib_cs.Color(0, 255, 255, 255));

        var GroundBodyPos = GroundBody.GetPosition();
        var GroundRect = new Rectangle(GroundBodyPos.X * PTM, GroundBodyPos.Y * PTM, 10.0f * PTM, 2.0f * PTM);
        DrawRectanglePro(GroundRect, new Vector2(5.0f * PTM, 1.0f * PTM), 0.0f, new Raylib_cs.Color(0, 255, 0, 100));
        // DrawRectangleLines((int)GroundBodyPos.X - 20, (int)GroundBodyPos.Y - 2, 40, 4, new Raylib_cs.Color(0, 255, 0, 255));
    }
}