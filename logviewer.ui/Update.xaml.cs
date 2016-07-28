using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace logviewer.ui
{
    /// <summary>
    /// Interaction logic for Update.xaml
    /// </summary>
    public partial class Update : Window
    {
        public Update()
        {
            this.InitializeComponent();
        }

        private void OnNo(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnYes(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Yes
        }

        private void OnUpdate(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Update
        }
    }
}
