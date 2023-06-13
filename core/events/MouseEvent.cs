using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

enum InputDirection { Down, Up };

// Mouse Down
class MouseDownEvent : Event {
    public MouseButton MouseButton;
    public Vector2i Position;

    public MouseDownEvent(MouseButton mouse_button, Vector2i position) : base($"MouseDown:{mouse_button}") {
        MouseButton = mouse_button;
        Position = position;
    }
}

// Mouse Up
class MouseUpEvent : Event {
    public MouseButton MouseButton;
    public Vector2i Position;

    public MouseUpEvent(MouseButton mouse_button, Vector2i position) : base($"MouseUp:{mouse_button}") {
        MouseButton = mouse_button;
        Position = position;
    }
}


// Wheel
class MouseWheelEvent : Event {
    public int Amount { get; private set; }

    public MouseWheelEvent(int amount) : base($"MouseWheel:{amount}") {
        Amount = amount;
    }
}