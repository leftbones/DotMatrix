using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.KeyboardKey;
using Newtonsoft.Json;

namespace DotMatrix;

/// <summary>
/// Handles loading and saving of user-defined configuration (user_config.json), if this file is not found, loads default_config.json instead
/// </summary>
 
// TODO: Add a check to create default_config.json if it is not found, currently throws an unhandled exception
// Also add a "user" folder or something (if it doesn't exist) to store the user config and key mapping file (from Input.cs)

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
            Items = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText("sys/config.json"))!;
            Pepper.Log("Configuration loaded successfully", LogType.SYSTEM);
        } catch {
            Items = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText("sys/default_config.json"))!;
            Pepper.Log("No configuration found, default configuration restored", LogType.SYSTEM, LogLevel.WARNING);
        }
    }

    // Apply all changes to the config file to the subsystems
    public void ApplyChanges() {
        Pepper.ApplyConfig(this);
        Engine.ApplyConfig(this);
        Matrix.ApplyConfig(this);
        Canvas.ApplyConfig(this);
        Camera.ApplyConfig(this);
    }

    // Load the key bindings from keymap.json or default_keymap.json (fallback)
    public Dictionary<String, List<String>> LoadKeymap() {
        var Keymap = new Dictionary<String, List<String>>();

        try {
            Keymap = JsonConvert.DeserializeObject<Dictionary<String, List<String>>>(File.ReadAllText("sys/keymap.json"))!;
            Pepper.Log("Keymap loaded successfully", LogType.SYSTEM);
        } catch {
            Keymap = JsonConvert.DeserializeObject<Dictionary<String, List<String>>>(File.ReadAllText("sys/default_keymap.json"))!;
            Pepper.Log("No keymap found, default keymap restored", LogType.SYSTEM, LogLevel.WARNING);
        }

        return Keymap;
    }
}