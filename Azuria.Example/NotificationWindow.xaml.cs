﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Azuria.Community;
using Azuria.ErrorHandling;
using Azuria.EventArguments;
using Azuria.Example.Utilities;
using Azuria.Exceptions;
using Azuria.Main;
using Azuria.Notifications;

namespace Azuria.Example
{
    public partial class NotificationWindow : Window
    {
        private readonly Senpai _senpai;

        internal NotificationWindow(Senpai senpai)
        {
            this._senpai = senpai;

            //Überprüfen, ob der Senpai eingeloggt ist, sonst werden die Events nicht abgerufen
            if (!this._senpai.LoggedIn)
            {
                MessageBox.Show("Du musst für diese Aktion eingeloggt sein!");
                return;
            }

            this.InitializeComponent();

            this._senpai.NotificationRaised += this.NotificationRaised;
            this._senpai.AmUpdateNotificationRaised += this.AmUpdateNotificationRaised;
            this._senpai.FriendNotificationRaised += this.FriendNotificationRaised;
            this._senpai.NewsNotificationRaised += this.NewsNotificationRaised;
            this._senpai.PmNotificationRaised += this.PmNotificationRaised;
            this._senpai.ErrorDuringNotificationFetch += this.ErrorDuringNotificationFetch;
        }

        #region

        private async void AmTextBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                AnimeMangaUpdateObject lUpdateObject = (sender as ListBox).SelectedItem as AnimeMangaUpdateObject;

                if (lUpdateObject == null) return;

                //Speicher die ID des Anime oder Manga ab
                int lAnimeMangaId =
                    Convert.ToInt32(Utility.GetTagContents(lUpdateObject.Link.OriginalString, "/watch/",
                        "/" + lUpdateObject.Number + "/").First());
                //Rufe den Anime oder Manga der Benachrichtigung ab
                IAnimeMangaObject lAnimeMangaObject =
                    (await ProxerClass.GetAnimeManga(lAnimeMangaId, this._senpai)).OnError(null);
                //Öffne das Anime-/Manga-Fenster
                new AnimeMangaWindow(lAnimeMangaObject).Show();
            }
        }

        private void AmUpdateNotificationRaised(Senpai sender, AmNotificationEventArgs e)
        {
            //e enthält die Anzahl der Benachrichtigungen(NotificationCount) 
            //als auch die Benachrichtigungen an sich(Benachrichtigungen)
            this.LoadAmUpdateNotifications(e.Benachrichtigungen);
        }

        private void ErrorDuringNotificationFetch(Senpai sender, IEnumerable<Exception> exceptions)
        {
            //Aktion, die ausgeführt wird, wenn ein Fehler bei dem Abrufen der Benachrichtigungs-Events aufgetreten ist
        }

        private void FriendNotificationRaised(Senpai sender, FriendNotificationEventArgs e)
        {
            //e enthält die Anzahl der Benachrichtigungen(NotificationCount) 
            //als auch die Benachrichtigungen an sich(Benachrichtigungen)
            this.ProcessFriendRequests(e.Benachrichtigungen);
        }

        private async void LoadAmUpdateNotifications(AnimeMangaUpdateCollection collection)
        {
            //Läd die Anime- und Manga-Benachrichtigungen und stellt sie in einer Liste dar.

            //WICHTIG: Das Event, dass diese Methode aufruft läuft auf einem anderen Thread, da es von 
            //einem Timer ausgelöst wird
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.LoadAmUpdateNotifications(collection));
                return;
            }

            this.AmListBox.Items.Clear();

            //Rufe die Benachrichtigungen ab
            ProxerResult<IEnumerable<AnimeMangaUpdateObject>> lResult = await collection.GetAllAnimeMangaUpdates();
            if (!lResult.Success)
            {
                //Falls die Methode fehlschlägt kann hier überprüft werden, was der Grund ist.

                //Beispiel: Wenn die Aufzählung der Ausnahmen von lResult eine NotLoggedInException enthält, 
                //dann ist der Benutzer nicht eingeloggt
                MessageBox.Show(lResult.Exceptions.OfType<NotLoggedInException>().Any()
                    ? "Bitte logge dich ein bevor du fortfährst!"
                    : "Es ist ein Fehler während der Anfrage aufgetreten! (LoadAmUpdateNotifications)");

                return;
            }

            //Wenn die Aktion erfolgreich war füge die Benachrichtigungen der ListBox hinzu
            foreach (AnimeMangaUpdateObject notification in lResult.Result)
            {
                this.AmListBox.Items.Add(notification);
            }
        }

        private async void LoadNewsNotifications(NewsCollection collection)
        {
            //Läd die letzten 15 News und stellt sie in einer ListBox dar.

            //WICHTIG: Das Event, dass diese Methode aufruft läuft auf einem anderen Thread, da es von 
            //einem Timer ausgelöst wird
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.LoadNewsNotifications(collection));
                return;
            }

            this.NewsListBox.Items.Clear();

            //Rufe die letzten 15 News ab
            //Durch Limitationen des hauseigenen Proxer API können mit einer Anfrage nicht mehr News 
            //auf einmal abgerufen werden
            ProxerResult<IEnumerable<NewsObject>> lResult = await collection.GetAllNews();
            if (!lResult.Success)
            {
                //Falls die Methode fehlschlägt kann hier überprüft werden was der Grund ist.

                //Beispiel: Wenn die Aufzählung der Ausnahmen von lResult eine NotLoggedInException enthält, 
                //dann ist der Benutzer nicht eingeloggt
                MessageBox.Show(lResult.Exceptions.OfType<NotLoggedInException>().Any()
                    ? "Bitte logge dich ein bevor du fortfährst!"
                    : "Es ist ein Fehler während der Anfrage aufgetreten! (LoadNewsNotifications)");

                return;
            }

            //Wenn die Methode erfolgreich war stelle die Benachrichtigungen in einer ListBox dar.
            foreach (NewsObject notification in lResult.Result)
            {
                this.NewsListBox.Items.Add(notification);
            }
        }

        private async void LoadPmNotifications(PmCollection collection)
        {
            //Läd alle Privatnachricht-Benachrichtigungen und stellt sie in einer ListBox dar.

            //WICHTIG: Das Event, dass diese Methode aufruft läuft auf einem anderen Thread, da es von 
            //einem Timer ausgelöst wird
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.LoadPmNotifications(collection));
                return;
            }

            this.PmListBox.Items.Clear();

            //Ruft alle Privatnachricht-Benachrichtigungen ab
            ProxerResult<IEnumerable<PmObject>> lResult = await collection.GetAllPrivateMessages();
            if (!lResult.Success)
            {
                //Falls die Methode fehlschlägt kann hier überprüft werden was der Grund ist.

                //Beispiel: Wenn die Aufzählung der Ausnahmen von lResult eine NotLoggedInException enthält, 
                //dann ist der Benutzer nicht eingeloggt
                MessageBox.Show(lResult.Exceptions.OfType<NotLoggedInException>().Any()
                    ? "Bitte logge dich ein bevor du fortfährst!"
                    : "Es ist ein Fehler während der Anfrage aufgetreten! (LoadPmNotifications)");

                return;
            }

            //Wenn die Methode erfolgreich war stelle die Benachrichtigungen in einer ListBox dar.
            foreach (PmObject notification in lResult.Result)
            {
                this.PmListBox.Items.Add(notification);
            }
        }

        private void NewsListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                MessageBox.Show(((sender as ListBox).SelectedItem as NewsObject).NewsId.ToString());
            }
        }

        private void NewsNotificationRaised(Senpai sender, NewsNotificationEventArgs e)
        {
            //e enthält die Anzahl der Benachrichtigungen(NotificationCount) 
            //als auch die Benachrichtigungen an sich(Benachrichtigungen)
            this.LoadNewsNotifications(e.Benachrichtigungen);
        }

        private void NotificationRaised(Senpai sender, IEnumerable<INotificationEventArgs> e)
        {
            //Wird nach dem Überprüfen aller Benachrichtigungen einmal aufgerufen, 
            //wenn mindestens eine Benachrichtigung verfügbar ist.
            //Der Parameter e enthält alle EventArgs, die in den Benachrichtigungs-Events 
            //zuvor aufgerufen wurden.

            //Wird benötigt, da die Methode von einem anderen Thread aufgerufen wird (von Timer)
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.NotificationRaised(sender, e));
                return;
            }

            if (this.IsFocused) return;

            IEnumerable<INotificationEventArgs> notificationEventArgses = e as IList<INotificationEventArgs> ??
                                                                          e.ToList();
            if (notificationEventArgses.Any())
            {
                int lAnzahl = 0;
                notificationEventArgses.ToList().ForEach(x => lAnzahl += x.NotificationCount);
                this.Title = "Benachrichtigungen (" + lAnzahl + ")";
            }
            else
            {
                this.Title = "Benachrichtigungen";
            }

            //man kann hier auch einen Ton abspielen
        }

        private void PmListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Conference lConference = new Conference(((sender as ListBox).SelectedItem as PmObject).Id, this._senpai);
                new ConferenceWindow(lConference, this._senpai).Show();
            }
        }

        private void PmNotificationRaised(Senpai sender, PmNotificationEventArgs e)
        {
            //e enthält die Anzahl der Benachrichtigungen(NotificationCount) 
            //als auch die Benachrichtigungen an sich(Benachrichtigungen)
            this.LoadPmNotifications(e.Benchrichtigungen);
        }

        private async void ProcessFriendRequests(FriendRequestCollection collection)
        {
            //Läd die Freundschaftsanfragen und fragt den Benutzer, ob er sie annehmen will

            //WICHTIG: Das Event, dass diese Methode aufruft läuft auf einem anderen Thread, da es von 
            //einem Timer ausgelöst wird
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.ProcessFriendRequests(collection));
                return;
            }

            //Rufe die Freundschaftsanfragen ab
            ProxerResult<IEnumerable<FriendRequestObject>> lResult = await collection.GetAllFriendRequests();
            if (!lResult.Success)
            {
                //Falls die Methode fehlschlägt kann hier überprüft werden was der Grund ist.

                //Beispiel: Wenn die Aufzählung der Ausnahmen von lResult eine NotLoggedInException enthält, 
                //dann ist der Benutzer nicht eingeloggt
                MessageBox.Show(lResult.Exceptions.OfType<NotLoggedInException>().Any()
                    ? "Bitte logge dich ein bevor du fortfährst!"
                    : "Es ist ein Fehler während der Anfrage aufgetreten! (ProcessFriendRequests)");

                return;
            }

            //Wenn die Aktion erfolgreich war gehe durch die Liste der Freundschaftsanfragen 
            //und frage den Benutzer, ob er sie annehmen will
            foreach (FriendRequestObject requestObject in lResult.Result)
            {
                new UserWindow(new User(requestObject.UserId, this._senpai), this._senpai).Show();
                MessageBoxResult lBoxResult =
                    MessageBox.Show(
                        "Möchtest du die Freundschaftsanfrage von " + requestObject.UserName + " [ID:" +
                        requestObject.UserId + "] annehmen?", "Freundschaftsanfrage", MessageBoxButton.YesNoCancel);

                switch (lBoxResult)
                {
                    case MessageBoxResult.Yes:
                        //Benutzer hat die Freundschaftsanfrage akzeptiert
                        ProxerResult<bool> lAcceptResult = await requestObject.AcceptRequest();
                        if (lAcceptResult.Success && lAcceptResult.Result)
                            MessageBox.Show("Die Freundschaftsanfrage wurde erfolgreich angenommen!");
                        else
                            MessageBox.Show("Es ist ein Fehler passiert!");
                        break;
                    case MessageBoxResult.No:
                        //Benutzer hat die Freundschaftsanfrage abgelehnt
                        ProxerResult<bool> lDenyResult = await requestObject.DenyRequest();
                        if (lDenyResult.Success && lDenyResult.Result)
                            MessageBox.Show("Die Freundschaftsanfrage wurde erfolgreich abgelehnt!");
                        else
                            MessageBox.Show("Es ist ein Fehler passiert!");
                        break;
                }
            }
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Title = "Benachrichtigungen";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Muss aufgerufen werden, damit die Senpai-Klasse anfängt im Hintergrund 
            //die Benachrichtigungen abzurufen (alle 15 Minuten)
            this._senpai.InitNotifications();

            //Lade alle Inhalte der Benachrichtigungen des Benutzers und stelle sie da.
            //Muss gemacht werden, da die Events erst nach einer Zeit aktiviert werden.
            this.LoadAmUpdateNotifications(this._senpai.AnimeMangaUpdates);
            this.ProcessFriendRequests(this._senpai.FriendRequests);
            this.LoadNewsNotifications(this._senpai.News);
            this.LoadPmNotifications(this._senpai.PrivateMessages);
        }

        #endregion
    }
}