// Created by: egr
// Created at: 11.11.2015
// © 2012-2015 Alexander Egorov

using System.Windows.Input;

namespace logviewer.ui
{
    public class LogviewerCommands
    {
        private static readonly RoutedUICommand update;

        static LogviewerCommands()
        {
            var inputs = new InputGestureCollection
            {
                new KeyGesture(Key.F5, ModifierKeys.None, "F5")
            };
            update = new RoutedUICommand("Update", "Update", typeof(LogviewerCommands), inputs);
        }

        public static RoutedUICommand Update => update;
    }
}