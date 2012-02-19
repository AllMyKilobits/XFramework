using System;
using System.Collections.Generic;
using System.IO;

namespace XF
{
    static class FileWatch
    {
        static List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();

        static internal void add_folder(string path)
        {
            if (!Application.asset_tracking) return;

            var w = new FileSystemWatcher(path);            
            w.Path = path;
            w.IncludeSubdirectories = true;
            w.NotifyFilter = NotifyFilters.LastWrite;
            w.Changed += new FileSystemEventHandler(trigger_on_change);
            w.EnableRaisingEvents = true;
            _watchers.Add(w);

        }

        static private void trigger_on_change(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                var id = Graphics.Texture.id_from_path(e.FullPath);
                Graphics.enqueue_texture_reload(id);
            }
        }

    }
}
