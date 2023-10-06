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

class Input {
    public Engine Engine { get; private set; }
    public Interface Interface { get { return Engine.Interface; } }
    public Camera Camera { get { return Engine.Camera; } }
    public Canvas Canvas { get { return Engine.Canvas; } }
    public Config Config { get { return Engine.Config; } }
    public Pepper Pepper { get { return Engine.Pepper; } }

    private Dictionary<int, List<Event>> KeyBindings;

    private List<Key> InputStream = new List<Key>();
    private List<int> HeldKeys = new List<int>();

    // Debug
    private bool LogKeyMapping = false;

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
        else if (IsMouseButtonDown(MOUSE_BUTTON_LEFT)) InputStream.Add(new Key(EventType.Hold, (int)MOUSE_BUTTON_LEFT));
        else if (IsMouseButtonReleased(MOUSE_BUTTON_LEFT)) InputStream.Add(new Key(EventType.Release, (int)MOUSE_BUTTON_LEFT));

        if (IsMouseButtonPressed(MOUSE_BUTTON_RIGHT)) InputStream.Add(new Key(EventType.Press, (int)MOUSE_BUTTON_RIGHT));
        else if (IsMouseButtonDown(MOUSE_BUTTON_RIGHT)) InputStream.Add(new Key(EventType.Hold, (int)MOUSE_BUTTON_RIGHT));
        else if (IsMouseButtonReleased(MOUSE_BUTTON_RIGHT)) InputStream.Add(new Key(EventType.Release, (int)MOUSE_BUTTON_RIGHT));

        if (IsMouseButtonPressed(MOUSE_BUTTON_MIDDLE)) InputStream.Add(new Key(EventType.Press, (int)MOUSE_BUTTON_MIDDLE));
        else if (IsMouseButtonDown(MOUSE_BUTTON_MIDDLE)) InputStream.Add(new Key(EventType.Hold, (int)MOUSE_BUTTON_MIDDLE));
        else if (IsMouseButtonReleased(MOUSE_BUTTON_MIDDLE)) InputStream.Add(new Key(EventType.Release, (int)MOUSE_BUTTON_MIDDLE));


        // Handle held keys
        foreach (var Code in HeldKeys) {
            var Key = new Key(EventType.Hold, Code);
            InputStream.Add(Key);
        }

        // Process all Keys in the InputStream, firing their Event if there is an EventType match
        foreach (var Key in InputStream) {
            // Try sending the key to Interface first, if it returns true, skip further processing for that key
            if (Interface.FireEvent(Key)) {
                continue;
            }

            // Try sending the key to each Control token in the Control System, if an event is fired, skip further processing for tha tkey
            var Fired = false;
            foreach (var Token in ControlSystem.Tokens) {
                if (Token.FireEvent(Key)) {
                    Fired = true;
                    continue;
                }
            }

            if (!Fired && KeyBindings.ContainsKey(Key.Code)) {
                var EventList = KeyBindings[Key.Code];

                foreach (var Event in EventList) {
                    if (Key.Type == Event.Type) {
                        // Pepper.Log($"{Key.Code} ({Key.Type})", LogType.DEBUG);
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

            { "PaintStart", new Event(EventType.Press, new Action(() => { Canvas.Painting = true; })) },
            { "PaintEnd", new Event(EventType.Release, new Action(() => { Canvas.Painting = false; })) },
            { "EraseStart", new Event(EventType.Press, new Action(() => { Canvas.Erasing = true; })) },
            { "EraseEnd", new Event(EventType.Release, new Action(() => { Canvas.Erasing = false; })) },
            { "BrushSizeUp", new Event(EventType.Press, new Action(Canvas.BrushSizeUp)) },
            { "BrushSizeDown", new Event(EventType.Press, new Action(Canvas.BrushSizeDown)) },

            { "CameraPanUp", new Event(EventType.Hold, new Action(() => { Camera.Pan(Direction.Up); })) },
            { "CameraPanDown", new Event(EventType.Hold, new Action(() => { Camera.Pan(Direction.Down); })) },
            { "CameraPanLeft", new Event(EventType.Hold, new Action(() => { Camera.Pan(Direction.Left); })) },
            { "CameraPanRight", new Event(EventType.Hold, new Action(() => { Camera.Pan(Direction.Right); })) },
            { "CameraReset", new Event(EventType.Press, new Action(Camera.Reset)) },
        };

        // Get the values from keymap.json (falls back to default_keymap.json if none is found)
        var KeyMap = Config.LoadKeymap();

        // Map actions to inputs, invalid keys/values are skipped and logged as warnings
        foreach (var Data in KeyMap) {
            if (Global.InputMap.ContainsKey(Data.Key)) {
                var EventList = new List<Event>();
                foreach (var Event in Data.Value) {
                    if (EventMap.ContainsKey(Event)) {
                        EventList.Add(EventMap[Event]);

                        if (LogKeyMapping) {
                            Pepper.Log($"Mapped {Data.Key} ({EventMap[Event].Type}) to {Event}", LogType.INPUT, LogLevel.DEBUG);
                        }
                    } else {
                        if (LogKeyMapping) {
                            Pepper.Log($"Value '{Data.Value}' not found in EventMap", LogType.INPUT, LogLevel.WARNING);
                        }
                    }
                }
                KeyBindings.Add(Global.InputMap[Data.Key], EventList);
            } else {
                Pepper.Log($"Key '{Data.Key}' not found in InputMap", LogType.INPUT, LogLevel.WARNING);
            }
        }
    }
}