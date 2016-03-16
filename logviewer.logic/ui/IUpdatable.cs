// Created by: egr
// Created at: 04.10.2014
// © 2012-2016 Alexander Egorov

using System;

namespace logviewer.logic.ui
{
    public interface IUpdatable
    {
        void ShowDialogAboutNewVersionAvaliable(Version current, Version latest, string targetAddress); 
        void ShowNoUpdateAvaliable(); 
    }
}