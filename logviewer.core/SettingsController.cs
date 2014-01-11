// Created by: egr
// Created at: 25.09.2013
// © 2012-2013 Alexander Egorov


using System;
using System.Globalization;
using System.Linq;
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

        public void UpdateLevel(string value, LogLevel level)
        {
            foreach (var info in from info in (from info in typeof(ParsingTemplate).GetProperties()
                where info.IsDefined(typeof(LogLevelAttribute), false)
                select info) let attr = (LogLevelAttribute)info.GetCustomAttributes(typeof(LogLevelAttribute), false)[0] where attr.Level == level select info)
            {
                info.SetValue(this.template, value, null);
            }
            
            this.view.EnableSave(true);
        }
    }
}