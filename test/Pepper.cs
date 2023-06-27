using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

// The in-engine debug console, named "Pepper" after my daughter because the name "Console" is used already in .NET

// TODO: Implement Colorful.Console for nicer looking output (https://github.com/tomakita/Colorful.Console)

enum LogType { ENGINE, MATRIX, INTERFACE, OTHER, DEBUG };
enum LogLevel { MESSAGE, WARNING, ERROR, DEBUG };

class Pepper {
    public Engine Engine { get; private set; }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Interface Interface { get { return Engine.Interface; } }
    public Theme Theme { get { return Engine.Theme; } }

    // private int MaxLogs = 5; // Maxiumum number of log files to keep before they are overwritten by new ones

    public Pepper(Engine engine) {
        Engine = engine;
    }

    // Generate the current timestamp (for logs)
    public string Timestamp() {
        return DateTime.Now.ToString("[HH:mm:ss]");
    }

    // Log a message to the console as well as the current log file
    public void Log(LogType type, LogLevel level, string message) {
        Console.WriteLine($"{Timestamp()} [{type}] {level}: {message}");
    }

    // Write to the current log file, creating one if it doesn't exist already
    public void WriteLogFile(string path) {
        // TODO: Write logs to a log file as they are written to the console
    }
}