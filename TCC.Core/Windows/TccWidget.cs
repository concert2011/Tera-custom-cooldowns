﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Controls;
using TCC.Controls;
using TCC.Data;
using TCC.Settings;

namespace TCC.Windows
{
    public class TccWidget : Window
    {
        private bool _ignoreSize;
        private bool _undimOnFlyingGuardian;
        private DoubleAnimation _opacityAnimation;
        private DoubleAnimation _hideButtons;
        private DoubleAnimation _showButtons;
        private DispatcherTimer _buttonsTimer;

        protected WindowButtons ButtonsRef;
        protected UIElement MainContent;

        public WindowSettings WindowSettings { get; private set; }

        public IntPtr Handle { get; private set; }
        public bool CanMove { get; set; } = true;
        public void ReloadPosition()
        {
            Dispatcher.Invoke(() =>
            {
                Left = WindowSettings.X * SettingsHolder.ScreenW;
                Top = WindowSettings.Y * SettingsHolder.ScreenH;

                //if(_settings.Name == nameof(WindowManager.CharacterWindow)) Console.WriteLine($"Reloading {_settings.Name}: {_settings.X}, {_settings.Y}");
                CheckBounds();

                if (ButtonsRef != null)
                {
                    switch (WindowSettings.ButtonsPosition)
                    {
                        case ButtonsPosition.Above:
                            Grid.SetRow(ButtonsRef, 0);
                            break;
                        case ButtonsPosition.Below:
                            Grid.SetRow(ButtonsRef, 2);
                            break;
                    }
                    UpdateButtons();
                }
            });
        }
        public void ResetToCenter()
        {
            Dispatcher.Invoke(() =>
            {
                Left = Screen.PrimaryScreen.Bounds.X + Screen.PrimaryScreen.Bounds.Width / 2 - ActualWidth / 2;
                Top = Screen.PrimaryScreen.Bounds.Y + Screen.PrimaryScreen.Bounds.Height / 2 - ActualHeight / 2;
                WindowSettings.X = Left / SettingsHolder.ScreenW;
                WindowSettings.Y = Top / SettingsHolder.ScreenH;
            });
        }
        protected void Init(WindowSettings settings, bool ignoreSize = true, bool undimOnFlyingGuardian = true, bool perClassPosition = true)
        {
            settings.PerClassPosition = perClassPosition;
            WindowSettings = settings;
            _ignoreSize = ignoreSize;
            _undimOnFlyingGuardian = undimOnFlyingGuardian;

            _opacityAnimation = new DoubleAnimation { Duration = TimeSpan.FromMilliseconds(100) };

            MainContent.Opacity = 0;
            Topmost = true;
            Left = settings.X * SettingsHolder.ScreenW;
            Top = settings.Y * SettingsHolder.ScreenH;
            CheckBounds();
            if (!ignoreSize)
            {
                if (settings.H != 0) Height = settings.H;
                if (settings.W != 0) Width = settings.W;
            }

            WindowSettings.EnabledChanged += OnEnabledChanged;
            WindowSettings.ClickThruModeChanged += OnClickThruModeChanged;
            WindowSettings.VisibilityChanged += OnWindowVisibilityChanged;
            WindowSettings.ResetToCenter += ResetToCenter;

            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;

            WindowManager.ForegroundManager.VisibilityChanged += OnVisibilityChanged;
            WindowManager.ForegroundManager.DimChanged += OnDimChanged;
            WindowManager.ForegroundManager.ClickThruChanged += OnClickThruModeChanged;
            FocusManager.FocusTick += OnFocusTick;

            //if (WindowSettings.Enabled) Show();
            OnClickThruModeChanged();
            OnVisibilityChanged();
            OnWindowVisibilityChanged();

            FocusManager.MakeUnfocusable(Handle);


            if (ButtonsRef == null)
            {
                if(CanMove) MouseLeftButtonDown += Drag;
                return;
            }

            _hideButtons = new DoubleAnimation(0, TimeSpan.FromMilliseconds(1000));
            _showButtons = new DoubleAnimation(1, TimeSpan.FromMilliseconds(150));
            _buttonsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _buttonsTimer.Tick += OnButtonsTimerTick;

            MouseEnter += (_, __) =>
            {
                if (!SettingsHolder.HideHandles) ButtonsRef.BeginAnimation(OpacityProperty, _showButtons);
            };
            MouseLeave += (_, __) => _buttonsTimer.Start();
            if (CanMove) ButtonsRef.MouseLeftButtonDown += Drag;
        }

        private void OnFocusTick()
        {
            if (WindowManager.ForegroundManager.Visible) RefreshTopmost();
        }

        private void OnWindowVisibilityChanged()
        {
            SetVisibility(WindowSettings.Visible);
        }

        private void OnButtonsTimerTick(object sender, EventArgs e)
        {
            _buttonsTimer.Stop();
            if (IsMouseOver) return;
            ButtonsRef.BeginAnimation(OpacityProperty, _hideButtons);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!WindowSettings.AllowOffScreen) CheckBounds();
            if (_ignoreSize) return;
            if (WindowSettings.W != ActualWidth ||
                WindowSettings.H != ActualHeight)
            {
                WindowSettings.W = ActualWidth;
                WindowSettings.H = ActualHeight;
                if (!App.Loading) SettingsWriter.Save();
            }
        }

        protected void OnLoaded(object sender, RoutedEventArgs e)
        {
            Handle = new WindowInteropHelper(this).Handle;
            FocusManager.MakeUnfocusable(Handle);
            FocusManager.HideFromToolBar(Handle);
            if (!WindowSettings.Enabled) Hide();
        }
        private void OnDimChanged()
        {
            if (!WindowManager.ForegroundManager.Visible) return;

            if (!WindowSettings.AutoDim)
                AnimateContentOpacity(1);
            else
            {
                if (_undimOnFlyingGuardian) AnimateContentOpacity(WindowManager.ForegroundManager.Dim ? WindowSettings.DimOpacity : 1);
                else if (FlyingGuardianDataProvider.IsInProgress) AnimateContentOpacity(WindowSettings.DimOpacity);
                else AnimateContentOpacity(WindowManager.ForegroundManager.Dim ? WindowSettings.DimOpacity : 1);
            }

            OnClickThruModeChanged();
        }
        private void OnVisibilityChanged()
        {
            if (WindowManager.ForegroundManager.Visible)
            {
                if (WindowManager.ForegroundManager.Dim && WindowSettings.AutoDim)
                {
                    AnimateContentOpacity(WindowSettings.DimOpacity);
                }
                else
                {
                    AnimateContentOpacity(1);
                }
                RefreshTopmost();
            }
            else
            {
                if (WindowSettings.ShowAlways) return;
                AnimateContentOpacity(0);
            }
        }
        private void OnClickThruModeChanged()
        {
            switch (WindowSettings.ClickThruMode)
            {
                case ClickThruMode.Never:
                    FocusManager.UndoClickThru(Handle);
                    break;
                case ClickThruMode.Always:
                    FocusManager.MakeClickThru(Handle);
                    break;
                case ClickThruMode.WhenDim:
                    if (WindowManager.ForegroundManager.Dim) FocusManager.MakeClickThru(Handle);
                    else FocusManager.UndoClickThru(Handle);
                    break;
                case ClickThruMode.WhenUndim:
                    if (WindowManager.ForegroundManager.Dim) FocusManager.UndoClickThru(Handle);
                    else FocusManager.MakeClickThru(Handle);
                    break;
                case ClickThruMode.GameDriven:
                    if (SessionManager.InGameUiOn) FocusManager.UndoClickThru(Handle);
                    else FocusManager.MakeClickThru(Handle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        protected void OnEnabledChanged()
        {
            try
            {
                if (WindowSettings.Enabled) Show();
                else Hide();
            }
            catch { }
        }
        private void AnimateContentOpacity(double opacity)
        {
            if (MainContent == null) return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _opacityAnimation.To = opacity;
                MainContent.BeginAnimation(OpacityProperty, _opacityAnimation);
            })
            , DispatcherPriority.DataBind);
        }
        private void RefreshTopmost()
        {
            if (FocusManager.PauseTopmost) return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Topmost = false; Topmost = true;
            }), DispatcherPriority.DataBind);
        }
        private void SetVisibility(bool v)
        {
            if (!Dispatcher.Thread.IsAlive) return;
            Dispatcher.Invoke(() =>
            {
                Visibility = !v ? Visibility.Visible : Visibility.Collapsed; // meh ok
                Visibility = v ? Visibility.Visible : Visibility.Collapsed;
            });
        }
        private void CheckBounds()
        {
            if (WindowSettings.AllowOffScreen) return;
            if (Left + ActualWidth > SettingsHolder.ScreenW)
            {
                Left = SettingsHolder.ScreenW - ActualWidth;
            }
            if (Top + ActualHeight > SettingsHolder.ScreenH)
            {
                Top = SettingsHolder.ScreenH - ActualHeight;
            }
            CheckIndividualScreensBounds();

            WindowSettings.X = Left / SettingsHolder.ScreenW;
            WindowSettings.Y = Top / SettingsHolder.ScreenH;
        }

        private void CheckIndividualScreensBounds()
        {
            if (IsWindowFullyVisible()) return;
            var nearestScreen = FindNearestScreen();

            if (Top + ActualHeight > nearestScreen.Bounds.Y + nearestScreen.Bounds.Height) Top = nearestScreen.Bounds.Y + nearestScreen.Bounds.Height - ActualHeight;
            else if (Top < nearestScreen.Bounds.Y) Top = nearestScreen.Bounds.Y;
            if (Left + ActualWidth > nearestScreen.Bounds.X + nearestScreen.Bounds.Width) Left = nearestScreen.Bounds.X + nearestScreen.Bounds.Width - ActualWidth;
            else if (Left < nearestScreen.Bounds.X) Left = nearestScreen.Bounds.X;
        }

        private Screen FindNearestScreen()
        {
            if (ScreenFromWindowCenter() != null) return ScreenFromWindowCenter();
            var distances = new List<Vector>();
            foreach (var screen in Screen.AllScreens)
            {
                var screenCenter = new Point(screen.Bounds.X + screen.Bounds.Size.Width / 2,
                                             screen.Bounds.Y + screen.Bounds.Size.Height / 2);

                var dist = screenCenter - WindowCenter;
                distances.Add(dist);
            }

            var min = new Vector(double.MaxValue, double.MaxValue);
            foreach (var distance in distances)
            {
                if (distance.Length < min.Length) min = distance;
            }
            var index = distances.IndexOf(min);
            return Screen.AllScreens[index != -1 ? index : 0];
        }

        private Point WindowCenter => new Point(Left + ActualWidth / 2, Top + ActualHeight / 2);

        private bool IsWindowFullyVisible()
        {
            var tl = false; var tr = false; var bl = false; var br = false;
            foreach (var screen in Screen.AllScreens)
            {
                if (IsTopLeftCornerInScreen(screen)) tl = true;
                if (IsTopRightCornerInScreen(screen)) tr = true;
                if (IsBottomLeftCornerInScreen(screen)) bl = true;
                if (IsBottomRightCornerInScreen(screen)) br = true;
            }

            return tl && tr && bl && br;

        }
        private bool IsTopLeftCornerInScreen(Screen screen)
        {
            return screen.Bounds.Contains(Convert.ToInt32(Left), Convert.ToInt32(Top));
        }
        private bool IsBottomRightCornerInScreen(Screen screen)
        {
            return screen.Bounds.Contains(Convert.ToInt32(Left + ActualWidth), Convert.ToInt32(Top + ActualHeight));
        }
        private bool IsTopRightCornerInScreen(Screen screen)
        {
            return screen.Bounds.Contains(Convert.ToInt32(Left + ActualWidth), Convert.ToInt32(Top));
        }
        private bool IsBottomLeftCornerInScreen(Screen screen)
        {
            return screen.Bounds.Contains(Convert.ToInt32(Left), Convert.ToInt32(Top + ActualHeight));
        }

        private Screen ScreenFromWindowCenter()
        {
            return Screen.AllScreens.FirstOrDefault(x =>
                x.Bounds.Contains(Convert.ToInt32(WindowCenter.X), Convert.ToInt32(WindowCenter.Y)));
        }
        private void UpdateButtons()
        {
            if (ButtonsRef == null) return;

            var screenMiddle = SettingsHolder.ScreenH / 2;
            var middle = Top + Height / 2;
            var deadzone = SettingsHolder.ScreenH / 15;
            var distance = Math.Abs(screenMiddle - middle);

            if (!(distance > deadzone)) return;
            if (middle >= screenMiddle)
            {
                WindowSettings.ButtonsPosition = ButtonsPosition.Above;
                Grid.SetRow(ButtonsRef, 0);
            }
            else
            {
                WindowSettings.ButtonsPosition = ButtonsPosition.Below;
                Grid.SetRow(ButtonsRef, 2);
            }
        }
        protected void Drag(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!_ignoreSize) ResizeMode = ResizeMode.NoResize;
                DragMove();
                UpdateButtons();
                CheckBounds();
                if (!_ignoreSize) ResizeMode = ResizeMode.CanResize;
                WindowSettings.X = Left / SettingsHolder.ScreenW;
                WindowSettings.Y = Top / SettingsHolder.ScreenH;
                SettingsWriter.Save();
            }
            catch
            {
                // ignored
            }
        }
        public void CloseWindowSafe()
        {
            Dispatcher.Invoke(() => Close());
            Dispatcher.InvokeShutdown();
            //Hide();
        }
    }

    //public class TccWindow : Window, INotifyPropertyChanged
    //{
    //    private WindowSettings _settings;
    //    private bool _ignoreSize;
    //    private bool _clickThru;
    //    private DispatcherTimer _t;
    //    private DoubleAnimation _showButtons;
    //    private DoubleAnimation _hideButtons;
    //    protected IntPtr Handle;
    //    protected WindowButtons ButtonsRef;
    //    protected UIElement MainContentRef;
    //    public bool ClickThru
    //    {
    //        get => _clickThru;
    //        set
    //        {
    //            _clickThru = value;

    //            if (_clickThru) FocusManager.MakeClickThru(Handle);
    //            else FocusManager.UndoClickThru(Handle);
    //            NPC();
    //        }
    //    }
    //    public WindowSettings WindowSettings => _settings;
    //    public static event Action<TccWindow> RecreateWindow;
    //    protected void InitWindow(WindowSettings ws, bool canClickThru = true, bool canHide = true, bool ignoreSize = true)
    //    {
    //        Topmost = true;
    //        _settings = ws;
    //        //_settings.SafeClosed += CloseWindowSafe;
    //        _settings.EnabledChanged += EnableWindow;
    //        _settings.PropertyChanged += _settings_PropertyChanged;
    //        Left = ws.X * Settings.ScreenW;
    //        Top = ws.Y * Settings.ScreenH;
    //        if (!ignoreSize)
    //        {
    //            if (ws.H != 0) Height = ws.H;
    //            if (ws.W != 0) Width = ws.W;
    //        }
    //        _ignoreSize = ignoreSize;
    //        SetVisibility(ws.Visible);
    //        //Visibility = ws.Visible ? Visibility.Visible : Visibility.Hidden;
    //        SetClickThru(ws.ClickThruMode == ClickThruMode.Always);
    //        if (_settings.AutoDim) AnimateContentOpacity(_settings.DimOpacity);
    //        if (!WindowManager.IsTccVisible) AnimateContentOpacity(0);

    //        WindowManager.TccVisibilityChanged += OpacityChange;
    //        WindowManager.TccDimChanged += OpacityChange;
    //        SizeChanged += TccWindow_SizeChanged;
    //        Closed += TccWindow_Closed;
    //        Loaded += TccWindow_Loaded;

    //        /*******************************CONTINUE FROM HERE************************************/
    //        if (ButtonsRef == null) return;

    //        _hideButtons = new DoubleAnimation(0, TimeSpan.FromMilliseconds(1000));
    //        _showButtons = new DoubleAnimation(1, TimeSpan.FromMilliseconds(150));

    //        _t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
    //        _t.Tick += (s, ev) =>
    //        {
    //            _t.Stop();
    //            if (IsMouseOver) return;
    //            ButtonsRef.BeginAnimation(OpacityProperty, _hideButtons);
    //        };

    //        MouseEnter += (s, ev) => ButtonsRef.BeginAnimation(OpacityProperty, _showButtons);
    //        MouseLeave += (s, ev) => _t.Start();
    //        ButtonsRef.MouseLeftButtonDown += Drag;

    //        if (ws.Enabled) Show();

    //    }

    //    private void EnableWindow()
    //    {
    //        RecreateWindow?.Invoke(this);
    //    }

    //    private void _settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    //    {
    //        if (e.PropertyName == nameof(_settings.ClickThruMode))
    //        {
    //            switch (_settings.ClickThruMode)
    //            {
    //                case ClickThruMode.Never:
    //                    FocusManager.UndoClickThru(Handle);
    //                    break;
    //                case ClickThruMode.Always:
    //                    FocusManager.MakeClickThru(Handle);
    //                    break;
    //                case ClickThruMode.WhenDim:
    //                    if (WindowManager.IsTccDim) FocusManager.MakeClickThru(Handle);
    //                    else FocusManager.UndoClickThru(Handle);
    //                    break;
    //                case ClickThruMode.WhenUndim:
    //                    if (WindowManager.IsTccDim) FocusManager.UndoClickThru(Handle);
    //                    else FocusManager.MakeClickThru(Handle);
    //                    break;
    //                default:
    //                    throw new ArgumentOutOfRangeException();
    //            }
    //        }
    //        else if (e.PropertyName == nameof(_settings.Scale))
    //        {
    //            Dispatcher.Invoke(() =>
    //            {
    //                var vm = (TccWindowViewModel)DataContext;
    //                vm.GetDispatcher().Invoke(() => vm.Scale = _settings.Scale);
    //            });
    //        }
    //        else if (e.PropertyName == nameof(_settings.Visible))
    //        {
    //            SetVisibility(_settings.Visible);
    //        }
    //    }

    //    protected void TccWindow_Loaded(object sender, RoutedEventArgs e)
    //    {
    //        Handle = new WindowInteropHelper(this).Handle;
    //        FocusManager.MakeUnfocusable(Handle);
    //        FocusManager.HideFromToolBar(Handle);
    //        if (!_settings.Enabled) Hide();
    //    }

    //    private void TccWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    //    {
    //        CheckBounds();
    //        if (_ignoreSize) return;
    //        _settings.W = ActualWidth;
    //        _settings.H = ActualHeight;
    //        Settings.Save();
    //    }

    //    private void TccWindow_Closed(object sender, EventArgs e)
    //    {
    //        //Dispatcher.InvokeShutdown();
    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;
    //    protected void NPC([CallerMemberName] string p = null)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    //    }
    //    private void OpacityChange(object sender, PropertyChangedEventArgs e)
    //    {
    //        if (e.PropertyName == "IsTccVisible")
    //        {
    //            if (WindowManager.IsTccVisible)
    //            {
    //                if (WindowManager.IsTccDim && _settings.AutoDim)
    //                {
    //                    AnimateContentOpacity(_settings.DimOpacity);
    //                }
    //                else
    //                {
    //                    AnimateContentOpacity(1);
    //                }
    //            }
    //            else
    //            {
    //                if (_settings.ShowAlways) return;
    //                AnimateContentOpacity(0);
    //            }
    //        }

    //        //TODO: rework dim/undim and clickthru logic
    //        if (e.PropertyName == "IsTccDim")
    //        {
    //            if (!WindowManager.IsTccVisible) return;
    //            if (!_settings.AutoDim) return;

    //            AnimateContentOpacity(WindowManager.IsTccDim ? _settings.DimOpacity : 1);
    //            if (_settings.ClickThruMode == ClickThruMode.WhenUndim)
    //            {
    //                SetClickThru(!WindowManager.IsTccDim);
    //            }
    //            else if (_settings.ClickThruMode == ClickThruMode.WhenDim)
    //            {
    //                SetClickThru(WindowManager.IsTccDim);
    //            }
    //        }
    //    }

    //    private void SetClickThru(bool t)
    //    {
    //        ClickThru = t;
    //    }
    //    public void SetVisibility(Visibility v)
    //    {
    //        if (!Dispatcher.Thread.IsAlive)
    //        {
    //            return;
    //        }
    //        Dispatcher.Invoke(() =>
    //        {
    //            Visibility = v;
    //            NPC(nameof(Visibility));
    //        });
    //    }

    //    private void SetVisibility(bool v)
    //    {
    //        if (!Dispatcher.Thread.IsAlive)
    //        {
    //            return;
    //        }
    //        Dispatcher.Invoke(() =>
    //        {
    //            Visibility = !v ? Visibility.Visible : Visibility.Collapsed; // meh ok
    //            Visibility = v ? Visibility.Visible : Visibility.Collapsed;
    //            NPC(nameof(Visibility));
    //        });
    //    }

    //    private void AnimateContentOpacity(double opacity)
    //    {
    //        if (MainContentRef == null) return;
    //        Dispatcher.InvokeIfRequired(() =>
    //        {
    //            //var grid = ((Grid)this.Content);
    //            MainContentRef.BeginAnimation(OpacityProperty, new DoubleAnimation(opacity, TimeSpan.FromMilliseconds(250)));
    //        }, DispatcherPriority.DataBind);
    //    }
    //    public void RefreshTopmost()
    //    {
    //        Dispatcher.InvokeIfRequired(() => { Topmost = false; Topmost = true; }, DispatcherPriority.DataBind);
    //    }
    //    public void RefreshSettings(WindowSettings ws)
    //    {
    //        _settings = ws;
    //    }

    //    protected void Drag(object sender, MouseButtonEventArgs e)
    //    {
    //        try
    //        {
    //            if (!_ignoreSize) ResizeMode = ResizeMode.NoResize;
    //            DragMove();
    //            CheckBounds();
    //            if (!_ignoreSize) ResizeMode = ResizeMode.CanResize;
    //            var unused = Screen.FromHandle(new WindowInteropHelper(this).Handle);
    //            var source = PresentationSource.FromVisual(this);
    //            if (source?.CompositionTarget == null) return;
    //            var m = source.CompositionTarget.TransformToDevice;
    //            var dx = m.M11;
    //            var dy = m.M22;
    //            var newLeft = Left * dx;
    //            var newTop = Top * dx;
    //            //_settings.X = newLeft / dx;
    //            //_settings.Y = newTop / dy;

    //            _settings.X = Left / Settings.ScreenW;
    //            _settings.Y = Top / Settings.ScreenH;
    //            Settings.Save();
    //        }
    //        catch
    //        {
    //            // ignored
    //        }
    //    }

    //    private void CheckBounds()
    //    {
    //        if ((Left + ActualWidth) > Settings.ScreenW)
    //        {
    //            Left = Settings.ScreenW - ActualWidth;
    //        }
    //        if ((Top + ActualHeight) > Settings.ScreenH)
    //        {
    //            Top = Settings.ScreenH - ActualHeight;
    //        }
    //        if (Left < 0) Left = 0;
    //        if (Top < 0) Top = 0;
    //    }

    //    public void CloseWindowSafe()
    //    {
    //        if (Dispatcher.CheckAccess())
    //            Hide();
    //        else
    //            Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(Hide));
    //    }

    //    private bool _isTempShow;

    //    private ClickThruMode _oldCt;
    //    private bool _oldDim;
    //    private bool _oldShowAlways;

    //    public void TempShow()
    //    {
    //        _isTempShow = !_isTempShow;
    //        if (!_settings.Enabled || !_settings.Visible) return;
    //        if (_isTempShow)
    //        {
    //            _oldCt = _settings.ClickThruMode;
    //            _oldDim = _settings.AutoDim;
    //            _oldShowAlways = _settings.ShowAlways;

    //            _settings.ClickThruMode = ClickThruMode.Never;
    //            _settings.AutoDim = false;
    //            _settings.ShowAlways = true;
    //        }
    //        else
    //        {
    //            _settings.ClickThruMode = _oldCt;
    //            _settings.AutoDim = _oldDim;
    //            _settings.ShowAlways = _oldShowAlways;
    //        }
    //    }
    //}
}