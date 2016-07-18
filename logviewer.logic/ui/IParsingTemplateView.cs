// Created by: egr
// Created at: 17.10.2014
// © 2012-2016 Alexander Egorov

using logviewer.logic.Annotations;
using logviewer.logic.models;

namespace logviewer.logic.ui
{
    public interface IParsingTemplateView
    {
        void ShowInvalidTemplateError(string message, object control);

        void HideInvalidTemplateError(object control);

        void OnFixTemplate(object control);

        [PublicAPI]
        void LoadTemplate(ParsingTemplate template);

        object MessageStartControl { get; }
        
        object FilterControl { get; }
    }
}