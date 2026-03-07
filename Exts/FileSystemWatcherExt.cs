using System.IO;

namespace Sheas_Cealer.Exts;

internal static class FileSystemWatcherExt
{
    extension(FileSystemWatcher watcher)
    {
        internal void Start() => watcher.EnableRaisingEvents = true;
        internal void Stop() => watcher.EnableRaisingEvents = false;
    }
}