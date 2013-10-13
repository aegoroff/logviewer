// Created by: egr
// Created at: 25.09.2013
// © 2012-2013 Alexander Egorov


using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace logviewer.core
{
    public class SettingsController
    {
        private readonly FormData formData = new FormData();
        private readonly ISettingsProvider settings;
        private readonly ISettingsView view;
        private ParsingTemplate template;

        public SettingsController(ISettingsView view, ISettingsProvider settings)
        {
            this.view = view;
            this.settings = settings;
        }

        public void Load()
        {
            var uiContext = TaskScheduler.FromCurrentSynchronizationContext();

            Action loader = delegate
            {
                this.formData.OpenLastFile = this.settings.OpenLastFile;
                this.formData.PageSize = this.settings.PageSize.ToString(CultureInfo.CurrentUICulture);
                this.formData.KeepLastNFiles = this.settings.KeepLastNFiles.ToString(CultureInfo.CurrentUICulture);

                this.template = this.settings.ReadParsingTemplate();
            };
            Action<Task> onComplete = delegate
            {
                this.view.LoadFormData(this.formData);
                this.view.LoadParsingTemplate(this.template);
                this.view.EnableSave(false);
            };

            Task.Factory.StartNew(loader).ContinueWith(onComplete, CancellationToken.None, TaskContinuationOptions.None, uiContext);
        }

        public void Save()
        {
            int pageSize;
            if (int.TryParse(this.formData.PageSize, out pageSize))
            {
                this.settings.PageSize = pageSize;
            }
            int value;
            if (int.TryParse(this.formData.KeepLastNFiles, out value))
            {
                this.settings.KeepLastNFiles = value;
            }
            this.settings.OpenLastFile = this.formData.OpenLastFile;
            this.settings.UpdateParsingProfile(this.template);
            this.view.EnableSave(false);
        }

        public void UpdateOpenLastFile(bool value)
        {
            this.formData.OpenLastFile = value;
            this.view.EnableSave(true);
        }

        public void UpdatePageSize(string value)
        {
            this.formData.PageSize = value;
            this.view.EnableSave(true);
        }

        public void UpdateMessageStartPattern(string value)
        {
            this.template.StartMessage = value;
            this.view.EnableSave(true);
        }

        public void UpdateKeepLastNFiles(string value)
        {
            this.formData.KeepLastNFiles = value;
            this.view.EnableSave(true);
        }

        public void UpdateTraceLevel(string value)
        {
            this.template.Trace = value;
            this.view.EnableSave(true);
        }

        public void UpdateDebugLevel(string value)
        {
            this.template.Debug = value;
            this.view.EnableSave(true);
        }

        public void UpdateInfoLevel(string value)
        {
            this.template.Info = value;
            this.view.EnableSave(true);
        }

        public void UpdateWarnLevel(string value)
        {
            this.template.Warn = value;
            this.view.EnableSave(true);
        }

        public void UpdateErrorLevel(string value)
        {
            this.template.Error = value;
            this.view.EnableSave(true);
        }

        public void UpdateFatalLevel(string value)
        {
            this.template.Fatal = value;
            this.view.EnableSave(true);
        }
    }
}