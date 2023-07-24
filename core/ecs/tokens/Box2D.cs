using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

enum HitboxShape { Box, Ball };

class Box2D : Token {
    public World World { get; private set; }
    public BodyType BodyType { get; private set; }
    public HitboxShape HitboxShape { get; private set; }
    public float? Width { get; private set; }
    public float? Height { get; private set; }
    public int? Radius { get; private set; }

    public Body Body { get; private set; }

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
        } else {
            Radius = radius;
            BallShape.Radius = (float)radius!;
        }

        Radius = radius;


        // BodyDef + Body
        BodyDef = new BodyDef();
        BodyDef.type = body_type;
        BodyDef.position = position.ToVector2() / Global.MatrixScale;

        Body = World.CreateBody(BodyDef);
        Body.SetFixedRotation(fixed_rotation);


        // Fixture
        FixtureDef = new FixtureDef();

        FixtureDef.shape = HitboxShape == HitboxShape.Box ? BoxShape : BallShape;
        FixtureDef.density = 1.0f;
        FixtureDef.friction = 0.3f;
        FixtureDef.restitution = bounciness ?? 0.1f;

        Fixture = Body.CreateFixture(FixtureDef);

        // Finish
        Box2DSystem.Register(this);
    }

    public override void Update(float delta) {
        var Transform = Entity?.GetToken<Transform>();

        Transform!.Position = new Vector2i(
            Math.Ceiling(Position.X * Global.MatrixScale),
            Math.Ceiling(Position.Y * Global.MatrixScale)
        );

        Transform!.Rotation = Body.GetAngle() * RAD2DEG;
    }
}