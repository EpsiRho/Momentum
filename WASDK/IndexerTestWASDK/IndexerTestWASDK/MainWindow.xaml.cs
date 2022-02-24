using Microsoft.AppCenter.Crashes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IndexerTestWASDK
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WinUIEx.WindowEx
    {
        ObservableCollection<IndexedFileInfo> collection;
        Thread TimerThread;
        int TimeTillSearch;

        public MainWindow()
        {
            this.InitializeComponent();
            collection = new ObservableCollection<IndexedFileInfo>();
            OnLoad();
        }

        private async void OnLoad()
        {
            bool didAppCrash = await Crashes.HasCrashedInLastSessionAsync();
            if (didAppCrash)
            {
                ErrorReport crashReport = await Crashes.GetLastSessionCrashReportAsync();
            }
            if (await FileIndexer.IsIndexAvailable())
            {
                Thread t = new Thread(FileIndexer.LoadIndexesFromFile);
                t.Start();
                Thread b = new Thread(WaitForIndex);
                b.Start();
            }
            else
            {
                LoadProgress.Visibility = Visibility.Collapsed;
                LoadingSaved.Visibility = Visibility.Collapsed;
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadProgress.Visibility = Visibility.Visible;
            Thread t = new Thread(FileIndexer.IndexFiles);
            t.Start(FolderChoiceBox.Text);
            FileIndexer.NeedsDisplay = true;
            Thread d = new Thread(WatchDisplay);
            d.Start();
        }

        private async void WaitForIndex()
        {
            while (!FileIndexer.IsFullyLoaded)
            {

            }
            this.DispatcherQueue.TryEnqueue(() =>
            {
                LoadProgress.Visibility = Visibility.Collapsed;
                LoadingSaved.Visibility = Visibility.Collapsed;
            });
        }

        private void WatchDisplay()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (FileIndexer.NeedsDisplay)
            {
                try
                {
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        WatchText.Text = $"{watch.Elapsed.Hours.ToString("D2")}:{watch.Elapsed.Minutes.ToString("D2")}:{watch.Elapsed.Seconds.ToString("D2")}";
                        IndexedCount.Text = $"Indexed {FileIndexer.IndexedFiles} files";
                    });
                }

                catch (Exception ex)
                {

                }
                Thread.Sleep(100);
            }
            watch.Stop();
            this.DispatcherQueue.TryEnqueue(() =>
            {
                LoadProgress.Visibility = Visibility.Collapsed;
            });
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TimeTillSearch = 3;
                if (TimerThread == null)
                {
                    TimerThread = new Thread(() =>
                    {
                        try
                        {
                            while (TimeTillSearch > 0)
                            {
                                TimeTillSearch--;
                                Thread.Sleep(100);
                            }
                            Search();
                            TimerThread = null;
                        }
                        catch (Exception)
                        {

                        }
                    });
                    TimerThread.Start();
                }
            }
            catch (Exception)
            {

            }
        }

        private async void Search()
        {
            try
            {
                string text = "|||";
                DispatcherQueue.TryEnqueue(() =>
                {
                    collection.Clear();
                    text = SearchBox.Text.ToLower();
                });
                while (text == "|||") { }
                var list = FileIndexer.Files.Where(o => o.Key.Contains(text)).ToList();
                for (int i = 0; i < 100; i++)
                {
                    try
                    {
                        foreach (var line in list[i].Value)
                        {
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                collection.Add(new IndexedFileInfo() { Name = line.Name, Path = line.Path, Icon=line.Icon });
                            });
                        }
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
            }
            catch(Exception) 
            {

            }
        }
    }
}
