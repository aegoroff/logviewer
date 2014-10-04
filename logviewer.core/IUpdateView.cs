// Created by: egr
// Created at: 04.10.2014
// © 2012-2014 Alexander Egorov

namespace logviewer.core
{
    public interface IUpdateView
    {
        void EnableUpdateStartControl(bool enable);
        
        void OnProgress(int percent, long totalBytes, long readBytes);
        
        void DisableYesControl();
        
        void ShowErrorMessage(string message);

        void Close();
    }
}