﻿namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// Contains general settings information for the entire Umbraco instance based on information from  web.config appsettings 
    /// </summary>
    public interface IGlobalSettings
    {
        /// <summary>
        /// Gets the reserved urls from web.config.
        /// </summary>
        /// <value>The reserved urls.</value>
        string ReservedUrls { get; }

        /// <summary>
        /// Gets the reserved paths from web.config
        /// </summary>
        /// <value>The reserved paths.</value>
        string ReservedPaths { get; }

        /// <summary>
        /// Gets the name of the content XML file.
        /// </summary>
        /// <value>The content XML.</value>
        /// <remarks>
        /// Defaults to ~/App_Data/umbraco.config
        /// </remarks>
        string ContentXmlFile { get; }

        /// <summary>
        /// Gets the path to umbraco's root directory (/umbraco by default).
        /// </summary>
        string Path { get; }
        
        /// <summary>
        /// Gets or sets the configuration status. This will return the version number of the currently installed umbraco instance.
        /// </summary>
        string ConfigurationStatus { get; set; }

        /// <summary>
        /// Gets the time out in minutes.
        /// </summary>
        int TimeOutInMinutes { get; }

        /// <summary>
        /// Gets a value indicating whether umbraco uses directory urls.
        /// </summary>
        /// <value><c>true</c> if umbraco uses directory urls; otherwise, <c>false</c>.</value>
        bool UseDirectoryUrls { get; }

        /// <summary>
        /// Gets the default UI language.
        /// </summary>
        /// <value>The default UI language.</value>
        // ReSharper disable once InconsistentNaming
        string DefaultUILanguage { get; }

        /// <summary>
        /// Gets a value indicating whether umbraco should hide top level nodes from generated urls.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if umbraco hides top level nodes from urls; otherwise, <c>false</c>.
        /// </value>
        bool HideTopLevelNodeFromPath { get; }

        /// <summary>
        /// Gets a value indicating whether umbraco should force a secure (https) connection to the backoffice.
        /// </summary>
        bool UseHttps { get; }

        /// <summary>
        /// Returns a string value to determine if umbraco should skip version-checking.
        /// </summary>
        /// <value>The version check period in days (0 = never).</value>
        int VersionCheckPeriod { get; }

        /// <summary>
        /// This is the location type to store temporary files such as cache files or other localized files for a given machine
        /// </summary>
        /// <remarks>
        /// Used for some cache files and for specific environments such as Azure
        /// </remarks>
        LocalTempStorage LocalTempStorageLocation { get; }
    }
}
