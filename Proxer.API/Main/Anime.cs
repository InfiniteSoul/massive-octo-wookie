﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Proxer.API.Exceptions;
using Proxer.API.Main.Minor;
using Proxer.API.Utilities;

namespace Proxer.API.Main
{
    /// <summary>
    ///     Eine Klasse, die einen <see cref="Anime" /> darstellt.
    /// </summary>
    public class Anime : IAnimeMangaObject
    {
        /// <summary>
        ///     Eine Enumeration, die die Sprache eines <see cref="Anime" /> darstellt.
        /// </summary>
        public enum Language
        {
            /// <summary>
            ///     Deutsche Untertitel
            /// </summary>
            GerSub,

            /// <summary>
            ///     Englische Untertitel
            /// </summary>
            EngSub,

            /// <summary>
            ///     Englisch vertont
            /// </summary>
            EngDub,

            /// <summary>
            ///     Deutsch vertont
            /// </summary>
            GerDub
        }

        private readonly Senpai _senpai;
        private string _beschreibung;
        private string _englischTitel;
        private int _episodenZahl;
        private Dictionary<string, Uri> _fsk;
        private string[] _genre;
        private Group[] _gruppen;
        private Industry[] _industrie;
        private string _japanTitel;
        private bool _lizensiert;
        private string[] _season;
        private Language[] _sprachen;
        private AnimeMangaStatus _status;
        private string _synonym;

        internal Anime(string name, int id, Senpai senpai)
        {
            this._senpai = senpai;
            this.ObjectType = AnimeMangaType.Anime;
            this.Name = name;
            this.Id = id;
            this.CoverUri = new Uri("http://cdn.proxer.me/cover/" + this.Id + ".jpg");

            this.IstInitialisiert = false;
        }

        #region Geerbt

        /// <summary>
        ///     Gibt die Beschreibung des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        /// <exception cref="InitializeNeededException">Wird ausgelöst, wenn das Objekt noch nicht initialisiert wurde.</exception>
        /// <seealso cref="Init" />
        public string Beschreibung
        {
            get
            {
                if (!this.IstInitialisiert) throw new InitializeNeededException();
                return this._beschreibung;
            }
            private set { this._beschreibung = value; }
        }


        /// <summary>
        ///     Gibt den Link zum Cover des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public Uri CoverUri { get; private set; }


        /// <summary>
        ///     Gibt den Englische Titel des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        /// <exception cref="InitializeNeededException">Wird ausgelöst, wenn das Objekt noch nicht initialisiert wurde.</exception>
        /// <seealso cref="Init" />
        public string EnglischTitel
        {
            get
            {
                if (!this.IstInitialisiert) throw new InitializeNeededException();
                return this._englischTitel;
            }
            private set { this._englischTitel = value; }
        }


        /// <summary>
        ///     Gibt die Links zu allen FSK-Beschränkungen des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        /// <exception cref="InitializeNeededException">Wird ausgelöst, wenn das Objekt noch nicht initialisiert wurde.</exception>
        /// <seealso cref="Init" />
        public Dictionary<string, Uri> Fsk
        {
            get
            {
                if (!this.IstInitialisiert) throw new InitializeNeededException();
                return this._fsk;
            }
            private set { this._fsk = value; }
        }


        /// <summary>
        ///     Gitb die Genres des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        /// <exception cref="InitializeNeededException">Wird ausgelöst, wenn das Objekt noch nicht initialisiert wurde.</exception>
        /// <seealso cref="Init" />
        public string[] Genre
        {
            get
            {
                if (!this.IstInitialisiert) throw new InitializeNeededException();
                return this._genre;
            }
            private set { this._genre = value; }
        }


        /// <summary>
        ///     Gibt die Gruppen zurück, die den <see cref="Anime" /> oder <see cref="Manga" /> übersetzten oder übersetzt haben.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        /// <seealso cref="Minor.Group" />
        /// <seealso cref="Init" />
        /// <exception cref="InitializeNeededException">Wird ausgelöst, wenn das Objekt noch nicht initialisiert wurde.</exception>
        public Group[] Gruppen
        {
            get
            {
                if (!this.IstInitialisiert) throw new InitializeNeededException();
                return this._gruppen;
            }
            private set { this._gruppen = value; }
        }


        /// <summary>
        ///     Gibt die ID des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public int Id { get; private set; }


        /// <summary>
        ///     Gibt die Industrie des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        /// <seealso cref="Minor.Industry" />
        /// <seealso cref="Init" />
        /// <exception cref="InitializeNeededException">Wird ausgelöst, wenn das Objekt noch nicht initialisiert wurde.</exception>
        public Industry[] Industrie
        {
            get
            {
                if (!this.IstInitialisiert) throw new InitializeNeededException();
                return this._industrie;
            }
            private set { this._industrie = value; }
        }


        /// <summary>
        ///     Gibt zurück, ob das Objekt bereits Initialisiert ist.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public bool IstInitialisiert { get; private set; }


        /// <summary>
        ///     Gibt den japanischen Titel des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        /// <exception cref="InitializeNeededException">Wird ausgelöst, wenn das Objekt noch nicht initialisiert wurde.</exception>
        /// <seealso cref="Init" />
        public string JapanTitel
        {
            get
            {
                if (!this.IstInitialisiert) throw new InitializeNeededException();
                return this._japanTitel;
            }
            private set { this._japanTitel = value; }
        }


        /// <summary>
        ///     Gibt zurück, ob der <see cref="Anime" /> oder <see cref="Manga" /> lizensiert ist.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        /// <exception cref="InitializeNeededException">Wird ausgelöst, wenn das Objekt noch nicht initialisiert wurde.</exception>
        /// <seealso cref="Init" />
        public bool Lizensiert
        {
            get
            {
                if (!this.IstInitialisiert) throw new InitializeNeededException();
                return this._lizensiert;
            }
            private set { this._lizensiert = value; }
        }


        /// <summary>
        ///     Gibt den Namen des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        ///     Gibt zurück, ob es sich um einen <see cref="Anime" /> oder <see cref="Manga" /> handelt.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        /// <seealso cref="AnimeMangaType" />
        public AnimeMangaType ObjectType { get; private set; }


        /// <summary>
        ///     Gibt die Season des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        /// <exception cref="InitializeNeededException">Wird ausgelöst, wenn das Objekt noch nicht initialisiert wurde.</exception>
        /// <seealso cref="Init" />
        public string[] Season
        {
            get
            {
                if (!this.IstInitialisiert) throw new InitializeNeededException();
                return this._season;
            }
            private set { this._season = value; }
        }


        /// <summary>
        ///     Gibt den Status des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        /// <seealso cref="AnimeMangaStatus" />
        /// <seealso cref="Init" />
        /// <exception cref="InitializeNeededException">Wird ausgelöst, wenn das Objekt noch nicht initialisiert wurde.</exception>
        public AnimeMangaStatus Status
        {
            get
            {
                if (!this.IstInitialisiert) throw new InitializeNeededException();
                return this._status;
            }
            private set { this._status = value; }
        }


        /// <summary>
        ///     Gibt das Synonym des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        /// <exception cref="InitializeNeededException">Wird ausgelöst, wenn das Objekt noch nicht initialisiert wurde.</exception>
        /// <seealso cref="Init" />
        public string Synonym
        {
            get
            {
                if (!this.IstInitialisiert) throw new InitializeNeededException();
                return this._synonym;
            }
            private set { this._synonym = value; }
        }


        /// <summary>
        ///     Initialisiert das Objekt.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        /// <exception cref="NotLoggedInException">Wird ausgelöst, wenn der Benutzer noch nicht eingeloggt ist.</exception>
        /// <seealso cref="Senpai.Login" />
        public async Task Init()
        {
            try
            {
                await this.InitMain();
                await this.InitEpisodeCount();
                await this.InitAvailableLang();

                this.IstInitialisiert = true;
            }
            catch (NotLoggedInException)
            {
                throw new NotLoggedInException();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gibt die Episodenanzahl eines <see cref="Anime" /> zurück.
        /// </summary>
        /// <exception cref="InitializeNeededException">Wird ausgelöst, wenn das Objekt noch nicht initialisiert wurde.</exception>
        /// <seealso cref="Init" />
        public int EpisodenZahl
        {
            get
            {
                if (!this.IstInitialisiert) throw new InitializeNeededException();
                return this._episodenZahl;
            }
            private set { this._episodenZahl = value; }
        }

        /// <summary>
        ///     Gibt die verfügbaren Sprachen des <see cref="Anime" /> zurück.
        /// </summary>
        /// <seealso cref="Language" />
        /// <exception cref="InitializeNeededException">Wird ausgelöst, wenn das Objekt noch nicht initialisiert wurde.</exception>
        /// <seealso cref="Init" />
        public Language[] Sprachen
        {
            get
            {
                if (!this.IstInitialisiert) throw new InitializeNeededException();
                return this._sprachen;
            }
            private set { this._sprachen = value; }
        }

        #endregion

        #region

        /// <summary>
        ///     Gibt die Episoden des <see cref="Anime" /> in einer bestimmten <see cref="Language">Sprache</see> zurück.
        /// </summary>
        /// <param name="language">Die Sprache der Episoden.</param>
        /// <seealso cref="Anime.Episode" />
        /// <returns>Einen Array mit length = <see cref="EpisodenZahl" />.</returns>
        /// <exception cref="InitializeNeededException">Wird ausgelöst, wenn das Objekt noch nicht initialisiert wurde.</exception>
        public Episode[] GetEpisodes(Language language)
        {
            if (this.Sprachen == null) throw new InitializeNeededException();

            if (!this.Sprachen.Contains(language)) return null;

            List<Episode> lEpisodes = new List<Episode>();
            for (int i = 1; i <= this.EpisodenZahl; i++)
            {
                lEpisodes.Add(new Episode(this, i, language, this._senpai));
            }

            return lEpisodes.ToArray();
        }

        private async Task InitAvailableLang()
        {
            if (!this._senpai.LoggedIn) throw new NotLoggedInException();

            HtmlDocument lDocument = new HtmlDocument();
            string lResponse =
                (await HttpUtility.GetWebRequestResponse("http://proxer.me/edit/entry/" + this.Id + "/languages",
                    this._senpai.LoginCookies))
                    .Replace("</link>", "")
                    .Replace("\n", "");

            if (!Utility.CheckForCorrectResponse(lResponse, this._senpai.ErrHandler)) return;

            try
            {
                lDocument.LoadHtml(lResponse);

                List<Language> languageList = new List<Language>();
                foreach (
                    HtmlNode childNode in
                        lDocument.DocumentNode.ChildNodes[1].ChildNodes[2].ChildNodes[2].ChildNodes[2].ChildNodes[4]
                            .ChildNodes[5].ChildNodes.Where(
                                childNode =>
                                    childNode.ChildNodes.Count > 3 &&
                                    childNode.ChildNodes[3].FirstChild.GetAttributeValue("value", "+").Equals("-")))
                {
                    switch (childNode.FirstChild.InnerText)
                    {
                        case "GerSub":
                            languageList.Add(Language.GerSub);
                            break;
                        case "EngSub":
                            languageList.Add(Language.EngSub);
                            break;
                        case "EngDub":
                            languageList.Add(Language.EngDub);
                            break;
                        case "GerDub":
                            languageList.Add(Language.GerDub);
                            break;
                    }
                }

                this.Sprachen = languageList.ToArray();
            }
            catch (NullReferenceException)
            {
            }
            catch (IndexOutOfRangeException)
            {
            }
        }

        private async Task InitEpisodeCount()
        {
            if (!this._senpai.LoggedIn) throw new NotLoggedInException();

            HtmlDocument lDocument = new HtmlDocument();
            string lResponse =
                (await HttpUtility.GetWebRequestResponse("http://proxer.me/edit/entry/" + this.Id + "/count",
                    this._senpai.LoginCookies))
                    .Replace("</link>", "")
                    .Replace("\n", "");

            if (!Utility.CheckForCorrectResponse(lResponse, this._senpai.ErrHandler)) return;

            try
            {
                lDocument.LoadHtml(lResponse);

                if (
                    lDocument.DocumentNode.ChildNodes[1].ChildNodes[2].ChildNodes[2].ChildNodes[2].ChildNodes[4]
                        .ChildNodes[5].FirstChild.FirstChild.InnerText.Equals("Aktuelle Anzahl:"))
                    this.EpisodenZahl =
                        Convert.ToInt32(
                            lDocument.DocumentNode.ChildNodes[1].ChildNodes[2].ChildNodes[2].ChildNodes[2].ChildNodes[4]
                                .ChildNodes[5].FirstChild.ChildNodes[1].InnerText);
            }
            catch (NullReferenceException)
            {
            }
            catch (IndexOutOfRangeException)
            {
            }
        }

        private async Task InitMain()
        {
            HtmlDocument lDocument = new HtmlDocument();
            string lResponse =
                (await HttpUtility.GetWebRequestResponse("https://proxer.me/info/" + this.Id, this._senpai.LoginCookies))
                    .Replace("</link>", "")
                    .Replace("\n", "");

            if (!Utility.CheckForCorrectResponse(lResponse, this._senpai.ErrHandler)) return;
            try
            {
                lDocument.LoadHtml(lResponse);

                HtmlNode lTableNode =
                    lDocument.DocumentNode.ChildNodes[1].ChildNodes[2].ChildNodes[2].ChildNodes[2].ChildNodes[5]
                        .ChildNodes[2].FirstChild.ChildNodes[1].FirstChild;
                foreach (HtmlNode childNode in lTableNode.ChildNodes.Where(childNode => childNode.Name.Equals("tr")))
                {
                    switch (childNode.FirstChild.FirstChild.InnerText)
                    {
                        case "Original Titel":
                            this.Name = childNode.ChildNodes[1].InnerText;
                            break;
                        case "Eng. Titel":
                            this.EnglischTitel = childNode.ChildNodes[1].InnerText;
                            break;
                        case "Jap. Titel":
                            this.JapanTitel = childNode.ChildNodes[1].InnerText;
                            break;
                        case "Synonym":
                            this.Synonym = childNode.ChildNodes[1].InnerText;
                            break;
                        case "Genre":
                            List<string> lGenreList = new List<string>();
                            childNode.ChildNodes[1].ChildNodes.ToList()
                                .ForEach(
                                    delegate(HtmlNode htmlNode)
                                    {
                                        if (htmlNode.Name.Equals("a")) lGenreList.Add(htmlNode.InnerText);
                                    });
                            this.Genre = lGenreList.ToArray();
                            break;
                        case "FSK":
                            this.Fsk = new Dictionary<string, Uri>();
                            childNode.ChildNodes[1].ChildNodes.ToList()
                                .ForEach(
                                    delegate(HtmlNode htmlNode)
                                    {
                                        if (htmlNode.Name.Equals("span") &&
                                            !this.Fsk.ContainsKey(htmlNode.GetAttributeValue("title", "ERROR")))
                                            this.Fsk.Add(htmlNode.GetAttributeValue("title", "ERROR"),
                                                new Uri("https://proxer.me" +
                                                        htmlNode.FirstChild.GetAttributeValue("src", "/")));
                                    });
                            break;
                        case "Season":
                            List<string> lSeasonList = new List<string>();
                            childNode.ChildNodes[1].ChildNodes.ToList()
                                .ForEach(
                                    delegate(HtmlNode htmlNode)
                                    {
                                        if (htmlNode.Name.Equals("a"))
                                            lSeasonList.Add(htmlNode.InnerText);
                                    });
                            this.Season = lSeasonList.ToArray();
                            break;
                        case "Status":
                            switch (childNode.ChildNodes[1].InnerText)
                            {
                                case "Airing":
                                    this.Status = AnimeMangaStatus.Airing;
                                    break;
                                case "Abgeschlossen":
                                    this.Status = AnimeMangaStatus.Abgeschlossen;
                                    break;
                                case "Nicht erschienen (Pre-Airing)":
                                    this.Status = AnimeMangaStatus.PreAiring;
                                    break;
                                default:
                                    this.Status = AnimeMangaStatus.Abgebrochen;
                                    break;
                            }
                            break;
                        case "Gruppen":
                            if (childNode.ChildNodes[1].InnerText.Contains("Keine Gruppen eingetragen.")) break;
                            this.Gruppen = (from htmlNode in childNode.ChildNodes[1].ChildNodes
                                where htmlNode.Name.Equals("a")
                                select
                                    new Group(
                                        Convert.ToInt32(
                                            Utility.GetTagContents(
                                                htmlNode.GetAttributeValue("href", "/translatorgroups?id=-1#top"),
                                                "/translatorgroups?id=", "#top")[0]), htmlNode.InnerText)).ToArray();
                            break;
                        case "Industrie":
                            if (childNode.ChildNodes[1].InnerText.Contains("Keine Unternehmen eingetragen.")) break;
                            List<Industry> lIndustries = new List<Industry>();
                            foreach (
                                HtmlNode htmlNode in
                                    childNode.ChildNodes[1].ChildNodes.Where(htmlNode => htmlNode.Name.Equals("a")))
                            {
                                Industry.IndustryType lIndustryType;
                                if (htmlNode.NextSibling.InnerText.Contains("Studio"))
                                    lIndustryType = Industry.IndustryType.Studio;
                                else if (htmlNode.NextSibling.InnerText.Contains("Publisher"))
                                    lIndustryType = Industry.IndustryType.Publisher;
                                else if (htmlNode.NextSibling.InnerText.Contains("Producer"))
                                    lIndustryType = Industry.IndustryType.Producer;
                                else lIndustryType = Industry.IndustryType.None;

                                lIndustries.Add(new Industry(Convert.ToInt32(
                                    Utility.GetTagContents(
                                        htmlNode.GetAttributeValue("href", "/industry?id=-1#top"),
                                        "/industry?id=", "#top")[0]), htmlNode.InnerText, lIndustryType));
                            }
                            this.Industrie = lIndustries.ToArray();
                            break;
                        case "Lizenz":
                            this.Lizensiert = childNode.ChildNodes[1].InnerText.StartsWith("Lizenziert!");
                            break;
                        case "Beschreibung:":
                            childNode.FirstChild.FirstChild.Remove();
                            this.Beschreibung = "";
                            foreach (HtmlNode htmlNode in childNode.FirstChild.ChildNodes)
                            {
                                if (htmlNode.Name.Equals("br")) this.Beschreibung += "\n";
                                else this.Beschreibung += htmlNode.InnerText;
                            }
                            if (this.Beschreibung.StartsWith("\n")) this.Beschreibung = this.Beschreibung.TrimStart();
                            break;
                    }
                }
            }
            catch (NullReferenceException)
            {
            }
            catch (IndexOutOfRangeException)
            {
            }
        }

        #endregion

        /// <summary>
        ///     Eine Klasse, die die <see cref="Episode">Episode</see> eines <see cref="Anime" /> darstellt.
        /// </summary>
        public class Episode
        {
            private readonly Language _lang;
            private readonly Senpai _senpai;
            private List<KeyValuePair<Stream.StreamPartner, Stream>> _streams;

            internal Episode(Anime anime, int nr, Language lang, Senpai senpai)
            {
                this.ParentAnime = anime;
                this.EpisodeNr = nr;
                this._lang = lang;
                this._senpai = senpai;

                this.IstInitialisiert = false;
            }

            #region Properties

            /// <summary>
            ///     Gibt die Nummer der Episode zurück.
            /// </summary>
            public int EpisodeNr { get; set; }

            /// <summary>
            ///     Gibt zurück, ob das Objekt bereits initialisiert ist.
            /// </summary>
            public bool IstInitialisiert { get; private set; }

            /// <summary>
            ///     Gibt den Anime der Episode zurück.
            /// </summary>
            public Anime ParentAnime { get; set; }

            /// <summary>
            ///     Gibt die Stream
            ///     <para>Wenn nach Aufruf von Init() immer noch null, dann sind keine Streams für diese Episode verfügbar.</para>
            /// </summary>
            /// <exception cref="InitializeNeededException">Wird ausgelöst, wenn das Objekt noch nicht initialisiert wurde.</exception>
            /// <seealso cref="Episode.Init" />
            public List<KeyValuePair<Stream.StreamPartner, Stream>> Streams
            {
                get
                {
                    if (!this.IstInitialisiert) throw new InitializeNeededException();
                    return this._streams;
                }
                private set { this._streams = value; }
            }

            #endregion

            #region

            /// <summary>
            ///     Initialisiert das Objekt.
            /// </summary>
            /// <exception cref="NotLoggedInException">Wird ausgelöst, wenn der Benutzer noch nicht eingeloggt ist.</exception>
            /// <exception cref="CaptchaException">Wird ausgelöst, wenn Proxer vom Benutzer das ausfüllen eines Captchas verlangt.</exception>
            /// <seealso cref="Senpai.Login" />
            public async Task Init()
            {
                if (!this._senpai.LoggedIn) throw new NotLoggedInException();

                HtmlDocument lDocument = new HtmlDocument();
                string lResponse =
                    (await HttpUtility.GetWebRequestResponse(
                        "https://proxer.me/watch/" + this.ParentAnime.Id + "/" + this.EpisodeNr + "/" +
                        this._lang.ToString().ToLower(),
                        this._senpai.MobileLoginCookies))
                        .Replace("</link>", "")
                        .Replace("\n", "");

                if (!Utility.CheckForCorrectResponse(lResponse, this._senpai.ErrHandler)) return;
                try
                {
                    lDocument.LoadHtml(lResponse);

                    HtmlNode[] lAllHtmlNodes = Utility.GetAllHtmlNodes(lDocument.DocumentNode.ChildNodes).ToArray();

                    if (
                        lAllHtmlNodes.Any(
                            x =>
                                x.Name.Equals("img") && x.HasAttributes &&
                                x.GetAttributeValue("src", "").Equals("/images/misc/stopyui.jpg")))
                        throw new CaptchaException();

                    if (
                        lAllHtmlNodes.Any(
                            x =>
                                x.Name.Equals("img") && x.HasAttributes &&
                                x.GetAttributeValue("src", "").Equals("/images/misc/404.png")))
                        return;

                    this.Streams = new List<KeyValuePair<Stream.StreamPartner, Stream>>();

                    foreach (
                        HtmlNode childNode in
                            lDocument.DocumentNode.ChildNodes[1].ChildNodes[2].FirstChild.ChildNodes[1].ChildNodes[1]
                                .ChildNodes.Where(childNode => !childNode.FirstChild.Name.Equals("#text")))
                    {
                        switch (childNode.InnerText)
                        {
                            case "Dailymotion":
                                this.Streams.Add(
                                    new KeyValuePair<Stream.StreamPartner, Stream>(Stream.StreamPartner.Dailymotion,
                                        new Stream(
                                            new Uri(childNode.FirstChild.GetAttributeValue("href", "http://proxer.me/")),
                                            Stream.StreamPartner.Dailymotion)));
                                break;
                            case "MP4Upload":
                                this.Streams.Add(
                                    new KeyValuePair<Stream.StreamPartner, Stream>(Stream.StreamPartner.Mp4Upload,
                                        new Stream(
                                            new Uri(childNode.FirstChild.GetAttributeValue("href", "http://proxer.me/")),
                                            Stream.StreamPartner.Mp4Upload)));
                                break;
                            case "Streamcloud":
                                this.Streams.Add(
                                    new KeyValuePair<Stream.StreamPartner, Stream>(Stream.StreamPartner.Streamcloud,
                                        new Stream(
                                            new Uri(childNode.FirstChild.GetAttributeValue("href", "http://proxer.me/")),
                                            Stream.StreamPartner.Streamcloud)));
                                break;
                            case "Videobam":
                                this.Streams.Add(
                                    new KeyValuePair<Stream.StreamPartner, Stream>(Stream.StreamPartner.Videobam,
                                        new Stream(
                                            new Uri(childNode.FirstChild.GetAttributeValue("href", "http://proxer.me/")),
                                            Stream.StreamPartner.Videobam)));
                                break;
                            case "YourUpload":
                                this.Streams.Add(
                                    new KeyValuePair<Stream.StreamPartner, Stream>(Stream.StreamPartner.YourUpload,
                                        new Stream(
                                            new Uri(childNode.FirstChild.GetAttributeValue("href", "http://proxer.me/")),
                                            Stream.StreamPartner.YourUpload)));
                                break;
                            case "Proxer-Stream":
                                this.Streams.Add(
                                    new KeyValuePair<Stream.StreamPartner, Stream>(Stream.StreamPartner.ProxerStream,
                                        new Stream(
                                            new Uri(childNode.FirstChild.GetAttributeValue("href", "http://proxer.me/")),
                                            Stream.StreamPartner.ProxerStream)));
                                break;
                        }
                    }

                    this.IstInitialisiert = true;
                }
                catch (NullReferenceException)
                {
                }
                catch (IndexOutOfRangeException)
                {
                }
            }

            #endregion

            /// <summary>
            ///     Eine Klasse, die einen <see cref="Stream">Stream</see> einer <see cref="Episode">Episode</see> eines
            ///     <see cref="Anime" />
            ///     darstellt.
            /// </summary>
            public class Stream
            {
                /// <summary>
                ///     Eine Enumeration, die alle unterstützten Streampartner aufführt.
                /// </summary>
                public enum StreamPartner
                {
                    /// <summary>
                    ///     Stellt den Streampartner Streamcloud da.
                    /// </summary>
                    Streamcloud,

                    /// <summary>
                    ///     Stellt den Streampartner MP4Upload da.
                    /// </summary>
                    Mp4Upload,

                    /// <summary>
                    ///     Stellt den Streampartner Dailymotion da.
                    /// </summary>
                    Dailymotion,

                    /// <summary>
                    ///     Stellt den Streampartner Videobam da.
                    /// </summary>
                    Videobam,

                    /// <summary>
                    ///     Stellt den Streampartner YourUpload da.
                    /// </summary>
                    YourUpload,

                    /// <summary>
                    ///     Stellt den Streampartner Proxer da.
                    /// </summary>
                    ProxerStream
                }

                internal Stream(Uri link, StreamPartner streamPartner)
                {
                    this.Link = link;
                    this.SPartner = streamPartner;
                }

                #region Properties

                /// <summary>
                ///     Gibt den Link des <see cref="Stream">Streams</see> zurück.
                /// </summary>
                public Uri Link { get; private set; }

                /// <summary>
                ///     Gibt den <see cref="StreamPartner">Streampartner</see> des <see cref="Stream">Streams</see> zurück.
                /// </summary>
                public StreamPartner SPartner { get; private set; }

                #endregion
            }
        }
    }
}