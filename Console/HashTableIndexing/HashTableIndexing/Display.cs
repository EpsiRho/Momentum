using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashTableIndexing
{
    public static class Display
    {
        public static Stopwatch watch;
        public static bool NeedsDisplay;
        public static double TotalIndexed;
        public static double IndexedFolders;
        public static double TotalErrored;
        public static string Message;
        public static double Indent;
        public static void Draw()
        {
            Console.CursorVisible = false;
            Indent = 0;
            watch = Stopwatch.StartNew();
            NeedsDisplay = true;
            while (NeedsDisplay)
            {
                Console.WriteLine($"[!] Indexing Files ");
                Console.WriteLine($"[~] {watch.Elapsed.Hours.ToString("D2")}:{watch.Elapsed.Minutes.ToString("D2")}:{watch.Elapsed.Seconds.ToString("D2")}");
                Console.WriteLine($"[+] Indexed {TotalIndexed} files so far");
                Console.WriteLine($"[+] Indexed {IndexedFolders} folders so far");
                Console.WriteLine($"[x] {TotalErrored} errored folders");
                Console.WriteLine($"[#] {Message}");
                Console.WriteLine($"[@] Current indent is {Indent}");
                Console.CursorLeft = 0;
                Console.CursorTop = 0;

            }
        }
    }
}
