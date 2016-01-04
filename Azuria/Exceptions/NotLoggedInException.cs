﻿using System;

namespace Azuria.Exceptions
{
    /// <summary>
    ///     Stellt einen Fehler da, der ausgelöst wird, wenn der Benutzer nicht eingeloggt ist, die Methode dies aber
    ///     erfordert.
    /// </summary>
    public class NotLoggedInException : Exception
    {
        /// <summary>
        ///     Initialisiert eine neue Instanz der <see cref="NotLoggedInException" />-Klasse.
        /// </summary>
        public NotLoggedInException()
        {
        }

        /// <summary>
        ///     Initialisiert eine neue Instanz der <see cref="NotLoggedInException" />-Klasse.
        /// </summary>
        /// <param name="senpai">Der Benutzer, der nicht eingeloggt ist.</param>
        public NotLoggedInException(Senpai senpai)
        {
            this.Senpai = senpai;
        }

        /// <summary>
        ///     Initialisiert eine neue Instanz der <see cref="NotLoggedInException" />-Klasse mit einer angegebenen Fehlermeldung.
        /// </summary>
        /// <param name="message">Die Fehlermeldung, in der die Ursache der Ausnahme erklärt wird.</param>
        public NotLoggedInException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Initialisiert eine neue Instanz der <see cref="NotLoggedInException" />-Klasse mit einer
        ///     angegebenen Fehlermeldung und einem Verweis auf die innere Ausnahme, die diese Ausnahme verursacht hat.
        /// </summary>
        /// <param name="message">Die Fehlermeldung, in der die Ursache der Ausnahme erklärt wird.</param>
        /// <param name="inner">
        ///     Die Ausnahme, die die aktuelle Ausnahme ausgelöst hat, oder ein Nullverweis (Nothing in Visual Basic),
        ///     wenn keine innere Ausnahme angegeben ist.
        /// </param>
        public NotLoggedInException(string message, Exception inner) : base(message, inner)
        {
        }

        #region Properties

        /// <summary>
        ///     Gibt den <see cref="Senpai" /> zurück, der mit dem Fehler in Verbindung steht.
        /// </summary>
        public Senpai Senpai { get; set; }

        #endregion
    }
}