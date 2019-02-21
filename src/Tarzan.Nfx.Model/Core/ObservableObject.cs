using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Tarzan.Nfx.Model.Core
{
    /// <summary>
    /// Abstract class for all observable objects.
    /// </summary>
    [Serializable]
    public abstract class ObservableObject
    {
        /// <summary>
        /// The type property identifies the type of Observable Object.
        /// </summary>
        [JsonProperty("type")]
        public abstract string Type { get; }
        /// <summary>
        /// Identifiers MUST follow the form "object-type--UUIDv4" pattern.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The created property represents the time at which the first version of this object was created.
        /// </summary>
        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }
        /// <summary>
        /// The modified property represents the time that this particular version of the object was created. 
        /// </summary>
        [JsonProperty("modified")]
        public DateTimeOffset Modified { get; set; }

        /// <summary>
        /// The labels property specifies a set of classifications.
        /// </summary>
        [JsonProperty("labels")]
        public IList<string> Labels { get; set; }

        [JsonProperty("extensions")]
        public string[] Extensions { get; set; }
    }


    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}