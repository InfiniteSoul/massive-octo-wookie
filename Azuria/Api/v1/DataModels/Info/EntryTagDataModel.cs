﻿using Azuria.AnimeManga.Properties;
using Azuria.Api.v1.Converters;
using Newtonsoft.Json;

namespace Azuria.Api.v1.DataModels.Info
{
    internal class EntryTagDataModel : IDataModel
    {
        #region Properties

        [JsonProperty("id")]
        internal int Id { get; set; }

        [JsonProperty("rate_flag"), JsonConverter(typeof(IntToBoolConverter))]
        internal bool IsRated { get; set; }

        [JsonProperty("spoiler_flag"), JsonConverter(typeof(IntToBoolConverter))]
        internal bool IsSpoiler { get; set; }

        [JsonProperty("tid")]
        internal TagType Tag { get; set; }

        #endregion
    }
}