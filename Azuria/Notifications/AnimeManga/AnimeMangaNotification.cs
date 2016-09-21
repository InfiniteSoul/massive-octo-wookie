﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azuria.ErrorHandling;
using Azuria.Media;
using Azuria.Media.Properties;

namespace Azuria.Notifications.AnimeManga
{
    /// <summary>
    /// Represents an <see cref="Anime" />- or <see cref="Manga" />-notification.
    /// </summary>
    public class AnimeMangaNotification<T> : INotification where T : IAnimeMangaObject
    {
        internal AnimeMangaNotification(int notificationId, T animeMangaObject, int contentIndex,
            AnimeMangaLanguage language, DateTime timeStamp, Senpai senpai)
        {
            this.AnimeMangaObject = animeMangaObject;
            this.ContentIndex = contentIndex;
            this.Language = language;
            this.NotificationId = notificationId;
            this.Senpai = senpai;
            this.TimeStamp = timeStamp;
        }

        #region Properties

        /// <summary>
        /// </summary>
        public T AnimeMangaObject { get; set; }

        /// <summary>
        /// </summary>
        public int ContentIndex { get; set; }

        /// <summary>
        /// </summary>
        public AnimeMangaLanguage Language { get; set; }

        /// <summary>
        /// </summary>
        public int NotificationId { get; }

        string INotification.NotificationId => this.NotificationId.ToString();

        /// <inheritdoc />
        public Senpai Senpai { get; }

        /// <summary>
        /// </summary>
        public DateTime TimeStamp { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public Task<ProxerResult> Delete()
        {
            return AnimeMangaNotificationManager.Create(this.Senpai).DeleteNotification(this.NotificationId);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task<ProxerResult<IAnimeMangaContent<T>>> GetContentObject()
        {
            if (this.AnimeMangaObject is Anime)
            {
                ProxerResult<IEnumerable<Anime.Episode>> lEpisodesResult =
                    await (this.AnimeMangaObject as Anime).GetEpisodes((AnimeLanguage) this.Language);
                if (!lEpisodesResult.Success || (lEpisodesResult.Result == null))
                    return new ProxerResult<IAnimeMangaContent<T>>(lEpisodesResult.Exceptions);

                return
                    new ProxerResult<IAnimeMangaContent<T>>(
                        lEpisodesResult.Result.FirstOrDefault(episode => episode.ContentIndex == this.ContentIndex) as
                            IAnimeMangaContent<T>);
            }
            if (this.AnimeMangaObject is Manga)
            {
                ProxerResult<IEnumerable<Manga.Chapter>> lChaptersResult =
                    await (this.AnimeMangaObject as Manga).GetChapters((Language) this.Language);
                if (!lChaptersResult.Success || (lChaptersResult.Result == null))
                    return new ProxerResult<IAnimeMangaContent<T>>(lChaptersResult.Exceptions);

                return
                    new ProxerResult<IAnimeMangaContent<T>>(
                        lChaptersResult.Result.FirstOrDefault(chapter => chapter.ContentIndex == this.ContentIndex) as
                            IAnimeMangaContent<T>);
            }

            return new ProxerResult<IAnimeMangaContent<T>>(new Exception[0]);
        }

        #endregion
    }
}