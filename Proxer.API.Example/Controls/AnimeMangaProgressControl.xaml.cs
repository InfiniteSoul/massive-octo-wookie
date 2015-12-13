﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Proxer.API.Main.User;

namespace Proxer.API.Example.Controls
{
    public partial class AnimeMangaProgressControl : UserControl
    {
        public AnimeMangaProgressControl(AnimeMangaProgressObject animeMangaProgressObject)
        {
            //Schreibt das AnimeMangaProgressObject in die zugehörige Eigenschaft
            //Die Werte der Controls werden mit Bindings gelöst
            this.AnimeMangaProgressObject = animeMangaProgressObject;
            this.InitializeComponent();
        }

        #region Properties

        public AnimeMangaProgressObject AnimeMangaProgressObject { get; set; }

        #endregion

        #region

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ProgressUserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            new AnimeMangaWindow(this.AnimeMangaProgressObject.AnimeMangaObject).Show();
        }

        #endregion
    }
}