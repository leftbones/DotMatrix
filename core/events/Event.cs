using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

public enum EventType { Press, Release, Hold, Any };

class Event {
    public EventType Type { get; private set; }
    public Action? Action { get; private set; }

    public Event(EventType type, Action? action) { // TODO: Change string name to use the keycode identifier instead, not as nice for debugging purposes but faster to check than comparing strings
        Type = type;
        Action = action;
    }

    public void Fire() {
        if (Action is not null) {
            Action.Invoke();
        }
    }
}

class Key {
    public EventType Type { get; private set; }
    public int Code { get; private set; }

    public Key(EventType type, int code) {
        Type = type;
        Code = code;
    }
}