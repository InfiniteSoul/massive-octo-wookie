﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Proxer.API.Exceptions;
using Proxer.API.Utilities;
using Proxer.API.Utilities.Net;
using RestSharp;

namespace Proxer.API
{
    /// <summary>
    ///     Repräsentiert einen Proxer-Benutzer
    /// </summary>
    public class User
    {
        /// <summary>
        ///     Representiert das System.
        /// </summary>
        public static User System = new User("System", -1, new Senpai());

        private readonly Senpai _senpai;
        private Uri _avatar;
        private List<User> _freunde;
        private string _info;
        private string _rang;
        private string _status;
        private string userName;

        internal User(string name, int userId, Senpai senpai)
        {
            this._senpai = senpai;
            this.IstInitialisiert = false;

            this.UserName = name;
            this.Id = userId;
            this.Avatar =
                new Uri(
                    "https://proxer.me/components/com_comprofiler/plugin/templates/default/images/avatar/nophoto_n.png");
        }

        internal User(string name, int userId, Uri avatar, Senpai senpai)
        {
            this._senpai = senpai;
            this.IstInitialisiert = false;

            this.UserName = name;
            this.Id = userId;
            if (avatar != null) this.Avatar = avatar;
            else
                this.Avatar =
                    new Uri(
                        "https://proxer.me/components/com_comprofiler/plugin/templates/default/images/avatar/nophoto_n.png");
        }

        /// <summary>
        ///     Initialisiert die Klasse mit allen Standardeinstellungen.
        /// </summary>
        /// <param name="userId">Die ID des Benutzers</param>
        /// <param name="senpai">Wird benötigt um einige Eigenschaften abzurufen</param>
        public User(int userId, Senpai senpai)
        {
            this._senpai = senpai;
            this.IstInitialisiert = false;

            this.Id = userId;
            this.Avatar =
                new Uri(
                    "https://proxer.me/components/com_comprofiler/plugin/templates/default/images/avatar/nophoto_n.png");
        }

        #region Properties

        /// <summary>
        ///     Gibt den Link zu dem Avatar des Benutzers zurück.
        ///     <para />
        ///     <para>Diese Eigenschaft muss durch <see cref="Init" /> initialisiert werden.</para>
        /// </summary>
        /// <seealso cref="Init" />
        public Uri Avatar
        {
            get
            {
                return this._avatar ??
                       new Uri(
                           "https://proxer.me/components/com_comprofiler/plugin/templates/default/images/avatar/nophoto_n.png");
            }
            private set { this._avatar = value; }
        }

        /// <summary>
        ///     Gibt die Freunde des Benutzers in einer Liste zurück.
        ///     <para />
        ///     <para>Diese Eigenschaft muss durch <see cref="Init" /> initialisiert werden.</para>
        /// </summary>
        /// <seealso cref="Init" />
        public List<User> Freunde
        {
            get { return this._freunde ?? new List<User>(); }
            private set { this._freunde = value; }
        }

        /// <summary>
        ///     Gibt die ID des Benutzers zurück.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Gibt die Info des Benutzers als Html-Dokument zurück.
        ///     <para />
        ///     <para>Diese Eigenschaft muss durch <see cref="Init" /> initialisiert werden.</para>
        /// </summary>
        /// <seealso cref="Init" />
        public string Info
        {
            get { return this._info ?? ""; }
            private set { this._info = value; }
        }

        /// <summary>
        ///     Gibt an, ob das Objekt bereits Initialisiert ist.
        /// </summary>
        public bool IstInitialisiert { get; private set; }

        /// <summary>
        ///     Gibt zurück, ob der Benutzter zur Zeit online ist.
        ///     <para />
        ///     <para>Diese Eigenschaft muss durch <see cref="Init" /> initialisiert werden.</para>
        /// </summary>
        /// <seealso cref="Init" />
        public bool Online { get; private set; }

        /// <summary>
        ///     Gibt zurück, wie viele Punkte der Benutzter momentan hat.
        ///     <para />
        ///     <para>Diese Eigenschaft muss durch <see cref="Init" /> initialisiert werden.</para>
        /// </summary>
        /// <seealso cref="Init" />
        public int Punkte { get; private set; }

        /// <summary>
        ///     Gibt den Rangnamen des Benutzers zurück.
        ///     <para />
        ///     <para>Diese Eigenschaft muss durch <see cref="Init" /> initialisiert werden.</para>
        /// </summary>
        /// <seealso cref="Init" />
        public string Rang
        {
            get { return this._rang ?? ""; }
            private set { this._rang = value; }
        }

        /// <summary>
        ///     Gibt den Status des Benutzers zurück.
        ///     <para />
        ///     <para>Diese Eigenschaft muss durch <see cref="Init" /> initialisiert werden.</para>
        /// </summary>
        /// <seealso cref="Init" />
        public string Status
        {
            get { return this._status ?? ""; }
            private set { this._status = value; }
        }

        /// <summary>
        ///     Gibt den Benutzernamen des Benutzers zurück.
        /// </summary>
        public string UserName
        {
            get { return this.userName ?? ""; }
            private set { this.userName = value; }
        }

        #endregion

        #region

        /// <summary>
        ///     Initialisiert die Eigenschaften der Klasse
        ///     <para>Mögliche Fehler, die <see cref="ProxerResult" /> enthalten kann:</para>
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Ausnahme</term>
        ///             <description>Beschreibung</description>
        ///         </listheader>
        ///         <item>
        ///             <term>
        ///                 <see cref="WrongResponseException" />
        ///             </term>
        ///             <description>Wird ausgelöst, wenn die Antwort des Servers nicht der Erwarteten entspricht.</description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 <see cref="NoAccessException" />
        ///             </term>
        ///             <description>
        ///                 Wird ausgelöst, wenn Teile der Initialisierung nicht durchgeführt werden können,
        ///                 da der <see cref="Senpai">Benutzer</see> nicht die nötigen Rechte dafür hat.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <seealso cref="Senpai.Login" />
        public async Task<ProxerResult> Init()
        {
            int lFailedInits = 0;
            ProxerResult lReturn = new ProxerResult();

            ProxerResult lResult;
            if (!(lResult = await this.GetMainInfo()).Success)
            {
                lReturn.AddExceptions(lResult.Exceptions);
                lFailedInits++;
            }

            if (!(lResult = await this.GetFriends()).Success)
            {
                lReturn.AddExceptions(lResult.Exceptions);
                lFailedInits++;
            }

            if (!(lResult = await this.GetInfos()).Success)
            {
                lReturn.AddExceptions(lResult.Exceptions);
                lFailedInits++;
            }

            this.IstInitialisiert = true;

            if (lFailedInits < 3)
                lReturn.Success = true;

            return lReturn;
        }


        private async Task<ProxerResult> GetMainInfo()
        {
            if (this.Id != -1)
            {
                HtmlDocument lDocument = new HtmlDocument();
                string lResponse;

                IRestResponse lResponseObject =
                    await
                        HttpUtility.GetWebRequestResponse("https://proxer.me/user/" + this.Id + "/overview?format=raw",
                            this._senpai.LoginCookies);
                if (lResponseObject.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(lResponseObject.Content))
                    lResponse = global::System.Web.HttpUtility.HtmlDecode(lResponseObject.Content).Replace("\n", "");
                else return new ProxerResult(new[] {new WrongResponseException(), lResponseObject.ErrorException});

                if (!string.IsNullOrEmpty(lResponse) &&
                    lResponse.Equals(
                        "<div class=\"inner\">\n<h3>Du hast keine Berechtigung um diese Seite zu betreten.</h3>\n</div>"))
                    return new ProxerResult(new Exception[] {new NoAccessException(nameof(this.GetMainInfo))});

                if (string.IsNullOrEmpty(lResponse) ||
                    !Utility.CheckForCorrectResponse(lResponse, this._senpai.ErrHandler))
                    return new ProxerResult(new Exception[] {new WrongResponseException {Response = lResponse}});

                try
                {
                    lDocument.LoadHtml(lResponse);

                    HtmlNodeCollection lProfileNodes =
                        lDocument.DocumentNode.SelectNodes("//table[@class='profile']");

                    this.Avatar =
                        new Uri("https://proxer.me" +
                                lProfileNodes[0].ParentNode.ParentNode.ChildNodes[1].ChildNodes[0]
                                    .Attributes["src"].Value);
                    this.Punkte =
                        Convert.ToInt32(
                            Utility.GetTagContents(lProfileNodes[0].FirstChild.InnerText, "Summe: ", " - ")[
                                0]);
                    this.Rang =
                        Utility.GetTagContents(lProfileNodes[0].FirstChild.InnerText, this.Punkte + " - ",
                            "[?]")
                            [0];
                    this.Online = lProfileNodes[0].ChildNodes[1].InnerText.Equals("Status Online");
                    this.Status = lProfileNodes[0].ChildNodes.Count == 7
                        ? lProfileNodes[0].ChildNodes[6].InnerText
                        : "";

                    this.UserName =
                        lDocument.DocumentNode.SelectNodes("//div[@id='pageMetaAjax']")[0].InnerText
                                                                                          .Split(' ')[1];

                    return new ProxerResult();
                }
                catch
                {
                    return new ProxerResult((await ErrorHandler.HandleError(this._senpai, lResponse, false)).Exceptions);
                }
            }
            this.Status = "";
            this.Online = false;
            this.Rang = "";
            this.Punkte = -1;
            this.Avatar =
                new Uri(
                    "https://proxer.me/components/com_comprofiler/plugin/templates/default/images/avatar/nophoto_n.png");

            return new ProxerResult();
        }

        private async Task<ProxerResult> GetFriends()
        {
            if (this.Id != -1)
            {
                int lSeite = 1;
                IRestResponse lResponseObject;
                string lResponse;
                HtmlDocument lDocument = new HtmlDocument();

                this.Freunde = new List<User>();

                while (
                    (lResponseObject =
                        (await HttpUtility.GetWebRequestResponse(
                            "https://proxer.me/user/" + this.Id + "/connections/" + lSeite + "?format=raw",
                            this._senpai.LoginCookies))).StatusCode == HttpStatusCode.OK &&
                    !(lResponse = global::System.Web.HttpUtility.HtmlDecode(lResponseObject.Content)).Replace("\n", "")
                                                                                                     .Contains(
                                                                                                         "Dieser Benutzer hat bisher keine Freunde"))
                {
                    if (!string.IsNullOrEmpty(lResponse) &&
                        lResponse.Equals(
                            "<div class=\"inner\">\n<h3>Du hast keine Berechtigung um diese Seite zu betreten.</h3>\n</div>"))
                        return new ProxerResult(new Exception[] {new NoAccessException(nameof(this.GetFriends))});

                    if (string.IsNullOrEmpty(lResponse) ||
                        !Utility.CheckForCorrectResponse(lResponse, this._senpai.ErrHandler))
                        return new ProxerResult(new Exception[] {new WrongResponseException {Response = lResponse}});

                    try
                    {
                        lDocument.LoadHtml(lResponse);

                        HtmlNodeCollection lProfileNodes =
                            lDocument.DocumentNode.SelectNodes("//table[@id='box-table-a']");

                        if (lProfileNodes != null)
                        {
                            lProfileNodes[0].ChildNodes.Remove(0);
                            foreach (HtmlNode curFriendNode in lProfileNodes[0].ChildNodes)
                            {
                                string lUsername = curFriendNode.ChildNodes[2].InnerText;
                                int lId =
                                    Convert.ToInt32(
                                        curFriendNode.Attributes["id"].Value.Substring("entry".Length));
                                this.Freunde.Add(new User(lUsername, lId, this._senpai));
                            }
                        }


                        lSeite++;
                    }
                    catch
                    {
                        return
                            new ProxerResult((await ErrorHandler.HandleError(this._senpai, lResponse, false)).Exceptions);
                    }
                }

                return lResponseObject.StatusCode != HttpStatusCode.OK
                    ? new ProxerResult(new Exception[] {new WebException()})
                    : new ProxerResult();
            }

            this.Freunde = new List<User>();
            return new ProxerResult();
        }

        private async Task<ProxerResult> GetInfos()
        {
            if (this.Id != -1)
            {
                HtmlDocument lDocument = new HtmlDocument();
                string lResponse;

                IRestResponse lResponseObject =
                    await
                        HttpUtility.GetWebRequestResponse("https://proxer.me/user/" + this.Id + "/about?format=raw",
                            this._senpai.LoginCookies);
                if (lResponseObject.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(lResponseObject.Content))
                    lResponse = global::System.Web.HttpUtility.HtmlDecode(lResponseObject.Content).Replace("\n", "");
                else return new ProxerResult(new[] {new WrongResponseException(), lResponseObject.ErrorException});

                if (!string.IsNullOrEmpty(lResponse) &&
                    lResponse.Equals(
                        "<div class=\"inner\">\n<h3>Du hast keine Berechtigung um diese Seite zu betreten.</h3>\n</div>"))
                    return new ProxerResult(new Exception[] {new NoAccessException(nameof(this.GetInfos))});

                if (string.IsNullOrEmpty(lResponse) ||
                    !Utility.CheckForCorrectResponse(lResponse, this._senpai.ErrHandler))
                    return new ProxerResult(new Exception[] {new WrongResponseException {Response = lResponse}});

                try
                {
                    lDocument.LoadHtml(lResponse);

                    HtmlNodeCollection lProfileNodes =
                        lDocument.DocumentNode.SelectNodes("//table[@class='profile']");

                    if (lProfileNodes != null)
                    {
                        this.Info = lProfileNodes[0].ChildNodes[10].InnerText;
                    }
                }
                catch
                {
                    return new ProxerResult((await ErrorHandler.HandleError(this._senpai, lResponse, false)).Exceptions);
                }
            }
            else
            {
                this.Info = "";
            }

            return new ProxerResult();
        }


        /*
         * -----------------------------------
         * --------Statische Methoden---------
         * -----------------------------------
        */

        /// <summary>
        ///     Überprüft, ob zwei Benutzter Freunde sind.
        ///     <para>Mögliche Fehler, die <see cref="ProxerResult" /> enthalten kann:</para>
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Ausnahme</term>
        ///             <description>Beschreibung</description>
        ///         </listheader>
        ///         <item>
        ///             <term>
        ///                 <see cref="InitializeNeededException" />
        ///             </term>
        ///             <description>Wird ausgelöst, wenn die Eigenschaften des Objektes noch nicht initialisiert sind.</description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 <see cref="ArgumentNullException" />
        ///             </term>
        ///             <description>
        ///                 Wird ausgelöst, wenn <paramref name="user1" /> oder <paramref name="user2" /> null (oder
        ///                 Nothing in Visual Basic) sind.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="user1">Benutzer 1</param>
        /// <param name="user2">Benutzer 2</param>
        /// <returns>Benutzer sind Freunde. True oder False.</returns>
        public static ProxerResult<bool> IsUserFriendOf(User user1, User user2)
        {
            if (user1 == null)
                return new ProxerResult<bool>(new Exception[] {new ArgumentNullException(nameof(user1))});

            return user2 == null
                ? new ProxerResult<bool>(new Exception[] {new ArgumentNullException(nameof(user2))})
                : new ProxerResult<bool>(user1.Freunde.Any(item => item.Id == user2.Id));
        }

        /// <summary>
        ///     Gibt den Benutzernamen eines Benutzers mit der spezifizierten ID zurück.
        ///     <para>Mögliche Fehler, die <see cref="ProxerResult" /> enthalten kann:</para>
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Ausnahme</term>
        ///             <description>Beschreibung</description>
        ///         </listheader>
        ///         <item>
        ///             <term>
        ///                 <see cref="NotLoggedInException" />
        ///             </term>
        ///             <description>Wird ausgelöst, wenn der <see cref="Senpai">Benutzer</see> nicht eingeloggt ist.</description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 <see cref="WrongResponseException" />
        ///             </term>
        ///             <description>Wird ausgelöst, wenn die Antwort des Servers nicht der Erwarteten entspricht.</description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 <see cref="ArgumentNullException" />
        ///             </term>
        ///             <description>Wird ausgelöst, wenn <paramref name="senpai" /> null (oder Nothing in Visual Basic) ist.</description>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="id">Die ID des Benutzers</param>
        /// <param name="senpai">Login-Cookies werden benötigt</param>
        /// <seealso cref="Senpai.Login" />
        /// <returns></returns>
        public static async Task<ProxerResult<string>> GetUNameFromId(int id, Senpai senpai)
        {
            if (senpai == null)
                return new ProxerResult<string>(new Exception[] {new ArgumentNullException(nameof(senpai))});

            if (!senpai.LoggedIn) return new ProxerResult<string>(new Exception[] {new NotLoggedInException()});

            HtmlDocument lDocument = new HtmlDocument();
            string lResponse;

            IRestResponse lResponseObject =
                await
                    HttpUtility.GetWebRequestResponse("https://proxer.me/user/" + id + "/overview?format=raw",
                        senpai.LoginCookies);
            if (lResponseObject.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(lResponseObject.Content))
                lResponse = global::System.Web.HttpUtility.HtmlDecode(lResponseObject.Content).Replace("\n", "");
            else return new ProxerResult<string>(new[] {new WrongResponseException(), lResponseObject.ErrorException});

            if (string.IsNullOrEmpty(lResponse) || !Utility.CheckForCorrectResponse(lResponse, senpai.ErrHandler))
                return new ProxerResult<string>(new Exception[] {new WrongResponseException {Response = lResponse}});

            try
            {
                lDocument.LoadHtml(lResponse);
                HtmlNodeCollection lNodes = lDocument.DocumentNode.SelectNodes("//div[@id='pageMetaAjax']");
                return new ProxerResult<string>(lNodes[0].InnerText.Split(' ')[1]);
            }
            catch
            {
                return new ProxerResult<string>((await ErrorHandler.HandleError(senpai, lResponse, false)).Exceptions);
            }
        }

        #endregion
    }
}