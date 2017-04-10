// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 12.08.2016
// © 2012-2017 Alexander Egorov

using System.ComponentModel;
using System.Runtime.CompilerServices;
using logviewer.logic.Annotations;

namespace logviewer.logic.ui.statistic
{
    [PublicAPI]
    public class StatFilterViewModel : INotifyPropertyChanged
    {
        private string filter;

        private bool userRegexp;

        public string Filter
        {
            get => this.filter;
            set
            {
                this.OnPropertyChanged(nameof(this.Filter));
                this.filter = value;
            }
        }

        public bool UserRegexp
        {
            get => this.userRegexp;
            set
            {
                this.OnPropertyChanged(nameof(this.UserRegexp));
                this.userRegexp = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
