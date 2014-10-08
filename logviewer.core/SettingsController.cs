// Created by: egr
// Created at: 25.09.2013
// © 2012-2014 Alexander Egorov


using System;
using System.Collections.Generic;
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
        private IList<string> templateList;
        private readonly int parsingTemplateIndex;

        public SettingsController(ISettingsView view, ISettingsProvider settings)
        {
            this.view = view;
            this.settings = settings;
            parsingTemplateIndex = 0;
        }

        public void Load()
        {
            var uiContext = TaskScheduler.FromCurrentSynchronizationContext();

            Action loader = delegate
            {
                this.formData.OpenLastFile = this.settings.OpenLastFile;
                this.formData.AutoRefreshOnFileChange = this.settings.AutoRefreshOnFileChange;
                this.formData.PageSize = this.settings.PageSize.ToString(CultureInfo.CurrentUICulture);
                this.formData.KeepLastNFiles = this.settings.KeepLastNFiles.ToString(CultureInfo.CurrentUICulture);

                this.templateList = this.settings.ReadParsingTemplateList();
                this.template = this.settings.ReadParsingTemplate(parsingTemplateIndex);
            };
            Action<Task> onComplete = delegate
            {
                this.view.LoadFormData(this.formData);
                this.view.LoadParsingTemplate(this.template);
                this.view.EnableSave(false);
                foreach (var name in this.templateList)
                {
                    this.view.AddTemplateName(name);
                }
                this.view.UpdateTraceColor(LogMessage.Colorize(LogLevel.Trace));
                this.view.UpdateDebugColor(LogMessage.Colorize(LogLevel.Debug));
                this.view.UpdateInfoColor(LogMessage.Colorize(LogLevel.Info));
                this.view.UpdateWarnColor(LogMessage.Colorize(LogLevel.Warn));
                this.view.UpdateErrorColor(LogMessage.Colorize(LogLevel.Error));
                this.view.UpdateFatalColor(LogMessage.Colorize(LogLevel.Fatal));
                
                this.view.SelectParsingTemplateByName(this.templateList[this.parsingTemplateIndex]);
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
            this.settings.AutoRefreshOnFileChange = this.formData.AutoRefreshOnFileChange;
            this.settings.UpdateParsingProfile(this.template);
            this.view.EnableSave(false);
        }

        public static IEnumerable<LogLevel> AllLevels
        {
            get
            {
                var levels = (LogLevel[])Enum.GetValues(typeof (LogLevel));
                return levels.Where(l => l != LogLevel.None);
            }
        }

        public void OnChangeLevelColor(LogLevel level)
        {
            var result = this.view.PickColor(LogMessage.Colorize(level));
            if (!result.Result)
            {
                return;
            }
            LogMessage.UpdateColor(level, result.SelectedColor);
            switch (level)
            {
                case LogLevel.Trace:
                    this.view.UpdateTraceColor(result.SelectedColor);
                    break;
                case LogLevel.Debug:
                    this.view.UpdateDebugColor(result.SelectedColor);
                    break;
                case LogLevel.Info:
                    this.view.UpdateInfoColor(result.SelectedColor);
                    break;
                case LogLevel.Warn:
                    this.view.UpdateWarnColor(result.SelectedColor);
                    break;
                case LogLevel.Error:
                    this.view.UpdateErrorColor(result.SelectedColor);
                    break;
                case LogLevel.Fatal:
                    this.view.UpdateFatalColor(result.SelectedColor);
                    break;
            }
        }

        public void UpdateOpenLastFile(bool value)
        {
            this.formData.OpenLastFile = value;
            this.view.EnableSave(true);
        }

        public void UpdateAutoRefreshOnFileChange(bool value)
        {
            this.formData.AutoRefreshOnFileChange = value;
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
        
        public void UpdateParsingTemplateName(string value)
        {
            this.template.Name = value;
            this.view.EnableSave(true);
        }

        public void UpdateKeepLastNFiles(string value)
        {
            this.formData.KeepLastNFiles = value;
            this.view.EnableSave(true);
        }
    }
}