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
}