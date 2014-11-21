// Created by: egr
// Created at: 04.10.2014
// © 2012-2014 Alexander Egorov

using logviewer.engine;

namespace logviewer.core
{
    public interface IUpdateView
    {
        void EnableUpdateStartControl(bool enable);

        void OnProgress(int percent, FileSize totalBytes, FileSize readBytes);
        
        void DisableYesControl();
        
        void ShowErrorMessage(string message);

        void Close();
    }
}