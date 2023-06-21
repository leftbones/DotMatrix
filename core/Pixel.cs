using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

enum PixelType { Element, Particle };

class Pixel {
    // Attributes
    public int ID { get; set; }                     = -1;                           // Unique identifier for comparison with other Pixels
    public Body? Body { get; set; }                 = null;                         // Physics body this Pixel belongs to (null when not part of a Body)

    // Position
    public Vector2i Position { get; set; }          = Vector2i.Zero;                // Current position within the Matrix
    public Vector2i LastPosition { get; set; }      = Vector2i.Zero;                // Previous position within the Matrix

    // State
    public PixelType PixelType { get; set; }        = PixelType.Element;            // Determines which rules are used when performing the Step method
    public int Health {  get; set; }                = 1;                            // How much "damage" a Pixel can take before it is destroyed
    public int Lifetime { get; set; }               = -1;                           // How long a Pixel will live before expiring naturally (-1 is no lifetime)
    public int TicksLived { get; set; }             = 0;                            // How many game ticks a Pixel has been alive for
    public bool Stepped { get; set; }               = false;                        // If a Pixel has already had it's Step method called in the current tick
    public bool Ticked { get; set; }                = false;                        // If a Pixel has already had it's Tick method called in the current tick
    public bool Acted { get; set; }                 = false;                        // If a Pixel has already had it's ActOnOther method called (successfully) in the current tick
    public bool Active { get; set; }                = true;                         // If true, call Step once each time Engine is updated

    // Properties
    public int Weight { get; set; }                 = 0;                            // Weight of a Pixel relative to air/empty space, used for Y-sorting
    public int Friction { get; set; }               = 0;                            // How likely a Pixel is to stop moving when not free falling (0 will stop only when unable to move)
    public int Flammable { get; set; }              = 0;                            // How likely a Pixel is to ignite when contacting fire (0 is inflammable)
    public int Explosive { get; set; }              = 0;                            // How likely a Pixel is to explode when on fire (0 is non-explosive) 
    public int Conductive { get; set; }             = 0;                            // How well a Pixel conducts electricity
    public int Dissolve { get; set; }               = 0;                            // How quickly a Powder will dissolve in a Liquid
    public int Dillute { get; set; }                = 0;                            // How quickly a Liquid will dillute another Liquid
    public int Fluidity { get; set; }               = 0;                            // How easily a Liquid will flow horizontally
    public int Diffusion { get; set; }              = 0;                            // How much a Gas will "wiggle" horizontally

    // Status
    public bool OnFire { get; set; }                = false;                        // Pixel is currently on fire (attempts to spread fire to neighbors)
    public bool Electrified { get; set; }           = false;                        // Pixel is electrified (attempts to transfer to conductive neighbors)
    public int Soaked { get; set; }                 = -1;                           // Pixel is soaked in a Liquid, ID of the Liquid type (-1 is none)

    // Rendering
    public Color Color { get; set; }                = new Color(0, 0, 0, 0);        // RGBA color used to render a Pixel
    public Color BaseColor { get; set; }            = new Color(0, 0, 0, 0);        // Default color of a Pixel
    public bool ColorSet { get; set; }              = false;                        // If a Pixel's color has already been set
    public int ColorOffset { get; set; }            = 0;                            // Maximum offset that can be applied to a Pixel's color (in both directions)
    public double ColorFade { get; set; }           = 255.0;                        // Used for fading opacity for Pixels with a limited Lifetime


    public Pixel(int? id=null, Vector2i? position=null, Color? color=null) {
        ID = id ?? ID;
        Position = position ?? Position;
        Color = color ?? Color;
    }

    // Act on neighboring Pixels
    public void ActOnNeighbors(Matrix M) {
        if (!Acted) {
            foreach (var Dir in Direction.ShuffledFull) {
                if (M.InBounds(Position + Dir)) {
                    var P = M.Get(Position + Dir);
                    if (ActOnOther(M, P)) {
                        Acted = true;
                        return;
                    }
                }
            }
        }
    }

    // Act on another Pixel, return true on success
    public virtual bool ActOnOther(Matrix M, Pixel O) {
        return false;
    }

    // Performed once each time the Engine updates, as long as Active is set to true
    public virtual void Step(Matrix M) {

    }

    // Performed once each time the Engine updates, after the Step method, as long as Active is set to true
    public void Tick(Matrix M) {
        TicksLived++;
        if (Lifetime > -1 && TicksLived >= Lifetime)
            Expire(M);
    }

    // Lighten or darken a Pixel's Color by the given amount
    public void ShiftColor(int amount) {
        Color = new Color(
            Math.Clamp(Color.r + amount, 0, 255),
            Math.Clamp(Color.g + amount, 0, 255),
            Math.Clamp(Color.b + amount, 0, 255),
            255
        );
    }

    // Fade a Pixel's opacity relative to the amount of remaining Lifetime
    public void FadeOpacity() {
        ColorFade -= ColorFade / (Lifetime - TicksLived);
        Color = new Color(Color.r, Color.g, Color.b, (byte)ColorFade);
    }

    // Remove a Pixel from the Matrix
    public virtual void Expire(Matrix M) {
        M.Set(Position, new Pixel());
    }
}