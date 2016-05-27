﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azuria.Exceptions;
using Azuria.Main.Minor;
using Azuria.Main.User.Comment;
using Azuria.Main.User.ControlPanel;
using Azuria.Utilities.ErrorHandling;
using Azuria.Utilities.Extensions;
using Azuria.Utilities.Net;
using Azuria.Utilities.Properties;
using HtmlAgilityPack;
using JetBrains.Annotations;

// ReSharper disable LoopCanBeConvertedToQuery

namespace Azuria.Main
{
    /// <summary>
    ///     Eine Klasse, die einen <see cref="Manga" /> darstellt.
    /// </summary>
    public class Manga : IAnimeMangaObject
    {
        /// <summary>
        ///     Eine Enumeration, die den Typ des Mangas darstellt.
        /// </summary>
        public enum MangaType
        {
            /// <summary>
            ///     Stellt eine Manga-Serie dar.
            /// </summary>
            Series,

            /// <summary>
            ///     Stellt einen Manga One-Shot dar.
            /// </summary>
            OneShot,

            /// <summary>
            ///     Stellt einen unbekannten Mangatypen dar.
            /// </summary>
            Unknown
        }

        private readonly Senpai _senpai;

        [UsedImplicitly]
        internal Manga()
        {
            this.AvailableLanguages = new InitialisableProperty<IEnumerable<Language>>(this.InitAvailableLang);
            this.ContentCount = new InitialisableProperty<int>(this.InitChapterCount);
            this.Description = new InitialisableProperty<string>(this.InitMain);
            this.EnglishTitle = new InitialisableProperty<string>(this.InitMain, string.Empty)
            {
                IsInitialisedOnce = false
            };
            this.Fsk = new InitialisableProperty<IEnumerable<FskObject>>(this.InitMain);
            this.Genre = new InitialisableProperty<IEnumerable<GenreObject>>(this.InitMain);
            this.GermanTitle = new InitialisableProperty<string>(this.InitMain, string.Empty)
            {
                IsInitialisedOnce = false
            };
            this.Groups = new InitialisableProperty<IEnumerable<Group>>(this.InitMain);
            this.Industry = new InitialisableProperty<IEnumerable<Industry>>(this.InitMain);
            this.IsLicensed = new InitialisableProperty<bool>(this.InitMain);
            this.JapaneseTitle = new InitialisableProperty<string>(this.InitMain, string.Empty)
            {
                IsInitialisedOnce = false
            };
            this.MangaTyp = new InitialisableProperty<MangaType>(this.InitType);
            this.Name = new InitialisableProperty<string>(this.InitMain, string.Empty) {IsInitialisedOnce = false};
            this.Season = new InitialisableProperty<IEnumerable<string>>(this.InitMain);
            this.Status = new InitialisableProperty<AnimeMangaStatus>(this.InitMain);
            this.Synonym = new InitialisableProperty<string>(this.InitMain, string.Empty) {IsInitialisedOnce = false};
        }

        internal Manga([NotNull] string name, int id, [NotNull] Senpai senpai) : this()
        {
            this.Id = id;
            this._senpai = senpai;

            this.Name = new InitialisableProperty<string>(this.InitMain, name);
        }

        internal Manga([NotNull] string name, int id, [NotNull] Senpai senpai,
            [NotNull] IEnumerable<GenreObject> genreList, AnimeMangaStatus status,
            MangaType type) : this(name, id, senpai)
        {
            this.Genre = new InitialisableProperty<IEnumerable<GenreObject>>(this.InitMain, genreList);
            this.Status = new InitialisableProperty<AnimeMangaStatus>(this.InitMain, status);
            this.MangaTyp = new InitialisableProperty<MangaType>(this.InitType, type);
        }

        #region Geerbt

        /// <summary>
        ///     Gibt den Link zum Cover des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public Uri CoverUri => new Uri("http://cdn.proxer.me/cover/" + this.Id + ".jpg");

        /// <summary>
        /// </summary>
        public InitialisableProperty<int> ContentCount { get; }

        /// <summary>
        ///     Gibt die Beschreibung des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public InitialisableProperty<string> Description { get; }

        /// <summary>
        ///     Gibt den englische Titel des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public InitialisableProperty<string> EnglishTitle { get; }

        /// <summary>
        ///     Gibt die Links zu allen FSK-Beschränkungen des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public InitialisableProperty<IEnumerable<FskObject>> Fsk { get; }

        /// <summary>
        ///     Gitb die Genres des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public InitialisableProperty<IEnumerable<GenreObject>> Genre { get; }

        /// <summary>
        ///     Gibt den deutschen Titel des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public InitialisableProperty<string> GermanTitle { get; }

        /// <summary>
        ///     Gibt die Gruppen zurück, die den <see cref="Anime" /> oder <see cref="Manga" /> übersetzten oder übersetzt haben.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public InitialisableProperty<IEnumerable<Group>> Groups { get; }

        /// <summary>
        ///     Gibt die ID des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Gibt die Industrie des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public InitialisableProperty<IEnumerable<Industry>> Industry { get; }

        /// <summary>
        ///     Gibt zurück, ob der <see cref="Anime" /> oder <see cref="Manga" /> lizensiert ist.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public InitialisableProperty<bool> IsLicensed { get; }

        /// <summary>
        ///     Gibt den japanischen Titel des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public InitialisableProperty<string> JapaneseTitle { get; }

        /// <summary>
        ///     Gibt den Namen des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public InitialisableProperty<string> Name { get; }

        /// <summary>
        ///     Gibt die Season des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public InitialisableProperty<IEnumerable<string>> Season { get; }

        /// <summary>
        ///     Gibt den Status des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public InitialisableProperty<AnimeMangaStatus> Status { get; }

        /// <summary>
        ///     Gibt das Synonym des <see cref="Anime" /> oder <see cref="Manga" /> zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public InitialisableProperty<string> Synonym { get; }

        /// <summary>
        ///     Initialisiert das Objekt.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public async Task<ProxerResult> Init()
        {
            return await this.InitAllInitalisableProperties();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        async Task<ProxerResult> IAnimeMangaObject.AddToPlanned(UserControlPanel userControlPanel)
        {
            return await this.AddToPlanned(userControlPanel);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gibt die verfügbaren Sprachen zurück.
        /// </summary>
        [NotNull]
        public InitialisableProperty<IEnumerable<Language>> AvailableLanguages { get; }

        /// <summary>
        ///     Gibt den Typ eines Anime zurück.
        /// </summary>
        [NotNull]
        public InitialisableProperty<MangaType> MangaTyp { get; }

        /// <summary>
        ///     Gibt zurück, ob es sich um einen <see cref="Anime" /> oder <see cref="Manga" /> handelt.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        public AnimeMangaType ObjectType => AnimeMangaType.Manga;

        #endregion

        #region

        /// <summary>
        /// </summary>
        /// <param name="userControlPanel"></param>
        /// <returns></returns>
        public async Task<ProxerResult<AnimeMangaUcpObject<Manga>>> AddToPlanned(
            UserControlPanel userControlPanel = null)
        {
            userControlPanel = userControlPanel ?? new UserControlPanel(this._senpai);
            return await userControlPanel.AddToPlanned(this);
        }

        /// <summary>
        ///     Gibt alle <see cref="Chapter">Kapitel</see> des <see cref="Manga" /> in der ausgewählten Sprache zurück.
        /// </summary>
        /// <exception cref="LanguageNotAvailableException">
        ///     Wird ausgelöst, wenn der Manga nicht in der angegebenen Sprache
        ///     verfügbar ist.
        /// </exception>
        /// <param name="language">Die Sprache der <see cref="Chapter">Kapitel</see>.</param>
        /// <seealso cref="AvailableLanguages" />
        /// <returns>Ein Array mit length = <see cref="ContentCount" /></returns>
        [NotNull]
        [ItemNotNull]
        public async Task<ProxerResult<IEnumerable<Chapter>>> GetChapters(Language language)
        {
            if (!(await this.AvailableLanguages.GetObject(new Language[0])).Contains(language))
                return new ProxerResult<IEnumerable<Chapter>>(new Exception[] {new LanguageNotAvailableException()});

            List<Chapter> lChapters = new List<Chapter>();
            for (int i = 1; i <= await this.ContentCount.GetObject(-1); i++)
            {
                lChapters.Add(new Chapter(this, i, language, this._senpai));
            }

            return new ProxerResult<IEnumerable<Chapter>>(lChapters.ToArray());
        }

        /// <summary>
        ///     Gibt die Kommentare des <see cref="Anime" /> oder <see cref="Manga" /> chronologisch geordnet zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        /// <param name="startIndex">Der Start-Index der ausgegebenen Kommentare.</param>
        /// <param name="count">Die Anzahl der ausgegebenen Kommentare ab dem angegebenen <paramref name="startIndex" />.</param>
        /// <returns>Eine Aufzählung mit den Kommentaren.</returns>
        public async Task<ProxerResult<IEnumerable<Comment<Manga>>>> GetCommentsLatest(int startIndex, int count)
        {
            return
                await
                    Comment<Manga>.GetCommentsFromUrl(startIndex, count,
                        "https://proxer.me/info/" + this.Id + "/comments/",
                        "latest", this._senpai, this);
        }

        /// <summary>
        ///     Gibt die Kommentare des <see cref="Anime" /> oder <see cref="Manga" />, nach ihrer Beliebtheit sortiert, zurück.
        ///     <para>(Vererbt von <see cref="IAnimeMangaObject" />)</para>
        /// </summary>
        /// <param name="startIndex">Der Start-Index der ausgegebenen Kommentare.</param>
        /// <param name="count">Die Anzahl der ausgegebenen Kommentare ab dem angegebenen <paramref name="startIndex" />.</param>
        /// <returns>Eine Aufzählung mit den Kommentaren.</returns>
        public async Task<ProxerResult<IEnumerable<Comment<Manga>>>> GetCommentsRating(int startIndex, int count)
        {
            return
                await
                    Comment<Manga>.GetCommentsFromUrl(startIndex, count,
                        "https://proxer.me/info/" + this.Id + "/comments/",
                        "rating", this._senpai, this);
        }

        /// <summary>
        ///     Gibt die aktuell am beliebtesten <see cref="Manga" /> zurück.
        /// </summary>
        /// <exception cref="WrongResponseException">Wird ausgelöst, wenn die Antwort des Servers nicht der Erwarteten entspricht.</exception>
        /// <param name="senpai">Der aktuelle Benutzer.</param>
        /// <returns>Ein Array mit den aktuell beliebtesten <see cref="Manga" />.</returns>
        [ItemNotNull]
        public static async Task<ProxerResult<IEnumerable<Manga>>> GetPopularManga([NotNull] Senpai senpai)
        {
            HtmlDocument lDocument = new HtmlDocument();
            ProxerResult<string> lResult =
                await
                    HttpUtility.GetResponseErrorHandling(
                        new Uri("https://proxer.me/manga?format=raw"),
                        null,
                        senpai.ErrHandler,
                        senpai);

            if (!lResult.Success)
                return new ProxerResult<IEnumerable<Manga>>(lResult.Exceptions);

            string lResponse = lResult.Result;

            try
            {
                lDocument.LoadHtml(lResponse);

                return
                    new ProxerResult<IEnumerable<Manga>>(
                        (from childNode in lDocument.DocumentNode.ChildNodes[5].FirstChild.FirstChild.ChildNodes
                            let lId =
                                Convert.ToInt32(
                                    childNode.FirstChild.GetAttributeValue("href", "/info/-1#top").Split('/')[2].Split(
                                        '#')
                                        [0])
                            select new Manga(childNode.FirstChild.GetAttributeValue("title", "ERROR"), lId, senpai))
                            .ToArray());
            }
            catch
            {
                return new ProxerResult<IEnumerable<Manga>>(ErrorHandler.HandleError(senpai, lResponse).Exceptions);
            }
        }

        [ItemNotNull]
        private async Task<ProxerResult> InitAvailableLang()
        {
            HtmlDocument lDocument = new HtmlDocument();
            Func<string, ProxerResult> lCheckFunc = s =>
            {
                if (!string.IsNullOrEmpty(s) &&
                    s.Equals("Bitte logge dich ein."))
                    return new ProxerResult(new Exception[] {new NoAccessException(nameof(this.InitAvailableLang))});

                return new ProxerResult();
            };
            ProxerResult<string> lResult =
                await
                    HttpUtility.GetResponseErrorHandling(
                        new Uri("http://proxer.me/edit/entry/" + this.Id + "/languages?format=raw"),
                        this._senpai.LoginCookies,
                        this._senpai.ErrHandler,
                        this._senpai,
                        new[] {lCheckFunc});

            if (!lResult.Success)
                return new ProxerResult(lResult.Exceptions);

            string lResponse = lResult.Result;

            try
            {
                lDocument.LoadHtml(lResponse);

                List<Language> languageList = new List<Language>();
                foreach (
                    HtmlNode childNode in
                        lDocument.DocumentNode.ChildNodes[4]
                            .ChildNodes[5].ChildNodes.Where(
                                childNode =>
                                    childNode.ChildNodes.Count > 3 &&
                                    childNode.ChildNodes[3].FirstChild.GetAttributeValue("value", "+").Equals("-")))
                {
                    switch (childNode.FirstChild.InnerText)
                    {
                        case "Englisch":
                            languageList.Add(Language.English);
                            break;
                        case "Deutsch":
                            languageList.Add(Language.German);
                            break;
                    }
                }

                this.AvailableLanguages.SetInitialisedObject(languageList.ToArray());

                return new ProxerResult();
            }
            catch
            {
                return new ProxerResult((await ErrorHandler.HandleError(this._senpai, lResponse, false)).Exceptions);
            }
        }

        [ItemNotNull]
        private async Task<ProxerResult> InitChapterCount()
        {
            HtmlDocument lDocument = new HtmlDocument();
            Func<string, ProxerResult> lCheckFunc = s =>
            {
                if (!string.IsNullOrEmpty(s) &&
                    s.Equals("Bitte logge dich ein."))
                    return new ProxerResult(new Exception[] {new NoAccessException(nameof(this.InitChapterCount))});

                return new ProxerResult();
            };
            ProxerResult<string> lResult =
                await
                    HttpUtility.GetResponseErrorHandling(
                        new Uri("http://proxer.me/edit/entry/" + this.Id + "/count?format=raw"),
                        this._senpai.LoginCookies,
                        this._senpai.ErrHandler,
                        this._senpai,
                        new[] {lCheckFunc});

            if (!lResult.Success)
                return new ProxerResult(lResult.Exceptions);

            string lResponse = lResult.Result;

            try
            {
                lDocument.LoadHtml(lResponse);

                this.ContentCount.SetInitialisedObject(
                    Convert.ToInt32(
                        lDocument.DocumentNode.ChildNodes[4]
                            .ChildNodes[5].FirstChild.ChildNodes[1].InnerText));

                return new ProxerResult();
            }
            catch
            {
                return new ProxerResult((await ErrorHandler.HandleError(this._senpai, lResponse, false)).Exceptions);
            }
        }

        [ItemNotNull]
        private async Task<ProxerResult> InitMain()
        {
            HtmlDocument lDocument = new HtmlDocument();
            ProxerResult<Tuple<string, CookieContainer>> lResult =
                await
                    HttpUtility.GetResponseErrorHandling(
                        new Uri("https://proxer.me/info/" + this.Id + "?format=raw"),
                        null,
                        this._senpai.ErrHandler,
                        this._senpai,
                        new Func<string, ProxerResult>[0], false);

            if (!lResult.Success || lResult.Result == null)
                return new ProxerResult(lResult.Exceptions);

            string lResponse = lResult.Result.Item1;

            try
            {
                lDocument.LoadHtml(lResponse);

                HtmlNode lTableNode =
                    lDocument.DocumentNode.ChildNodes[5]
                        .ChildNodes[2].FirstChild.ChildNodes[1].FirstChild;
                foreach (HtmlNode childNode in lTableNode.ChildNodes.Where(childNode => childNode.Name.Equals("tr")))
                {
                    switch (childNode.FirstChild.FirstChild.InnerText)
                    {
                        case "Original Titel":
                            this.Name.SetInitialisedObject(childNode.ChildNodes[1].InnerText);
                            break;
                        case "Eng. Titel":
                            this.EnglishTitle.SetInitialisedObject(childNode.ChildNodes[1].InnerText);
                            break;
                        case "Ger. Titel":
                            this.GermanTitle.SetInitialisedObject(childNode.ChildNodes[1].InnerText);
                            break;
                        case "Jap. Titel":
                            this.JapaneseTitle.SetInitialisedObject(childNode.ChildNodes[1].InnerText);
                            break;
                        case "Synonym":
                            this.Synonym.SetInitialisedObject(childNode.ChildNodes[1].InnerText);
                            break;
                        case "Genre":
                            List<GenreObject> lGenreList = new List<GenreObject>();
                            foreach (HtmlNode htmlNode in childNode.ChildNodes[1].ChildNodes.ToList())
                            {
                                if (htmlNode.Name.Equals("a"))
                                    lGenreList.Add(new GenreObject(htmlNode.InnerText));
                            }
                            this.Genre.SetInitialisedObject(lGenreList.ToArray());
                            break;
                        case "FSK":
                            List<FskObject> lTempList = new List<FskObject>();
                            foreach (
                                HtmlNode htmlNode in
                                    childNode.ChildNodes[1].ChildNodes.ToList()
                                        .Where(htmlNode => htmlNode.Name.Equals("span") &&
                                                           lTempList.All(
                                                               o =>
                                                                   o.Fsk !=
                                                                   FskHelper.StringToFskDictionary[
                                                                       htmlNode.FirstChild.GetAttributeValue("src",
                                                                           "/images/fsk/unknown.png")
                                                                           .GetTagContents("/", ".png")
                                                                           .First()])))
                            {
                                string lFskString = htmlNode.FirstChild.GetAttributeValue("src",
                                    "/images/fsk/unknown.png")
                                    .GetTagContents("fsk/", ".png")
                                    .First();
                                lTempList.Add(new FskObject(FskHelper.StringToFskDictionary[lFskString],
                                    new Uri($"https://proxer.me/images/fsk/{lFskString}.png")));
                            }
                            this.Fsk.SetInitialisedObject(lTempList);
                            break;
                        case "Season":
                            List<string> lSeasonList = new List<string>();
                            foreach (HtmlNode htmlNode in childNode.ChildNodes[1].ChildNodes.ToList())
                            {
                                if (htmlNode.Name.Equals("a"))
                                    lSeasonList.Add(htmlNode.InnerText);
                            }
                            this.Season.SetInitialisedObject(lSeasonList);
                            break;
                        case "Status":
                            switch (childNode.ChildNodes[1].InnerText)
                            {
                                case "Airing":
                                    this.Status.SetInitialisedObject(AnimeMangaStatus.Airing);
                                    break;
                                case "Abgeschlossen":
                                    this.Status.SetInitialisedObject(AnimeMangaStatus.Completed);
                                    break;
                                case "Nicht erschienen (Pre-Airing)":
                                    this.Status.SetInitialisedObject(AnimeMangaStatus.PreAiring);
                                    break;
                                default:
                                    this.Status.SetInitialisedObject(AnimeMangaStatus.Canceled);
                                    break;
                            }
                            break;
                        case "Gruppen":
                            if (childNode.ChildNodes[1].InnerText.Contains("Keine Gruppen eingetragen.")) break;
                            this.Groups.SetInitialisedObject(from htmlNode in childNode.ChildNodes[1].ChildNodes
                                where htmlNode.Name.Equals("a")
                                select
                                    new Group(
                                        Convert.ToInt32(
                                            htmlNode.GetAttributeValue("href",
                                                "/translatorgroups?id=-1#top")
                                                .GetTagContents("/translatorgroups?id=", "#top")[0]), htmlNode.InnerText));
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
                                    lIndustryType = Minor.Industry.IndustryType.Studio;
                                else if (htmlNode.NextSibling.InnerText.Contains("Publisher"))
                                    lIndustryType = Minor.Industry.IndustryType.Publisher;
                                else if (htmlNode.NextSibling.InnerText.Contains("Producer"))
                                    lIndustryType = Minor.Industry.IndustryType.Producer;
                                else lIndustryType = Minor.Industry.IndustryType.Unknown;

                                lIndustries.Add(new Industry(Convert.ToInt32(
                                    htmlNode.GetAttributeValue("href", "/industry?id=-1#top")
                                        .GetTagContents("/industry?id=", "#top")[0]), htmlNode.InnerText, lIndustryType));
                            }
                            this.Industry.SetInitialisedObject(lIndustries.ToArray());
                            break;
                        case "Lizenz":
                            this.IsLicensed.SetInitialisedObject(
                                childNode.ChildNodes[1].InnerText.StartsWith("Lizenziert!"));
                            break;
                        case "Beschreibung:":
                            childNode.FirstChild.FirstChild.Remove();
                            string lTempString = "";
                            foreach (HtmlNode htmlNode in childNode.FirstChild.ChildNodes)
                            {
                                if (htmlNode.Name.Equals("br")) lTempString += "\n";
                                else lTempString += htmlNode.InnerText;
                            }
                            if (lTempString.StartsWith("\n")) lTempString = lTempString.TrimStart();
                            this.Description.SetInitialisedObject(lTempString);
                            break;
                    }
                }
            }
            catch
            {
                return new ProxerResult(ErrorHandler.HandleError(this._senpai, lResponse).Exceptions);
            }

            return new ProxerResult();
        }

        [ItemNotNull]
        private async Task<ProxerResult> InitType()
        {
            HtmlDocument lDocument = new HtmlDocument();
            Func<string, ProxerResult> lCheckFunc = s =>
            {
                if (!string.IsNullOrEmpty(s) &&
                    s.Equals("Bitte logge dich ein."))
                    return new ProxerResult(new Exception[] {new NoAccessException(nameof(this.InitType))});

                return new ProxerResult();
            };
            ProxerResult<string> lResult =
                await
                    HttpUtility.GetResponseErrorHandling(
                        new Uri("http://proxer.me/edit/entry/" + this.Id + "/medium?format=raw"),
                        this._senpai.LoginCookies,
                        this._senpai.ErrHandler,
                        this._senpai,
                        new[] {lCheckFunc});

            if (!lResult.Success)
                return new ProxerResult(lResult.Exceptions);

            string lResponse = lResult.Result;

            try
            {
                lDocument.LoadHtml(lResponse);

                HtmlNode lNode =
                    lDocument.GetElementbyId("medium").ChildNodes.First(node => node.Attributes.Contains("selected"));
                switch (lNode.Attributes["value"].Value)
                {
                    case "mangaseries":
                        this.MangaTyp.SetInitialisedObject(MangaType.Series);
                        break;
                    case "oneshot":
                        this.MangaTyp.SetInitialisedObject(MangaType.OneShot);
                        break;
                    default:
                        this.MangaTyp.SetInitialisedObject(MangaType.Series);
                        break;
                }
            }
            catch
            {
                return new ProxerResult((await ErrorHandler.HandleError(this._senpai, lResponse, false)).Exceptions);
            }

            return new ProxerResult();
        }

        #endregion

        /// <summary>
        ///     Eine Klasse, die ein <see cref="Chapter">Kapitel</see> eines <see cref="Manga">Mangas</see> darstellt.
        /// </summary>
        public class Chapter : IAnimeMangaContent<Manga>
        {
            private readonly Senpai _senpai;

            internal Chapter([NotNull] Manga parentManga, int chapterNumber, Language lang, [NotNull] Senpai senpai)
            {
                this._senpai = senpai;
                this.ContentIndex = chapterNumber;
                this.Language = lang;
                this.ParentObject = parentManga;

                this.IsAvailable = new InitialisableProperty<bool>(this.InitInfo);
                this.Date = new InitialisableProperty<DateTime>(this.InitInfo);
                this.Pages = new InitialisableProperty<IEnumerable<Uri>>(this.InitPages);
                this.ScanlatorGroup = new InitialisableProperty<Group>(this.InitInfo);
                this.Titel = new InitialisableProperty<string>(this.InitInfo);
                this.UploaderName = new InitialisableProperty<string>(this.InitInfo);
            }

            internal Chapter([NotNull] Manga parentManga, int chapterNumber, Language lang, bool isOnline,
                [NotNull] Senpai senpai) : this(parentManga, chapterNumber, lang, senpai)
            {
                this.IsAvailable = new InitialisableProperty<bool>(this.InitInfo, isOnline);
            }

            #region Geerbt

            /// <summary>
            /// </summary>
            /// <returns></returns>
            public async Task<ProxerResult<AnimeMangaBookmarkObject<Manga>>> AddToBookmarks(
                UserControlPanel userControlPanel = null)
            {
                userControlPanel = userControlPanel ?? new UserControlPanel(this._senpai);
                return await userControlPanel.AddToBookmarks(this);
            }

            /// <summary>
            /// </summary>
            public Manga ParentObject { get; }

            Language IAnimeMangaContent<Manga>.GeneralLanguage => this.Language;

            /// <summary>
            /// </summary>
            public InitialisableProperty<bool> IsAvailable { get; }

            /// <summary>
            /// </summary>
            public int ContentIndex { get; }

            #endregion

            #region Properties

            /// <summary>
            ///     Gibt das Erscheinungsdatum des <see cref="Chapter">Kapitels</see> zurück.
            /// </summary>
            [NotNull]
            public InitialisableProperty<DateTime> Date { get; }

            /// <summary>
            ///     Gibt die <see cref="Minor.Language">Sprache</see> des <see cref="Chapter">Kapitels</see> zurück.
            /// </summary>
            public Language Language { get; }

            /// <summary>
            ///     Gibt die Links zu den einzelnen Seiten des <see cref="Chapter">Kapitels</see> zurück.
            /// </summary>
            [NotNull]
            public InitialisableProperty<IEnumerable<Uri>> Pages { get; }

            /// <summary>
            ///     Gibt die <see cref="Group">Gruppe</see> zurück, die das Kapitel übersetzt hat.
            /// </summary>
            [NotNull]
            public InitialisableProperty<Group> ScanlatorGroup { get; }

            /// <summary>
            ///     Gibt den Titel des <see cref="Chapter">Kapitels</see> zurück.
            /// </summary>
            [NotNull]
            public InitialisableProperty<string> Titel { get; }

            /// <summary>
            ///     Gibt den Namen des Uploaders des <see cref="Chapter">Kapitels</see> zurück.
            /// </summary>
            [NotNull]
            public InitialisableProperty<string> UploaderName { get; }

            #endregion

            #region

            /// <summary>
            ///     Initialisiert das Objekt.
            /// </summary>
            [ItemNotNull]
            [Obsolete("Bitte benutze die Methoden der jeweiligen Eigenschaften, um sie zu initalisieren!")]
            public async Task<ProxerResult> Init()
            {
                return await this.InitAllInitalisableProperties();
            }

            [ItemNotNull]
            private async Task<ProxerResult> InitInfo()
            {
                HtmlDocument lDocument = new HtmlDocument();
                Func<string, ProxerResult> lCheckFunc = s =>
                {
                    if (!string.IsNullOrEmpty(s) &&
                        s.Equals("Du hast keine Berechtigung um diese Seite zu betreten."))
                        return new ProxerResult(new Exception[] {new NoAccessException(nameof(this.InitInfo))});

                    return new ProxerResult();
                };
                ProxerResult<string> lResult =
                    await
                        HttpUtility.GetResponseErrorHandling(
                            new Uri("https://proxer.me/chapter/" + this.ParentObject.Id + "/" + this.ContentIndex + "/" +
                                    this.Language.ToString().ToLower().Substring(0, 2) + "?format=raw"),
                            null,
                            this._senpai.ErrHandler,
                            this._senpai,
                            new[] {lCheckFunc});

                if (!lResult.Success)
                    return new ProxerResult(lResult.Exceptions);

                string lResponse = lResult.Result;

                if (lResponse == null || lResponse.Contains("Dieses Kapitel ist leider noch nicht verfügbar :/"))
                {
                    this.IsAvailable.SetInitialisedObject(false);
                    return new ProxerResult();
                }

                this.IsAvailable.SetInitialisedObject(true);

                try
                {
                    lDocument.LoadHtml(lResponse);

                    HtmlNode[] lAllHtmlNodes = lDocument.DocumentNode.DescendantsAndSelf().ToArray();

                    if (
                        lAllHtmlNodes.Any(
                            x =>
                                x.Name.Equals("img") && x.HasAttributes &&
                                x.GetAttributeValue("src", "").Equals("/images/misc/stopyui.jpg")))
                        return new ProxerResult(new Exception[] {new CaptchaException()});

                    if (
                        lAllHtmlNodes.Any(
                            x =>
                                x.Name.Equals("img") && x.HasAttributes &&
                                x.GetAttributeValue("src", "").Equals("/images/misc/404.png")))
                        return new ProxerResult(new Exception[] {new WrongResponseException {Response = lResponse}});

                    foreach (
                        HtmlNode childNode in
                            lAllHtmlNodes.First(
                                x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "details")
                                .ChildNodes)
                    {
                        switch (childNode.FirstChild.InnerText)
                        {
                            case "Titel":
                                this.Titel.SetInitialisedObject(childNode.ChildNodes[1].InnerText);
                                break;
                            case "Uploader":
                                this.UploaderName.SetInitialisedObject(childNode.ChildNodes[1].InnerText);
                                break;
                            case "Scanlator-Gruppe":
                                if (childNode.ChildNodes[1].InnerText.Equals("siehe Kapitelcredits"))
                                    this.ScanlatorGroup.SetInitialisedObject(new Group(-1, "siehe Kapitelcredits"));
                                else
                                    this.ScanlatorGroup.SetInitialisedObject(new Group(
                                        Convert.ToInt32(
                                            childNode.ChildNodes[1].FirstChild.GetAttributeValue("href",
                                                "/translatorgroups?id=-1#top")
                                                .GetTagContents("/translatorgroups?id=", "#top")[0]),
                                        childNode.ChildNodes[1].InnerText));
                                break;
                            case "Datum":
                                this.Date.SetInitialisedObject(childNode.ChildNodes[1].InnerText.ToDateTime());
                                break;
                        }
                    }

                    return new ProxerResult();
                }
                catch
                {
                    return new ProxerResult((await ErrorHandler.HandleError(this._senpai, lResponse, false)).Exceptions);
                }
            }

            [ItemNotNull]
            private async Task<ProxerResult> InitPages()
            {
                HtmlDocument lDocument = new HtmlDocument();
                Func<string, ProxerResult> lCheckFunc = s =>
                {
                    if (!string.IsNullOrEmpty(s) &&
                        s.Equals("Du hast keine Berechtigung um diese Seite zu betreten."))
                        return new ProxerResult(new Exception[] {new NoAccessException(nameof(this.InitInfo))});

                    return new ProxerResult();
                };
                ProxerResult<string> lResult =
                    await
                        HttpUtility.GetResponseErrorHandling(
                            new Uri("https://proxer.me/read/" + this.ParentObject.Id + "/" + this.ContentIndex + "/" +
                                    this.Language.ToString().ToLower().Substring(0, 2) + "?format=json"),
                            this._senpai.MobileLoginCookies,
                            this._senpai.ErrHandler,
                            this._senpai,
                            new[] {lCheckFunc});

                if (!lResult.Success)
                    return new ProxerResult(lResult.Exceptions);

                string lResponse = lResult.Result;

                try
                {
                    lDocument.LoadHtml(lResponse);

                    HtmlNode[] lAllHtmlNodes = lDocument.DocumentNode.DescendantsAndSelf().ToArray();

                    if (
                        lAllHtmlNodes.Any(
                            x =>
                                x.Name.Equals("img") && x.HasAttributes &&
                                x.GetAttributeValue("src", "").Equals("/images/misc/stopyui.jpg")))
                        return new ProxerResult(new Exception[] {new CaptchaException()});

                    if (
                        lAllHtmlNodes.Any(
                            x =>
                                x.Name.Equals("img") && x.HasAttributes &&
                                x.GetAttributeValue("src", "").Equals("/images/misc/404.png")))
                        return new ProxerResult(new Exception[] {new WrongResponseException {Response = lResponse}});

                    this.Pages.SetInitialisedObject(
                        (from s in lDocument.DocumentNode.ChildNodes[1].InnerText.Split(';')[0].GetTagContents("[", "]")
                            where !s.StartsWith("[")
                            select new Uri("http://upload.proxer.me/manga/" + this.ParentObject.Id + "_" +
                                           this.Language.ToString().ToLower().Substring(0, 2) + "/" + this.ContentIndex +
                                           "/" +
                                           s.GetTagContents("\"", "\"")[0])).ToArray());

                    return new ProxerResult();
                }
                catch
                {
                    return new ProxerResult((await ErrorHandler.HandleError(this._senpai, lResponse, false)).Exceptions);
                }
            }

            /// <summary>
            ///     Returns a string that represents the current object.
            /// </summary>
            /// <returns>
            ///     A string that represents the current object.
            /// </returns>
            public override string ToString()
            {
                return "Chapter " + this.ContentIndex;
            }

            #endregion
        }
    }
}