// Created by: egr
// Created at: 11.09.2015
// © 2012-2016 Alexander Egorov

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using logviewer.logic.Annotations;

namespace logviewer.logic.support
{
    public class AsymCrypt
    {
        /// <summary>
        ///     Must be set as base64 encoded
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        ///     Must be set as base64 encoded
        /// </summary>
        public string PrivateKey { get; set; }

        public void GenerateKeys(int keySize = 2048)
        {
            var csp = new RSACryptoServiceProvider(keySize);

            var privKey = csp.ExportParameters(true);
            var pubKey = csp.ExportParameters(false);

            this.PublicKey = Serialize(pubKey);
            this.PrivateKey = Serialize(privKey);
        }

        private static string Serialize<T>(T obj)
        {
            var settings = new XmlWriterSettings { Indent = false };
            var stringWriter = new StringWriter();
            var serializer = new XmlSerializer(typeof (T));

            using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                serializer.Serialize(xmlWriter, obj);
            }

            return ToBase64String(stringWriter.ToString());
        }

        [Pure]
        public string Encrypt(string plain)
        {
            var provider = CreateProvider(this.PublicKey);
            var decryptedBytes = Encoding.Unicode.GetBytes(plain);
            var cryptedBytes = provider.Encrypt(decryptedBytes, false);

            return Convert.ToBase64String(cryptedBytes);
        }

        [Pure]
        public string Decrypt(string crypted)
        {
            var provider = CreateProvider(this.PrivateKey);
            var cryptedBytes = Convert.FromBase64String(crypted);
            var decryptedBytes = provider.Decrypt(cryptedBytes, false);
            return Encoding.Unicode.GetString(decryptedBytes);
        }

        private static RSACryptoServiceProvider CreateProvider(string key)
        {
            var reader = new StringReader(FromBase64String(key));
            var serializer = new XmlSerializer(typeof (RSAParameters));

            var parameters = (RSAParameters) serializer.Deserialize(reader);

            var provider = new RSACryptoServiceProvider();
            provider.ImportParameters(parameters);
            return provider;
        }

        [Pure]
        public static string ToBase64String(string plain)
        {
            var bytes = Encoding.Unicode.GetBytes(plain);
            return Convert.ToBase64String(bytes);
        }

        [Pure]
        public static string FromBase64String(string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            return Encoding.Unicode.GetString(bytes);
        }
    }
}