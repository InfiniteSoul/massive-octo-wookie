﻿using System;
using System.Threading.Tasks;
using Azuria.ErrorHandling;

namespace Azuria.Helpers.Extensions
{
    /// <summary>
    /// </summary>
    public static class ProxerResultExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="result"></param>
        /// <param name="onError"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <returns></returns>
        public static TOut OnError<T, TOut>(this T result, TOut onError) where T : IProxerResult<TOut>
        {
            return result.Success ? result.Result : onError;
        }

        /// <summary>
        /// </summary>
        /// <param name="task"></param>
        /// <param name="onError"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <returns></returns>
        public static async Task<TOut> OnError<T, TOut>(this Task<T> task, TOut onError)
            where T : IProxerResult<TOut>
        {
            T lResult = await task.ConfigureAwait(false);
            return lResult.Success ? lResult.Result : onError;
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static async Task<T> ThrowFirstForNonSuccess<T>(this Task<IProxerResult<T>> task)
        {
            IProxerResult<T> lResult = await task.ConfigureAwait(false);
            return lResult.Success ? lResult.Result : throw new AggregateException(lResult.Exceptions);
        }

        /// <summary>
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static async Task ThrowFirstForNonSuccess<T>(this Task<T> task) where T : IProxerResult
        {
            IProxerResult lResult = await task.ConfigureAwait(false);
            if (!lResult.Success) throw new AggregateException(lResult.Exceptions);
        }
    }
}