using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

enum InputDirection { Down, Up };

// Abstract Key Event
abstract class KeyEvent : Event {
    public KeyboardKey Key { get; private set; }

    public KeyEvent(KeyboardKey key, String name, EventType type, Action? action) : base(type, action) {
        Key = key;
    }
}

// Key Press
class KeyPressEvent : KeyEvent {
    public KeyPressEvent(KeyboardKey key, EventType type, Action? action) : base(key, $"KeyPress:{key}", type, action) { }
}

// Key Release
class KeyReleaseEvent : KeyEvent {
    public KeyReleaseEvent(KeyboardKey key, EventType type, Action? action) : base(key, $"KeyRelease:{key}", type, action) { }
}

// Key Held
class KeyDownEvent : KeyEvent {
    public KeyDownEvent(KeyboardKey key, EventType type, Action? action) : base(key, $"KeyDown:{key}", type, action) { }
}