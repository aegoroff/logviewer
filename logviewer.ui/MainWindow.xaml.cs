// Created by: egr
// Created at: 04.08.2015
// © 2012-2015 Alexander Egorov

using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using logviewer.core;
using MenuItem = Fluent.MenuItem;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace logviewer.ui
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly UiController controller;

        public MainWindow()
        {
            this.InitializeComponent();
            this.controller = new UiController(MainViewModel.Current);
            this.controller.ReadCompleted += this.OnReadCompleted;
        }

        private void OnReadCompleted(object sender, LogReadCompletedEventArgs e)
        {
            this.controller.CreateCollection(o => this.DataContext = o);
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

        private void OnOpenFile(object sender, ExecutedRoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Log files|*.log|All files|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                MainViewModel.Current.LogPath = openFileDialog.FileName;
                this.controller.StartReading();
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            this.controller.Dispose();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.controller.LoadLastOpenedFile();
        }

        private void OnUpdate(object sender, ExecutedRoutedEventArgs e)
        {
            this.controller.StartReading();
        }
    }
}