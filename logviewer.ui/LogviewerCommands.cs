// Created by: egr
// Created at: 11.11.2015
// © 2012-2015 Alexander Egorov

using System.Windows.Input;

namespace logviewer.ui
{
    public class LogviewerCommands
    {
        static LogviewerCommands()
        {
            var inputs = new InputGestureCollection
            {
                new KeyGesture(Key.F5, ModifierKeys.None, "F5")
            };
            Update = new RoutedUICommand("Update", "Update", typeof(LogviewerCommands), inputs);
            Statistic = new RoutedUICommand("Statistic", "Statistic", typeof(LogviewerCommands));
            Settings = new RoutedUICommand("Settings", "Settings", typeof(LogviewerCommands));
            Updates = new RoutedUICommand("Updates", "Updates", typeof(LogviewerCommands));
        }

        public static RoutedUICommand Update { get; }
        public static RoutedUICommand Statistic { get; }
        public static RoutedUICommand Settings { get; }
        public static RoutedUICommand Updates { get; }
    }
}