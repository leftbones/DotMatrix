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

    // Box2D
    public Vector2 Gravity { get; private set; }
    public World World { get; private set; }

    public List<Entity> Bodies { get; private set; }

    private float TimeStep = 1.0f / 144.0f;
    private int VelocityIterations = 6;
    private int PositionIterations = 2;

    // Settings
    private bool Active = true;
    private bool DebugDraw = true;

    private Raylib_cs.Color DebugColor = new Raylib_cs.Color(0, 255, 255, 50);

    public Physics(Engine engine) {
        Engine = engine;

        Gravity = new Vector2(0.0f, 10.0f);
        World = new World(Gravity);

        Bodies = new List<Entity>();

        // Finish
        Pepper.Log("Physics initialized", LogType.PHYSICS);

        // TESTING
        var Platform = new Entity();
        var PlatformPos = new Vector2i(Matrix.Size.X / 2, Matrix.Size.Y - 100);
        // Platform.AddToken(new Render());
        // Platform.AddToken(new PixelMap(101, 200, 20));
        Platform.AddToken(new Transform(PlatformPos));
        Platform.AddToken(new Box2D(World, PlatformPos, BodyType.Static, false, HitboxShape.Box, 20, 2));

        Engine.Entities.Add(Platform);
        Bodies.Add(Platform);
    }

    public void SetActive(bool flag) {
        Active = flag;
        var FlagStr = Active ? "active" : "inactive";
        Pepper.Log($"Physics simulation is now {FlagStr}", LogType.PHYSICS);
    }

    public void Update() {
        if (!Active) return;

        // TESTING
        if (IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) && IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) {
            var MousePosAdj = (((Engine.Camera.Position - (Engine.WindowSize / 2)) / Matrix.Scale) + (Engine.Canvas.MousePos / Matrix.Scale));

            var Barrel = new Entity();
            Barrel.AddToken(new Render());
            Barrel.AddToken(new PixelMap("res/objects/barrel_pm.png", "res/objects/barrel_mm.png"));
            Barrel.AddToken(new Transform(MousePosAdj));
            Barrel.AddToken(new Box2D(World, MousePosAdj, BodyType.Dynamic, false, HitboxShape.Box));

            Engine.Entities.Add(Barrel);
            Bodies.Add(Barrel);
        }

        World.Step(TimeStep, VelocityIterations, PositionIterations);
    }

    public void Draw() {
        if (!DebugDraw) return;

        foreach (var Entity in Bodies) {
            var Box2D = Entity.GetToken<Box2D>();

            if (Box2D!.HitboxShape == HitboxShape.Box) {
                DrawRectanglePro(Box2D!.Rect, Box2D!.Origin, (Box2D!.Body.GetAngle() * RAD2DEG), DebugColor);
            } else {
                // DrawPolyLines((Box2D!.Position * Global.PTM), 16, (float)(Box2D!.Radius! * Global.PTM), (-Box2D!.Body.GetAngle() * RAD2DEG) + 45, DebugColor);
            }
        }
    }
}