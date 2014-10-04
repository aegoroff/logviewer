// Created by: egr
// Created at: 04.10.2014
// © 2012-2014 Alexander Egorov

using System.Windows.Forms;
using logviewer.Properties;

namespace logviewer
{
    public partial class UpdateDlg : Form
    {
        public UpdateDlg(string message)
        {
            this.InitializeComponent();
            this.Localize(message);
        }

        private void Localize(string message)
        {
            this.groupBox1.Text = Resources.NewVersion;
            this.label1.Text = message;
        }

        private void OnNo(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}