// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 12.11.2015
// © 2012-2017 Alexander Egorov

using System;
using System.Windows;
using System.Windows.Interop;
using logviewer.logic.Annotations;

namespace logviewer.ui
{
    public class WindowWrapper : System.Windows.Forms.IWin32Window
    {
        [PublicAPI]
        public WindowWrapper(IntPtr handle)
        {
            this.Handle = handle;
        }

        [PublicAPI]
        public WindowWrapper(Window window)
        {
            this.Handle = new WindowInteropHelper(window).Handle;
        }

        public IntPtr Handle { get; }
    }
}