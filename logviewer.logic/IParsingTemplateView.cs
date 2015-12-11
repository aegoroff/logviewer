// Created by: egr
// Created at: 17.10.2014
// © 2012-2015 Alexander Egorov

namespace logviewer.logic
{
    public interface IParsingTemplateView
    {
        void ShowInvalidTemplateError(string message, object control);

        void HideInvalidTemplateError(object control);

        void OnFixTemplate(object control);

        void LoadTemplate(ParsingTemplate template);

        object MessageStartControl { get; }
        
        object FilterControl { get; }
    }
}