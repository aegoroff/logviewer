// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 04.08.2015
// © 2012-2016 Alexander Egorov

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        private readonly LogWorkflow workflow;

        public MainWindow()
        {
            this.InitializeComponent();
            MainViewModel.Current.Window = new WindowWrapper(this);
            this.model = new MainModel(MainViewModel.Current);
            this.workflow = new LogWorkflow(this.model, MainViewModel.Current);
        }

        private void OnExitApp(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnTemplateSelect(object sender, RoutedEventArgs e)
        {
            var src = (MenuItem)e.OriginalSource;
            var cmd = (TemplateCommandViewModel)src.DataContext;

            ReloadTemplates(cmd.Index);
        }

        private static void ReloadTemplates(int selected)
        {
            MainViewModel.Current.Templates.Clear();

            MainViewModel.Current.SelectedParsingTemplate = selected;
            var commands = MainViewModel.Current.ReadParsingTemplateCommands();
            foreach (var command in commands)
            {
                MainViewModel.Current.Templates.Add(command);
            }
        }

        private void OnOpenFile(object sender, ExecutedRoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = Properties.Resources.LogFilesFilterTemplate
            };
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }
            this.workflow.Open(openFileDialog.FileName);
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            this.workflow.Close();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.workflow.Start();
        }

        private void OnUpdate(object sender, ExecutedRoutedEventArgs e)
        {
            this.workflow.Reload();
        }

        private void OnStatistic(object sender, ExecutedRoutedEventArgs e)
        {
            new Statistic(this.model.Store, this.model.GetLogSize(true), this.model.CurrentEncoding).Show();
        }

        private void OnSettings(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new SettingsDlg(MainViewModel.Current.SettingsProvider);
            using (dlg)
            {
                dlg.SetApplyAction(refresh => this.model.UpdateMatcherAndRefreshLog(refresh));
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