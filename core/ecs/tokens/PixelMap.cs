using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

// Creates and stores a miniature matrix of Pixels taken from a material map image and pixel map image

// Pixel Map - An image used to set the colors of the Pixels set by the Material Map image
// Material Map - An image that denotes which Pixel materials are placed where, like a silhouette of the Pixel Map

// If no material_map path is passed in, it is assumed to be 103 ("Meat"), as in a creature of some kind

class PixelMap : Token {
    public Vector2i Position { get; private set; }
    public int? MaterialID { get; private set; } = null;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public Pixel[,] Pixels { get; private set; }

    public Vector2i Origin { get { return new Vector2i(Width / 2, Height / 2); } }

    private Texture2D Texture;
    private Image Buffer;

    private Color Transparent = new Color(0, 0, 0, 0);

    // Constructor for material id and size
    public unsafe PixelMap(Vector2i position, int material_id, int width, int height) {
        Position = position;
        MaterialID = material_id;
        Width = width;
        Height = height;

        Pixels = new Pixel[width, height];

        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                var Pos = Position + new Vector2i(x, y);
                var Pixel = new Solid((int)MaterialID, Pos);
                Pixel.BaseColor = Atlas.MaterialMaps[(int)MaterialID].GetColor(Pos);
                Pixel.Color = Pixel.BaseColor;
                Pixel.ColorSet = true;
                Pixels[x, y] = Pixel;
            }
        }

        Buffer = GenImageColor(Width * 4, Height * 4, Transparent);
        Texture = LoadTextureFromImage(Buffer);

        // Finish
        PixelMapSystem.Register(this);
    }

    // Constructor for pixel map and optional material map, material id defaults to 103 ("Meat") if not specified
    public unsafe PixelMap(Vector2i position, string pixel_map, string? material_map=null) {
        Position = position;

        var PixelImage = LoadImage(pixel_map);
        var PixelColors = LoadImageColors(PixelImage);

        Width = PixelImage.width;
        Height = PixelImage.height;

        Pixels = new Pixel[Width, Height];

        if (material_map is not null) {
            var MaterialImage = LoadImage(material_map);
            var MaterialColors = LoadImageColors(MaterialImage);

            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    int Index = (y * Width) + x;
                    var Color = PixelColors[Index];
                    int ID = Atlas.GetIDFromColor(MaterialColors[Index]);

                    var Pos = Position + new Vector2i(x, y);
                    var Pixel = new Pixel();
                    if (ID > -1) {
                        var STRID = ID.ToString();
                        switch (STRID[0]) {
                            case '1': Pixel = new Solid(ID, Pos); break;
                            case '2': Pixel = new Liquid(ID, Pos); break;
                            case '3': Pixel = new Gas(ID, Pos); break;
                            case '4': Pixel = new Powder(ID, Pos); break;
                        }

                        Pixel.BaseColor = Color;
                        Pixel.Color = Color;
                        Pixel.ColorSet = true;
                        Pixels[x, y] = Pixel;
                    }
                }
            }
        } else {
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    var Pos = Position + new Vector2i(x, y);
                    var Color = GetColor(Convert.ToUInt32(Atlas.Colors[103], 16));
                    var Pixel = new Solid(103, Pos);
                    Pixel.BaseColor = Color;
                    Pixels[x, y] = Pixel;
                }
            }
        }

        Buffer = GenImageColor(Width * 4, Height * 4, Transparent);
        Texture = LoadTextureFromImage(Buffer);

        // Finish
        PixelMapSystem.Register(this);
    }

    public override unsafe void Update(float delta) {
        // Update Position
        var Transform = Entity!.GetToken<Transform>()!;
        Position = Transform.Position;

        // Update Texture
        ImageClearBackground(ref Buffer, Transparent);

        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                var P = Pixels[x, y];
                if (P is null) continue;

                ImageDrawPixel(ref Buffer, x, y, P.Color);
            }
        }

        UpdateTexture(Texture, Buffer.data);
        Entity!.GetToken<Render>()!.Texture = Texture;
    }
}