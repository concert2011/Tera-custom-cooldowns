﻿using System;
using System.Windows;
using System.Windows.Media.Animation;
using TCC.ViewModels;

namespace TCC.Controls.Classes
{
    /// <summary>
    /// Logica di interazione per NinjaLayout.xaml
    /// </summary>
    public partial class NinjaLayout
    {
        private NinjaLayoutVM _dc;
        private DoubleAnimation _an;

        public NinjaLayout()
        {
            InitializeComponent();
        }

        private void NinjaLayout_OnLoaded(object sender, RoutedEventArgs e)
        {
            _dc = (NinjaLayoutVM)DataContext;
            _an = new DoubleAnimation(_dc.StaminaTracker.Factor * 359.99, TimeSpan.FromMilliseconds(150));

            _dc.StaminaTracker.PropertyChanged += ST_PropertyChanged;
        }

        private void ST_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(_dc.StaminaTracker.Factor)) return;
            _an.To = _dc.StaminaTracker.Factor * (359.99 - 80) + 40;
            MainReArc.BeginAnimation(Arc.EndAngleProperty, _an);
        }
    }
}
