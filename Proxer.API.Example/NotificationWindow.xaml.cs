﻿using System.Linq;
using System.Windows;
using Proxer.API.EventArguments;
using Proxer.API.Exceptions;
using Proxer.API.Notifications;
using Proxer.API.Utilities.Net;

namespace Proxer.API.Example
{
    /// <summary>
    ///     Interaktionslogik für NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        private readonly Senpai _senpai;
        private int _unseenNotifications;

        internal NotificationWindow(Senpai senpai)
        {
            this._senpai = senpai;

            if (!this._senpai.LoggedIn)
            {
                MessageBox.Show("Du musst für diese Aktione eingeloggt sein!");
                return;
            }

            this.InitializeComponent();

            this._senpai.NotificationRaised += this.NotificationRaised;
            this._senpai.AmUpdateNotificationRaised += this.AmUpdateNotificationRaised;
        }

        #region

        private void NotificationRaised(Senpai sender, INotificationEventArgs e)
        {
            if (this.IsFocused) return;

            this._unseenNotifications += e.NotificationCount;
            this.Title = "Benachrichtigungen (" + this._unseenNotifications + ")";

            //man kann auch einen Ton abspielen
        }

        private async void AmUpdateNotificationRaised(Senpai sender, AmNotificationEventArgs e)
        {
            this.AMTextBox.Clear();
            this.AMTextBox.IsReadOnly = false;

            ProxerResult<AnimeMangaUpdateObject[]> lResult = await e.Benachrichtigungen.GetAllAnimeMangaUpdates();
            if (!lResult.Success)
            {
                MessageBox.Show(lResult.Exceptions.OfType<NotLoggedInException>().Any()
                    ? "Bitte logge dich ein bevor du fortfährst!"
                    : "Es ist ein Fehler während der Anfrage aufgetreten!");

                new LoginWindow().Show();
                this.Close();
                return;
            }

            foreach (AnimeMangaUpdateObject notification in lResult.Result)
            {
                this.AMTextBox.Text += notification.Name + "#" + notification.Number + " ist jetzt online! ( " +
                                       notification.Link.AbsoluteUri + " )\n\n";
            }
            this.AMTextBox.IsReadOnly = true;
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            this._unseenNotifications = 0;
            this.Title = "Benachrichtigungen";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await this._senpai.InitNotifications();

            this.AMTextBox.IsReadOnly = false;

            ProxerResult<AnimeMangaUpdateObject[]> lResult =
                await this._senpai.AnimeMangaUpdates.GetAllAnimeMangaUpdates();
            if (!lResult.Success)
            {
                MessageBox.Show(lResult.Exceptions.OfType<NotLoggedInException>().Any()
                    ? "Bitte logge dich ein bevor du fortfährst!"
                    : "Es ist ein Fehler während der Anfrage aufgetreten!");

                new LoginWindow().Show();
                this.Close();
                return;
            }

            foreach (AnimeMangaUpdateObject notification in lResult.Result)
            {
                this.AMTextBox.Text += notification.Name + " #" + notification.Number + " ist jetzt online! ( " +
                                       notification.Link.AbsoluteUri + " )\n\n";
            }
            this.AMTextBox.IsReadOnly = true;
        }

        #endregion
    }
}