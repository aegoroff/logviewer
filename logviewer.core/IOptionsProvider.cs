// Created by: egr
// Created at: 03.12.2015
// © 2012-2015 Alexander Egorov

namespace logviewer.core
{
    public interface IOptionsProvider
    {
        string ReadStringOption(string option, string defaultValue = null);
        bool ReadBooleanOption(string option, bool defaultValue = false);
        int ReadIntegerOption(string option, int defaultValue = 0);
        void UpdateStringOption(string option, string value);
        void UpdateBooleanOption(string option, bool value);
        void UpdateIntegerOption(string option, int value);
    }
}