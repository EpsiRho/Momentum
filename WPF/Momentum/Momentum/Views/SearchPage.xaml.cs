using Microsoft.WindowsAPICodePack.Shell;
using Momentum.Models;
using Momentum.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Momentum.Views
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class SearchPage : Page
    {
        // Page Vars
        Thread TimerThread;
        private bool NeedsStop;
        SearchViewModel ViewModel;
        int TimeTillSearch;

        public SearchPage()
        {
            InitializeComponent();
            TimerThread = null;
            NeedsStop = false;
            ViewModel = new SearchViewModel();
            ViewModel.FileCollection = new ObservableCollection<IndexedFileInfo>();
            ViewModel.ProgressCollection = new ObservableCollection<FileIndexer>();
            ViewModel.Folders = new ObservableCollection<string>();
            DataContext = this;
            SearchListView.ItemsSource = ViewModel.FileCollection;
            OnLoad();
        }

        private async void OnLoad()
        {
            FileIndexer.Files = new ConcurrentDictionary<string, List<IndexedFileInfo>>();
            if (await FileIndexer.IsIndexAvailable())
            {
                Thread t = new Thread(FileIndexer.LoadIndexesFromFile);
                t.Start();
                Thread b = new Thread(WaitForIndex);
                b.Start();
            }
            else
            {
                //LoadProgress.Visibility = Visibility.Collapsed;
                //folders.Clear();
                //SettingsGrid.Visibility = Visibility.Visible;
                //CloseSettingsButton.IsEnabled = false;
                //var driveLetters = DriveInfo.GetDrives().Select(x => x.RootDirectory.Root).ToList().OrderBy(x => x.Root.FullName).ToList();
                //foreach (var driveInfo in driveLetters)
                //{
                //    folders.Add(driveInfo.ToString());
                //
                //}
            }
        }

        private async void WaitForIndex()
        {
            while (!FileIndexer.IsFullyLoaded)
            {
                if (FileIndexer.Files.Count() == 200)
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
                            NeedsStop = false;
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
            this.Dispatcher.Invoke(() =>
            {
                LoadProgress.Visibility = Visibility.Collapsed;
            });
        }

        private async void Search()
        {
            try
            {
                string text = "|||";
                await this.Dispatcher.InvokeAsync(() =>
                {
                    ViewModel.FileCollection.Clear();
                    text = SearchBox.Text.ToLower();
                });
                while (text == "|||")
                {
                    if (NeedsStop)
                    {
                        ViewModel.FileCollection.Clear();
                        NeedsStop = false;
                        return;
                    }
                }

                int count = 0;
                var exact = FileIndexer.Files.ContainsKey(text);
                var list = FileIndexer.Files.Where(o => o.Key.Contains(text)).ToList();
                if (exact)
                {
                    foreach (var line in FileIndexer.Files[text])
                    {
                        if (NeedsStop)
                        {
                            ViewModel.FileCollection.Clear();
                            NeedsStop = false;
                            return;
                        }
                        this.Dispatcher.InvokeAsync(() =>
                        {
                            ViewModel.FileCollection.Add(new IndexedFileInfo() { Name = line.Name, Path = line.Path, Type = line.Type });
                        }, System.Windows.Threading.DispatcherPriority.Render);
                        count++;
                    }
                }

                for (int i = count; i < 200; i++)
                {
                    if (NeedsStop)
                    {
                        ViewModel.FileCollection.Clear();
                        NeedsStop = false;
                        return;
                    }
                    try
                    {
                        if (list[i].Key == text)
                        {
                            continue;
                        }
                        foreach (var line in list[i].Value)
                        {
                            this.Dispatcher.InvokeAsync(() =>
                            {
                                ViewModel.FileCollection.Add(new IndexedFileInfo() { Name = line.Name, Path = line.Path, Type = line.Type });
                            }, System.Windows.Threading.DispatcherPriority.Render);
                        }
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }

                Thread.Sleep(500);

                return;

                foreach (var line in ViewModel.FileCollection)
                {
                    if (NeedsStop)
                    {
                        ViewModel.FileCollection.Clear();
                        NeedsStop = false;
                        return;
                    }
                    try
                    {
                        StorageItemThumbnail icon = null;
                        if (line.Type == "File")
                        {
                            StorageFile file = await StorageFile.GetFileFromPathAsync(line.Path);
                            icon = await file.GetThumbnailAsync(ThumbnailMode.ListView);
                        }
                        else
                        {
                            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(line.Path);
                            icon = await folder.GetThumbnailAsync(ThumbnailMode.ListView);
                        }
                        this.Dispatcher.InvokeAsync(() =>
                        {
                            try
                            {
                                var image = new BitmapImage();
                                image.StreamSource = icon.AsStream();
                                line.Image = image;
                            }
                            catch (Exception ex)
                            {

                            }

                        }, System.Windows.Threading.DispatcherPriority.Background);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TimeTillSearch = 2;
                if (TimerThread == null)
                {
                    NeedsStop = true;
                    if (TimerThread != null)
                    {
                        int count = 0;
                        while (NeedsStop)
                        {
                            count++;
                            if (count == 10)
                            {
                                ViewModel.FileCollection.Clear();
                                NeedsStop = false;
                            }
                            Thread.Sleep(50);
                        }
                    }
                    TimerThread = new Thread(() =>
                    {
                        try
                        {
                            while (TimeTillSearch > 0)
                            {
                                TimeTillSearch--;
                                Thread.Sleep(100);
                            }
                            NeedsStop = false;
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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SearchListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchListView.SelectedIndex = -1;
        }

        private void SearchListView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
