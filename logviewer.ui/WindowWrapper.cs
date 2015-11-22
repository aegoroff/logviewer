// Created by: egr
// Created at: 12.11.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Windows;
using System.Windows.Interop;

namespace logviewer.ui
{
    public class WindowWrapper : System.Windows.Forms.IWin32Window
    {
        public WindowWrapper(IntPtr handle)
        {
            this.Handle = handle;
        }

        public WindowWrapper(Window window)
        {
            this.Handle = new WindowInteropHelper(window).Handle;
        }

        public IntPtr Handle { get; }
    }
}