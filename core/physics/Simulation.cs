using System.Numerics;
using Box2D.NetStandard.Common;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Simulation {
    // Core
    public Engine Engine { get; private set; }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Canvas Canvas { get { return Engine.Canvas; } }
    public Interface Interface { get { return Engine.Interface; } }
    public Pepper Pepper { get { return Engine.Pepper; } }

    // World
    public World World { get; private set; }
    public Vector2 Gravity { get; private set; }

    // Properties
    private float TimeStep = 1.0f / 60.0f;

    private int VelocityIterations = 6;
    private int PositionIterations = 2;

    // Settings
    private bool Active = false;
    private bool DebugDraw = false;

    // Testing
    private BodyDef TestBodyDef;
    private Body TestBody;
    private PolygonShape TestShape;
    private FixtureDef TestFixtureDef;

    public Simulation(Engine engine) {
        Engine = engine;

        Gravity = new Vector2(0.0f, 10.0f);
        World = new World(Gravity);

        // Testing
        TestBodyDef = new BodyDef();
        TestBodyDef.position = new Vector2(100.0f, 100.0f);
        TestBody = World.CreateBody(TestBodyDef);

        PolygonShape TestShape = new PolygonShape();
        TestShape.SetAsBox(15.0f, 15.0f);

        TestFixtureDef = new FixtureDef();
        TestFixtureDef.shape = TestShape;
        TestFixtureDef.density = 1.0f;
        TestFixtureDef.friction = 0.3f;

        TestBody.CreateFixture(TestFixtureDef);
        TestBody.SetAwake(true);

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

        TestBody.ApplyLinearImpulseToCenter(new Vector2(2000.0f, 0.0f), true);
        World.Step(GetFrameTime(), VelocityIterations, PositionIterations);
    }

    public void Draw() {
        if (!Active) return;

        var Pos = TestBody.GetPosition();
        DrawRectangleLines((int)Pos.X, (int)Pos.Y, 30, 30, Raylib_cs.Color.GREEN);
    }
}