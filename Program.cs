using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.TraceLogLevel;

namespace DotMatrix;

class Program {
    static void Main(string[] args) {
        ////
        // Init

        SetTraceLogLevel(LOG_WARNING | LOG_ERROR | LOG_FATAL);
        InitWindow(Global.WindowSize.X, Global.WindowSize.Y, $"{Global.WindowTitle} {Global.VersionString}");
        SetExitKey(KeyboardKey.KEY_NULL);
        SetTargetFPS(144);

        var DefaultColor = new Color(34, 35, 35, 255);

        ////
        // Setup

        Atlas.Initialize();
        var Engine = new Engine(Global.WindowSize, Global.MatrixScale);


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
            ClearBackground(DefaultColor);

            Engine.Draw();

            EndDrawing();


            ////
            // Exit Check

            if (Engine.ShouldExit)
                break;
        }


        ////
        // Exit
        Engine.Pepper.Log("Program exited successfully", LogType.SYSTEM);
        CloseWindow();
    }
}
