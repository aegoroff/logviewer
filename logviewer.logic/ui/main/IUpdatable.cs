// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 04.10.2014
// © 2012-2017 Alexander Egorov

using System;

namespace logviewer.logic.ui.main
{
    public interface IUpdatable
    {
        void ShowDialogAboutNewVersionAvaliable(Version current, Version latest, string targetAddress); 
        void ShowNoUpdateAvaliable(); 
    }
}