using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IndexerTestWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadProgress.Visibility = Visibility.Visible;
            Thread t = new Thread(FileIndexer.LoadIndexesFromFile);
            t.Start();
            //Thread.Sleep(1000);
            //Thread d = new Thread(WatchDisplay);
            //d.Start();
        }

        private void WatchDisplay()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (FileIndexer.NeedsDisplay)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    WatchText.Text = $"{watch.Elapsed.Hours.ToString("D2")}:{watch.Elapsed.Minutes.ToString("D2")}:{watch.Elapsed.Seconds.ToString("D2")}";
                    IndexedCount.Text = $"Indexed {FileIndexer.IndexedFiles} files";
                }));
                Thread.Sleep(100);
            }
            watch.Stop();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadProgress.Visibility = Visibility.Collapsed;
            }));
        }
    }
}
