﻿using System;
using Newtonsoft.Json;

namespace Azuria.Api.v1.Converters
{
    /// <summary>
    /// </summary>
    public class InvertBoolConverter : JsonConverter
    {
        #region Methods

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            bool lValue = reader.Value.ToString() == "1";
            return !lValue;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}