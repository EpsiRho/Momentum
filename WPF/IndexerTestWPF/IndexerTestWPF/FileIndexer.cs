using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndexerTestWPF
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
        public static async void IndexFiles()
        {
            NeedsDisplay = true;
            //Watch = Stopwatch.StartNew();
            Message = "Indexing Files";
            var dictionary = await SearchDirectory("C:\\");
            Message = "Saving to File";
            Watch = Stopwatch.StartNew();
            SaveIndexesToFile(dictionary);
            Watch.Stop();
            NeedsDisplay = false;
            //Watch.Stop();
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
                    list.Add(new IndexedFileInfo() { Icon = "&#xE130;", Path = file });
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
                flist.Add(new IndexedFileInfo() { Icon = "&#xE8B7;", Path = path });
                IndexedFolders++;
                if (dictionary.ContainsKey(foldername))
                {
                    dictionary[foldername].Add(new IndexedFileInfo() { Icon = "&#xE8B7;", Path = path });
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
            string text = JsonConvert.SerializeObject(dictionary, Formatting.Indented);


            File.WriteAllText($"C:\\Users\\jhset\\Desktop\\Index.json", text);

        }

        public static async void LoadIndexesFromFile()
        {
            Stopwatch sw = Stopwatch.StartNew();
            string text = File.ReadAllText("C:\\Users\\jhset\\Desktop\\Index.json");

            var obj = JsonConvert.DeserializeObject<Dictionary<string, List<IndexedFileInfo>>>(text);
           
            sw.Stop();
            string efef= sw.Elapsed.ToString();
        }
    }
}
