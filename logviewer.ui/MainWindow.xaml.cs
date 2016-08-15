// Created by: egr
// Created at: 04.08.2015
// © 2012-2016 Alexander Egorov

using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using logviewer.logic.support;
using logviewer.logic.ui;
using logviewer.logic.ui.main;
using MenuItem = Fluent.MenuItem;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace logviewer.ui
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainModel model;
        private readonly FileSystemWatcher logWatch;

        public MainWindow()
        {
            this.InitializeComponent();
            MainViewModel.Current.Window = new WindowWrapper(this);
            this.model = new MainModel(MainViewModel.Current);
            this.logWatch = new FileSystemWatcher();
            this.logWatch.Changed += this.OnChangeLog;
            this.logWatch.NotifyFilter = NotifyFilters.Size;
        }

        private void WatchLogFile(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }
            this.logWatch.Path = Path.GetDirectoryName(path);
            this.logWatch.Filter = Path.GetFileName(path);
            this.logWatch.EnableRaisingEvents = true;
        }

        private void OnExitApp(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnTemplateSelect(object sender, RoutedEventArgs e)
        {
            var src = (MenuItem)e.OriginalSource;
            var cmd = (TemplateCommand)src.DataContext;

            ReloadTemplates(cmd.Index);
        }

        private static void ReloadTemplates(int selected)
        {
            MainViewModel.Current.Templates.Clear();

            MainViewModel.Current.SelectedParsingTemplate = selected;
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
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }
            MainViewModel.Current.LogPath = openFileDialog.FileName;
            this.model.ReadNewLog();
            this.WatchLogFile(MainViewModel.Current.LogPath);
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            this.logWatch.Dispose();
            this.model.Dispose();
            MainViewModel.Current.Dispose();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.model.LoadLastOpenedFile();
            this.WatchLogFile(MainViewModel.Current.LogPath);
        }

        private void OnUpdate(object sender, ExecutedRoutedEventArgs e)
        {
            this.model.ReadNewLog();
        }

        private void OnChangeLog(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                Log.Instance.TraceFormatted("Log {0} change event: {1}", e.FullPath, e.ChangeType);
                this.model.UpdateLog(e.FullPath);
            }
        }

        private void OnStatistic(object sender, ExecutedRoutedEventArgs e)
        {
            //var dlg = new StatisticDlg(this.model.Store, this.model.GetLogSize(true), this.model.CurrentEncoding);
            //dlg.Show(MainViewModel.Current.Window);
            new Statistic(this.model.Store, this.model.GetLogSize(true), this.model.CurrentEncoding).Show();
        }

        private void OnSettings(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new SettingsDlg(MainViewModel.Current.SettingsProvider);
            using (dlg)
            {
                dlg.SetApplyAction(refresh => this.model.UpdateSettings(refresh));
                dlg.ShowDialog();
            }
            ReloadTemplates(MainViewModel.Current.SelectedParsingTemplate);
        }

        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var panel = FindVisualChild<VirtualizingStackPanel>(this.LogControlStyle);
            if (this.LogControlStyle.Items.Count <= 0 || panel == null)
            {
                return;
            }
            var offset = panel.Orientation == Orientation.Horizontal ? (int)panel.HorizontalOffset : (int)panel.VerticalOffset;
            var limit = panel.Orientation == Orientation.Horizontal ? (int)panel.ViewportWidth : (int)panel.ViewportHeight;
            MainViewModel.Current.Visible = new Range
            {
                First = offset,
                Last = offset + limit
            };
        }

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);

                var visualChild = child as T;
                if (visualChild != null)
                {
                    return visualChild;
                }
                child = FindVisualChild<T>(child);
                if (child != null)
                {
                    return (T)child;
                }
            }
            return null;
        }

        private void OnCheckUpdates(object sender, ExecutedRoutedEventArgs e)
        {
            this.model.CheckUpdates(true);
        }

        private void OnHelp(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new AboutDlg();
            using (dlg)
            {
                dlg.ShowDialog();
            }
        }

        private void OnNetworkSettings(object sender, ExecutedRoutedEventArgs e)
        {
            new Network().ShowDialog();
        }
    }
}