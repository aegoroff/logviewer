// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 12.08.2016
// © 2012-2017 Alexander Egorov

using System.Windows.Input;
using logviewer.logic.storage;
using logviewer.logic.ui.statistic;
using logviewer.ui.Annotations;

namespace logviewer.ui
{
    /// <summary>
    /// Interaction logic for Statistic.xaml
    /// </summary>
    [PublicAPI]
    public partial class Statistic
    {
        private StatisticModel model;

        public Statistic(ILogStore store, string size, string encoding)
        {
            this.InitializeComponent();
            this.model = new StatisticModel(store, size, encoding);
            this.ListView.ItemsSource = this.model.Items;
            this.DataContext = this.model.FilterViewModel;
            this.model.LoadStatistic();
        }

        private void OnClose(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
    }
}
