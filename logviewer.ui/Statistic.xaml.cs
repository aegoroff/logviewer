using System.Windows;
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
        public Statistic(LogStore store, string size, string encoding)
        {
            this.InitializeComponent();
            var model = new StatisticModel(store, size, encoding);
            this.ListView.ItemsSource = model.Items;
            this.DataContext = model.FilterViewModel;
            model.LoadStatistic();
        }
    }
}
