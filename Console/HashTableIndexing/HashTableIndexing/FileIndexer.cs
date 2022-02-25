using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HashTableIndexing
{
    public static class FileIndexer
    {
        public static void IndexFiles()
        {
            var dictionary = SearchDirectory("C:\\");
            Display.Message = "Saving to File";
            Display.watch.Stop();
            Console.ReadLine();
            Display.watch.Reset();
            Display.watch.Start();
            SaveIndexesToFile(dictionary);
        }

        private static Dictionary<string, List<IndexedFileInfo>> SearchDirectory(string path)
        {
            Display.Message = "Searching dir";
            var dictionary = new Dictionary<string, List<IndexedFileInfo>>();
            try
            {
                var dirs = Directory.GetDirectories(path);
                if (dirs.Length > 0)
                {
                    foreach (var dir in dirs)
                    {
                        Display.Indent++;
                        var ret = SearchDirectory(dir);
                        Display.Indent--;
                        foreach (var f in ret)
                        {
                            Display.Message = "Indexing";
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
                            Display.TotalIndexed++;
                        }
                    }
                }

                var files = Directory.GetFiles(path);
                Display.Message = "Indexing";
                foreach (var file in files)
                {
                    string name = file.Split("\\").Last().ToLower();
                    var list = new List<IndexedFileInfo>();
                    list.Add(new IndexedFileInfo() { Icon = "&#xE130;", Path = file});
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
                    Display.TotalIndexed++;
                }
                string foldername = path.Split('\\').Last().ToLower();
                var flist = new List<IndexedFileInfo>();
                flist.Add(new IndexedFileInfo() { Icon = "&#xE8B7;", Path = path });
                Display.IndexedFolders++;
                if (dictionary.ContainsKey(foldername))
                {
                    dictionary[foldername].Add(new IndexedFileInfo() { Icon = "&#xE8B7;", Path = path });
                }
                else
                {
                    dictionary.Add(foldername, flist);
                }
            }
            catch (Exception ex)
            {
                Display.TotalErrored++;
                return new Dictionary<string, List<IndexedFileInfo>>();
            }

            return dictionary;
        }

        private static void SaveIndexesToFile(Dictionary<string, List<IndexedFileInfo>> dictionary)
        {
            //using (var memoryStream = new MemoryStream())
            //{
            //    var binaryFormatter = new BinaryFormatter();
            //
            //    binaryFormatter.Serialize(memoryStream, dictionary);
            //
            //    var stream = File.Open("C:\\Users\\jhset\\Desktop\\Index.bin", FileMode.Create, FileAccess.Write);
            //    stream.Write(memoryStream.ToArray(), 0, (int)memoryStream.Length);
            //    stream.Flush();
            //    stream.Close();
            //}

            string text = JsonConvert.SerializeObject(dictionary, Formatting.Indented);

            File.WriteAllText("C:\\Users\\jhset\\Desktop\\Index.json", text);
        }

        public static Dictionary<string, List<IndexedFileInfo>> LoadIndexesFromFile()
        {
            try
            {
                //using (var memoryStream = new MemoryStream())
                //{
                //    var stream = File.Open("C:\\Users\\jhset\\Desktop\\Index.bin", FileMode.Open, FileAccess.Read);
                //
                //    var binaryFormatter = new BinaryFormatter();
                //
                //    stream.CopyTo(memoryStream);
                //    memoryStream.Seek(0, SeekOrigin.Begin);
                //
                //    return (Dictionary<string, List<string>>)binaryFormatter.Deserialize(memoryStream);
                //}

                string text =File.ReadAllText("C:\\Users\\jhset\\Desktop\\Index.json");

                var obj = JsonConvert.DeserializeObject<Dictionary<string, List<IndexedFileInfo>>>(text);
                return obj;

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
