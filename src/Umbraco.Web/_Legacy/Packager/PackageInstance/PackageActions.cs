﻿using System;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core._Legacy.PackageActions;


namespace umbraco.cms.businesslogic.packager
{

    /// <summary>
    /// Package actions are executed on packge install / uninstall.
    /// </summary>
    public class PackageAction
    {

        /// <summary>
        /// Runs the package action with the specified action alias.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="actionAlias">The action alias.</param>
        /// <param name="actionXml">The action XML.</param>
        public static void RunPackageAction(string packageName, string actionAlias, XmlNode actionXml)
        {

            foreach (var ipa in Current.PackageActions)
            {
                try
                {
                    if (ipa.Alias() == actionAlias)
                    {

                        ipa.Execute(packageName, actionXml);
                    }
                }
                catch (Exception ipaExp)
                {
                    Current.Logger.Error<PackageAction>(string.Format("Error loading package action '{0}' for package {1}", ipa.Alias(), packageName), ipaExp);
                }
            }
        }

        /// <summary>
        /// Undos the package action with the specified action alias.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="actionAlias">The action alias.</param>
        /// <param name="actionXml">The action XML.</param>
        public static void UndoPackageAction(string packageName, string actionAlias, System.Xml.XmlNode actionXml)
        {

            foreach (IPackageAction ipa in Current.PackageActions)
            {
                try
                {
                    if (ipa.Alias() == actionAlias)
                    {

                        ipa.Undo(packageName, actionXml);
                    }
                }
                catch (Exception ipaExp)
                {
                    Current.Logger.Error<PackageAction>(string.Format("Error undoing package action '{0}' for package {1}", ipa.Alias(), packageName), ipaExp);
                }
            }
        }

    }
}
