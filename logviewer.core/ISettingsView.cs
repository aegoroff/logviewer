﻿// Created by: egr
// Created at: 25.09.2013
// © 2012-2014 Alexander Egorov

using System.Drawing;

namespace logviewer.core
{
    public interface ISettingsView
    {
        void EnableSave(bool enabled);
        
        void LoadFormData(FormData formData);
        
        void LoadParsingTemplate(ParsingTemplate template);
        
        void AddTemplateName(string name);
        
        void SelectParsingTemplateByName(string name);
        
        void SelectParsingTemplate(int ix);

        void RemoveParsingTemplateName(int ix);

        ColorPickResult PickColor(Color startColor);
        
        void UpdateTraceColor(Color color);
        
        void UpdateDebugColor(Color color);
        
        void UpdateInfoColor(Color color);
        
        void UpdateWarnColor(Color color);
        
        void UpdateErrorColor(Color color);
        
        void UpdateFatalColor(Color color);
        
        void EnableChangeOrClose(bool enabled);
        
        void EnableResetColors(bool enabled);
        
        void ShowNewParsingTemplateForm(bool show);

        ParsingTemplate NewParsingTemplateData { get; }
    }
}