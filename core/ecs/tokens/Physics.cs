using System.Numerics;

namespace DotMatrix;

/// <summary>
/// Defines many physical attributes and properties of an entity to control how they behave in the world, requires a Hitbox token
/// Required Tokens: Transform, Hitbox
/// </summary>

class Physics : Token {
    public bool Static { get; private set; }                    = false;                // Entity does not move
    public bool Projectile { get; private set; }                = false;                // Entity is a projectile

    public Vector2i GravityDirection { get; private set; }      = Direction.Down;       // Direction to apply gravity

    public bool FloatOnLiquids { get; private set; }            = false;                // This entity should float to the top of liquids
    public bool FallThroughPowders { get; private set; }        = false;                // This entity falls through powders rather than resting on top of them

    public bool AutoClean { get; private set; }                 = false;                // This entity should be destroyed when possible (obscured, offscreen, etc.)
}