using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Box : Rigidbody {
    public PolygonShape Shape { get; private set; }
    public float Width { get; private set; }
    public float Height { get; private set; }

    public Rectangle Rect { get { return new Rectangle(Position.X * PTM, Position.Y * PTM, Width * PTM, Height * PTM); } }
    public Vector2 Origin { get { return new Vector2((Width / 2) * PTM, (Height / 2) * PTM); } }

    private Raylib_cs.Color MainColor = new Raylib_cs.Color(255, 165, 0, 100);
    private Raylib_cs.Color OutlineColor = new Raylib_cs.Color(255, 165, 0, 255);

    public Box(World world, float ptm, Vector2 position, float width, float height, bool fixed_rotation=false) : base(world, ptm, position, fixed_rotation) {
        Width = width * 2;
        Height = height * 2;

        Shape = new PolygonShape();
        Shape.SetAsBox(width, height);

        FixtureDef.shape = Shape;
        FixtureDef.density = 1.0f;
        FixtureDef.friction = 0.3f;
        Body.CreateFixture(FixtureDef);

    }

    public void Draw() {
        DrawRectanglePro(Rect, Origin, Body.GetAngle() * RAD2DEG, MainColor);
        DrawPolyLines(Position * PTM, 4, ((Width / 2) * 1.414f) * PTM, (-Body.GetAngle() * RAD2DEG) + 45, OutlineColor);
    }
}