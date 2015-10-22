// Created by: egr
// Created at: 04.08.2015
// © 2012-2015 Alexander Egorov

using System.Windows;
using Fluent;
using Microsoft.Win32;

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

        private void OnTemplateSelect(object sender, RoutedEventArgs e)
        {
            var src = (MenuItem)e.OriginalSource;
            var cmd = (TemplateCommand)src.DataContext;

            MainViewModel.Current.Templates.Clear();

            MainViewModel.Current.SelectedParsingTemplate = cmd.Index;
            var commands = MainViewModel.Current.CreateTemplateCommands();
            foreach (var command in commands)
            {
                MainViewModel.Current.Templates.Add(command);
            }
        }

        private void OnOpenFile(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Log files|*.log|All files|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                MainViewModel.Current.LogPath = openFileDialog.FileName;
            }
        }
    }
}