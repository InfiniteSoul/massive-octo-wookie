﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Azuria.Main;
using Azuria.Main.User;

namespace Azuria.Example.Controls
{
    public partial class AnimeMangaProgressControl : UserControl
    {
        private readonly Senpai _senpai;

        public AnimeMangaProgressControl(AnimeMangaProgressObject animeMangaProgressObject, Senpai senpai)
        {
            this._senpai = senpai;
            //Die Werte der Controls werden mit Bindings gelöst
            this.AnimeMangaProgressObject = animeMangaProgressObject;
            this.InitializeComponent();
        }

        public AnimeMangaProgressControl(IAnimeMangaObject animeMangaObject, Senpai senpai)
        {
            this._senpai = senpai;
            //Die Werte der Controls werden mit Bindings gelöst
            //Dies ist eine Möglichkeit einen Favoriten darzustellen (nicht empfohlen)
            this.AnimeMangaProgressObject = new AnimeMangaProgressObject(User.System, animeMangaObject, -1, -1,
                AnimeMangaProgress.Finished);
            this.InitializeComponent();
        }

        #region Properties

        public AnimeMangaProgressObject AnimeMangaProgressObject { get; set; }

        #endregion

        #region

        private void ProgressUserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            new AnimeMangaWindow(this.AnimeMangaProgressObject.AnimeMangaObject, this._senpai).Show();
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
        }

        #endregion
    }
}