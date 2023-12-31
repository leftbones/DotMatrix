using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

enum HitboxShape { Box, Ball, Chain };

/// <summary>
/// Simulates the entity's physics during the Box2D physics world simulation
/// Required Tokens: Transform
/// Optional Tokens: PixelMap
/// </summary>

class Box2D : Token {
    public World World { get; private set; }
    public BodyType BodyType { get; private set; }
    public HitboxShape HitboxShape { get; private set; }
    public float? Width { get; private set; }
    public float? Height { get; private set; }
    public int? Radius { get; private set; }

    public Body Body { get; private set; }
    public Shape Shape { get; private set; }

    public Vector2 Position { get { return Body.GetPosition(); } }
    public Vector2 Size { get { return new Vector2(Width is null ? (float)Radius! : (float)Width, Height is null ? (float)Radius! : (float)Height); } }

    public Rectangle Rect { get { return new Rectangle(Position.X, Position.Y, Size.X, Size.Y); } }
    public Vector2 Origin { get { return new Vector2(Size.X / 2, Size.Y / 2); } }

    public Vector2i MatrixPosition { get { return new Vector2i(Position.X * Global.MatrixScale, Position.Y * Global.MatrixScale); } }

    public Vector2 ScaledSize { get { return Size * Global.PTM * Global.MatrixScale; } }
    public Rectangle ScaledRect { get { return new Rectangle(Position.X * Global.PTM, Position.Y * Global.PTM, ScaledSize.X, ScaledSize.Y); } }
    public Vector2 ScaledOrigin { get { return Origin * Global.PTM; } }

    public Fixture Fixture { get; private set; }

    private BodyDef BodyDef;
    private FixtureDef FixtureDef;

    // Chain Shape
    public Box2D(World world, Vector2i position, BodyType body_type, bool fixed_rotation, ChainShape chain_shape) {
        World = world;
        BodyType = body_type;
        HitboxShape = HitboxShape.Chain;

        // BodyDef + Body
        BodyDef = new BodyDef
        {
            type = body_type,
            position = position.ToVector2() / Global.MatrixScale
        };

        Body = World.CreateBody(BodyDef);
        Body.SetFixedRotation(fixed_rotation);

        Shape = chain_shape;

        // Fixture
        FixtureDef = new FixtureDef {
            shape = Shape,
            density = 1.0f,
            friction = 0.3f,
            restitution = 0.1f
        };

        Fixture = Body.CreateFixture(FixtureDef);

        // Finish
        Box2DSystem.Register(this);
    }

    // Box + Ball Shape
    public Box2D(World world, Vector2i position, BodyType body_type, bool fixed_rotation, HitboxShape hitbox_shape, float? width=null, float? height=null, int? radius=null, float? bounciness=null) {
        World = world;
        BodyType = body_type;

        // Shape
        HitboxShape = hitbox_shape;
        var BoxShape = new PolygonShape();
        var BallShape = new CircleShape();

        if (HitboxShape == HitboxShape.Box) {
            Width = width * 2;
            Height = height * 2;
            BoxShape.SetAsBox((float)width!, (float)height!);
            Shape = BoxShape;
        } else {
            Radius = radius;
            BallShape.Radius = (float)radius!;
            Shape = BallShape;
        }

        Radius = radius;


        // BodyDef + Body
        BodyDef = new BodyDef {
            type = body_type,
            position = position.ToVector2() / Global.MatrixScale
        };

        Body = World.CreateBody(BodyDef);
        Body.SetFixedRotation(fixed_rotation);


        // Fixture
        FixtureDef = new FixtureDef {
            shape = HitboxShape == HitboxShape.Box ? BoxShape : BallShape,
            density = 1.0f,
            friction = 0.3f,
            restitution = bounciness ?? 0.1f
        };

        Fixture = Body.CreateFixture(FixtureDef);

        // Finish
        Box2DSystem.Register(this);
    }

    public override void Update(float delta) {
        if (BodyType == BodyType.Static) {
            return;
        }

        var Transform = Entity?.GetToken<Transform>()!;
        var PixelMap = Entity?.GetToken<PixelMap>();

        Transform.Position = new Vector2i(
            Math.Ceiling(Position.X * Global.MatrixScale),
            Math.Ceiling(Position.Y * Global.MatrixScale)
        );

        if (PixelMap is not null) {
            for (int x = 0; x < PixelMap.Width; x++) {
                for (int y = 0; y < PixelMap.Height; y++) {
                    var Pixel = PixelMap.Pixels[x, y];
                    if (Pixel is null) continue;

                    Pixel.Position = Transform.Position - PixelMap.Origin + new Vector2i(x, y);
                }
            }
        }

        Transform!.Rotation = Body.GetAngle() * RAD2DEG;
    }
}