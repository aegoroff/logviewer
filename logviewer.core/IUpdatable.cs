// Created by: egr
// Created at: 04.10.2014
// © 2012-2014 Alexander Egorov

using System;

namespace logviewer.core
{
    public interface IUpdatable
    {
        void ShowDialogAboutNewVersionAvaliable(Version current, Version latest, string uri); 
        void ShowNoUpdateAvaliable(); 
    }
}