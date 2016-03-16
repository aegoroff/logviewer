// Created by: egr
// Created at: 29.05.2015
// © 2012-2016 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Deployment.WindowsInstaller;

namespace logviewer.install.mca
{
    public class CustomActions
    {
        private const string FakePrefix = "x";
		private const string AppConfig = "logviewer.exe.config";
        private const string Product = "logviewer";

        private static readonly Dictionary<string, string> tempToConfigMap = new Dictionary<string, string>
            {
                { TempAppConfigFile, Path.Combine(AppPath, AppConfig) }
            };

        private static string TempAppConfigFile => Path.Combine(Path.GetTempPath(), AppConfig);

        private static string AppPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Product);

        [CustomAction]
        public static ActionResult KeepConfigurationFiles(Session session)
        {
            KeepOldConfig(session, TempAppConfigFile);

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult RestoreConfigurationFiles(Session session)
        {
            RestoreConfig(session, TempAppConfigFile, RestoreAppConfig);
            return ActionResult.Success;
        }

        private static void KeepOldConfig(Session session, string temp)
        {
            KeepOldConfig(session, tempToConfigMap[temp], temp);
        }
        
        private static void KeepOldConfig(Session session, string source, string temp)
        {
            try
            {
                File.Copy(source, temp, true);
                session.Log("keep config {1} to: {0}", temp, source);
            }
            catch (Exception e)
            {
                session.Log(e.ToString());
            }
        }

        private static void RestoreConfig(Session session, string keptFile, Action<Session, XmlDocument, XmlDocument> restoreAction)
        {
            RestoreConfig(session, keptFile, tempToConfigMap[keptFile], restoreAction);
        }

        private static void RestoreConfig(Session session, string keptFile, string targetFile, Action<Session, XmlDocument, XmlDocument> restoreAction)
        {
            if (!File.Exists(keptFile))
            {
                session.Log("No file stored before: {0}", keptFile);
                return;
            }
            try
            {
                session.Log("Restoring: {0}", keptFile);

                var src = new XmlDocument();
                src.Load(keptFile);
                
                var tgt = new XmlDocument();
                var tgtPath = targetFile;
                tgt.Load(tgtPath);
                
                restoreAction(session, src, tgt);

                tgt.Save(tgtPath);
                session.Log("Result saved to: {0}", tgtPath);
            }
            catch (Exception e)
            {
                session.Log(e.ToString());
            }
            finally
            {
                CreateBackup(session, keptFile, targetFile);
                File.Delete(keptFile);
            }
            session.Log("{0} restored", keptFile);
        }

        private static void CreateBackup(Session session, string keptFile, string targetFile)
        {
            try
            {
                File.Copy(keptFile, targetFile + ".bak", true);
            }
            catch (Exception e)
            {
                session.Log(e.ToString());
            }
        }

        private static void RestoreAppConfig(Session session, XmlDocument src, XmlDocument tgt)
        {
            SaveMultipleNodes(session, src, tgt, "/configuration/appSettings/add", "key", "value");
        }

        private static void SaveMultipleNodes(Session session, XmlDocument src, XmlDocument tgt, string root, string keyAttr, string valueAttr, string namespaceUri = null)
        {
            XmlNodeList srcNodes;
            if (namespaceUri != null)
            {
                var nsMgr = new XmlNamespaceManager(src.NameTable);
                nsMgr.AddNamespace(FakePrefix, namespaceUri);
                srcNodes = src.SelectNodes(root, nsMgr);
            }
            else
            {
                srcNodes = src.SelectNodes(root);
            }
            if (srcNodes == null)
            {
                session.Log("No data found in source {0}", src.BaseURI);
                return;
            }

            var srcDict =
                srcNodes.Cast<XmlNode>()
                        .Where(setting => setting.Attributes?[keyAttr] != null && setting.Attributes[valueAttr] != null)
                        .ToDictionary(setting => setting.Attributes[keyAttr].Value,
                                      setting => setting.Attributes[valueAttr].Value);

            XmlNodeList tgtNodes;
            if (namespaceUri != null)
            {
                var nsMgr = new XmlNamespaceManager(tgt.NameTable);
                nsMgr.AddNamespace(FakePrefix, namespaceUri);
                tgtNodes = tgt.SelectNodes(root, nsMgr);
            }
            else
            {
                tgtNodes = tgt.SelectNodes(root);
            }

            if (tgtNodes == null)
            {
                session.Log("No data found in target {0}", tgt.BaseURI);
                return;
            }

            foreach (
                var node in
                    from XmlNode setting in tgtNodes
                    where setting.Attributes?[keyAttr] != null && setting.Attributes[valueAttr] != null
                    where srcDict.ContainsKey(setting.Attributes[keyAttr].Value)
                    select setting)
            {
                node.Attributes[valueAttr].Value = srcDict[node.Attributes[keyAttr].Value];
                session.Log("Node {0} set to {1}", node.Attributes[keyAttr].Value, node.Attributes[valueAttr].Value);
            }
        }
    }
}