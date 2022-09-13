using Momentum.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Momentum
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            OnLoad();
        }

        public void OnLoad()
        {
            ContentFrame.Navigate(new SearchPage());
            Loaded += (sender, args) =>
            {
                WPFUI.Appearance.Watcher.Watch(this, WPFUI.Appearance.BackgroundType.Mica, true);
            };
        }

        private void RootTitleBar_MinimizeClicked(object sender, RoutedEventArgs e)
        {

        }

        private void RootTitleBar_NotifyIconClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
