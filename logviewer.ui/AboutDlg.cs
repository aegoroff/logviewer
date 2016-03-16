// Created by: egr
// Created at: 18.09.2012
// © 2012-2016 Alexander Egorov

using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using logviewer.ui.Properties;

namespace logviewer.ui
{
    partial class AboutDlg : Form
    {
        public AboutDlg()
        {
            this.InitializeComponent();
            this.Text = string.Format(Resources.AboutTemplate, this.AssemblyTitle);
            this.labelProductName.Text = this.AssemblyProduct;
            this.labelVersion.Text = string.Format(Resources.VersionTemplate, this.AssemblyVersion);
            this.labelCopyright.Text = this.AssemblyCopyright + " " + this.AssemblyCompany;
            this.textBoxDescription.Text = this.AssemblyDescription;
            this.linkLabel1.Links.Add(new LinkLabel.Link(0, this.linkLabel1.Text.Length, "https://github.com/aegoroff/logviewer"));
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                var attributes =
                    Assembly.GetExecutingAssembly().GetCustomAttributes(typeof (AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    var titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (!string.IsNullOrWhiteSpace(titleAttribute.Title))
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public string AssemblyDescription
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof (AssemblyDescriptionAttribute), false);
                return attributes.Length == 0 ? string.Empty : ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof (AssemblyProductAttribute), false);
                return attributes.Length == 0 ? string.Empty : ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof (AssemblyCopyrightAttribute), false);
                return attributes.Length == 0 ? string.Empty : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof (AssemblyCompanyAttribute), false);
                return attributes.Length == 0 ? string.Empty : ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        #endregion

        private void OnLinkClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Task.Factory.StartNew(() => Process.Start((string)e.Link.LinkData));
        }
    }
}