﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents the variant info for a content item
    /// </summary>
    [DataContract(Name = "contentVariant", Namespace = "")]
    public class ContentVariation
    {
        [DataMember(Name = "language", IsRequired = true)]
        [Required]
        public Language Language { get; set; }

        [DataMember(Name = "segment")]
        public string Segment { get; set; }

        /// <summary>
        /// The content name of the variant
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "state")]
        public string PublishedState { get; set; }

        /// <summary>
        /// Determines if the content variant for this culture has been created
        /// </summary>
        [DataMember(Name = "exists")]
        public bool Exists { get; set; }

        [DataMember(Name = "isEdited")]
        public bool IsEdited { get; set; }

        /// <summary>
        /// Determines if this is the variant currently being edited
        /// </summary>
        [DataMember(Name = "current")]
        public bool IsCurrent { get; set; }

        /// <summary>
        /// If the variant is a required variant for validation purposes
        /// </summary>
        [DataMember(Name = "mandatory")]
        public bool Mandatory { get; set; }
    }
}
