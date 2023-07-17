using Raylib_cs;
using static Raylib_cs.Raylib;
using Newtonsoft.Json;

namespace DotMatrix;

enum ElementType { Solid, Liquid, Gas, Powder };

static class Atlas {
    public static Dictionary<string, ElementData> Solid = JsonConvert.DeserializeObject<Dictionary<string, ElementData>>(File.ReadAllText("core/world/elements/json/Solid.json"))!;
    public static Dictionary<string, ElementData> Liquid = JsonConvert.DeserializeObject<Dictionary<string, ElementData>>(File.ReadAllText("core/world/elements/json/Liquid.json"))!;
    public static Dictionary<string, ElementData> Gas = JsonConvert.DeserializeObject<Dictionary<string, ElementData>>(File.ReadAllText("core/world/elements/json/Gas.json"))!;
    public static Dictionary<string, ElementData> Powder = JsonConvert.DeserializeObject<Dictionary<string, ElementData>>(File.ReadAllText("core/world/elements/json/Powder.json"))!;

    public static Dictionary<int, string> Colors = new Dictionary<int, string>();
    public static Dictionary<int, MaterialMap> MaterialMaps = new Dictionary<int, MaterialMap>();

    public static void Initialize() {
        // Colors
        foreach (var Data in Solid) Colors[Data.Value.ID] = Data.Value.Color;
        foreach (var Data in Liquid) Colors[Data.Value.ID] = Data.Value.Color;
        foreach (var Data in Gas) Colors[Data.Value.ID] = Data.Value.Color;
        foreach (var Data in Powder) Colors[Data.Value.ID] = Data.Value.Color;

        // Textures
        foreach (var Data in Solid) MaterialMaps[Data.Value.ID] = new MaterialMap(LoadImage(Data.Value.Texture));
    }

    public static int GetIDFromColor(Color color) {
        if (color.a > 0) {
            string Hex = ColorToInt(color).ToString("X");
            if (Hex != "FF") {
                if (Hex.Length == 7) Hex = "0" + Hex;
                return Colors.Where(P => P.Value[0..^2] == Hex[0..^2]).Select(P => P.Key).FirstOrDefault();
            }
        }
        return -1;
    }
}

struct ElementData {
    public int ID;
    public string Name;
    public string Color;
    public string Texture;
    public ElementType Type;

    public ElementData(int id, string name, string color, string texture, ElementType type) {
        ID = id;
        Name = name;
        Color = color;
        Texture = texture;
        Type = type;
    }
}

unsafe class MaterialMap {
    public Image Image { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    private Color* Colors;

    public unsafe MaterialMap(Image image) {
        Image = image;
        Width = image.width;
        Height = image.height;

        Colors = LoadImageColors(Image);
    }

    public Color GetColor(Vector2i pos) {
        int RX = pos.X == 0 ? 0 : Math.Abs(pos.X) % Width;
        int RY = pos.Y == 0 ? 0 : Math.Abs(pos.Y) % Height;

        int Index = (RY * Width) + RX;
        return Colors[Index];
    }
}