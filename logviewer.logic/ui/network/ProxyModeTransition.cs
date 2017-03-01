// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 01.03.2017
// © 2007-2017 Alexander Egorov

namespace logviewer.logic.ui.network
{
    /// <summary>
    ///     Represents all possible proxy modes.
    /// </summary>
    public enum ProxyModeTransition
    {
        /// <summary>
        ///     Do not use proxy
        /// </summary>
        ToNone,

        /// <summary>
        ///     Automatic proxy detection
        /// </summary>
        ToAutoProxyDetection,

        /// <summary>
        ///     Custom proxy settings
        /// </summary>
        ToCustom,

        /// <summary>
        ///     Custom proxy with default user
        /// </summary>
        ToCustomDefaultUser,
        
        /// <summary>
        ///     Custom proxy with custom user
        /// </summary>
        ToCustomWithManualUser
    }
}