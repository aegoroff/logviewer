// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 02.03.2017
// © 2012-2017 Alexander Egorov

using logviewer.logic.Annotations;

namespace logviewer.logic.support
{
    [PublicAPI]
    public interface IAsymCrypt
    {
        string Encrypt(string plain);
        string Decrypt(string crypted);

        /// <summary>
        ///     Must be set as base64 encoded
        /// </summary>
        string PublicKey { get; set; }

        /// <summary>
        ///     Must be set as base64 encoded
        /// </summary>
        string PrivateKey { get; set; }
    }
}