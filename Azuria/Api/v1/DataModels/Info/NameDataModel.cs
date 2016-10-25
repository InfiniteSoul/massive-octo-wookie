﻿using Azuria.Api.v1.Converters.Info;
using Azuria.Api.v1.Enums;
using Newtonsoft.Json;

namespace Azuria.Api.v1.DataModels.Info
{
    internal class NameDataModel : IDataModel
    {
        #region Properties

        [JsonProperty("name")]
        internal string Name { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(NameTypeConverter))]
        internal MediaNameType Type { get; set; }

        #endregion
    }
}