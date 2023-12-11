using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

/// <summary>
/// Renders an entity's texture to the screen
/// Required Tokens: Transform, PixelMap (if sprite_path is not specified)
/// Optional Tokens: PixelMap
/// </summary>

class Render : Token {
    public Texture2D Texture { get; set; }

    public int Width { get { return Texture.width; } }
    public int Height { get { return Texture.height; } }

    public Rectangle Rect { get { return new Rectangle(0, 0, Width, Height); } }

    public Render(string? sprite_path=null) {
        if (sprite_path is not null) {
            Texture = LoadTexture(sprite_path);
        }

        // Finish
        RenderSystem.Register(this);
    }

    public override void Update(float delta) {
        if (Entity is null) return;

        var Transform = Entity!.GetToken<Transform>();

        var DestRect = new Rectangle(
            Transform!.Position.X - (Width / 2 * Global.MatrixScale),
            Transform!.Position.Y - (Height / 2 * Global.MatrixScale),
            Width * Global.MatrixScale,
            Height * Global.MatrixScale
        );

        float Rotation = Transform!.Rotation;
        DrawTexturePro(Texture, Rect, DestRect, Vector2.Zero, Rotation, Color.WHITE);
    }
}