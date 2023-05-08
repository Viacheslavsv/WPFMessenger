using ChatClient.MVVM.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel viewModel;
        public MainWindow()
        {
            viewModel = new MainViewModel();
            InitializeComponent();
        }

        private void ListBoxItem_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(sender as ListBoxItem).IsSelected)
            {
                (sender as ListBoxItem).IsSelected = true; ;
            }
        }
    }
}
