// Created by: egr
// Created at: 04.08.2015
// © 2012-2015 Alexander Egorov

using System.Windows;

namespace logviewer.ui
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void OnExitApp(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}