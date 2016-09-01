﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azuria.Api;
using Azuria.Exceptions;
using Azuria.Utilities.ErrorHandling;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Azuria.Notifications.News
{
    /// <summary>
    /// </summary>
    public sealed class NewsNotificationEnumerator : INotificationEnumerator<NewsNotification>
    {
        private const int NewsPerPage = 15;
        private readonly Senpai _senpai;
        private NewsNotification[] _currentPageContent;
        private int _currentPageItemIndex = 14;
        private int _nextPageToLoad = 1;

        internal NewsNotificationEnumerator(Senpai senpai)
        {
            this._senpai = senpai;
        }

        #region Properties

        /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        public NewsNotification Current => this._currentPageContent[this._currentPageItemIndex];

        /// <summary>Gets the current element in the collection.</summary>
        /// <returns>The current element in the collection.</returns>
        object IEnumerator.Current => this.Current;

        #endregion

        #region Inherited

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            this._currentPageContent = new NewsNotification[0];
        }

        /// <summary>Advances the enumerator to the next element of the collection.</summary>
        /// <returns>
        ///     true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the
        ///     end of the collection.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        public bool MoveNext()
        {
            this._currentPageItemIndex++;
            if (this._currentPageItemIndex < NewsPerPage) return true;

            this._currentPageItemIndex = 0;
            ProxerResult lGetNotificationsResult = Task.Run(this.GetNextPage).Result;
            if (!lGetNotificationsResult.Success)
                throw lGetNotificationsResult.Exceptions.FirstOrDefault() ?? new WrongResponseException();
            return lGetNotificationsResult.Success && this._currentPageContent.Any();
        }

        /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        public void Reset()
        {
            this._nextPageToLoad = 1;
            this._currentPageItemIndex = 14;
            this._currentPageContent = new NewsNotification[0];
        }

        #endregion

        #region

        [ItemNotNull]
        private async Task<ProxerResult> GetNextPage()
        {
            ProxerResult<string> lResult =
                await
                    ApiInfo.HttpClient.GetRequest(
                        new Uri("https://proxer.me/notifications?format=json&s=news&p=" + this._nextPageToLoad),
                        this._senpai);

            if (!lResult.Success)
                return new ProxerResult(lResult.Exceptions);

            string lResponse = lResult.Result;

            if ((lResponse == null) || !lResponse.StartsWith("{\"error\":0"))
                return new ProxerResult
                {
                    Success = false
                };

            try
            {
                Dictionary<string, List<NewsNotification>> lDeserialized =
                    JsonConvert.DeserializeObject<Dictionary<string, List<NewsNotification>>>("{" +
                                                                                              lResponse.Substring(
                                                                                                  "{\"error\":0,".Length));

                this._currentPageContent = lDeserialized["notifications"].ToArray();

                this._nextPageToLoad++;
                return new ProxerResult();
            }
            catch
            {
                return new ProxerResult(ErrorHandler.HandleError(lResponse, false).Exceptions);
            }
        }

        #endregion
    }
}