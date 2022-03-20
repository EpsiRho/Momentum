using Microsoft.AppCenter.Crashes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Media.Imaging;
using WinRT;

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
        ObservableCollection<FileIndexer> ProgressCollection;
        ObservableCollection<string> folders;
        List<string> SelectedFolders;
        int IndexingThreads;
        Thread TimerThread;
        int TimeTillSearch;
        private bool NeedsStop;

        public MainWindow()
        {
            this.InitializeComponent();
            TimerThread = null;
            NeedsStop = false;
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            collection = new ObservableCollection<IndexedFileInfo>();
            ProgressCollection = new ObservableCollection<FileIndexer>();
            folders = new ObservableCollection<string>();
            OnLoad();
        }

        private async void OnLoad()
        {
            FileIndexer.Files = new ConcurrentDictionary<string, List<IndexedFileInfo>>();
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
                folders.Clear();
                SettingsGrid.Visibility = Visibility.Visible;
                CloseSettingsButton.IsEnabled = false;
                var driveLetters = DriveInfo.GetDrives().Select(x => x.RootDirectory.Root).ToList().OrderBy(x => x.Root.FullName).ToList();
                foreach (var driveInfo in driveLetters)
                {
                    folders.Add(driveInfo.ToString());

                }
            }
        }

        private async void WaitForIndex()
        {
            while (!FileIndexer.IsFullyLoaded)
            {

            }
            this.DispatcherQueue.TryEnqueue(() =>
            {
                LoadProgress.Visibility = Visibility.Collapsed;
            });
        }

        //private void WatchDisplay()
        //{
        //    Stopwatch watch = new Stopwatch();
        //    watch.Start();
        //    while (FileIndexer.NeedsDisplay)
        //    {
        //        try
        //        {
        //            this.DispatcherQueue.TryEnqueue(() =>
        //            {
        //                WatchText.Text = $"{watch.Elapsed.Hours.ToString("D2")}:{watch.Elapsed.Minutes.ToString("D2")}:{watch.Elapsed.Seconds.ToString("D2")}";
        //                IndexedCount.Text = $"Indexed {FileIndexer.IndexedFiles} files";
        //            });
        //        }

        //        catch (Exception ex)
        //        {

        //        }
        //        Thread.Sleep(100);
        //    }
        //    watch.Stop();
        //    this.DispatcherQueue.TryEnqueue(() =>
        //    {
        //        LoadProgress.Visibility = Visibility.Collapsed;
        //    });
        //}

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //TimeTillSearch = 3;
                //if (TimerThread == null)
                //{
                if (SearchBox.Text == "")
                {
                    collection.Clear();
                    return;
                }
                NeedsStop = true;
                if (TimerThread != null)
                {
                    while(NeedsStop){}
                }
                TimerThread = new Thread(() =>
                {
                    try
                    {
                        //while (TimeTillSearch > 0)
                        //{
                        //    TimeTillSearch--;
                        //    Thread.Sleep(200);
                        //}
                        NeedsStop = false;
                        Search();
                        TimerThread = null;
                    }
                    catch (Exception)
                    {

                    }
                });
                TimerThread.Start();
                //}
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
                while (text == "|||")
                {
                    if (NeedsStop)
                    {
                        NeedsStop = false;
                        return;
                    }
                }

                if (text == "")
                {
                    NeedsStop = false;
                    return;
                }

                var list = FileIndexer.Files.Where(o => o.Key.Contains(text)).ToList();
                for (int i = 0; i < 100; i++)
                {
                    if (NeedsStop)
                    {
                        NeedsStop = false;
                        return;
                    }
                    try
                    {
                        foreach (var line in list[i].Value)
                        {
                            await DispatcherQueue.EnqueueAsync(() =>
                            {
                                collection.Add(new IndexedFileInfo() { Name = line.Name, Path = line.Path, Icon = line.Icon });
                            });
                        }
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }

                Thread.Sleep(500);

                foreach (var line in collection)
                {
                    if (NeedsStop)
                    {
                        NeedsStop = false;
                        return;
                    }
                    try
                    {
                        StorageFile file = await StorageFile.GetFileFromPathAsync(line.Path);
                        var icon = await file.GetThumbnailAsync(ThumbnailMode.ListView);
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            try
                            {
                                var image = new BitmapImage();
                                image.SetSource(icon);
                                line.Image = image;
                            }
                            catch (Exception ex)
                            {

                            }

                        });
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch(Exception) 
            {

            }
        }

        private async void SearchListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (IndexedFileInfo)e.ClickedItem;
            try
            {

                var file = await StorageFile.GetFileFromPathAsync(item.Path);

                if (file != null)
                {
                    // Launch the retrieved file
                    var success = await Windows.System.Launcher.LaunchFileAsync(file);

                    if (!success)
                    {
                        var folder = await StorageFolder.GetFolderFromPathAsync(item.Path);
                        Windows.System.Launcher.LaunchFolderAsync(folder);
                    }
                }
            }
            catch (Exception)
            {
                try
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(item.Path);
                    Windows.System.Launcher.LaunchFolderAsync(folder);
                }
                catch (Exception)
                {

                }
            }
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            folders.Clear();
            ProgressCollection.Clear();
            SettingsGrid.Visibility = Visibility.Visible;
            var driveLetters = DriveInfo.GetDrives().Select(x => x.RootDirectory.Root).ToList().OrderBy(x => x.Root.FullName).ToList();
            foreach (var driveInfo in driveLetters)
            {
                folders.Add(driveInfo.ToString());
            }
        }

        private void CloseSettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Visibility = Visibility.Collapsed;
        }

        private void SaveSettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            DrivesList.IsEnabled = false;
            CloseSettingsButton.IsEnabled = false;
            SaveSettingsButton.IsEnabled = false;
            FileIndexer.Files.Clear();
            Thread t = new Thread(IndexHandler);
            t.Start();
            CloseSettingsButton.IsEnabled = false;
        }

        private void IndexHandler()
        {
            IndexingThreads = 0;
            var DriveList = DriveInfo.GetDrives();
            foreach (var drive in SelectedFolders)
            {
                var item = new FileIndexer();
                item.Name = drive;
                item.NeedsDisplay = true;
                item.Maximum = 1;

                //foreach (var d in DriveList)
                //{
                //    if (d.Name == item.Name)
                //    {
                //        item.Maximum = d.TotalSize;
                //    }
                //}
                DispatcherQueue.TryEnqueue(() =>
                {
                    item.IsIndexing = true;
                    ProgressCollection.Add(item);
                });

            }

            Thread.Sleep(500);

            foreach (var item in ProgressCollection)
            {
                Thread d = new Thread(item.IndexFiles);
                d.Start(DispatcherQueue);
                IndexingThreads++;
                //if (IndexingThreads > 1)
                //{
                //    while (item.NeedsDisplay){}
                //}
            }

            while (true)
            {
                bool check = true;
                foreach (var item in ProgressCollection)
                {
                    if (item.NeedsDisplay)
                    {
                        check = false;
                    }
                }

                if (check)
                {
                    break;
                }
            }

            DispatcherQueue.TryEnqueue(() =>
            {
                SavingGrid.Visibility = Visibility.Visible;
            });

            FileIndexer.SaveIndexesToFile();
            DispatcherQueue.TryEnqueue(() =>
            {
                SettingsGrid.Visibility = Visibility.Collapsed;
                SavingGrid.Visibility = Visibility.Collapsed;
                DrivesList.IsEnabled = true;
                CloseSettingsButton.IsEnabled = true;
                SaveSettingsButton.IsEnabled = true;
            });
        }

        private void DrivesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedFolders = new List<string>();
            foreach (var drive in DrivesList.SelectedItems)
            {
                SelectedFolders.Add(drive as string);
            }
        }
    }
}
