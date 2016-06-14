﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azuria.Utilities.ErrorHandling;
using Azuria.Utilities.Net;
using JetBrains.Annotations;

namespace Azuria.Notifications.FriendRequest
{
    /// <summary>
    ///     Represents a friend request notification.
    /// </summary>
    public class FriendRequestNotification : INotification
    {
        private readonly Senpai _senpai;
        private bool _handled;

        internal FriendRequestNotification([NotNull] User user, DateTime requestDate,
            [NotNull] Senpai senpai)
        {
            this.Date = requestDate;
            this.User = user;
            this._senpai = senpai;
        }

        #region Geerbt

        /// <summary>
        ///     Gets the type of the notification.
        /// </summary>
        public NotificationType Type => NotificationType.FriendRequest;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the date of the friend request.
        /// </summary>
        public DateTime Date { get; }

        /// <summary>
        ///     Gets the user that send the friend request.
        /// </summary>
        [NotNull]
        public User User { get; }

        #endregion

        #region

        /// <summary>
        ///     Accepts the friend request.
        /// </summary>
        /// <returns>If the action was successful.</returns>
        [ItemNotNull]
        public async Task<ProxerResult> AcceptRequest()
        {
            if (this._handled) return new ProxerResult {Success = false};

            Dictionary<string, string> lPostArgs = new Dictionary<string, string> {{"type", "accept"}};

            Func<string, ProxerResult> lCheckFunc =
                s => !s.StartsWith("{\"error\":0") ? new ProxerResult(new Exception[0]) : new ProxerResult();

            ProxerResult<string> lResult = await
                HttpUtility.PostResponseErrorHandling(
                    new Uri("https://proxer.me/user/my?format=json&cid=" + this.User.Id),
                    lPostArgs, this._senpai.LoginCookies, this._senpai, new[] {lCheckFunc});

            if (!lResult.Success) return new ProxerResult(lResult.Exceptions);

            this._handled = true;
            return new ProxerResult();
        }

        /// <summary>
        ///     Denies the friend request.
        /// </summary>
        /// <returns>If the action was successful.</returns>
        [ItemNotNull]
        public async Task<ProxerResult> DenyRequest()
        {
            if (this._handled) return new ProxerResult {Success = false};

            Dictionary<string, string> lPostArgs = new Dictionary<string, string> {{"type", "deny"}};

            Func<string, ProxerResult> lCheckFunc =
                s => !s.StartsWith("{\"error\":0") ? new ProxerResult(new Exception[0]) : new ProxerResult();

            ProxerResult<string> lResult = await
                HttpUtility.PostResponseErrorHandling(
                    new Uri("https://proxer.me/user/my?format=json&cid=" + this.User.Id),
                    lPostArgs, this._senpai.LoginCookies, this._senpai, new[] {lCheckFunc});

            if (!lResult.Success) return new ProxerResult(lResult.Exceptions);

            this._handled = true;
            return new ProxerResult();
        }

        #endregion
    }
}