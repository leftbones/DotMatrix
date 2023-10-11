using System.Drawing;
using Console = Colorful.Console;

namespace DotMatrix;

/// <summary>
/// The in-engine debug console, named "Pepper" after my daughter because the name "Console" is used already in .NET
/// Currently only handles .NET console output and can be used for throwing exceptions with an in-game window
/// Will be used for creating and managing log files in the future
/// </summary>

// TODO: Implement Colorful.Console for nicer looking output (https://github.com/tomakita/Colorful.Console)

enum LogType { SYSTEM, ENGINE, MATRIX, PHYSICS, INTERFACE, INPUT, DEBUG, OTHER };
enum LogLevel { MESSAGE, WARNING, ERROR, FATAL, DEBUG };

class Pepper {
    public Engine Engine { get; private set; }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Canvas Canvas { get { return Engine.Canvas; } }
    public Interface Interface { get { return Engine.Interface; } }
    public Theme Theme { get { return Engine.Theme; } }

    private string LogFile;                         // Name of the current log file
    private int MaxLogs = 5;                        // Max number of log files kept before overwriting

    private readonly Color MessageColor = Color.Transparent;
    private readonly Color WarningColor = Color.Yellow;
    private readonly Color ErrorColor = Color.Orange;
    private readonly Color FatalColor = Color.Red;
    private readonly Color DebugColor = Color.Magenta;

    // Settings
    public bool LogMessage = true;
    public bool LogWarning = true;
    public bool LogError = true;
    public bool LogFatal = true;
    public bool LogDebug = true;

    public Pepper(Engine engine) {
        Engine = engine;

        // Create log directory (if it doesn't exist)
        Directory.CreateDirectory("logs");
        LogFile = $"logs/{Timestamp(1)}_log.txt";

        // Finish
        Log("Pepper Initialized", LogType.SYSTEM);
    }

    // Apply changes to the Config
    public void ApplyConfig(Config C) {
        Log("Canvas config applied", LogType.SYSTEM);
    }

    // Generate the current timestamp (for logs)
    public string Timestamp(int type=0) {
        if (type == 0) {
            return DateTime.Now.ToString("[HH:mm:ss]");             // Log write format
        } else if (type == 1) {
            return DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss");    // File name format
        }

        Log("Invalid Timestamp type passed to Pepper.Timestamp (requires 0-1)", LogType.OTHER, LogLevel.ERROR);
        return "TimeIsNotReal";
    }

    // Throw an exception and write the exception to the current log file
    public void Throw(string message, LogType type=LogType.DEBUG, LogLevel level=LogLevel.FATAL) {
        Log(message, type, level);
        Canvas.ExceptionWindow.AddWidget(new Multiline(Canvas.ExceptionWindow, $"{type} {level} %N %N {message}", 750));
        Canvas.ExceptionWindow.AddWidget(new Button(Canvas.ExceptionWindow, "Exit", () => { Environment.Exit(0); }, new Vector2i(75, 25), anchor: Anchor.Right));
        Canvas.ExceptionWindow.Toggle();
        Engine.Halt();
    }

    // Log a message to the console as well as the current log file
    public void Log(string message, LogType type=LogType.OTHER, LogLevel level=LogLevel.MESSAGE) {
        if (level == LogLevel.MESSAGE && !LogMessage) { return; }
        else if (level == LogLevel.WARNING && !LogWarning) { return; }
        else if (level == LogLevel.ERROR && !LogError) { return; }
        else if (level == LogLevel.FATAL && !LogFatal) { return; }
        else if (level == LogLevel.DEBUG && !LogDebug) { return; }

        var Col = MessageColor;
        switch(level) {
            case LogLevel.WARNING: Col = WarningColor; break;
            case LogLevel.ERROR: Col = ErrorColor; break;
            case LogLevel.FATAL: Col = FatalColor; break;
            case LogLevel.DEBUG: Col = DebugColor; break;
            default: break;
        }

        if (type is LogType.DEBUG) {
            Col = DebugColor;
        }

        var Entry = $"{Timestamp()} [{type}] {level}: {message}";
        Console.WriteLine(Entry, Col);
        Write(Entry);
    }

    // Write to the current log file, creating it if it doesn't exist, and delete the oldest log if MaxLogs is exceeded
    public void Write(string entry) {
        var LogDir = new DirectoryInfo("logs");
        var LogFiles = LogDir.GetFiles().Where(F => F.ToString().Contains("_log.txt"));

        if (LogFiles.Count() > MaxLogs) {
            var Oldest = LogFiles.OrderBy(F => F.LastWriteTime).First();
            File.Delete(Oldest.ToString());
        }

        using var SW = new StreamWriter(LogFile, append: true);
        SW.Write(entry + "\n");
        SW.Close();
    }
}