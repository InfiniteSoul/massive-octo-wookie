﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Azuria.Api;
using Azuria.Api.v1;
using Azuria.Api.v1.DataModels.Messenger;
using Azuria.Exceptions;
using Azuria.UserInfo;
using Azuria.Utilities.ErrorHandling;
using Azuria.Utilities.Extensions;
using Azuria.Utilities.Properties;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Azuria.Community
{
    /// <summary>
    ///     Represents a messaging Conference.
    /// </summary>
    [DebuggerDisplay("Conference: {Title} [{Id}]")]
    public class Conference
    {
        /// <summary>
        ///     Represent a method, which is raised when an exception is thrown during the message fetching.
        /// </summary>
        /// <param name="sender">The conference that raised the event.</param>
        /// <param name="exception">The exception thrown.</param>
        public delegate void ErrorThrownAutoMessageFetchEventHandler(Conference sender, Exception exception);

        /// <summary>
        ///     Represents a method, which is raised when new messages were recieved or once everytime Active is set to true.
        /// </summary>
        /// <param name="sender">The conference that raised the event.</param>
        /// <param name="e">
        ///     Contains the new messages.
        /// </param>
        public delegate void NewMessageRecievedEventHandler(Conference sender, IEnumerable<Message> e);

        private static int _conferencesPerPage;

        private readonly Timer _checkMessagesTimer;
        private readonly Senpai _senpai;
        private Message _autoLastMessageRecieved;

        internal Conference([NotNull] ConferenceDataModel dataModel, [NotNull] Senpai senpai)
        {
            this.Id = dataModel.ConferenceId;
            this._senpai = senpai;

            this._checkMessagesTimer = new Timer {Interval = new TimeSpan(0, 0, 15).TotalMilliseconds};
            this._checkMessagesTimer.Elapsed += this.OnCheckMessagesTimerElapsed;

            this.IsGroupConference = dataModel.IsConferenceGroup;
            this.Leader = new InitialisableProperty<User>(this.InitInfo);
            this.Participants = new InitialisableProperty<IEnumerable<User>>(this.InitInfo);
            this.Title = dataModel.ConferenceTitle;
        }

        #region Properties

        /// <summary>
        ///     Gets or sets a value indicating whether the conference is currently fetching new messages.
        /// </summary>
        public bool AutoCheck
        {
            get { return this._checkMessagesTimer.Enabled; }
            set
            {
                if (value) this._autoLastMessageRecieved = this.Messages.FirstOrDefault();
                this._checkMessagesTimer.Enabled = value;
            }
        }

        /// <summary>
        ///     Gets the Id of the conference.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// </summary>
        public bool IsGroupConference { get; }

        /// <summary>
        ///     Gets a value indicating whether the current object is fully initialised.
        /// </summary>
        public bool IsInitialized => this.IsFullyInitialised();

        /// <summary>
        ///     Gets a <see cref="User" /> that is the current leader of the conference.
        /// </summary>
        [NotNull]
        public InitialisableProperty<User> Leader { get; }

        /// <summary>
        /// </summary>
        /// <seealso cref="Init" />
        public static int MaxCharactersPerMessage { get; private set; }

        /// <summary>
        /// </summary>
        /// <seealso cref="Init" />
        public static int MaxCharactersTopic { get; private set; }

        /// <summary>
        /// </summary>
        /// <seealso cref="Init" />
        public static int MaxUsersPerConference { get; private set; }

        /// <summary>
        ///     Gets all messages of the current conference ordered by newest first.
        /// </summary>
        [NotNull]
        public IEnumerable<Message> Messages => new MessageCollection(this.Id, this._senpai);

        internal static int MessagesPerPage { get; private set; }

        /// <summary>
        ///     Gets all participants of the current conference.
        /// </summary>
        [NotNull]
        public InitialisableProperty<IEnumerable<User>> Participants { get; }

        /// <summary>
        ///     Gets the current title of the current conference.
        /// </summary>
        [NotNull]
        public string Title { get; private set; }

        #endregion

        #region

        private async void CheckForNewMessages()
        {
            if (this._autoLastMessageRecieved == null) return;

            Message[] lNewMessages = {};
            try
            {
                lNewMessages =
                    this.Messages.TakeWhile(message => message.MessageId != this._autoLastMessageRecieved.MessageId)
                        .ToArray();
            }
            catch (Exception ex)
            {
                this.ErrorThrownAutoMessageFetch?.Invoke(this, ex);
            }
            if (lNewMessages.Length == 0) return;
            if (lNewMessages.Any(message => message.Action != MessageAction.NoAction)) await this.InitInfo();
            this.NewMessageRecieved?.Invoke(this, lNewMessages);
        }

        /// <summary>
        /// </summary>
        public event ErrorThrownAutoMessageFetchEventHandler ErrorThrownAutoMessageFetch;

        /// <summary>
        /// </summary>
        /// <param name="senpai"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static async Task<ProxerResult<IEnumerable<ConferenceInfo>>> GetConferences(Senpai senpai,
            ConferenceListType type = ConferenceListType.Default)
        {
            if (_conferencesPerPage == default(int))
                return
                    new ProxerResult<IEnumerable<ConferenceInfo>>(new[]
                        {new NotInitialisedException("Please call " + nameof(Init))});

            List<ConferenceInfo> lConferences = new List<ConferenceInfo>();
            for (int page = 0; (page == 0) || (lConferences.Count%_conferencesPerPage == 0); page++)
            {
                ProxerResult<ProxerApiResponse<ConferenceDataModel[]>> lResult =
                    await RequestHandler.ApiRequest(ApiRequestBuilder.MessengerGetConferences(type, page, senpai));
                if (!lResult.Success || (lResult.Result == null))
                    return new ProxerResult<IEnumerable<ConferenceInfo>>(lResult.Exceptions);
                lConferences.AddRange(from conferenceDataModel in lResult.Result.Data
                    select new ConferenceInfo(conferenceDataModel, senpai));
            }
            return new ProxerResult<IEnumerable<ConferenceInfo>>(lConferences);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static async Task<ProxerResult> Init()
        {
            ProxerResult<ProxerApiResponse<ConstantsDataModel>> lResult =
                await RequestHandler.ApiRequest(ApiRequestBuilder.MessengerGetConstants());
            if (!lResult.Success || (lResult.Result == null)) return new ProxerResult(lResult.Exceptions);
            ConstantsDataModel lData = lResult.Result.Data;

            MaxCharactersPerMessage = lData.MaxCharactersPerMessage;
            MaxUsersPerConference = lData.MaxUsersPerConference;
            MaxCharactersTopic = lData.MaxCharactersTopic;
            MessagesPerPage = lData.MessagesPerPage;
            _conferencesPerPage = lData.ConferencesPerPage;
            return new ProxerResult();
        }

        private async Task<ProxerResult> InitInfo()
        {
            ProxerResult<ProxerApiResponse<ConferenceInfoDataModel>> lResult =
                await RequestHandler.ApiRequest(ApiRequestBuilder.MessengerGetConferenceInfo(this.Id, this._senpai));
            if (!lResult.Success || (lResult.Result == null)) return new ProxerResult(lResult.Exceptions);
            ConferenceInfoDataModel lData = lResult.Result.Data;

            this.Leader.SetInitialisedObject(new User(lData.MainInfo.LeaderUserId));
            this.Participants.SetInitialisedObject(from conferenceInfoParticipantDataModel in lData.ParticipantsInfo
                select new User(conferenceInfoParticipantDataModel));
            this.Title = lData.MainInfo.Title;

            return new ProxerResult();
        }

        /// <summary>
        ///     Occurs when new messages were recieved or once everytime Active is set to true.
        /// </summary>
        public event NewMessageRecievedEventHandler NewMessageRecieved;

        private void OnCheckMessagesTimerElapsed(object s, EventArgs eArgs)
        {
            Timer timer = s as Timer;
            timer?.Stop();
            this.CheckForNewMessages();
            timer?.Start();
        }

        /// <summary>
        ///     Sends a message to the current conference.
        /// </summary>
        /// <param name="message">The content of the message that is being send.</param>
        /// <returns>Whether the action was successfull.</returns>
        [ItemNotNull]
        public async Task<ProxerResult> SendMessage([NotNull] string message)
        {
            if (string.IsNullOrEmpty(message))
                return
                    new ProxerResult(new[]
                        {new ArgumentException("Argument is null or empty", nameof(message))});

            this._checkMessagesTimer.Stop();

            Dictionary<string, string> lPostArgs = new Dictionary<string, string>
            {
                {"message", message}
            };
            ProxerResult<string> lResult =
                await
                    ApiInfo.HttpClient.PostRequest(
                        new Uri("https://proxer.me/messages?id=" + this.Id + "&format=json&json=answer"),
                        lPostArgs,
                        this._senpai);

            if (!lResult.Success)
                return new ProxerResult(lResult.Exceptions);

            string lResponse = lResult.Result;

            try
            {
                Dictionary<string, string> lResponseJson =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(lResponse);

                if (this.AutoCheck) this.CheckForNewMessages();
                if (lResponseJson.Keys.Contains("message"))
                    return new ProxerResult();
                if (!lResponseJson.Keys.Contains("msg")) return new ProxerResult {Success = false};

                this._checkMessagesTimer.Start();
                return new ProxerResult();
            }
            catch
            {
                this._checkMessagesTimer.Start();
                return
                    new ProxerResult(ErrorHandler.HandleError(lResponse, false).Exceptions);
            }
        }

        private async Task<ProxerResult> SetBlock(bool isBlocked)
        {
            string lAction = isBlocked ? "block" : "unblock";
            ProxerResult<string> lResult =
                await
                    ApiInfo.HttpClient.GetRequest(
                        new Uri($"http://proxer.me/messages?format=json&json={lAction}&id={this.Id}"),
                        this._senpai);

            if (!lResult.Success)
                return new ProxerResult(lResult.Exceptions);

            string lResponse = lResult.Result;

            return lResponse?.StartsWith("{\"error\":0") ?? false
                ? new ProxerResult()
                : new ProxerResult {Success = false};
        }

        private async Task<ProxerResult> SetFavourite(bool isFavourite)
        {
            string lAction = isFavourite ? "favour" : "unfavour";
            ProxerResult<string> lResult =
                await
                    ApiInfo.HttpClient.GetRequest(
                        new Uri($"http://proxer.me/messages?format=json&json={lAction}&id={this.Id}"),
                        this._senpai);

            if (!lResult.Success)
                return new ProxerResult(lResult.Exceptions);

            string lResponse = lResult.Result;

            return lResponse?.StartsWith("{\"error\":0") ?? false
                ? new ProxerResult()
                : new ProxerResult {Success = false};
        }

        /// <summary>
        ///     Marks the current conference as unread.
        /// </summary>
        /// <returns>Whether the action was successfull.</returns>
        [ItemNotNull]
        public async Task<ProxerResult> SetUnread()
        {
            ProxerResult<string> lResult =
                await
                    ApiInfo.HttpClient.GetRequest(
                        new Uri("http://proxer.me/messages?format=json&json=setUnread&id=" + this.Id),
                        this._senpai);

            if (!lResult.Success)
                return new ProxerResult(lResult.Exceptions);

            string lResponse = lResult.Result;

            return lResponse?.StartsWith("{\"error\":0") ?? false
                ? new ProxerResult()
                : new ProxerResult {Success = false};
        }

        #endregion
    }
}