// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 19.03.2017
// © 2012-2017 Alexander Egorov

using System;
using System.Drawing;
using logviewer.logic.Annotations;

namespace logviewer.logic.ui.main
{
    [PublicAPI]
    public class TextFormat
    {
        public string Font { get; set; } = @"Courier New";

        public string SizeHead { get; set; } = "13";

        public string SizeBody { get; set; } = "12";

        public Color Color { get; set; } = Color.Black;

        public string ColorAsString { get; set; }

        public string ColorToString()
        {
            byte[] data = { this.Color.R, this.Color.G, this.Color.B };
            return "#" + BitConverter.ToString(data).Replace("-", "");
        }
    }
}
