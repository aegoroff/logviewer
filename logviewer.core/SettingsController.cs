// Created by: egr
// Created at: 25.09.2013
// © 2012-2014 Alexander Egorov


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace logviewer.core
{
    public class SettingsController : BaseGuiController
    {
        private readonly FormData formData = new FormData();
        private readonly ISettingsProvider settings;
        private readonly ISettingsView view;
        private ParsingTemplate template;
        private IList<string> templateList;
        private readonly int parsingTemplateIndex;
        private readonly Dictionary<LogLevel, Action<Color>> updateColorActions;

        public SettingsController(ISettingsView view, ISettingsProvider settings)
        {
            this.view = view;
            this.settings = settings;
            this.parsingTemplateIndex = 0;
            this.updateColorActions = new Dictionary<LogLevel, Action<Color>>(this.InitializeUpdateActions());
        }

        private Dictionary<LogLevel, Action<Color>> InitializeUpdateActions()
        {
            return new Dictionary<LogLevel, Action<Color>>
            {
                {LogLevel.Trace, this.view.UpdateTraceColor},
                {LogLevel.Debug, this.view.UpdateDebugColor},
                {LogLevel.Info, this.view.UpdateInfoColor},
                {LogLevel.Warn, this.view.UpdateWarnColor},
                {LogLevel.Error, this.view.UpdateErrorColor},
                {LogLevel.Fatal, this.view.UpdateFatalColor},
            };
        }

        public void Load()
        {
            Task.Factory.StartNew(delegate
            {
                this.formData.OpenLastFile = this.settings.OpenLastFile;
                this.formData.AutoRefreshOnFileChange = this.settings.AutoRefreshOnFileChange;
                this.formData.PageSize = this.settings.PageSize.ToString(CultureInfo.CurrentUICulture);
                this.formData.KeepLastNFiles = this.settings.KeepLastNFiles.ToString(CultureInfo.CurrentUICulture);

                this.templateList = this.settings.ReadParsingTemplateList();
                this.template = this.settings.ReadParsingTemplate(this.parsingTemplateIndex);
                this.formData.Colors = new Dictionary<LogLevel, Color>();
                foreach (var logLevel in SelectLevels(l => l != LogLevel.None))
                {
                    this.formData.Colors.Add(logLevel, this.settings.Colorize(logLevel));
                }
                this.RunOnGuiThread(delegate
                {
                    this.view.LoadFormData(this.formData);
                    this.view.LoadParsingTemplate(this.template);
                    this.view.EnableSave(false);
                    foreach (var name in this.templateList)
                    {
                        this.view.AddTemplateName(name);
                    }
                    foreach (var color in this.formData.Colors)
                    {
                        this.updateColorActions[color.Key](color.Value);
                    }
                    this.view.SelectParsingTemplateByName(this.templateList[this.parsingTemplateIndex]);
                });
            });
        }

        public void Save()
        {
            this.view.EnableSave(false);
            this.view.EnableChangeOrClose(false);
            Task.Factory.StartNew(delegate
            {
                try
                {
                    int pageSize;
                    if (int.TryParse(this.formData.PageSize, out pageSize))
                    {
                        if (this.settings.PageSize != pageSize)
                        {
                            this.RefreshOnClose = true;
                        }
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

                    foreach (var pair in this.formData.Colors)
                    {
                        this.settings.UpdateColor(pair.Key, pair.Value);
                    }
                }
                finally
                {
                    this.RunOnGuiThread(() => this.view.EnableChangeOrClose(true));
                }
            });
        }

        public bool RefreshOnClose { get; private set; }

        public static IEnumerable<LogLevel> SelectLevels(Func<LogLevel, bool> filter = null)
        {
            var levels = (LogLevel[])Enum.GetValues(typeof (LogLevel));
            // All levels by default
            return levels.Where(filter ?? (l => true));
        }

        public void OnChangeLevelColor(LogLevel level)
        {
            var result = this.view.PickColor(this.settings.Colorize(level));
            if (!result.Result)
            {
                return;
            }
            this.UpdateColor(level, result.SelectedColor);
            this.view.EnableSave(true);
        }

        private void UpdateColor(LogLevel level, Color color)
        {
            if (color != this.formData.Colors[level])
            {
                this.RefreshOnClose = true;
            }
            this.formData.Colors[level] = color;
            this.updateColorActions[level](color);
        }

        public void ResetColorsToDefault()
        {
            foreach (var color in this.settings.DefaultColors.Where(c => c.Key != LogLevel.None))
            {
                this.UpdateColor(color.Key, color.Value);
            }
            this.view.EnableSave(true);
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