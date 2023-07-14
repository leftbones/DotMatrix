using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class PhysicsOld {
    // Core
    public Engine Engine { get; private set; }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Canvas Canvas { get { return Engine.Canvas; } }
    public Interface Interface { get { return Engine.Interface; } }
    public Pepper Pepper { get { return Engine.Pepper; } }

    // Settings
    private bool Active = true;
    // private bool DebugDraw = false;

    // Testing
    public Vector2 Gravity { get; private set; }
    public World World { get; private set; }

    public List<Ball> Balls { get; private set; }
    public List<Box> Boxes { get; private set; }

    private float TimeStep = 1.0f / 144.0f;
    private int VelocityIterations = 6;
    private int PositionIterations = 2;

    private float PTM = 16;

    public PhysicsOld(Engine engine) {
        Engine = engine;

        // Testing
        Gravity = new Vector2(0.0f, 10.0f);
        World = new World(Gravity);

        Balls = new List<Ball>();
        Boxes = new List<Box>();

        // Finish
        Pepper.Log("Simulaton initialized", LogType.SIMULATION);
    }

    public void SetActive(bool flag) {
        Active = flag;
        var FlagStr = Active ? "active" : "inactive";
        Pepper.Log($"PhysicsOld simulation is now {FlagStr}", LogType.SIMULATION);
    }

    public void Update() {
        if (!Active) return;

        var MousePosAdj = ((Engine.Camera.Position - (Engine.WindowSize.ToVector2() / 2)) + Engine.Canvas.MousePos.ToVector2()) / PTM;

        if (IsKeyDown(KeyboardKey.KEY_ONE)) {
            var Box = new Box(World, PTM, MousePosAdj, 1.0f, 1.0f);
            Boxes.Add(Box);
        }

        if (IsKeyDown(KeyboardKey.KEY_TWO)) {
            var Ball = new Ball(World, PTM, MousePosAdj, 1.0f);
            Balls.Add(Ball);
        }

        World.Step(TimeStep, VelocityIterations, PositionIterations);
    }

    public void Draw() {
        if (!Active) return;

        foreach (var Ball in Balls)
            Ball.Draw();

        foreach (var Box in Boxes)
            Box.Draw();
    }
}