using static Raylib_cs.KeyboardKey;
using static Raylib_cs.MouseButton;

namespace DotMatrix;

/// <summary>
/// Various global variables that don't fit nicely anywhere else but might need to be accessed in a lot of different places
/// </summary>

// TODO: Document properties

static class Global {
    // General
    public static string VersionString = "1.0.0-alpha";

    // Window
    public static string WindowTitle = "DotMatrix Engine";
    public static Vector2i WindowSize = new Vector2i(1280, 800);

    // Engine

    // Matrix
    public static int MatrixScale = 4;

    // Interface

    // Physics
    public static int PTM = 16;

    // Input
    public static Dictionary<String, int> InputMap = new() {
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
}