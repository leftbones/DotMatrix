using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

/// <summary>
/// Simulates all active Box2D tokens, also handles the creation of new Box2D tokens by calling `Physics.CreateBody()`
/// `Physics.Update()` is called after `Matrix.UpdateEnd()`, `Physics.Draw()` is only used for debug drawing for Box2D
/// </summary>

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

    private readonly float TimeStep = 1.0f / 144.0f;
    private readonly int VelocityIterations = 6;
    private readonly int PositionIterations = 2;

    private bool Active = true;

    // Settings
    private bool DebugDraw = true;
    private bool DrawBox2DSimulation = false;
    private bool DrawPhysicsHitboxes = true;
    private bool DrawPixelMapBoxes = false;

    private Raylib_cs.Color DebugColor = new Raylib_cs.Color(255, 255, 0, 150);

    public Physics(Engine engine) {
        Engine = engine;

        Gravity = new Vector2(0.0f, 20.0f);
        World = new World(Gravity);

        Bodies = new List<Entity>();

        // Finish
        Pepper.Log("Physics initialized", LogType.PHYSICS);

        // TESTING (Adds a stone platform)
        // var Platform = new Entity();
        // var PlatformPos = new Vector2i(150, 150);
        // Platform.AddToken(new Render());
        // Platform.AddToken(new PixelMap(PlatformPos, 100, 200, 20));
        // Platform.AddToken(new Transform(PlatformPos));
        // Platform.AddToken(new Box2D(World, PlatformPos, BodyType.Static, false, HitboxShape.Box, 25f, 2.5f));

        // Engine.Entities.Add(Platform);
        // Bodies.Add(Platform);
    }

    public void SetActive(bool flag) {
        Active = flag;
        var FlagStr = Active ? "active" : "inactive";
        Pepper.Log($"Physics simulation is now {FlagStr}", LogType.PHYSICS);
    }

    public void ApplyConfig(Config C) {
        DrawBox2DSimulation = C.Items["DrawBox2DSimulation"];
        DrawPhysicsHitboxes = C.Items["DrawPhysicsHitboxes"];
        DrawPixelMapBoxes = C.Items["DrawPixelMapBoxes"];

        if (DrawBox2DSimulation || DrawPhysicsHitboxes || DrawPixelMapBoxes) DebugDraw = true;
        else DebugDraw = false;

        Pepper.Log("Physics config applied", LogType.SYSTEM);
    }

    public Box2D CreateBody(Entity E, Vector2i pos, BodyType body_type, bool fixed_rotation, HitboxShape hitbox_shape) {
        var Render = E.GetToken<Render>()!;
        var PixelMap = E.GetToken<PixelMap>()!;

        if (PixelMap is not null) {
            foreach (var Pixel in PixelMap.Pixels) {
                if (Pixel is null) continue;
                Pixel.Entity = E;
            }
        }

        float W = (PixelMap is null ? Render.Width : PixelMap.Width / (float)Global.PTM) * 2;
        float H = (PixelMap is null ? Render.Height : PixelMap.Height / (float)Global.PTM) * 2;
        return new Box2D(World, pos, body_type, fixed_rotation, hitbox_shape, W, H);

        // TODO Implement HitboxShape.Ball
    }

    public List<Box2D> CreateChunkBodies(Chunk C) {
        var Bodies = new List<Box2D>();

        foreach (var Boundary in C.Bounds) {
            var Chain = new ChainShape();
            // Chain.CreateChain(Boundary.ToArray(), Boundary.Last(), Boundary.First());
            Chain.CreateLoop(Boundary.ToArray());
            Bodies.Add(new Box2D(World, C.Position, BodyType.Static, true, Chain));
        }

        return Bodies;
    }

    public void Update() {
        if (!Active) return;

        // TESTING (Shift + Right click to create barrels)
        if (IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) && IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) {
            var MousePosAdj = (((new Vector2i(Engine.Camera.Position) - (Engine.WindowSize / 2)) / Matrix.Scale) + (Engine.Canvas.MousePos / Matrix.Scale));

            var Block = new Entity();
            Block.AddToken(new Render());
            Block.AddToken(new PixelMap(MousePosAdj, "res/objects/barrel_pm.png", "res/objects/barrel_mm.png"));
            Block.AddToken(new Transform(MousePosAdj));

            Block.AddToken(CreateBody(Block, MousePosAdj, BodyType.Dynamic, false, HitboxShape.Box));

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
                // Draw Unscaled Box2D Simulation
                if (DrawBox2DSimulation)
                    DrawRectanglePro(Box2D!.Rect, Box2D!.Origin, (Box2D!.Body.GetAngle() * RAD2DEG), DebugColor);

                // Draw Hitbox Debug Outline
                if (DrawPhysicsHitboxes) {
                    var Shape = Box2D!.Fixture.Shape as PolygonShape;
                    var Vertices = Shape!.GetVertices();
                    List<Vector2> AdjVerts = new List<Vector2>();

                    for (int i = 0; i < Vertices.Count(); i++) AdjVerts.Add(Box2D!.Body.GetWorldPoint(Vertices[i]) * Global.PTM);
                    AdjVerts.Add(Box2D!.Body.GetWorldPoint(Vertices[0]) * Global.PTM);

                    for (int i = 1; i < AdjVerts.Count(); i++) DrawLineEx(AdjVerts[i], AdjVerts[i - 1], 1.0f, DebugColor);
                    DrawLineEx(AdjVerts[AdjVerts.Count() / 2], AdjVerts[AdjVerts.Count() - 1], 1.0f, DebugColor);
                }

                // Draw PixelMap Position in Matrix
                if (DrawPixelMapBoxes) {
                    var EntityPos = Entity!.GetToken<Transform>()!.Position - Entity!.GetToken<PixelMap>()!.Origin;
                    DrawRectangle(
                        EntityPos.X * Global.MatrixScale,
                        EntityPos.Y * Global.MatrixScale,
                        (int)Box2D!.ScaledSize.X / Global.MatrixScale,
                        (int)Box2D!.ScaledSize.Y / Global.MatrixScale,
                        new Raylib_cs.Color(0, 255, 255, 50)
                    );
                }
            } else if (Box2D!.HitboxShape == HitboxShape.Ball) {
                // DrawPolyLines((Box2D!.Position * Global.PTM), 16, (float)(Box2D!.Radius! * Global.PTM), (-Box2D!.Body.GetAngle() * RAD2DEG) + 45, DebugColor);
            } else if (Box2D!.HitboxShape == HitboxShape.Chain) {
                var Chain = Box2D!.Shape as ChainShape;
                foreach (var Verts in Chain!.Vertices) {
                    var LP = Verts[0];
                    for (int i = 1; i < Chain!.Vertices.Length; i++) {
                        var P = Verts[i];
                        // Console.WriteLine($"{LP}, {P}");
                        // DrawLineEx(LP * Global.MatrixScale, P * Global.MatrixScale, 2.0f, DebugColor);
                        LP = P;
                    }
                }
            }
        }
    }
}