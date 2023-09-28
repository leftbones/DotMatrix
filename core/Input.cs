using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.KeyboardKey;
using static Raylib_cs.MouseButton;

namespace DotMatrix;

/// <summary>
/// Handler class for all input. This class is loaded last to make sure it has access to all other classes during keybinding assignment.
/// 
/// Once loaded, `ApplyKeymap()` is called and the keymap json file is read from `Config.LoadKeymap()`, if the data in the json file matches a valid
/// key name and event name, a binding is created and added to the Keybindings list -- The event names in the json file are an array, so multiple
/// events can be mapped to a single key.
/// 
/// Each time `Update()` is called, input is read and each keycode is assigned to a `Key` object, then added to an input stream list, until there is
/// no input left to read. Each `Key` object contains a keycode and an EventType depending on if the key was just pressed (press), was pressed
/// last frame and is still down (hold), or was pressed last frame and is no longer pressed (release).
/// 
/// Then, each Key object is processed in the order they were recevied. First, the Key is sent to the Interface for processing. If the Interface returns
/// true, that means something in the Interface handled the input, and it can be skipped. Otherwise, it's processed further. If they keycode of the
/// Key object exists in the Keybindings dictionary, each Event in the event list for that keycode is checked, and if the Event and Key share the
/// same EventType, the Event is fired. The Interface always gets priority over all input. See `Interface` for more details.
/// 
/// Events have an Action attached to them, and when `Event.Fire()` is called, `Action.Invoke()` is fired as long as `Event.Action` isn't null
/// </summary>

// TODO
// - Re-implement interface event priority
// - Figure out a way to assign multiple events to the same key (press, release, hold)

class Input {
    public Engine Engine { get; private set; }
    public Camera Camera { get { return Engine.Camera; } }
    public Canvas Canvas { get { return Engine.Canvas; } }
    public Config Config { get { return Engine.Config; } }
    public Pepper Pepper { get { return Engine.Pepper; } }

    private Dictionary<int, List<Event>> KeyBindings;

    private List<Key> InputStream = new List<Key>();
    private List<int> HeldKeys = new List<int>();

    public Input(Engine engine) {
        Engine = engine;

        KeyBindings = new Dictionary<int, List<Event>>();
        ApplyKeymap();
    }

    // Handle all input, creating events and then processing the events in the order they were received
    public void Update() {
        // Clear the list of key and mouse events from the last update
        InputStream.Clear();

        // Read input until there is none, creating a pressed event for each new key pressed (held keys are skipped)
        var Input = (int)GetKeyPressed();
        while (Input != 0) {
            var Key = new Key(EventType.Press, Input);
            InputStream.Add(Key);

            if (!HeldKeys.Contains(Input))
                HeldKeys.Add(Input);

            Input = GetKeyPressed();
        }

        // Check all held keys and remove those that are no longer held, calling a release event for released keys
        for (int i = HeldKeys.Count() - 1; i >= 0; i--) {
            if (!IsKeyDown((KeyboardKey)HeldKeys[i])) {
                var Key = new Key(EventType.Release, HeldKeys[i]);
                InputStream.Add(Key);
                HeldKeys.Remove(HeldKeys[i]);
            }
        }

        // Check for any mouse movement, button presses/releases, and mouse wheel movement
        var MousePos = new Vector2i(
            (int)Math.Round((double)GetMouseX() / Engine.MatrixScale) * Engine.MatrixScale,
            (int)Math.Round((double)GetMouseY() / Engine.MatrixScale) * Engine.MatrixScale
        );

        Canvas.MousePrev = Canvas.MousePos;
        Canvas.MousePos = MousePos;

        int MouseWheelMove = (int)GetMouseWheelMove();
        if (MouseWheelMove != 0) InputStream.Add(new Key(EventType.Press, MouseWheelMove < 0 ? 7 : 8));

        if (IsMouseButtonPressed(MOUSE_BUTTON_LEFT)) InputStream.Add(new Key(EventType.Press, (int)MOUSE_BUTTON_LEFT));
        if (IsMouseButtonPressed(MOUSE_BUTTON_RIGHT)) InputStream.Add(new Key(EventType.Press, (int)MOUSE_BUTTON_RIGHT));
        if (IsMouseButtonPressed(MOUSE_BUTTON_MIDDLE)) InputStream.Add(new Key(EventType.Press, (int)MOUSE_BUTTON_MIDDLE));

        if (IsMouseButtonReleased(MOUSE_BUTTON_LEFT)) InputStream.Add(new Key(EventType.Release, (int)MOUSE_BUTTON_LEFT));
        if (IsMouseButtonReleased(MOUSE_BUTTON_RIGHT)) InputStream.Add(new Key(EventType.Release, (int)MOUSE_BUTTON_RIGHT));
        if (IsMouseButtonReleased(MOUSE_BUTTON_MIDDLE)) InputStream.Add(new Key(EventType.Release, (int)MOUSE_BUTTON_MIDDLE));

        if (IsMouseButtonDown(MOUSE_BUTTON_LEFT)) InputStream.Add(new Key(EventType.Hold, (int)MOUSE_BUTTON_LEFT));
        if (IsMouseButtonDown(MOUSE_BUTTON_RIGHT)) InputStream.Add(new Key(EventType.Hold, (int)MOUSE_BUTTON_RIGHT));
        if (IsMouseButtonDown(MOUSE_BUTTON_MIDDLE)) InputStream.Add(new Key(EventType.Hold, (int)MOUSE_BUTTON_MIDDLE));

        // Handle held keys
        foreach (var Code in HeldKeys) {
            var Key = new Key(EventType.Hold, Code);
            InputStream.Add(Key);
        }

        // Process all Keys in the InputStream, firing their Event if there is an EventType match
        foreach (var Key in InputStream) {
            if (KeyBindings.ContainsKey(Key.Code)) {
                var EventList = KeyBindings[Key.Code];

                foreach (var Event in EventList) {
                    if (Key.Type == Event.Type) {
                        Event.Fire();
                    }
                }
            }
        }
    }

    // Apply the values from the keymap json file to the InputActions map
    public void ApplyKeymap() {
        // All possible events mapped to string names for easier configuration
        var EventMap = new Dictionary<String, Event>() {
            { "ForceQuit", new Event(EventType.Press, new Action(Engine.Exit)) },
            { "ToggleActive", new Event(EventType.Press, new Action(Engine.ToggleActive)) },
            { "TickOnce", new Event(EventType.Press, new Action(Engine.TickOnce)) },

            { "Paint", new Event(EventType.Hold, new Action(() => { Canvas.Painting = true; })) },
            { "Erase", new Event(EventType.Hold, new Action(() => { Canvas.Erasing = true; })) },
            { "BrushSizeUp", new Event(EventType.Press, new Action(Canvas.BrushSizeUp)) },
            { "BrushSizeDown", new Event(EventType.Press, new Action(Canvas.BrushSizeDown)) },

            { "CameraPanUp", new Event(EventType.Hold, new Action(() => { Camera.Pan(Direction.Up); })) },
            { "CameraPanDown", new Event(EventType.Hold, new Action(() => { Camera.Pan(Direction.Down); })) },
            { "CameraPanLeft", new Event(EventType.Hold, new Action(() => { Camera.Pan(Direction.Left); })) },
            { "CameraPanRight", new Event(EventType.Hold, new Action(() => { Camera.Pan(Direction.Right); })) },
            { "CameraReset", new Event(EventType.Press, new Action(Camera.Reset)) },
        };

        // All valid input keys/buttons mapped to string names for easier configuration
        var InputMap = new Dictionary<String, int>() {
            // Mouse
            { "mouse_1", (int)MOUSE_BUTTON_LEFT },
            { "mouse_2", (int)MOUSE_BUTTON_RIGHT },
            { "mouse_3", (int)MOUSE_BUTTON_MIDDLE },
            { "mouse_wheel_up", 7 },
            { "mouse_wheel_down", 8 },

            // Modifiers
            { "l_shift", (int)KEY_LEFT_SHIFT },
            { "r_shift", (int)KEY_RIGHT_SHIFT },
            { "l_ctrl", (int)KEY_LEFT_CONTROL },
            { "r_ctrl", (int)KEY_RIGHT_CONTROL },
            { "l_alt", (int)KEY_LEFT_ALT },
            { "r_alt", (int)KEY_RIGHT_ALT },

            // Special
            { "escape", (int)KEY_ESCAPE },
            { "tab", (int)KEY_TAB },
            { "enter", (int)KEY_ENTER },
            { "space", (int)KEY_SPACE },
            { "backspace", (int)KEY_BACKSPACE },

            // Letters
            { "a", (int)KEY_A },
            { "b", (int)KEY_B },
            { "c", (int)KEY_C },
            { "d", (int)KEY_D },
            { "e", (int)KEY_E },
            { "f", (int)KEY_F },
            { "g", (int)KEY_G },
            { "h", (int)KEY_H },
            { "i", (int)KEY_I },
            { "j", (int)KEY_J },
            { "k", (int)KEY_K },
            { "l", (int)KEY_L },
            { "m", (int)KEY_M },
            { "n", (int)KEY_N },
            { "o", (int)KEY_O },
            { "p", (int)KEY_P },
            { "q", (int)KEY_Q },
            { "r", (int)KEY_R },
            { "s", (int)KEY_S },
            { "t", (int)KEY_T },
            { "u", (int)KEY_U },
            { "v", (int)KEY_V },
            { "w", (int)KEY_W },
            { "x", (int)KEY_X },
            { "y", (int)KEY_Y },
            { "z", (int)KEY_Z },

            // Numbers
            { "1", (int)KEY_ONE },
            { "2", (int)KEY_TWO },
            { "3", (int)KEY_THREE },
            { "4", (int)KEY_FOUR },
            { "5", (int)KEY_FIVE },
            { "6", (int)KEY_SIX },
            { "7", (int)KEY_SEVEN },
            { "8", (int)KEY_EIGHT },
            { "9", (int)KEY_NINE },
            { "0", (int)KEY_ZERO },

            // Function keys
            { "f1", (int)KEY_F1 },
            { "f2", (int)KEY_F2 },
            { "f3", (int)KEY_F3 },
            { "f4", (int)KEY_F4 },
            { "f5", (int)KEY_F5 },
            { "f6", (int)KEY_F6 },
            { "f7", (int)KEY_F7 },
            { "f8", (int)KEY_F8 },
            { "f9", (int)KEY_F9 },
            { "f10", (int)KEY_F10 },
            { "f11", (int)KEY_F11 },
            { "f12", (int)KEY_F12 },

            // Symbols
            { "-", (int)KEY_MINUS },
            { "=", (int)KEY_EQUAL },
            { "[", (int)KEY_LEFT_BRACKET },
            { "]", (int)KEY_RIGHT_BRACKET },
            { ";", (int)KEY_SEMICOLON },
            { "'", (int)KEY_APOSTROPHE },
            { "`", (int)KEY_GRAVE },
            { "/", (int)KEY_SLASH },
            { "\\", (int)KEY_BACKSLASH },
            { ",", (int)KEY_COMMA },
            { ".", (int)KEY_PERIOD },

            // Keypad
            { "kp_1", (int)KEY_KP_1 },
            { "kp_2", (int)KEY_KP_2 },
            { "kp_3", (int)KEY_KP_3 },
            { "kp_4", (int)KEY_KP_4 },
            { "kp_5", (int)KEY_KP_5 },
            { "kp_6", (int)KEY_KP_6 },
            { "kp_7", (int)KEY_KP_7 },
            { "kp_8", (int)KEY_KP_8 },
            { "kp_9", (int)KEY_KP_9 },
            { "kp_0", (int)KEY_KP_0 },
            { "kp_enter", (int)KEY_KP_ENTER },
            { "kp_add", (int)KEY_KP_ADD },
            { "kp_subtract", (int)KEY_KP_SUBTRACT },
            { "kp_multiply", (int)KEY_KP_MULTIPLY },
            { "kp_divide", (int)KEY_KP_DIVIDE },
            { "kp_decimal", (int)KEY_KP_DECIMAL },
        };

        // Get the values from keymap.json (falls back to default_keymap.json if none is found)
        var KeyMap = Config.LoadKeymap();

        // Map actions to inputs, invalid keys/values are skipped and logged as warnings
        foreach (var Data in KeyMap) {
            if (InputMap.ContainsKey(Data.Key)) {
                var EventList = new List<Event>();
                foreach (var Event in Data.Value) {
                    if (EventMap.ContainsKey(Event)) {
                        EventList.Add(EventMap[Event]);
                        Pepper.Log($"Mapped '{Data.Key}' to '{Event}'", LogType.INPUT);
                    } else {
                        Pepper.Log($"Value '{Data.Value}' not found in ActionMap", LogType.INPUT, LogLevel.WARNING);
                    }
                }
                KeyBindings.Add(InputMap[Data.Key], EventList);
            } else {
                Pepper.Log($"Key '{Data.Key}' not found in InputMap", LogType.INPUT, LogLevel.WARNING);
            }
        }
    }
}