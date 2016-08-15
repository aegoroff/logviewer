using System.Windows;
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
    public partial class Statistic : Window
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
