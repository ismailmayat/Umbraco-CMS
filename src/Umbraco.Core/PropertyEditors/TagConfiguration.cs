﻿using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the tag value editor.
    /// </summary>
    public class TagConfiguration
    {
        [ConfigurationField("group", "Tag group", "requiredfield",
            Description = "Define a tag group")]
        public string Group { get; set; } = "default";

        [ConfigurationField("storageType", "Storage Type", "views/propertyeditors/tags/tags.prevalues.html",
            Description = "Select whether to store the tags in cache as CSV (default) or as JSON. The only benefits of storage as JSON is that you are able to have commas in a tag value but this will require parsing the json in your views or using a property value converter")]
        public TagsStorageType StorageType { get; set; } = TagsStorageType.Csv;

        // not a field
        public char Delimiter { get; set; }
    }
}
