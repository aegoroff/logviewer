// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 17.03.2017
// © 2012-2017 Alexander Egorov

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace logviewer.logic.ui
{
    /// <summary>
    /// Represents VirtualizingCollection interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IVirtualizingCollection<T> : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        /// Changes visible in UI items Range
        /// </summary>
        /// <param name="range">Visible in UI items range</param>
        void ChangeVisible(Range range);

        /// <summary>
        ///     Loads the count of items.
        /// </summary>
        void LoadCount(int itemsCount);
    }
}