// Created by: egr
// Created at: 05.09.2007
// © 2007-2008 Alexander Egorov

namespace logviewer.logic.ui.network
{
    /// <summary>
    ///     Represents all possible proxy modes.
    /// </summary>
    public enum ProxyMode
    {
        /// <summary>
        ///     Do not use proxy
        /// </summary>
        None,

        /// <summary>
        ///     Automatic proxy detection
        /// </summary>
        AutoProxyDetection,

        /// <summary>
        ///     Manual proxy settings
        /// </summary>
        Custom
    }
}