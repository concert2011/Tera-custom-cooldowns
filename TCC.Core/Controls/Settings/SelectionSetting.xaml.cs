﻿using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TCC.Controls.Settings
{
    /// <summary>
    /// Logica di interazione per SelectionSetting.xaml
    /// </summary>
    public partial class SelectionSetting
    {
        public string SettingName
        {
            get => (string)GetValue(SettingNameProperty);
            set => SetValue(SettingNameProperty, value);
        }
        public static readonly DependencyProperty SettingNameProperty =
            DependencyProperty.Register("SettingName", typeof(string), typeof(SelectionSetting));

        public Geometry SvgIcon
        {
            get => (Geometry)GetValue(SvgIconProperty);
            set => SetValue(SvgIconProperty, value);
        }
        public static readonly DependencyProperty SvgIconProperty =
            DependencyProperty.Register("SvgIcon", typeof(Geometry), typeof(SelectionSetting));


        public IEnumerable Choices
        {
            get => (IEnumerable)GetValue(ChoicesProperty);
            set => SetValue(ChoicesProperty, value);
        }
        public static readonly DependencyProperty ChoicesProperty =
            DependencyProperty.Register("Choices", typeof(IEnumerable), typeof(SelectionSetting));

        public string SelectedItem
        {
            get => (string)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(string), typeof(SelectionSetting));

        public Type ChoicesType
        {
            get => (Type)GetValue(ChoicesTypeProperty);
            set => SetValue(ChoicesTypeProperty, value);
        }
        public static readonly DependencyProperty ChoicesTypeProperty =
            DependencyProperty.Register("ChoicesType", typeof(Type), typeof(SelectionSetting));



        public SelectionSetting()
        {
            InitializeComponent();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = (ComboBox)sender;
            SelectedItem = cb.SelectedItem.ToString();
        }

        private void SelectionSetting_OnLoaded(object sender, RoutedEventArgs e)
        {
            var i = 0;
            if(Choices == null) return;
            foreach (var choice in Choices)
            {
                if (choice.ToString() == SelectedItem)
                {
                    Cbox.SelectedIndex = i;
                }
                i++;
            }

        }

        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            Cbox.IsDropDownOpen = true;
        }
    }
}
