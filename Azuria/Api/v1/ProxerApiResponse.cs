﻿using System;
using System.Collections.Generic;
using Azuria.Api.v1.Converters;
using Azuria.Enums;
using Azuria.ErrorHandling;
using Newtonsoft.Json;

namespace Azuria.Api.v1
{
    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProxerApiResponse<T> : ProxerApiResponse, IProxerResult<T>
    {
        /// <summary>
        /// </summary>
        internal ProxerApiResponse()
        {
        }

        /// <inheritdoc />
        internal ProxerApiResponse(IEnumerable<Exception> exceptions) : base(exceptions)
        {
        }

        /// <inheritdoc />
        internal ProxerApiResponse(Exception exception) : base(exception)
        {
        }

        #region Properties

        /// <inheritdoc />
        [JsonProperty("data")]
        public T Result { get; internal set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Deconstruct(out bool sucess, out IEnumerable<Exception> exceptions, out T result)
        {
            sucess = this.Success;
            exceptions = this.Exceptions;
            result = this.Result;
        }

        #endregion
    }

    /// <summary>
    /// </summary>
    public class ProxerApiResponse : ProxerResult
    {
        /// <summary>
        /// </summary>
        internal ProxerApiResponse()
        {
        }

        /// <summary>
        /// Initialises a new instance with the exceptions that were thrown during method execution and indicates that the
        /// method failed to execute.
        /// </summary>
        /// <param name="exceptions">The exception that were thrown during method execution.</param>
        internal ProxerApiResponse(IEnumerable<Exception> exceptions)
        {
            this.Success = false;
            this.Exceptions = exceptions;
        }

        /// <summary>
        /// </summary>
        /// <param name="exception"></param>
        internal ProxerApiResponse(Exception exception) : this(new[] {exception})
        {
        }

        #region Properties

        [JsonProperty("code")]
        internal ErrorCode ErrorCode { get; set; } = ErrorCode.NoError;

        [JsonProperty("message", Required = Required.Always)]
        internal string Message { get; set; }

        /// <inheritdoc />
        [JsonProperty("error", Required = Required.Always)]
        [JsonConverter(typeof(InvertBoolConverter))]
        public new bool Success { get; set; }

        #endregion
    }
}