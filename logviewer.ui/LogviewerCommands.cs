// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 11.11.2015
// © 2012-2018 Alexander Egorov

using System.Windows.Input;

namespace logviewer.ui
{
    public class LogviewerCommands
    {
        static LogviewerCommands()
        {
            var updateKeyboardShortcuts = new InputGestureCollection
                         {
                             new KeyGesture(Key.F5, ModifierKeys.None, "F5")
                         };
            Update = new RoutedUICommand("Update", "Update", typeof(LogviewerCommands), updateKeyboardShortcuts);
            Statistic = new RoutedUICommand("Statistic", "Statistic", typeof(LogviewerCommands));
            Settings = new RoutedUICommand("Settings", "Settings", typeof(LogviewerCommands));
            NetworkSettings = new RoutedUICommand("NetworkSettings", "NetworkSettings", typeof(LogviewerCommands));
            Updates = new RoutedUICommand("Updates", "Updates", typeof(LogviewerCommands));

            var copyKeyboardShortcuts = new InputGestureCollection
                                          {
                                              new KeyGesture(Key.C, ModifierKeys.Control, "Ctrl+C")
                                          };
            CopySelected = new RoutedUICommand("Copy", "Copy", typeof(LogviewerCommands), copyKeyboardShortcuts);
        }

        public static RoutedUICommand Update { get; }

        public static RoutedUICommand Statistic { get; }

        public static RoutedUICommand Settings { get; }

        public static RoutedUICommand NetworkSettings { get; }

        public static RoutedUICommand Updates { get; }

        public static RoutedUICommand CopySelected { get; }
    }
}
