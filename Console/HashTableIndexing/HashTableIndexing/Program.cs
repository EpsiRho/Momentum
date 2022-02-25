// See https://aka.ms/new-console-template for more information
using HashTableIndexing;
using System.Diagnostics;
using System.Linq;

Console.Clear();
Thread t = new Thread(Display.Draw);
t.Start();
FileIndexer.IndexFiles();
Display.watch.Stop();
Display.NeedsDisplay = false;
Thread.Sleep(200);
//Console.WriteLine($"[!] Indexing Complete: {Display.watch.Elapsed.Hours.ToString("D2")}:{Display.watch.Elapsed.Minutes.ToString("D2")}:{Display.watch.Elapsed.Seconds.ToString("D2")}");
Console.WriteLine($"> Complete, Hit Enter to continue <");
Console.ReadLine();

return;

Stopwatch stopwatch = Stopwatch.StartNew();
Console.WriteLine("[-] Loading Index File:");
var files = FileIndexer.LoadIndexesFromFile();
stopwatch.Stop();
Console.Clear();
Console.WriteLine($"[+] Loading Index File: {stopwatch.Elapsed.Hours.ToString("D2")}:{stopwatch.Elapsed.Minutes.ToString("D2")}:{stopwatch.Elapsed.Seconds.ToString("D2")}");
Console.WriteLine($"> Hit Enter to continue <");
Console.ReadLine();

Console.Clear();

string input = Console.ReadLine();
stopwatch.Reset();
stopwatch.Start();
var lst = files.Where(o => o.Key.Contains(input)).ToList();

for (int i = 0; i < 15; i++)
{
    try
    {
        foreach (var line in lst[i].Value)
        {
            Console.WriteLine($"{lst[i].Key}");
        }
    }
    catch (Exception)
    {

    }
}

stopwatch.Stop();
Console.WriteLine(Display.watch.Elapsed.ToString());

return;

while (true)
{
    Console.Write($"> {input} ");
    Console.CursorLeft--;
    var key = Console.ReadKey();
    Console.Clear();
    if (key.Key == ConsoleKey.Backspace)
    {
        try
        {
            input = input.Substring(0, input.Length - 1);
        }
        catch (Exception)
        {

        }
    }
    else if (key.Key == ConsoleKey.Enter)
    {
        Console.WriteLine("\n----------------------");
        //var lst = files.Where(o => o.Key.Contains(input)).ToList();
        foreach (var file in lst)
        {
            foreach (var line in file.Value)
            {
                Console.WriteLine($"{line.Path}");
            }
        }
        continue;
    }
    else
    {
        input += key.KeyChar;
    }

    Console.WriteLine("\n----------------------");
    
    var list = files.Where(o => o.Key.Contains(input)).ToList();

    for(int i = 0; i < 15; i++)
    {
        try
        {
            foreach (var line in list[i].Value)
            {
                Console.WriteLine($"{list[i].Key}");
            }
        }
        catch (Exception)
        {

        }
    }

    Console.CursorLeft = 0;
    Console.CursorTop = 0;
}