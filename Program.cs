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

        Engine.Pepper.Throw(LogType.MATRIX, LogLevel.ERROR, "The FitnessGram™ Pacer Test is a multistage aerobic capacity test that progressively gets more difficult as it continues. The 20 meter pacer test will begin in 30 seconds. Line up at the start. The running speed starts slowly, but gets faster each minute after you hear this signal. [beep] A single lap should be completed each time you hear this sound. [ding] Remember to run in a straight line, and run as long as possible. The second time you fail to complete a lap before the sound, your test is over. The test will begin on the word start. On your mark, get ready, start.");

        // Console.WriteLine(ColorToInt(Color.RED));
        // Console.WriteLine(ColorToInt(Color.RED).ToString("X"));

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
        Engine.Pepper.Log(LogType.OTHER, LogLevel.MESSAGE, "Program exited successfully");
        CloseWindow();
    }
}
