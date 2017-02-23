﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 08.10.2014
// © 2012-2017 Alexander Egorov

using System.Drawing;

namespace logviewer.logic.ui.settings
{
    public struct ColorPickResult
    {
        public bool Result { get; set; }
        public Color SelectedColor { get; set; }
    }
}