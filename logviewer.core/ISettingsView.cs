// Created by: egr
// Created at: 25.09.2013
// © 2012-2014 Alexander Egorov

namespace logviewer.core
{
    public interface ISettingsView
    {
        void EnableSave(bool enabled);
        
        void LoadFormData(FormData formData);
        
        void LoadParsingTemplate(ParsingTemplate template);
        
        void AddTemplateName(string name);
        
        void SelectParsingTemplateByName(string name);
    }
}