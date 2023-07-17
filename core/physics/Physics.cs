using System.Numerics;
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

    public Vector2 Gravity { get; private set; }
    public World World { get; private set; }

    public List<Rigidbody> Bodies { get; private set; }

    private float TimeStep = 1.0f / 144.0f;
    private int VelocityIterations = 6;
    private int PositionIterations = 2;

    private float PTM;

    // Settings
    private bool Active = true;

    public Physics(Engine engine) {
        Engine = engine;

        Gravity = new Vector2(0.0f, 10.0f);
        World = new World(Gravity);

        Bodies = new List<Rigidbody>();

        PTM = Matrix.Scale * Matrix.Scale;

        // Finish
        Pepper.Log("Physics initialized", LogType.PHYSICS);
    }

    public void SetActive(bool flag) {
        Active = flag;
        var FlagStr = Active ? "active" : "inactive";
        Pepper.Log($"Physics simulation is now {FlagStr}", LogType.PHYSICS);
    }

    public void Update() {
        if (!Active) return;

        // var MousePosAdj = ((Engine.Camera.Position - (Engine.WindowSize.ToVector2() / 2)) + Engine.Canvas.MousePos.ToVector2()) / PTM;

        World.Step(TimeStep, VelocityIterations, PositionIterations);
    }
}