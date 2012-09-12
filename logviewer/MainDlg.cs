using System.Windows.Forms;

namespace logviewer
{
    public partial class MainDlg : Form, ILogView
    {
        private readonly LogController controller;
        private LongRunningOperationDisplay longOperationDisplay;

        public MainDlg()
        {
            this.InitializeComponent();
            this.controller = new LogController(this);
        }

        #region ILogView Members

        public string LogPath { get; private set; }

        public void StartLongRunningDisplay()
        {
            this.longOperationDisplay = new LongRunningOperationDisplay(this, "");
        }

        public void StopLongRunningDisplay()
        {
            LongRunningOperationDisplay.Complete(this.longOperationDisplay);
        }

        #endregion
    }
}