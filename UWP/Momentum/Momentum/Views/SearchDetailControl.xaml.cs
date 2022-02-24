using System;

using Momentum.Core.Models;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Momentum.Views
{
    public sealed partial class SearchDetailControl : UserControl
    {
        public SampleOrder ListMenuItem
        {
            get { return GetValue(ListMenuItemProperty) as SampleOrder; }
            set { SetValue(ListMenuItemProperty, value); }
        }

        public static readonly DependencyProperty ListMenuItemProperty = DependencyProperty.Register("ListMenuItem", typeof(SampleOrder), typeof(SearchDetailControl), new PropertyMetadata(null, OnListMenuItemPropertyChanged));

        public SearchDetailControl()
        {
            InitializeComponent();
        }

        private static void OnListMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SearchDetailControl;
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
