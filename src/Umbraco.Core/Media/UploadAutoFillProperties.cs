﻿using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace Umbraco.Core.Media
{
    /// <summary>
    /// Provides methods to manage auto-fill properties for upload fields.
    /// </summary>
    internal class UploadAutoFillProperties
    {
        private readonly ILogger _logger;
        private readonly MediaFileSystem _mediaFileSystem;
        private readonly IContentSection _contentSettings;

        public UploadAutoFillProperties(MediaFileSystem mediaFileSystem, ILogger logger, IContentSection contentSettings)
        {
            _mediaFileSystem = mediaFileSystem;
            _logger = logger;
            _contentSettings = contentSettings;
        }

        /// <summary>
        /// Gets the auto-fill configuration for a specified property alias.
        /// </summary>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <returns>The auto-fill configuration for the specified property alias, or null.</returns>
        public IImagingAutoFillUploadField GetConfig(string propertyTypeAlias)
        {
            var autoFillConfigs = _contentSettings.ImageAutoFillProperties;
            return autoFillConfigs?.FirstOrDefault(x => x.Alias == propertyTypeAlias);
        }

        /// <summary>
        /// Resets the auto-fill properties of a content item, for a specified property alias.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <param name="culture">Variation language.</param>
        /// <param name="segment">Variation segment.</param>
        public void Reset(IContentBase content, string propertyTypeAlias, string culture, string segment)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (propertyTypeAlias == null) throw new ArgumentNullException(nameof(propertyTypeAlias));

            // get the config, no config = nothing to do
            var autoFillConfig = GetConfig(propertyTypeAlias);
            if (autoFillConfig == null) return; // nothing

            // reset
            Reset(content, autoFillConfig, culture, segment);
        }

        /// <summary>
        /// Resets the auto-fill properties of a content item, for a specified auto-fill configuration.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="autoFillConfig">The auto-fill configuration.</param>
        /// <param name="culture">Variation language.</param>
        /// <param name="segment">Variation segment.</param>
        public void Reset(IContentBase content, IImagingAutoFillUploadField autoFillConfig, string culture, string segment)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (autoFillConfig == null) throw new ArgumentNullException(nameof(autoFillConfig));

            ResetProperties(content, autoFillConfig, culture, segment);
        }

        /// <summary>
        /// Populates the auto-fill properties of a content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <param name="filepath">The filesystem-relative filepath, or null to clear properties.</param>
        /// <param name="culture">Variation language.</param>
        /// <param name="segment">Variation segment.</param>
        public void Populate(IContentBase content, string propertyTypeAlias, string filepath, string culture, string segment)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (propertyTypeAlias == null) throw new ArgumentNullException(nameof(propertyTypeAlias));

            // no property = nothing to do
            if (content.Properties.Contains(propertyTypeAlias) == false) return;

            // get the config, no config = nothing to do
            var autoFillConfig = GetConfig(propertyTypeAlias);
            if (autoFillConfig == null) return; // nothing

            // populate
            Populate(content, autoFillConfig, filepath, culture, segment);
        }

        /// <summary>
        /// Populates the auto-fill properties of a content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <param name="filepath">The filesystem-relative filepath, or null to clear properties.</param>
        /// <param name="filestream">The stream containing the file data.</param>
        /// <param name="culture">Variation language.</param>
        /// <param name="segment">Variation segment.</param>
        public void Populate(IContentBase content, string propertyTypeAlias, string filepath, Stream filestream, string culture, string segment)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (propertyTypeAlias == null) throw new ArgumentNullException(nameof(propertyTypeAlias));

            // no property = nothing to do
            if (content.Properties.Contains(propertyTypeAlias) == false) return;

            // get the config, no config = nothing to do
            var autoFillConfig = GetConfig(propertyTypeAlias);
            if (autoFillConfig == null) return; // nothing

            // populate
            Populate(content, autoFillConfig, filepath, filestream, culture, segment);
        }

        /// <summary>
        /// Populates the auto-fill properties of a content item, for a specified auto-fill configuration.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="autoFillConfig">The auto-fill configuration.</param>
        /// <param name="filepath">The filesystem path to the uploaded file.</param>
        /// <remarks>The <paramref name="filepath"/> parameter is the path relative to the filesystem.</remarks>
        /// <param name="culture">Variation language.</param>
        /// <param name="segment">Variation segment.</param>
        public void Populate(IContentBase content, IImagingAutoFillUploadField autoFillConfig, string filepath, string culture, string segment)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (autoFillConfig == null) throw new ArgumentNullException(nameof(autoFillConfig));

            // no file = reset, file = auto-fill
            if (filepath.IsNullOrWhiteSpace())
            {
                ResetProperties(content, autoFillConfig, culture, segment);
            }
            else
            {
                // if anything goes wrong, just reset the properties
                try
                {
                    using (var filestream = _mediaFileSystem.OpenFile(filepath))
                    {
                        var extension = (Path.GetExtension(filepath) ?? "").TrimStart('.');
                        var size = _mediaFileSystem.IsImageFile(extension) ? (Size?) _mediaFileSystem.GetDimensions(filestream) : null;
                        SetProperties(content, autoFillConfig, size, filestream.Length, extension, culture, segment);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(typeof(UploadAutoFillProperties), $"Could not populate upload auto-fill properties for file \"{filepath}\".", ex);
                    ResetProperties(content, autoFillConfig, culture, segment);
                }
            }
        }

        /// <summary>
        /// Populates the auto-fill properties of a content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="autoFillConfig"></param>
        /// <param name="filepath">The filesystem-relative filepath, or null to clear properties.</param>
        /// <param name="filestream">The stream containing the file data.</param>
        /// <param name="culture">Variation language.</param>
        /// <param name="segment">Variation segment.</param>
        public void Populate(IContentBase content, IImagingAutoFillUploadField autoFillConfig, string filepath, Stream filestream, string culture, string segment)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (autoFillConfig == null) throw new ArgumentNullException(nameof(autoFillConfig));

            // no file = reset, file = auto-fill
            if (filepath.IsNullOrWhiteSpace() || filestream == null)
            {
                ResetProperties(content, autoFillConfig, culture, segment);
            }
            else
            {
                var extension = (Path.GetExtension(filepath) ?? "").TrimStart('.');
                var size = _mediaFileSystem.IsImageFile(extension) ? (Size?)_mediaFileSystem.GetDimensions(filestream) : null;
                SetProperties(content, autoFillConfig, size, filestream.Length, extension, culture, segment);
            }
        }

        private static void SetProperties(IContentBase content, IImagingAutoFillUploadField autoFillConfig, Size? size, long length, string extension, string culture, string segment)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (autoFillConfig == null) throw new ArgumentNullException(nameof(autoFillConfig));

            if (content.Properties.Contains(autoFillConfig.WidthFieldAlias))
                content.Properties[autoFillConfig.WidthFieldAlias].SetValue(size.HasValue ? size.Value.Width.ToInvariantString() : string.Empty, culture, segment);

            if (content.Properties.Contains(autoFillConfig.HeightFieldAlias))
                content.Properties[autoFillConfig.HeightFieldAlias].SetValue(size.HasValue ? size.Value.Height.ToInvariantString() : string.Empty, culture, segment);

            if (content.Properties.Contains(autoFillConfig.LengthFieldAlias))
                content.Properties[autoFillConfig.LengthFieldAlias].SetValue(length, culture, segment);

            if (content.Properties.Contains(autoFillConfig.ExtensionFieldAlias))
                content.Properties[autoFillConfig.ExtensionFieldAlias].SetValue(extension, culture, segment);
        }

        private static void ResetProperties(IContentBase content, IImagingAutoFillUploadField autoFillConfig, string culture, string segment)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (autoFillConfig == null) throw new ArgumentNullException(nameof(autoFillConfig));

            if (content.Properties.Contains(autoFillConfig.WidthFieldAlias))
                content.Properties[autoFillConfig.WidthFieldAlias].SetValue(string.Empty, culture, segment);

            if (content.Properties.Contains(autoFillConfig.HeightFieldAlias))
                content.Properties[autoFillConfig.HeightFieldAlias].SetValue(string.Empty, culture, segment);

            if (content.Properties.Contains(autoFillConfig.LengthFieldAlias))
                content.Properties[autoFillConfig.LengthFieldAlias].SetValue(string.Empty, culture, segment);

            if (content.Properties.Contains(autoFillConfig.ExtensionFieldAlias))
                content.Properties[autoFillConfig.ExtensionFieldAlias].SetValue(string.Empty, culture, segment);
        }
    }
}
