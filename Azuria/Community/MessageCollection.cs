﻿using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Azuria.Community
{
    /// <summary>
    /// </summary>
    public class MessageCollection : IEnumerable<Message>
    {
        private readonly int _conferenceId;
        [NotNull] private readonly Senpai _senpai;

        internal MessageCollection(int conferenceId, [NotNull] Senpai senpai)
        {
            this._conferenceId = conferenceId;
            this._senpai = senpai;
        }

        #region Inherited

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<Message> GetEnumerator()
        {
            return new MessageEnumerator(this._conferenceId, this._senpai);
        }

        #endregion
    }
}