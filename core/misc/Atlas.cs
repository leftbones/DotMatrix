using Raylib_cs;
using static Raylib_cs.Raylib;
using Newtonsoft.Json;

namespace DotMatrix;

enum ElementType { Solid, Liquid, Gas, Powder };

static class Atlas {
    public static Dictionary<string, ElementData> Solid = JsonConvert.DeserializeObject<Dictionary<string, ElementData>>(File.ReadAllText("core/misc/elements/json/Solid.json"))!;
    public static Dictionary<string, ElementData> Liquid = JsonConvert.DeserializeObject<Dictionary<string, ElementData>>(File.ReadAllText("core/misc/elements/json/Liquid.json"))!;
    public static Dictionary<string, ElementData> Gas = JsonConvert.DeserializeObject<Dictionary<string, ElementData>>(File.ReadAllText("core/misc/elements/json/Gas.json"))!;
    public static Dictionary<string, ElementData> Powder = JsonConvert.DeserializeObject<Dictionary<string, ElementData>>(File.ReadAllText("core/misc/elements/json/Powder.json"))!;

    public static Dictionary<int, string> Colors = new Dictionary<int, string>();

    public static void Initialize() {
        // Colors
        foreach (var Data in Solid) Colors[Data.Value.ID] = Data.Value.Color;
        foreach (var Data in Liquid) Colors[Data.Value.ID] = Data.Value.Color;
        foreach (var Data in Gas) Colors[Data.Value.ID] = Data.Value.Color;
        foreach (var Data in Powder) Colors[Data.Value.ID] = Data.Value.Color;
    }

    public static int GetIDFromColor(Color color) {
        string Hex = ColorToInt(color).ToString("X");
        if (Hex != "FF") {
            if (Hex.Length == 7) Hex = "0" + Hex;
            return Colors.Where(P => P.Value[0..^2] == Hex[0..^2]).Select(P => P.Key).FirstOrDefault();
        }
        return -1;
    }
}

struct ElementData {
    public int ID;
    public string Name;
    public string Color;
    public ElementType Type;

    public ElementData(int id, string name, string color, ElementType type) {
        ID = id;
        Name = name;
        Color = color;
        Type = type;
    }
}