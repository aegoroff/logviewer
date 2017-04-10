// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 28.07.2016
// © 2012-2017 Alexander Egorov

using System;
using System.Windows;
using logviewer.logic.ui.update;
using logviewer.ui.Annotations;

namespace logviewer.ui
{
    /// <summary>
    /// Interaction logic for Update.xaml
    /// </summary>
    [PublicAPI]
    public partial class Update
    {
        private UpdateViewModel viewModel;

        public Update(UpdateViewModel viewModel)
        {
            this.InitializeComponent();
            this.viewModel = viewModel;
            this.viewModel.Close += this.ViewModelOnClose;
            this.DataContext = this.viewModel;
            this.viewModel.YesEnabled = true;
        }

        private void ViewModelOnClose(object sender, EventArgs eventArgs)
        {
            this.Close();
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
