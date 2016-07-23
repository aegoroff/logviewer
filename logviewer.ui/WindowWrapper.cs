// Created by: egr
// Created at: 12.11.2015
// © 2012-2016 Alexander Egorov

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