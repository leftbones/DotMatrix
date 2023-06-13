using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Engine {
    public Vector2i WindowSize { get; private set; }
    public int MatrixScale { get; private set; }

    public int Tick { get; private set; }
    public Matrix Matrix { get; private set; }
    public Brush Brush { get; private set; }

    public bool ShouldExit { get; private set; }

    private List<KeyboardKey> HeldKeys = new List<KeyboardKey>();

    public Engine(Vector2i window_size, int matrix_scale) {
        WindowSize = window_size;
        MatrixScale = matrix_scale;

        Matrix = new Matrix(this, MatrixScale);
        Brush = new Brush(this);
    }

    public void HandleInput() {
        var Events = new List<Event>();

        // Key Press/Release
        var K = GetKeyPressed();
        while (K != 0) {
            var Key = (KeyboardKey)K;
            Events.Add(new KeyPressEvent(Key));
            HeldKeys.Add(Key);

            K = GetKeyPressed();
        }

        for (int i = HeldKeys.Count() - 1; i >= 0; i--) {
            var Key = HeldKeys[i];
            if (!IsKeyDown(Key)) {
                Events.Add(new KeyReleaseEvent(Key));
                HeldKeys.Remove(Key);
            }
        }

        // Mouse Movement/Buttons/Wheel
        var MousePos = new Vector2i(
            (int)Math.Round((double)GetMouseX() / MatrixScale) * MatrixScale,
            (int)Math.Round((double)GetMouseY() / MatrixScale) * MatrixScale
        );

        Brush.MousePrev = Brush.MousePos;
        Brush.MousePos = MousePos;

        int MouseWheelMove = (int)GetMouseWheelMove();
        if (MouseWheelMove != 0) Events.Add(new MouseWheelEvent(MouseWheelMove));

        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) Events.Add(new MouseDownEvent(MouseButton.MOUSE_BUTTON_LEFT, MousePos));
        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT)) Events.Add(new MouseDownEvent(MouseButton.MOUSE_BUTTON_RIGHT, MousePos));

        if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)) Events.Add(new MouseUpEvent(MouseButton.MOUSE_BUTTON_LEFT, MousePos));
        if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_RIGHT)) Events.Add(new MouseUpEvent(MouseButton.MOUSE_BUTTON_RIGHT, MousePos));

        // Handle Events (Temporary)
        foreach (var E in Events) {
            if (E.Name == "KeyPress:KEY_ESCAPE")
                ShouldExit = true;

            if (E.Name == "MouseDown:MOUSE_BUTTON_LEFT")
                Brush.Painting = true;

            if (E.Name == "MouseUp:MOUSE_BUTTON_LEFT")
                Brush.Painting = false;

            if (E.Name == "MouseDown:MOUSE_BUTTON_RIGHT") {
                Brush.Painting = true;
                Brush.ID = -1;
            }

            if (E.Name == "MouseUp:MOUSE_BUTTON_RIGHT") {
                Brush.Painting = false;
                Brush.ID = 0;
            }
        }
    }

    public void Update() {
        Matrix.Update();
        Brush.Update();

        Tick++;
    }

    public void Draw() {
        // Matrix
        Matrix.Draw();
    }
}