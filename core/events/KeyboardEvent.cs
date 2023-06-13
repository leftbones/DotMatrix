using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class KeyPressEvent : Event {
    public KeyboardKey Key { get; private set; }

    public KeyPressEvent(KeyboardKey key) : base($"KeyPress:{key}") {
        Key = key;
    }
}

class KeyReleaseEvent : Event {
    public KeyboardKey Key { get; private set; }

    public KeyReleaseEvent(KeyboardKey key) : base($"KeyRelease:{key}") {
        Key = key;
    }
}