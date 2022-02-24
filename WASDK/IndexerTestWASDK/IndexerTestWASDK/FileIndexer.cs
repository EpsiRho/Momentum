using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace IndexerTestWASDK
{
    public static class FileIndexer
    {
        public static int IndexedFiles;
        public static int IndexedFolders;
        public static int FolderErrors;
        public static int CurrentIndent;
        public static string Message;
        public static bool NeedsDisplay;
        public static Stopwatch Watch;
        public static ConcurrentDictionary<string, List<IndexedFileInfo>> Files;
        public static bool IsFullyLoaded;
        public static async void IndexFiles(object path)
        {
            string p = path as string;
            NeedsDisplay = true;
            Message = "Indexing Files";
            var dictionary = await SearchDirectory(p);
            Message = "Saving to File";
            SaveIndexesToFile(dictionary);
            NeedsDisplay = false;
        }

        private static async Task<Dictionary<string, List<IndexedFileInfo>>> SearchDirectory(string path)
        {
            //Message = "Searching dir";
            var dictionary = new Dictionary<string, List<IndexedFileInfo>>();
            try
            {
                var dirs = Directory.GetDirectories(path);
                if (dirs.Length > 0)
                {
                    foreach (var dir in dirs)
                    {
                        CurrentIndent++;
                        var ret = await SearchDirectory(dir);
                        CurrentIndent--;
                        foreach (var f in ret)
                        {
                            //Message = "Indexing Files";
                            if (dictionary.ContainsKey(f.Key))
                            {
                                foreach (var s in f.Value)
                                {
                                    dictionary[f.Key].Add(s);
                                }
                            }
                            else
                            {
                                dictionary.Add(f.Key, new List<IndexedFileInfo>(f.Value));
                            }
                            IndexedFiles++;
                        }
                    }
                }

                var files = Directory.GetFiles(path);
                //Message = "Indexing Files";
                foreach (var file in files)
                {
                    string name = file.Split('\\').Last().ToLower();
                    var list = new List<IndexedFileInfo>();
                    list.Add(new IndexedFileInfo() { Icon = "", Path = file });
                    if (dictionary.ContainsKey(name))
                    {
                        foreach (var s in list)
                        {
                            dictionary[name].Add(s);
                        }
                    }
                    else
                    {
                        dictionary.Add(name, list);
                    }
                    IndexedFiles++;
                }
                string foldername = path.Split('\\').Last().ToLower();
                var flist = new List<IndexedFileInfo>();
                flist.Add(new IndexedFileInfo() { Icon = "", Path = path });
                IndexedFolders++;
                if (dictionary.ContainsKey(foldername))
                {
                    dictionary[foldername].Add(new IndexedFileInfo() { Icon = "", Path = path });
                }
                else
                {
                    dictionary.Add(foldername, flist);
                }
            }
            catch (Exception)
            {
                FolderErrors++;
                return new Dictionary<string, List<IndexedFileInfo>>();
            }

            return dictionary;
        }

        private static void SaveIndexesToFile(Dictionary<string, List<IndexedFileInfo>> dictionary)
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
            Files = new ConcurrentDictionary<string, List<IndexedFileInfo>>();
            foreach (var item in dictionary)
            {
                Files.TryAdd(item.Key, item.Value);
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
                    writer.WriteValue(file.Icon);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                sw.Flush();
            }
            writer.WriteEndObject();
            
            writer.Flush();
            writer.Close();

            //File.WriteAllText($"C:\\Users\\jhset\\Desktop\\IndexReal.json", text);

        }

        public static async void LoadIndexesFromFile()
        {
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

                var file = await StorageFile.GetFileFromPathAsync($"{p}\\Momentum\\Index.json");
                StreamReader sr = File.OpenText(file.Path);
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
                            string icon = reader.Value.ToString();
                            reader.Read();
                            reader.Read();
                            if (Files.ContainsKey(name))
                            {
                                Files[name].Add(new IndexedFileInfo() { Name = name, Path = path, Icon=icon });
                            }
                            else
                            {
                                var lst = new List<IndexedFileInfo>();
                                lst.Add(new IndexedFileInfo() { Name = name, Path = path, Icon = icon });
                                Files.TryAdd(name, lst);
                            }
                            if (reader.TokenType == JsonToken.EndArray)
                            {
                                break;
                            }
                        }
                    }
                }
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
