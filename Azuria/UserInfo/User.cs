﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azuria.AnimeManga;
using Azuria.Api.v1;
using Azuria.Api.v1.DataModels.Messenger;
using Azuria.Api.v1.DataModels.User;
using Azuria.Api.v1.Enums;
using Azuria.ErrorHandling;
using Azuria.UserInfo.Comment;
using Azuria.Utilities.Properties;

namespace Azuria.UserInfo
{
    /// <summary>
    ///     Represents a user of proxer.
    /// </summary>
    public class User
    {
        /// <summary>
        ///     Represents the system as a user.
        /// </summary>
        public static User System = new User("System", -1);

        private readonly InitialisableProperty<Uri> _avatar;

        private readonly InitialisableProperty<UserPoints> _points;

        private readonly InitialisableProperty<UserStatus> _status;

        private readonly InitialisableProperty<IEnumerable<Anime>> _toptenAnime;

        private readonly InitialisableProperty<IEnumerable<Manga>> _toptenManga;

        private readonly InitialisableProperty<string> _userName;

        /// <summary>
        ///     Initialises a new instance of the class.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        public User(int userId)
        {
            this.Id = userId;

            this.Anime = new UserEntryEnumerable<Anime>(this);
            this._avatar = new InitialisableProperty<Uri>(this.InitMainInfo,
                new Uri("https://cdn.proxer.me/avatar/nophoto.png"))
            {
                IsInitialisedOnce = false
            };
            this._toptenAnime =
                new InitialisableProperty<IEnumerable<Anime>>(() => this.InitTopten(AnimeMangaEntryType.Anime));
            this.CommentsLatestAnime = new CommentEnumerable<Anime>(this);
            this.CommentsLatestManga = new CommentEnumerable<Manga>(this);
            this._toptenManga =
                new InitialisableProperty<IEnumerable<Manga>>(() => this.InitTopten(AnimeMangaEntryType.Manga));
            this.Manga = new UserEntryEnumerable<Manga>(this);
            this._points = new InitialisableProperty<UserPoints>(this.InitMainInfo);
            this._status = new InitialisableProperty<UserStatus>(this.InitMainInfo);
            this._userName = new InitialisableProperty<string>(this.InitMainInfo);
        }

        internal User(string name, int userId) : this(userId)
        {
            this._userName.SetInitialisedObject(name);
        }

        internal User(int userId, Uri avatar)
            : this(userId)
        {
            this._avatar.SetInitialisedObject(avatar ?? new Uri("https://cdn.proxer.me/avatar/nophoto.png"));
        }

        internal User(string name, int userId, Uri avatar)
            : this(name, userId)
        {
            this._avatar.SetInitialisedObject(avatar ?? new Uri("https://cdn.proxer.me/avatar/nophoto.png"));
        }

        internal User(UserInfoDataModel dataModel)
            : this(dataModel.Username, dataModel.UserId, new Uri("http://cdn.proxer.me/avatar/" + dataModel.AvatarId))
        {
            this._points.SetInitialisedObject(dataModel.Points);
            this._status.SetInitialisedObject(dataModel.Status);
        }

        internal User(ConferenceInfoParticipantDataModel dataModel)
            : this(dataModel.Username, dataModel.UserId, new Uri("http://cdn.proxer.me/avatar/" + dataModel.AvatarId))
        {
            this._status.SetInitialisedObject(new UserStatus(dataModel.UserStatus, DateTime.MinValue));
        }

        #region Properties

        /// <summary>
        ///     Gets all <see cref="AnimeManga.Anime" /> the <see cref="User" /> has in his profile.
        /// </summary>
        public IEnumerable<UserProfileEntry<Anime>> Anime { get; }

        /// <summary>
        ///     Gets the avatar of the user.
        /// </summary>
        public IInitialisableProperty<Uri> Avatar => this._avatar;

        /// <summary>
        /// </summary>
        public IEnumerable<Comment<Anime>> CommentsLatestAnime { get; }

        /// <summary>
        /// </summary>
        public IEnumerable<Comment<Manga>> CommentsLatestManga { get; }

        /// <summary>
        ///     Gets the id of the user.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Gets all <see cref="AnimeManga.Manga" /> the <see cref="User" /> has in his profile.
        /// </summary>
        public IEnumerable<UserProfileEntry<Manga>> Manga { get; }

        /// <summary>
        ///     Gets the current number of total points the user has.
        /// </summary>
        public IInitialisableProperty<UserPoints> Points => this._points;

        /// <summary>
        ///     Gets the current status of the user.
        /// </summary>
        public IInitialisableProperty<UserStatus> Status => this._status;

        /// <summary>
        ///     Gets all favourites of the user that are <see cref="AnimeManga.Anime">Anime</see>.
        /// </summary>
        public IInitialisableProperty<IEnumerable<Anime>> ToptenAnime => this._toptenAnime;

        /// <summary>
        ///     Gets the <see cref="AnimeManga.Manga">Manga</see> top ten of the user.
        /// </summary>
        public IInitialisableProperty<IEnumerable<Manga>> ToptenManga => this._toptenManga;

        /// <summary>
        ///     Gets the username of the user.
        /// </summary>
        public IInitialisableProperty<string> UserName => this._userName;

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static async Task<ProxerResult<User>> FromUsername(string username)
        {
            ProxerResult<ProxerApiResponse<UserInfoDataModel>> lResult =
                await RequestHandler.ApiRequest(ApiRequestBuilder.UserGetInfo(username));
            if (!lResult.Success || (lResult.Result == null)) return new ProxerResult<User>(lResult.Exceptions);
            return new ProxerResult<User>(new User(lResult.Result.Data));
        }

        private async Task<ProxerResult> InitMainInfo()
        {
            if (this.Id == -1) return new ProxerResult();

            ProxerResult<ProxerApiResponse<UserInfoDataModel>> lResult =
                await RequestHandler.ApiRequest(ApiRequestBuilder.UserGetInfo(this.Id));
            if (!lResult.Success || (lResult.Result == null)) return new ProxerResult(lResult.Exceptions);

            UserInfoDataModel lDataModel = lResult.Result.Data;
            this._avatar.SetInitialisedObject(new Uri("http://cdn.proxer.me/avatar/" + lDataModel.AvatarId));
            this._points.SetInitialisedObject(lDataModel.Points);
            this._status.SetInitialisedObject(lDataModel.Status);
            this._userName.SetInitialisedObject(lDataModel.Username);

            return new ProxerResult();
        }

        private async Task<ProxerResult> InitTopten(AnimeMangaEntryType category)
        {
            ProxerResult<ProxerApiResponse<ToptenDataModel[]>> lResult =
                await
                    RequestHandler.ApiRequest(ApiRequestBuilder.UserGetTopten(this.Id,
                        category.ToString().ToLower()));
            if (!lResult.Success || (lResult.Result == null)) return new ProxerResult(lResult.Exceptions);

            if (category == AnimeMangaEntryType.Anime)
                this._toptenAnime.SetInitialisedObject(from toptenDataModel in lResult.Result.Data
                    select new Anime(toptenDataModel.EntryName, toptenDataModel.EntryId));
            if (category == AnimeMangaEntryType.Manga)
                this._toptenManga.SetInitialisedObject(from toptenDataModel in lResult.Result.Data
                    select new Manga(toptenDataModel.EntryName, toptenDataModel.EntryId));

            return new ProxerResult();
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return this.UserName.GetObjectIfInitialised("ERROR") + " [" + this.Id + "]";
        }

        #endregion
    }
}