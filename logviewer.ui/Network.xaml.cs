// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Windows;
using System.Windows.Input;
using logviewer.logic.ui.network;

namespace logviewer.ui
{
    /// <summary>
    ///     Interaction logic for Network.xaml
    /// </summary>
    public partial class Network
    {
        private readonly NetworkSettingsModel model;

        public Network()
        {
            this.InitializeComponent();
            INetworkSettingsViewModel viewModel = new NetworkSettingsViewModel();
            viewModel.PasswordUpdated += this.ModelOnPasswordUpdated;
            this.DataContext = viewModel;
            this.model = new NetworkSettingsModel(viewModel, MainViewModel.Current.SettingsProvider.SimpleOptionsStore);
            this.model.Initialize();
        }

        private void ModelOnPasswordUpdated(object sender, string s)
        {
            this.Password.Password = s;
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.model.Write(this.Password.Password);
            this.Close();
        }

        private void OnChange(object sender, KeyEventArgs e)
        {
            this.model.InvokeSettingsChange();
        }
    }
}
