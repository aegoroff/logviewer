// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 13.10.2013
// © 2012-2017 Alexander Egorov

using System.Collections.Generic;
using System.Drawing;
using logviewer.engine;

namespace logviewer.logic.ui.settings
{
    public class FormData
    {
        public FormData()
        {
            this.Colors = new Dictionary<LogLevel, Color>();
        }

        public string KeepLastNFiles { get; set; }

        public bool OpenLastFile { get; set; }

        public bool AutoRefreshOnFileChange { get; set; }

        public string PageSize { get; set; }

        public IDictionary<LogLevel, Color> Colors { get; }
    }
}
