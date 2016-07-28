using System.Windows;
using logviewer.logic.ui.update;
using logviewer.ui.Annotations;

namespace logviewer.ui
{
    /// <summary>
    /// Interaction logic for Update.xaml
    /// </summary>
    [PublicAPI]
    public partial class Update : Window
    {
        private UpdateViewModel viewModel;

        public Update(string text, string targetAddress)
        {
            this.InitializeComponent();
            this.viewModel = new UpdateViewModel(targetAddress);
            this.DataContext = this.viewModel;
            this.viewModel.UpdateAvailableText = text;
            this.viewModel.YesEnabled = true;
        }

        private void OnNo(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnYes(object sender, RoutedEventArgs e)
        {
            this.viewModel.StartDownload();
        }

        private void OnUpdate(object sender, RoutedEventArgs e)
        {
            this.viewModel.StartUpdate();
        }
    }
}
