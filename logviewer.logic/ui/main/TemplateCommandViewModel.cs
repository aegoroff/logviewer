// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 23.08.2016
// © 2012-2017 Alexander Egorov

using logviewer.logic.Annotations;

namespace logviewer.logic.ui.main
{
    [PublicAPI]
    public class TemplateCommandViewModel
    {
        public string Text { get; set; }

        public bool Checked { get; set; }

        public int Index { get; set; }
    }
}
