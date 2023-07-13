using System.Numerics;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;

namespace DotMatrix;

class Rigidbody {
    public World World { get; private set; }
    public float PTM { get; private set; }

    public Body Body { get; set; }
    public BodyDef BodyDef { get; set; }
    public FixtureDef FixtureDef { get; set; }

    public Vector2 Position { get { return Body.GetPosition(); } }

    public Rigidbody(World world, float ptm, Vector2 position, bool fixed_rotation) {
        World = world;
        PTM = ptm;

        BodyDef = new BodyDef();
        BodyDef.type = BodyType.Dynamic;
        BodyDef.position = position;

        Body = World.CreateBody(BodyDef);
        Body.SetFixedRotation(fixed_rotation);

        FixtureDef = new FixtureDef();
    }
}