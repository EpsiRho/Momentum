using Microsoft.UI.Dispatching;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace IndexerTestWASDK
{
    public class FileIndexer : INotifyPropertyChanged
    {
        public bool NeedsDisplay;
        private double progress;
        public double Progress
        {
            get { return progress; }
            set
            {
                if (value != progress)
                {
                    progress = value;
                    NotifyPropertyChanged(nameof(Progress));
                }
            }
        }
        private bool isIndexing;
        public bool IsIndexing
        {
            get { return isIndexing; }
            set
            {
                if (value != isIndexing)
                {
                    isIndexing = value;
                    NotifyPropertyChanged(nameof(IsIndexing));
                }
            }
        }
        public string Name { get; set; }
        public double Maximum { get; set; }
        public static ConcurrentDictionary<string, List<IndexedFileInfo>> Files;
        public static bool IsFullyLoaded;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async void IndexFiles(object q)
        {
            DispatcherQueue queue = q as DispatcherQueue;
            var dictionary = await SearchDirectory(queue, Name);
            foreach (var item in dictionary)
            {
                bool x = Files.TryAdd(item.Key, item.Value);
                if (!x)
                {
                    foreach (var file in item.Value)
                    {
                        Files[item.Key].Add(file);
                    }
                }
            }
            queue.TryEnqueue(DispatcherQueuePriority.Low ,() =>
            {
                IsIndexing = false;
                Progress = 1;
            });
            NeedsDisplay = false;
        }

        private async Task<Dictionary<string, List<IndexedFileInfo>>> SearchDirectory(DispatcherQueue queue, string path)
        {
            var dictionary = new Dictionary<string, List<IndexedFileInfo>>(); 
            try
            {
                var dirs = Directory.GetDirectories(path);
                if (dirs.Length > 0)
                {
                    foreach (var dir in dirs)
                    {
                        var ret = await SearchDirectory(queue, dir);
                        foreach (var f in ret)
                        {
                            if (dictionary.ContainsKey(f.Key))
                            {
                                foreach (var s in f.Value)
                                {
                                    try
                                    {
                                        dictionary[f.Key].Add(s);

                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                            }
                            else
                            {
                                    try
                                    {
                                        dictionary.Add(f.Key, new List<IndexedFileInfo>(f.Value));
                                    }
                                    catch (Exception)
                                    {

                                    }
                            }
                        }
                    }
                }

                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    
                    string name = Path.GetFileName(file).ToLower();
                    var list = new List<IndexedFileInfo>();
                    list.Add(new IndexedFileInfo() { Name = name, Type = "File", Path = file });
                    if (dictionary.ContainsKey(name))
                    {
                        foreach (var s in list)
                        {
                            try
                            {
                                dictionary[name].Add(s);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    else
                    {
                            try
                            {
                                dictionary.Add(name, new List<IndexedFileInfo>(list));
                            }
                            catch (Exception)
                            {

                            }
                    }

                }
                string foldername = Path.GetDirectoryName(path).ToLower();
                var flist = new List<IndexedFileInfo>();
                flist.Add(new IndexedFileInfo() { Name = foldername, Type = "Folder", Path = path });
                if (dictionary.ContainsKey(foldername))
                {
                    dictionary[foldername].Add(new IndexedFileInfo() { Name = foldername, Type = "Folder", Path = path });
                }
                else
                {
                    dictionary.Add(foldername, flist);
                }
            }
            catch (Exception e)
            {

            }

            return dictionary;
        }

        public static void SaveIndexesToFile()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string[] dirs = Directory.GetDirectories(path);
            if (!dirs.Contains("Momentum"))
            {
                System.IO.Directory.CreateDirectory($"{path}\\Momentum");
            }

            StreamWriter sw = File.CreateText($"{path}\\Momentum\\Index.json");
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartObject();
            foreach (var item in Files)
            {
                writer.WritePropertyName(item.Key);
                writer.WriteStartArray();
                foreach (var file in item.Value)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Name");
                    writer.WriteValue(item.Key);
                    writer.WritePropertyName("Path");
                    writer.WriteValue(file.Path);
                    writer.WritePropertyName("Icon");
                    writer.WriteValue(file.Type);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                sw.Flush();
            }
            writer.WriteEndObject();
            
            writer.Flush();
            writer.Close();

        }

        public static async void LoadIndexesFromFile()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                IsFullyLoaded = false;
                Files = new ConcurrentDictionary<string, List<IndexedFileInfo>>();

                string p = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string[] dirs = Directory.GetDirectories(p);
                if (!dirs.Contains("Momentum"))
                {
                    System.IO.Directory.CreateDirectory($"{p}\\Momentum");
                }

                StreamReader sr = File.OpenText($"{p}\\Momentum\\Index.json");
                //string text = await FileIO.ReadTextAsync(file);

                JsonTextReader reader = new JsonTextReader(sr);

                while (reader.Read())
                {
                    if (reader.Value != null)
                    {
                        var item = new KeyValuePair<string, List<IndexedFileInfo>>();
                        reader.Read();
                        reader.Read();
                        while (true)
                        {
                            reader.Read();
                            reader.Read();
                            string name = reader.Value.ToString();
                            reader.Read();
                            reader.Read();
                            string path = reader.Value.ToString();
                            reader.Read();
                            reader.Read();
                            string type = reader.Value.ToString();
                            reader.Read();
                            reader.Read();
                            if (Files.ContainsKey(name))
                            {
                                Files[name].Add(new IndexedFileInfo() { Name = name, Path = path, Type= type });
                            }
                            else
                            {
                                var lst = new List<IndexedFileInfo>();
                                lst.Add(new IndexedFileInfo() { Name = name, Path = path, Type = type });
                                Files.TryAdd(name, lst);
                            }
                            if (reader.TokenType == JsonToken.EndArray)
                            {
                                break;
                            }
                        }
                    }
                }
                sw.Stop();
                IsFullyLoaded = true;

                //var obj = JsonReader .DeserializeObject<Dictionary<string, List<IndexedFileInfo>>>(text);
                //Files = obj;

            }
            catch (Exception)
            {
                return;
            }
        }

        public static async Task<bool> IsIndexAvailable()
        {
            try
            {
                string p = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string[] dirs = Directory.GetDirectories(p);
                if (!dirs.Contains("Momentum"))
                {
                    System.IO.Directory.CreateDirectory($"{p}\\Momentum");
                }

                var file = await StorageFile.GetFileFromPathAsync($"{p}\\Momentum\\Index.json");
                if (file != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
