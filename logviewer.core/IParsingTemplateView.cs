// Created by: egr
// Created at: 17.10.2014
// © 2012-2014 Alexander Egorov

namespace logviewer.core
{
    public interface IParsingTemplateView
    {
        void ShowInvalidTemplateError(string message);

        void HideInvalidTemplateError();

        void OnFixTemplate();

        void LoadTemplate(ParsingTemplate template);
    }
}