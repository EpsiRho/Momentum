using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace IndexerTestWASDK
{
    public static class BackgroundIndexer 
    {
        public static List<FileSystemWatcher> watchers;

        public static void InitWatcher()
        {
            watchers = new List<FileSystemWatcher>();

            //FileIndexer.Files = new ConcurrentDictionary<string, List<IndexedFileInfo>>();

            //FileIndexer.LoadIndexesFromFile();

            var driveLetters = DriveInfo.GetDrives().Select(x => x.RootDirectory.Root).ToList().OrderBy(x => x.Root.FullName).ToList();
            foreach (var driveInfo in driveLetters)
            {
                if (driveInfo.Name.Contains("C"))
                {
                    continue;
                }
                var watcher = new FileSystemWatcher(driveInfo.ToString());

                watcher.NotifyFilter = NotifyFilters.Attributes
                                       | NotifyFilters.CreationTime
                                       | NotifyFilters.DirectoryName
                                       | NotifyFilters.FileName
                                       | NotifyFilters.LastAccess
                                       | NotifyFilters.LastWrite
                                       | NotifyFilters.Security
                                       | NotifyFilters.Size;

                //watcher.Changed += OnChanged;
                watcher.Created += OnCreated;
                watcher.Deleted += OnDeleted;
                watcher.Renamed += OnRenamed;

                watcher.Filter = "";
                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;

                watchers.Add(watcher);
            }


            int Count = 0;
            while (true)
            {
                if (Process.GetProcessesByName("Momentum File Search").Length > 1)
                {
                    
                }
                else if (Process.GetProcessesByName("Momentum File Search").Length > 0)
                {

                }
                Thread.Sleep(100);
                Count++;
                if(Count == 1)
                {

                }
                //new ToastContentBuilder()
                //   .AddText("OH YEAH")
                //   .Show();
                //FileIndexer.SaveIndexesToFile();
            }
        }

        //private static void OnChanged(object sender, FileSystemEventArgs e)
        //{
        //    if (e.ChangeType != WatcherChangeTypes.Changed)
        //    {
        //        return;
        //    }
        //    Console.WriteLine($"[%] Changed: {e.FullPath}");
        //}

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            IndexedFileInfo item = new IndexedFileInfo();
            item.Name = e.Name.Split("\\").Last();
            item.Path = e.FullPath;
            try
            {
                FileAttributes attributes = File.GetAttributes(e.FullPath);

                switch (attributes)
                {
                    case FileAttributes.Directory:
                        item.Type = "Folder";
                        break;
                    default:
                        item.Type = "File";
                        break;
                }
            }
            catch (Exception)
            {

            }

            bool x = FileIndexer.Files.TryAdd(item.Name.ToLower(), new List<IndexedFileInfo>() { item });
            if (!x)
            {
                FileIndexer.Files[item.Name].Add(item);
            }
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            string Name = e.Name.Split("\\").Last().ToLower();
            try
            {
                foreach (var item in FileIndexer.Files[Name])
                {
                    if (item.Path == e.FullPath)
                    {
                        FileIndexer.Files[Name].Remove(item);
                    }
                }
            }
            catch (Exception) { }

            try
            {
                if (FileIndexer.Files[Name].Count() == 0)
                {
                    var item = FileIndexer.Files[Name];
                    FileIndexer.Files.Remove(Name, out item);
                }
            }
            catch (Exception) { }

        }

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            string oldName = e.OldName.Split("\\").Last().ToLower();
            string newName = e.Name.Split("\\").Last();

            try
            {
                foreach (var item in FileIndexer.Files[oldName])
                {
                    if (item.Path == e.OldFullPath)
                    {
                        FileIndexer.Files[oldName].Remove(item);
                    }
                }
            }
            catch (Exception) { }

            try
            {
                if (FileIndexer.Files[oldName].Count() == 0)
                {
                    var item = FileIndexer.Files[oldName];
                    FileIndexer.Files.Remove(oldName, out item);
                }
            }
            catch (Exception) { }

            IndexedFileInfo newitem = new IndexedFileInfo();
            newitem.Name = newName;
            newitem.Path = e.FullPath;
            try
            {
                FileAttributes attributes = File.GetAttributes(e.FullPath);

                switch (attributes)
                {
                    case FileAttributes.Directory:
                        newitem.Type = "Folder";
                        break;
                    default:
                        newitem.Type = "File";
                        break;
                }
            }
            catch (Exception)
            {

            }

            bool x = FileIndexer.Files.TryAdd(newitem.Name.ToLower(), new List<IndexedFileInfo>() { newitem });
            if (!x)
            {
                FileIndexer.Files[newitem.Name].Add(newitem);
            }
        }
    }
}
