using System.IO;
using System.Threading;

namespace Clothesline;

/// <summary>
/// 监听系统截图文件夹（Win+PrintScreen 落点），文件写完后回调 CaptureFound。
/// 文件系统事件在写完成前就会触发，所以做 350ms 去抖 + 可读性重试。
/// </summary>
public sealed class FolderWatcher : IDisposable
{
    private readonly FileSystemWatcher _fsw;
    private readonly Timer _timer;
    private readonly Queue<string> _queue = new();
    private readonly object Gate = new();
    private static readonly TimeSpan Debounce = TimeSpan.FromMilliseconds(350);

    public Action<string>? CaptureFound;

    public FolderWatcher(string dir)
    {
        Directory.CreateDirectory(dir);
        _fsw = new FileSystemWatcher(dir)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            IncludeSubdirectories = false,
        };
        _fsw.Created += (_, e) => Enqueue(e.FullPath);
        _fsw.Renamed += (_, e) => Enqueue(e.FullPath);
        _fsw.Error += (_, e) => Logger.Log("fsw error: " + e.GetException()?.Message);
        _fsw.EnableRaisingEvents = true;

        _timer = new Timer(Fire, null, Timeout.Infinite, Timeout.Infinite);
        Logger.Log("watching folder: " + dir);
    }

    private void Enqueue(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        if (ext is not (".png" or ".jpg" or ".jpeg")) return;
        lock (Gate) _queue.Enqueue(path);
        _timer.Change((int)Debounce.TotalMilliseconds, Timeout.Infinite);
    }

    private void Fire(object? state)
    {
        var batch = new List<string>();
        lock (Gate)
        {
            while (_queue.Count > 0) batch.Add(_queue.Dequeue());
        }
        foreach (var path in batch)
        {
            for (int i = 0; i < 6; i++)
            {
                try
                {
                    using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        break;
                    }
                }
                catch (IOException) { Thread.Sleep(250); }
                catch { break; }
            }
            CaptureFound?.Invoke(path);
        }
    }

    public void Dispose()
    {
        _fsw.Dispose();
        _timer.Dispose();
    }
}
