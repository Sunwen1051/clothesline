using System.IO;

namespace Clothesline;

internal static class Logger
{
    private static string? _path;
    private static readonly object Lock = new();

    public static void Init(string path)
    {
        _path = path;
        try
        {
            File.AppendAllText(path, $"\r\n===== session {DateTime.Now:yyyy-MM-dd HH:mm:ss} =====\r\n");
        }
        catch { }
    }

    public static void Log(string message)
    {
        if (_path == null) return;
        lock (Lock)
        {
            try
            {
                File.AppendAllText(_path, $"{DateTime.Now:HH:mm:ss.fff} {message}\r\n");
            }
            catch { }
        }
    }
}
