// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 18.04.2017
// © 2012-2018 Alexander Egorov

using System.Collections.Generic;

namespace logviewer.logic.storage
{
    public interface IStringCollectionStore
    {
        void Add(string item);

        void Remove(params string[] items);

        IEnumerable<string> ReadItems();

        string ReadLastUsedItem();
    }
}
