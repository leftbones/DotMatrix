using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

/// <summary>
/// The in-engine debug console, named "Pepper" after my daughter because the name "Console" is used already in .NET
/// Currently only handles .NET console output and can be used for throwing exceptions with an in-game window
/// Will be used for creating and managing log files in the future
/// </summary>

// TODO: Implement Colorful.Console for nicer looking output (https://github.com/tomakita/Colorful.Console)

enum LogType { SYSTEM, ENGINE, MATRIX, PHYSICS, INTERFACE, DEBUG, OTHER };
enum LogLevel { MESSAGE, WARNING, ERROR, EXCEPTION, DEBUG };

class Pepper {
    public Engine Engine { get; private set; }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Canvas Canvas { get { return Engine.Canvas; } }
    public Interface Interface { get { return Engine.Interface; } }
    public Theme Theme { get { return Engine.Theme; } }

    private string LogPath = "logs/";
    private string LogFile;
    // private int MaxLogs = 5; // Maxiumum number of log files to keep before they are overwritten by new ones

    public Pepper(Engine engine) {
        Engine = engine;

        LogFile = $"{LogPath}{Timestamp(1)}_log.txt";

        Log("Pepper Initialized", LogType.SYSTEM);
    }

    // Generate the current timestamp (for logs)
    public string? Timestamp(int type=0) {
        switch (type) {
            case 0: return DateTime.Now.ToString("[HH:mm:ss]");             // Log write format
            case 1: return DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss");    // File name format
            default: Throw("Invalid Timestamp type given (requires 0-1)", LogType.OTHER, LogLevel.EXCEPTION); return null;
        }
    }

    // Throw an exception and write the exception to the current log file
    public void Throw(string message, LogType type=LogType.DEBUG, LogLevel level=LogLevel.EXCEPTION) {
        Log(message, type, level);
        Canvas.ExceptionWindow.AddWidget(new Multiline(Canvas.ExceptionWindow, $"{type} {level} %N %N {message}", 750, new Quad(0, 20, 10, 10)));
        Canvas.ExceptionWindow.AddWidget(new Button(Canvas.ExceptionWindow, "Exit", () => { Environment.Exit(0); }, new Vector2i(75, 20), anchor: Anchor.Right));
        Canvas.ExceptionWindow.Toggle();
        Engine.Halt();
    }

    // Log a message to the console as well as the current log file
    public void Log(string message, LogType type=LogType.DEBUG, LogLevel level=LogLevel.MESSAGE) {
        Console.WriteLine($"{Timestamp()} [{type}] {level}: {message}");
    }

    // Write to the current log file, create it if it doesn't exist, and delete the oldest log if MaxLogs is exceeded
    public void Write() {
        // TODO: Write logs to a log file as they are written to the console
    }
}