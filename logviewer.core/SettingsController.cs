﻿// Created by: egr
// Created at: 25.09.2013
// © 2012-2015 Alexander Egorov


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using logviewer.core.Properties;
using logviewer.engine;

namespace logviewer.core
{
    public class SettingsController : BaseGuiController
    {
        private readonly FormData formData = new FormData();
        private readonly ISettingsProvider settings;
        private readonly ISettingsView view;
        private ParsingTemplate template;
        private IList<string> templateList;
        private int parsingTemplateIndex;
        private readonly Dictionary<LogLevel, Action<Color>> updateColorActions;


        public SettingsController(ISettingsView view, ISettingsProvider settings)
        {
            this.view = view;
            this.settings = settings;
            this.parsingTemplateIndex = settings.SelectedParsingTemplate;
            this.updateColorActions = new Dictionary<LogLevel, Action<Color>>(this.InitializeUpdateActions());
            this.view.SelectedTemplateController.TemplateChangeSuccess += SelectedTemplateControllerOnTemplateChangeSuccess;
            this.view.SelectedTemplateController.TemplateChangeFailure += SelectedTemplateControllerOnTemplateChangeFailure;
            this.view.NewTemplateController.TemplateChangeSuccess += NewTemplateControllerOnTemplateChangeSuccess;
            this.view.NewTemplateController.TemplateChangeFailure += NewTemplateControllerOnTemplateChangeFailure;
        }

        private void NewTemplateControllerOnTemplateChangeSuccess(object sender, EventArgs eventArgs)
        {
            this.view.EnableAddNewTemplate(true);
        }
        
        private void NewTemplateControllerOnTemplateChangeFailure(object sender, EventArgs eventArgs)
        {
            this.view.EnableAddNewTemplate(false);
        }

        private void SelectedTemplateControllerOnTemplateChangeSuccess(object sender, EventArgs eventArgs)
        {
            this.view.EnableSave(true);
        }

        private void SelectedTemplateControllerOnTemplateChangeFailure(object sender, EventArgs eventArgs)
        {
            this.view.EnableSave(false);
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
            this.view.ShowNewParsingTemplateForm(false);
            
            Task.Factory.StartNew(delegate
            {
                this.formData.OpenLastFile = this.settings.OpenLastFile;
                this.formData.AutoRefreshOnFileChange = this.settings.AutoRefreshOnFileChange;
                this.formData.PageSize = this.settings.PageSize.ToString(CultureInfo.CurrentUICulture);
                this.formData.KeepLastNFiles = this.settings.KeepLastNFiles.ToString(CultureInfo.CurrentUICulture);

                this.templateList = this.settings.ReadParsingTemplateList();
                if (this.parsingTemplateIndex > templateList.Count - 1)
                {
                    this.parsingTemplateIndex = 0;
                }
                this.template = this.settings.ReadParsingTemplate(this.parsingTemplateIndex);
                foreach (var logLevel in SelectLevels(l => l != LogLevel.None))
                {
                    this.formData.Colors.Add(logLevel, this.settings.ReadColor(logLevel));
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
                    this.view.EnableResetColors(this.IsColorsChanged);
                    this.view.EnableRemoveTemplateControl(this.parsingTemplateIndex > 0);
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
                    this.settings.UpdateParsingTemplate(this.template);

                    foreach (var pair in this.formData.Colors)
                    {
                        this.settings.UpdateColor(pair.Key, pair.Value);
                    }
                }
                finally
                {
                    this.RunOnGuiThread(() =>
                    {
                        this.view.EnableChangeOrClose(true);
                        this.view.EnableResetColors(this.IsColorsChanged);
                    });
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
            var result = this.view.PickColor(this.settings.ReadColor(level));
            if (!result.Result)
            {
                return;
            }
            this.UpdateColor(level, result.SelectedColor);
            this.view.EnableSave(true);
        }

        private void UpdateColor(LogLevel level, Color color)
        {
            if (color.ToArgb() != this.formData.Colors[level].ToArgb())
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

        private bool IsColorsChanged
        {
            get
            {
                return this.settings.DefaultColors.Where(c => c.Key != LogLevel.None)
                    .Aggregate(false, (current, c) => current | this.formData.Colors[c.Key].ToArgb() != c.Value.ToArgb());
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

        public void AddNewParsingTemplate()
        {
            this.view.ShowNewParsingTemplateForm(false);
            var t = this.view.NewParsingTemplateData;
            t.Index = this.templateList.Count;
            Task.Factory.StartNew(delegate
            {
                this.settings.InsertParsingTemplate(t);
                var newList = this.settings.ReadParsingTemplateList();
                this.RunOnGuiThread(delegate
                {
                    for (var i = t.Index; i < newList.Count; i++)
                    {
                        this.view.AddTemplateName(newList[i]);
                        this.templateList.Add(newList[i]);
                    }
                });
            });
        }

        public void RemoveSelectedParsingTemplate()
        {
            if (!view.ShowWarning(Resources.DeleteCurrentTemplateCaption, Resources.DeleteCurrentTemplateMessage))
            {
                return;
            }
            Task.Factory.StartNew(delegate
            {
                this.settings.DeleteParsingTemplate(this.parsingTemplateIndex);
                this.templateList.RemoveAt(this.parsingTemplateIndex);
                this.RunOnGuiThread(() =>
                {
                    this.view.RemoveParsingTemplateName(this.parsingTemplateIndex);
                    this.view.SelectParsingTemplate(this.parsingTemplateIndex - 1);
                });
            });
        }

        public void RestoreDefaultTemplates()
        {
            if (!view.ShowWarning(Resources.RestoreDefaultTemplateCaption, Resources.RestoreDefaultTemplateText))
            {
                return;
            }
            this.view.EnableChangeOrClose(false);
            Task.Factory.StartNew(delegate
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
                var lastIx = settings.ReadAllParsingTemplates().Count - 1;

                for (int i = lastIx; i >= 0; i--)
                {
                    this.settings.DeleteParsingTemplate(i);
                }
                
                foreach (var t in ParsingTemplate.Defaults)
                {
                    this.settings.InsertParsingTemplate(t);
                }
                this.parsingTemplateIndex = 0;
                this.templateList = this.settings.ReadParsingTemplateList();
                this.settings.SelectedParsingTemplate = this.parsingTemplateIndex;
                this.template = this.settings.ReadParsingTemplate(this.parsingTemplateIndex);

                this.RunOnGuiThread(delegate
                {
                    this.view.RemoveAllParsingTemplateNames();
                    foreach (var name in this.templateList)
                    {
                        this.view.AddTemplateName(name);
                    }
                    this.view.SelectParsingTemplate(0);
                    this.view.LoadParsingTemplate(this.template);
                    this.view.EnableChangeOrClose(true);
                    this.view.EnableRemoveTemplateControl(false);
                    this.view.EnableSave(false);
                });
            });

        }

        public void StartAddNewParsingTemplate()
        {
            view.ShowNewParsingTemplateForm(true);
        }
        
        public void CancelNewParsingTemplate()
        {
            view.ShowNewParsingTemplateForm(false);
        }

        public void LoadParsingTemplate(int index)
        {
            if (this.parsingTemplateIndex == index || index < 0)
            {
                return;
            }
            this.parsingTemplateIndex = index;

            Task.Factory.StartNew(delegate
            {
                this.RunOnGuiThread(() => this.view.EnableChangeOrClose(false));
                this.template = this.settings.ReadParsingTemplate(this.parsingTemplateIndex);
                this.RunOnGuiThread(delegate
                {
                    this.view.LoadParsingTemplate(this.template);
                    this.view.EnableChangeOrClose(true);
                    this.view.EnableRemoveTemplateControl(this.parsingTemplateIndex > 0);
                    this.view.EnableSave(false);
                });
            });
        }
    }
}