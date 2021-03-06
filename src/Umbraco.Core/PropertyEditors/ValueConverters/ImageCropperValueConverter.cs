﻿using System;
using System.Globalization;
using Newtonsoft.Json;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    /// <summary>
    /// Represents a value converter for the image cropper value editor.
    /// </summary>
    [DefaultPropertyValueConverter(typeof(JsonValueConverter))]
    public class ImageCropperValueConverter : PropertyValueConverterBase
    {
        /// <inheritdoc />
        public override bool IsConverter(PublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.ImageCropper);

        /// <inheritdoc />
        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (ImageCropperValue);

        /// <inheritdoc />
        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        /// <inheritdoc />
        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            ImageCropperValue value;
            try
            {
                value = JsonConvert.DeserializeObject<ImageCropperValue>(sourceString, new JsonSerializerSettings
                {
                    Culture = CultureInfo.InvariantCulture,
                    FloatParseHandling = FloatParseHandling.Decimal
                });
            }
            catch (Exception ex)
            {
                // cannot deserialize, assume it may be a raw image url
                Current.Logger.Error<ImageCropperValueConverter>($"Could not deserialize string \"{sourceString}\" into an image cropper value.", ex);
                value = new ImageCropperValue { Src = sourceString };
            }

            value.ApplyConfiguration(propertyType.DataType.ConfigurationAs<ImageCropperConfiguration>());

            return value;
        }
    }
}
