﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Proxer.API.Community;
using Proxer.API.EventArguments;
using Proxer.API.Exceptions;
using Proxer.API.Notifications;
using Proxer.API.Utilities;

namespace Proxer.API
{
    /// <summary>
    ///     Der Benutzer der Anwendung an sich
    /// </summary>
    public class Senpai
    {
        /// <summary>
        ///     Stellt die Methode da, die ausgelöst wird, wenn neue Anime- oder Manga-Benachrichtigungen verfügbar sind.
        /// </summary>
        /// <param name="sender">Der Benutzer, der die Benachrichtigung empfangen hat.</param>
        /// <param name="e">Die Benachrichtigungen</param>
        public delegate void AmNotificationEventHandler(Senpai sender, AmNotificationEventArgs e);

        /// <summary>
        ///     Stellt die Methode da, die ausgelöst wird, wenn neue Freundschafts-Benachrichtigungen verfügbar sind.
        /// </summary>
        /// <param name="sender">Der Benutzer, der die Benachrichtigung empfangen hat.</param>
        /// <param name="e">Die Benachrichtigungen</param>
        public delegate void FriendNotificiationEventHandler(Senpai sender, FriendNotificationEventArgs e);

        /// <summary>
        ///     Stellt die Methode da, die ausgelöst wird, wenn neue News verfügbar sind.
        /// </summary>
        /// <param name="sender">Der Benutzer, der die Benachrichtigung empfangen hat.</param>
        /// <param name="e">Die Benachrichtigungen</param>
        public delegate void NewsNotificationEventHandler(Senpai sender, NewsNotificationEventArgs e);

        /// <summary>
        ///     Stellt die Methode da, die ausgelöst wird, wenn neue Benachrichtigungen aller Art verfügbar sind.
        /// </summary>
        /// <param name="sender">Der Benutzer, der die Benachrichtigung empfangen hat.</param>
        /// <param name="e">Die Benachrichtigungen</param>
        public delegate void NotificationEventHandler(Senpai sender, INotificationEventArgs e);

        /// <summary>
        ///     Stellt die Methode da, die ausgelöst wird, wenn neue Privat-Nachricht-Benachrichtigungen verfügbar sind.
        /// </summary>
        /// <param name="sender">Der Benutzer, der die Benachrichtigung empfangen hat.</param>
        /// <param name="e">Die Benachrichtigungen</param>
        public delegate void PmNotificationEventHandler(Senpai sender, PmNotificationEventArgs e);

        private readonly Timer _loginCheckTimer;
        private readonly Timer _notificationCheckTimer;
        private readonly Timer _propertyUpdateTimer;
        private AnimeMangaUpdateCollection _animeMangaUpdates;
        private bool _checkAnimeMangaUpdate;
        private bool _checkFriendUpdates;
        private bool _checkNewsUpdate;
        private bool _checkPmUpdate;
        private FriendRequestCollection _friendUpdates;
        private bool _loggedIn;
        private NewsCollection _newsUpdates;
        private PmCollection _pmUpdates;
        private int _userId;


        /// <summary>
        ///     Standard-Konstruktor der Klasse.
        /// </summary>
        public Senpai()
        {
            this.ErrHandler = new ErrorHandler();

            this._loggedIn = false;
            this.LoginCookies = new CookieContainer();

            this._loginCheckTimer = new Timer
            {
                AutoReset = true,
                Interval = (new TimeSpan(0, 45, 0)).TotalMilliseconds
            };
            this._loginCheckTimer.Elapsed += async (s, eArgs) =>
            {
                this._loginCheckTimer.Interval = (new TimeSpan(0, 30, 0)).TotalMilliseconds;
                await this.CheckLogin();
            };

            this._notificationCheckTimer = new Timer
            {
                AutoReset = true,
                Interval = (new TimeSpan(0, 15, 0)).TotalMilliseconds
            };
            this._notificationCheckTimer.Elapsed += async (s, eArgs) => { await this.CheckNotifications(); };

            this._propertyUpdateTimer = new Timer(1) {AutoReset = true};
            this._propertyUpdateTimer.Elapsed += (s, eArgs) =>
            {
                this._propertyUpdateTimer.Interval = (new TimeSpan(0, 15, 0)).TotalMilliseconds;
                this._checkAnimeMangaUpdate = true;
                this._checkFriendUpdates = true;
                this._checkNewsUpdate = true;
                this._checkPmUpdate = true;
            };
        }

        #region Properties

        /// <summary>
        ///     Gibt ein Objekt zurück, mithilfe dessen alle Anime- und Manga-Benachrichtigungen abgerufen werden könne.
        /// </summary>
        /// <seealso cref="AnimeMangaUpdates" />
        /// <seealso cref="FriendRequests" />
        /// <seealso cref="News" />
        /// <seealso cref="PrivateMessages" />
        public AnimeMangaUpdateCollection AnimeMangaUpdates
        {
            get
            {
                if (!this._checkAnimeMangaUpdate && this._animeMangaUpdates != null) return this._animeMangaUpdates;

                this._animeMangaUpdates = new AnimeMangaUpdateCollection(this);
                this._checkAnimeMangaUpdate = false;

                return this._animeMangaUpdates;
            }
        }

        /// <summary>
        ///     Gibt den Error-Handler zurück, der benutzt wird, um Fehler in Serverantworten zu bearbeiten und frühzeitig zu
        ///     erkennen.
        /// </summary>
        public ErrorHandler ErrHandler { get; private set; }

        /// <summary>
        ///     Gibt ein Objekt zurück, mithilfe dessen alle Freundschafts-Benachrichtigungen abgerufen werden könne.
        /// </summary>
        /// <seealso cref="AnimeMangaUpdates" />
        /// <seealso cref="FriendRequests" />
        /// <seealso cref="News" />
        /// <seealso cref="PrivateMessages" />
        public FriendRequestCollection FriendRequests
        {
            get
            {
                if (!this._checkFriendUpdates && this._friendUpdates != null) return this._friendUpdates;

                this._friendUpdates = new FriendRequestCollection(this);
                this._checkFriendUpdates = false;

                return this._friendUpdates;
            }
        }


        /// <summary>
        ///     Gibt an, ob der Benutzter noch eingeloggt ist, wird aber nicht überprüft. (nur durch Timer alle 30 Minuten)
        /// </summary>
        public bool LoggedIn
        {
            get { return this._loggedIn; }
            private set
            {
                if (value)
                {
                    this._loginCheckTimer.Start();
                    this._loggedIn = true;
                }
                else
                {
                    this._loginCheckTimer.Stop();
                    this._notificationCheckTimer.Stop();
                    this._propertyUpdateTimer.Stop();
                    this._loggedIn = false;
                }
            }
        }

        /// <summary>
        ///     Gibt den CookieContainer zurück, der benutzt wird, um Aktionen im eingeloggten Status auszuführen.
        /// </summary>
        /// <seealso cref="MobileLoginCookies" />
        public CookieContainer LoginCookies { get; private set; }

        /// <summary>
        ///     Profil des Senpais.
        /// </summary>
        public User Me { get; private set; }

        /// <summary>
        ///     Gibt den CookieContainer zurück, der benutzt wird, um Aktionen im eingeloggten Status auszuführen.
        ///     Jedoch wird hierbei dem CookieContainer noch Cookies hinzugefügt, sodass die mobile Seite angezeigt wird.
        /// </summary>
        /// <seealso cref="LoginCookies" />
        public CookieContainer MobileLoginCookies
        {
            get
            {
                if (this.LoginCookies == null) return null;
                CookieContainer lMobileCookies = new CookieContainer();
                foreach (Cookie loginCookie in this.LoginCookies.GetCookies(new Uri("https://proxer.me/")))
                {
                    lMobileCookies.Add(loginCookie);
                }
                lMobileCookies.Add(new Cookie("device", "mobile", "/", "proxer.me"));
                return lMobileCookies;
            }
        }

        /// <summary>
        ///     Gibt ein Objekt zurück, mithilfe dessen alle News abgerufen werden könne.
        /// </summary>
        /// <seealso cref="AnimeMangaUpdates" />
        /// <seealso cref="FriendRequests" />
        /// <seealso cref="News" />
        /// <seealso cref="PrivateMessages" />
        public NewsCollection News
        {
            get
            {
                if (!this._checkNewsUpdate && this._newsUpdates != null) return this._newsUpdates;

                this._newsUpdates = new NewsCollection(this);
                this._checkNewsUpdate = false;

                return this._newsUpdates;
            }
        }

        /// <summary>
        ///     Gibt ein Objekt zurück, mithilfe dessen alle Privat-Nachricht-Benachrichtigungen abgerufen werden könne.
        /// </summary>
        /// <seealso cref="AnimeMangaUpdates" />
        /// <seealso cref="FriendRequests" />
        /// <seealso cref="News" />
        /// <seealso cref="PrivateMessages" />
        public PmCollection PrivateMessages
        {
            get
            {
                if (!this._checkPmUpdate && this._pmUpdates != null) return this._pmUpdates;

                this._pmUpdates = new PmCollection(this);
                this._checkPmUpdate = false;

                return this._pmUpdates;
            }
        }

        #endregion

        #region

        /// <summary>
        ///     Loggt den Benutzer ein.
        /// </summary>
        /// <param name="username">Der Benutzername des einzuloggenden Benutzers</param>
        /// <param name="password">Das Passwort des Benutzers</param>
        /// <returns>Gibt zurück, ob der Benutzer erfolgreich eingeloggt wurde.</returns>
        public async Task<bool> Login(string username, string password)
        {
            if (this.LoggedIn || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return false;

            Dictionary<string, string> postArgs = new Dictionary<string, string>
            {
                {"username", username},
                {"password", password}
            };
            string lResponse =
                await HttpUtility.PostWebRequestResponse("https://proxer.me/login?format=json&action=login",
                    this.LoginCookies, postArgs);

            if (!Utility.CheckForCorrectResponse(lResponse, this.ErrHandler)) return false;
            try
            {
                Dictionary<string, string> responseDes =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(lResponse);

                if (responseDes["error"].Equals("0"))
                {
                    this._userId = Convert.ToInt32(responseDes["uid"]);

                    //Avatar einfügen
                    this.Me = new User(username, this._userId, this);
                    this.LoggedIn = true;

                    return true;
                }
                this.LoggedIn = false;

                return false;
            }
            catch (Exception)
            {
                this.ErrHandler.Add(lResponse);
            }

            return false;
        }

        internal async Task<bool> CheckLogin()
        {
            if (!this.LoggedIn) return false;
            string lResponse = await HttpUtility.GetWebRequestResponse(
                "https://proxer.me/login?format=json&action=login", this.LoginCookies);

            if (!Utility.CheckForCorrectResponse(lResponse, this.ErrHandler)) return false;
            try
            {
                Dictionary<string, string> responseDes =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(lResponse);

                if (responseDes["error"].Equals("0"))
                {
                    this.LoggedIn = true;
                    return true;
                }
                this.LoggedIn = false;
                if (this.UserLoggedOutRaised != null) this.UserLoggedOutRaised(this, new EventArgs());
                return false;
            }
            catch (Exception)
            {
                this.ErrHandler.Add(lResponse);
            }

            return false;
        }

        /// <summary>
        ///     Initialisiert die Benachrichtigungen.
        /// </summary>
        /// <exception cref="NotLoggedInException">Wird ausgelöst, wenn der Benutzer noch nicht eingeloggt ist.</exception>
        /// <seealso cref="Login" />
        public async Task InitNotifications()
        {
            if (!this.LoggedIn) throw new NotLoggedInException();
            await this.CheckNotifications();

            this._notificationCheckTimer.Start();
            this._propertyUpdateTimer.Start();
        }

        /// <summary>
        ///     Zwingt die Eigenschaften sich beim nächsten Aufruf zu aktualisieren.
        /// </summary>
        public async Task ForcePropertyReload()
        {
            await this.CheckLogin();
            this._checkAnimeMangaUpdate = true;
            this._checkNewsUpdate = true;
            this._checkPmUpdate = true;
        }

        /// <summary>
        ///     Gibt alle Konferenzen des Senpais zurück. ACHTUNG: Bei den Konferenzen muss noch
        ///     <see cref="Conference.InitConference">InitConference()</see>
        ///     aufgerufen
        ///     werden!
        /// </summary>
        /// <exception cref="NotLoggedInException">Wird ausgelöst, wenn der Benutzer noch nicht eingeloggt ist.</exception>
        /// <seealso cref="Login" />
        /// <returns>Alle Konferenzen, in denen der Benutzer Teilnehmer ist.</returns>
        public async Task<List<Conference>> GetAllConferences()
        {
            if (!this.LoggedIn) throw new NotLoggedInException();
            string lResponse =
                (await HttpUtility.GetWebRequestResponse("http://proxer.me/messages", this.LoginCookies))
                    .Replace("</link>", "")
                    .Replace("\n", "");

            if (!Utility.CheckForCorrectResponse(lResponse, this.ErrHandler)) return null;
            try
            {
                HtmlDocument lDocument = new HtmlDocument();
                lDocument.LoadHtml(lResponse);

                if (!lDocument.ParseErrors.Any())
                {
                    List<Conference> lReturn = new List<Conference>();

                    HtmlNodeCollection lNodes =
                        lDocument.DocumentNode.SelectNodes("//a[@class='conferenceGrid ']");
                    if (lNodes != null)
                    {
                        lReturn.AddRange(from curNode in lNodes
                            let lId =
                                Convert.ToInt32(
                                    Utility.GetTagContents(curNode.Attributes["href"].Value, "/messages?id=",
                                        "#top")[0])
                            let lTitle = curNode.FirstChild.InnerText
                            select new Conference(lTitle, lId, this));
                    }

                    return lReturn;
                }
            }
            catch (Exception)
            {
                this.ErrHandler.Add(lResponse);
            }

            return null;
        }


        private async Task CheckNotifications()
        {
            if (!this.LoggedIn) return;
            string lResponse =
                await HttpUtility.GetWebRequestResponse("https://proxer.me/notifications?format=raw&s=count",
                    this.LoginCookies);

            if (!lResponse.StartsWith("0")) return;
            string[] lResponseSplit = lResponse.Split('#');

            if (!lResponseSplit[2].Equals("0"))
            {
                if (this.PmNotificationRaised != null)
                    this.PmNotificationRaised(this,
                        new PmNotificationEventArgs(Convert.ToInt32(lResponseSplit[2]), this));
                if (this.NotificationRaised != null)
                    this.NotificationRaised(this,
                        new PmNotificationEventArgs(Convert.ToInt32(lResponseSplit[2]), this));
                this._checkPmUpdate = true;
            }
            if (!lResponseSplit[3].Equals("0"))
            {
                if (this.FriendNotificationRaised != null)
                    this.FriendNotificationRaised(this,
                        new FriendNotificationEventArgs(Convert.ToInt32(lResponseSplit[3]), this));
                if (this.NotificationRaised != null)
                    this.NotificationRaised(this,
                        new FriendNotificationEventArgs(Convert.ToInt32(lResponseSplit[3]), this));
                this._checkFriendUpdates = true;
            }
            if (!lResponseSplit[4].Equals("0"))
            {
                if (this.NewsNotificationRaised != null)
                    this.NewsNotificationRaised(this,
                        new NewsNotificationEventArgs(Convert.ToInt32(lResponseSplit[4]), this));
                if (this.NotificationRaised != null)
                    this.NotificationRaised(this,
                        new NewsNotificationEventArgs(Convert.ToInt32(lResponseSplit[4]), this));
                this._checkNewsUpdate = true;
            }
            if (lResponseSplit[5].Equals("0")) return;
            if (this.AmUpdateNotificationRaised != null)
                this.AmUpdateNotificationRaised(this,
                    new AmNotificationEventArgs(Convert.ToInt32(lResponseSplit[5]), this));
            if (this.NotificationRaised != null)
                this.NotificationRaised(this,
                    new AmNotificationEventArgs(Convert.ToInt32(lResponseSplit[5]), this));
            this._checkAnimeMangaUpdate = true;
        }


        /// <summary>
        ///     Wird bei allen Benachrichtigungen ausgelöst. (15 Minuten Intervall)
        /// </summary>
        public event NotificationEventHandler NotificationRaised;

        /// <summary>
        ///     Wird ausgelöst, wenn eine neue Freundschaftsanfrage aussteht. (15 Minuten Intervall)
        /// </summary>
        public event FriendNotificiationEventHandler FriendNotificationRaised;

        /// <summary>
        ///     Wird ausgelöst, wenn neue ungelesene News vorhanden sind. (15 Minuten Intervall)
        /// </summary>
        public event NewsNotificationEventHandler NewsNotificationRaised;

        /// <summary>
        ///     Wird ausgelöst, wenn ungelesene PMs vorhanden sind. (15 Minuten Intervall)
        /// </summary>
        public event PmNotificationEventHandler PmNotificationRaised;

        /// <summary>
        ///     Wird ausgelöst, wenn neue Anime Folgen oder Manga Kapitel vorhanden sind. (15 Minuten Intervall)
        /// </summary>
        public event AmNotificationEventHandler AmUpdateNotificationRaised;

        /// <summary>
        ///     Wird ausgelöst, wenn die Login-Cookies verfallen sind. (15 Minuten Intervall)
        /// </summary>
        public event EventHandler UserLoggedOutRaised;

        #endregion
    }
}