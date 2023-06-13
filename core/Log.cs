namespace DotMatrix;

enum LogType { SYSTEM, WARNING, ERROR };

static class Log {
    public static string Timestamp() {
        return DateTime.Now.ToString("[HH:mm:ss]");
    }

    public static void Write(LogType type, string message) {
        Console.WriteLine($"{Timestamp()} {type}: {message}");
    }
}