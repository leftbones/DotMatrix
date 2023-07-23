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

    private Raylib_cs.Color DebugColor = new Raylib_cs.Color(255, 255, 0, 255);

    public Physics(Engine engine) {
        Engine = engine;

        Gravity = new Vector2(0.0f, 20.0f);
        World = new World(Gravity);

        Bodies = new List<Entity>();

        // Finish
        Pepper.Log("Physics initialized", LogType.PHYSICS);

        // TESTING
        var Platform = new Entity();
        // var PlatformPos = new Vector2i(Matrix.Size.X / 2, Matrix.Size.Y - 100);
        var PlatformPos = new Vector2i(150, 150);
        Platform.AddToken(new Render());
        Platform.AddToken(new PixelMap(101, 200, 20));
        Platform.AddToken(new Transform(PlatformPos));
        Platform.AddToken(new Box2D(World, PlatformPos, BodyType.Static, false, HitboxShape.Box, 25f, 2.5f));

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
            var MousePosAdj = (((new Vector2i(Engine.Camera.Position) - (Engine.WindowSize / 2)) / Matrix.Scale) + (Engine.Canvas.MousePos / Matrix.Scale));

            var Block = new Entity();
            Block.AddToken(new Render());
            Block.AddToken(new PixelMap(100, 16, 16));
            Block.AddToken(new Transform(MousePosAdj));
            Block.AddToken(new Box2D(World, MousePosAdj, BodyType.Dynamic, false, HitboxShape.Box, 2.0f, 2.0f));

            Engine.Entities.Add(Block);
            Bodies.Add(Block);
        }

        World.Step(TimeStep, VelocityIterations, PositionIterations);
    }

    public void Draw() {
        if (!DebugDraw) return;

        foreach (var Entity in Bodies) {
            var Box2D = Entity.GetToken<Box2D>();

            if (Box2D!.HitboxShape == HitboxShape.Box) {
                DrawRectanglePro(Box2D!.Rect, Box2D!.Origin, (Box2D!.Body.GetAngle() * RAD2DEG), DebugColor);

                var Shape = Box2D!.Fixture.Shape as PolygonShape;
                var Vertices = Shape!.GetVertices();
                List<Vector2> AdjVerts = new List<Vector2>();

                for (int i = 0; i < Vertices.Count(); i++) AdjVerts.Add(Box2D!.Body.GetWorldPoint(Vertices[i]) * Global.PTM);
                AdjVerts.Add(Box2D!.Body.GetWorldPoint(Vertices[0]) * Global.PTM);

                for (int i = 1; i < AdjVerts.Count(); i++) DrawLineEx(AdjVerts[i], AdjVerts[i - 1], 1.0f, DebugColor);
                DrawLineEx(AdjVerts[AdjVerts.Count() / 2], AdjVerts[AdjVerts.Count() - 1], 1.0f, DebugColor);
            } else {
                // DrawPolyLines((Box2D!.Position * Global.PTM), 16, (float)(Box2D!.Radius! * Global.PTM), (-Box2D!.Body.GetAngle() * RAD2DEG) + 45, DebugColor);
            }
        }
    }
}