using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

enum InputDirection { Down, Up };

// Key Press
class KeyPressEvent : Event {
    public KeyboardKey Key { get; private set; }

    public KeyPressEvent(KeyboardKey key) : base($"KeyPress:{key}") {
        Key = key;
    }
}

// Key Release
class KeyReleaseEvent : Event {
    public KeyboardKey Key { get; private set; }

    public KeyReleaseEvent(KeyboardKey key) : base($"KeyRelease:{key}") {
        Key = key;
    }
}

// Key Held
class KeyDownEvent : Event {
    public KeyboardKey Key { get; private set; }

    public KeyDownEvent(KeyboardKey key) : base($"KeyDown:{key}") {
        Key = key;
    }
}