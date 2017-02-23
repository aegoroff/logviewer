// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 17.10.2014
// © 2012-2017 Alexander Egorov

using logviewer.logic.Annotations;
using logviewer.logic.models;

namespace logviewer.logic.ui.settings
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