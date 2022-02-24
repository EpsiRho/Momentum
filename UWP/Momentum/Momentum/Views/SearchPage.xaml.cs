using System;

using Momentum.ViewModels;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Momentum.Views
{
    public sealed partial class SearchPage : Page
    {
        public SearchViewModel ViewModel { get; } = new SearchViewModel();

        public SearchPage()
        {
            InitializeComponent();
            Loaded += SearchPage_Loaded;
        }

        private async void SearchPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadDataAsync(ListDetailsViewControl.ViewState);
        }
    }
}
