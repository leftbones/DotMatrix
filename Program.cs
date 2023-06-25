using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.TraceLogLevel;

namespace DotMatrix;

class Program {
    static void Main(string[] args) {
        ////
        // Init

        var WindowTitle = "DotMatrix";
        var VersionString = "1.0.0-alpha";
        var WindowSize = new Vector2i(1280, 800);
        int MatrixScale = 4;

        SetTraceLogLevel(LOG_WARNING | LOG_ERROR | LOG_FATAL);
        InitWindow(WindowSize.X, WindowSize.Y, $"{WindowTitle} {VersionString}");
        SetExitKey(KeyboardKey.KEY_NULL);
        SetTargetFPS(240);

        ////
        // Setup

        var Engine = new Engine(WindowSize, MatrixScale);


        ////
        // Loop

        while (!WindowShouldClose()) {
            ////
            // Input

            Engine.HandleInput();


            ////
            // Update

            Engine.Update();


            ////
            // Draw
            BeginDrawing();
            ClearBackground(Color.BLACK);

            Engine.Draw();

            EndDrawing();


            ////
            // Exit Check

            if (Engine.ShouldExit)
                break;
        }


        ////
        // Exit

        CloseWindow();
    }
}
