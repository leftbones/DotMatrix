using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Dot {
    // Attributes
    public int ID { get; private set; }             = -1;                           // Unique identifier for comparison with other Dots

    // Position
    public Vector2i Position { get; set; }          = Vector2i.Zero;                // Current position within the Matrix
    public Vector2i LastPosition { get; set; }      = Vector2i.Zero;                // Previous position within the Matrix

    // State
    public bool Active { get; set; }                = true;                         // If true, call Step once each time Engine is updated

    // Rendering
    public Color Color { get; set; }                = new Color(0, 0, 0, 0);        // RGBA color used to render the Dot
    public int ColorOffset { get; set; }            = 0;


    public Dot(int? id=null, Vector2i? position=null, Color? color=null) {
        ID = id ?? ID;
        Position = position ?? Position;
        Color = color ?? Color;
    }

    // Performed once each time the Engine updates, as long as Active is set to true
    public virtual void Step(Matrix M) {

    }

    // Lighten or darken the Dot's Color by the given amount
    public void ShiftColorValue(int amount) {
        Color = new Color(
            Math.Clamp(Color.r + amount, 0, 255),
            Math.Clamp(Color.g + amount, 0, 255),
            Math.Clamp(Color.b + amount, 0, 255),
            255
        );
    }
}