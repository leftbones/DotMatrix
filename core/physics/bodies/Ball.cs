using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Ball : Rigidbody {
    public CircleShape Shape { get; private set; }
    public float Radius { get { return Shape.Radius; } }

    private Raylib_cs.Color MainColor = new Raylib_cs.Color(0, 255, 255, 100);
    private Raylib_cs.Color OutlineColor = new Raylib_cs.Color(0, 255, 255, 255);

    public Ball(World world, float ptm, Vector2 position, float radius, bool fixed_rotation=false) : base(world, ptm, position, fixed_rotation) {
        Shape = new CircleShape();
        Shape.Radius = radius;

        FixtureDef.shape = Shape;
        FixtureDef.density = 1.0f;
        FixtureDef.friction = 0.3f;
        FixtureDef.restitution = 0.5f;
        Body.CreateFixture(FixtureDef);
    }

    public void Draw() {
        DrawPoly(Position * PTM, 16, Radius * PTM, Body.GetAngle() * RAD2DEG, MainColor);
        DrawPolyLines(Position * PTM, 16, Radius * PTM, Body.GetAngle() * RAD2DEG, OutlineColor);

        var EndPoint = new Vector2(
            Position.X + Radius * (float)Math.Cos(Body.GetAngle()),
            Position.Y + Radius * (float)Math.Sin(Body.GetAngle())
        );
        DrawLine((int)(Position.X * PTM), (int)(Position.Y * PTM), (int)(EndPoint.X * PTM), (int)(EndPoint.Y * PTM), OutlineColor);
    }
}