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
    public Body Body { get; private set; }
    public HitboxShape HitboxShape { get; private set; }

    public float? Width { get; private set; }
    public float? Height { get; private set; }

    public int? Radius { get; private set; }

    public Vector2 Position { get { return Body.GetPosition(); } }

    public Shape Shape { get { if (BoxShape is null) return BallShape!; else return BoxShape; } }

    public Rectangle Rect { get { return new Rectangle(Position.X * Global.PTM, Position.Y * Global.PTM, (float)Width! * Global.PTM, (float)Height! * Global.PTM); } }
    public Vector2 Origin { get { return new Vector2(((float)Width! / 2) * Global.PTM, ((float)Height! / 2) * Global.PTM); } }

    public PolygonShape? BoxShape;
    private CircleShape? BallShape;

    private BodyDef BodyDef;
    private FixtureDef FixtureDef;

    public Box2D(World world, Vector2i position, BodyType body_type, bool fixed_rotation, HitboxShape hitbox_shape, float? width=null, float? height=null, int? radius=null, float? bounciness=null) {
        World = world;

        // Shape
        HitboxShape = hitbox_shape;

        if (HitboxShape == HitboxShape.Box) {
            if (width is null || height is null) {
                
            } else {
                Width = width * 2;
                Height = height * 2;
            }

            BoxShape = new PolygonShape();
            BoxShape!.SetAsBox((float)width!, (float)height!);
        } else {
            Radius = radius;
            BallShape = new CircleShape();
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

        FixtureDef.shape = Shape;
        FixtureDef.density = 1.0f;
        FixtureDef.friction = 0.3f;
        FixtureDef.restitution = bounciness ?? 0.1f;

        Body.CreateFixture(FixtureDef);

        // Finish
        Box2DSystem.Register(this);
    }

    public override void Update(float delta) {
        var Transform = Entity?.GetToken<Transform>();
        Transform!.Position = new Vector2i(Position.X * Global.MatrixScale, Position.Y * Global.MatrixScale);
    }
}