using Newtonsoft.Json;

namespace DotMatrix;

class Config {
    public Engine Engine { get; private set; }

    public Pepper Pepper { get { return Engine.Pepper; } }
    public Canvas Canvas { get { return Engine.Canvas; } }
    public Interface Interface { get { return Engine.Interface; } }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Camera Camera { get { return Engine.Camera; } }

    public Dictionary<string, dynamic> Items = new Dictionary<string, dynamic>();

    public Config(Engine engine) {
        Engine = engine;

        // Load user_config.json or fall back to default_config.json
        try {
            Items = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText("system/user_config.json"))!;
            Pepper.Log("User config loaded", LogType.SYSTEM);
        } catch {
            Items = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText("system/default_config.json"))!;
            Pepper.Log("Default config loaded", LogType.SYSTEM);
        }
    }
}

// class ConfigItem { 
//     public Type Type { get; private set; }
//     public string Name { get; private set; }
//     public string Value { get; set; }

//     public ConfigItem(Type type, string name, string value) {
//         Type = type;
//         Name = name;
//         Value = value;
//     }
// }