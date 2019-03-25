﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Threading;
using TCC.Data;
using TCC.Windows;

namespace TCC
{
    public sealed class KeyboardHook : IDisposable
    {

        private static KeyboardHook _instance;
        private readonly Window _window = new Window();
        private int _currentId;

        private bool _isRegistered;
        private bool _isInitialized;
        private KeyboardHook()
        {
            // register the event of the inner native window.
            _window.KeyPressed += delegate (object sender, KeyPressedEventArgs args) { KeyPressed?.Invoke(this, args); };
        }


        public static KeyboardHook Instance => _instance ?? (_instance = new KeyboardHook());

        private void SetHotkeys(bool value)
        {
            if (value && !_isRegistered)
            {
                Register();
                return;
            }

            if (value || !_isRegistered) return;
            ClearHotkeys();
        }

        private static void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Key == Settings.SettingsHolder.LfgHotkey.Key && e.Modifier == Settings.SettingsHolder.LfgHotkey.Modifier)
            {
                if (!ProxyInterop.Proxy.IsConnected) return;

                if (!WindowManager.LfgListWindow.IsVisible)
                {
                    WindowManager.LfgListWindow.VM.StayClosed = false;
                    ProxyInterop.Proxy.RequestLfgList();
                }
                else WindowManager.LfgListWindow.CloseWindow();
            }
            else if (e.Key == Settings.SettingsHolder.SettingsHotkey.Key && e.Modifier == Settings.SettingsHolder.SettingsHotkey.Modifier)
            {
                if (WindowManager.SettingsWindow.IsVisible) WindowManager.SettingsWindow.HideWindow();
                else WindowManager.SettingsWindow.ShowWindow();
            }
            else if (e.Key == Settings.SettingsHolder.InfoWindowHotkey.Key && e.Modifier == Settings.SettingsHolder.InfoWindowHotkey.Modifier)
            {
                if (WindowManager.Dashboard.IsVisible) WindowManager.Dashboard.HideWindow();
                else WindowManager.Dashboard.ShowWindow();
            }
            else if (e.Key == Keys.K && e.Modifier == ModifierKeys.Control)
            {
                WindowManager.CooldownWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (WindowManager.SkillConfigWindow != null && WindowManager.SkillConfigWindow.IsVisible) WindowManager.SkillConfigWindow.Close();
                    else new SkillConfigWindow().ShowWindow();
                }), DispatcherPriority.Background);
            }
            else if (e.Key == Keys.R && e.Modifier == (ModifierKeys.Alt | ModifierKeys.Control))
            {
                if (!SessionManager.Logged
                  || SessionManager.LoadingScreen
                  || SessionManager.Combat
                  || !ProxyInterop.Proxy.IsConnected) return;

                WindowManager.LfgListWindow.VM.ForceStopPublicize();
                ProxyInterop.Proxy.ReturnToLobby();
            }

            //if (e.Key == Settings.Settings.LootSettingsHotkey.Key && e.Modifier == Settings.Settings.LootSettingsHotkey.Modifier)
            //{
            //    if (!WindowManager.GroupWindow.VM.AmILeader) return;
            //    if (!Proxy.Proxy.IsConnected) return;
            //    Proxy.Proxy.LootSettings();
            //}

        }


/*
        public void Update()
        {
            ClearHotkeys();
            Register();
        }
*/

        public void RegisterKeyboardHook()
        {
            if (_isInitialized) return;
            WindowManager.FloatingButton.Dispatcher.Invoke(() =>
            {
                // register the event that is fired after the key press.
                Instance.KeyPressed += hook_KeyPressed;
                SessionManager.ChatModeChanged += CheckHotkeys;
                FocusManager.ForegroundChanged += CheckHotkeys;
                _isInitialized = true;
            });
        }
        public void UnRegisterKeyboardHook()
        {
            if (!_isInitialized) return;
            WindowManager.FloatingButton.Dispatcher.Invoke(() =>
            {
                // register the event that is fired after the key press.
                Instance.KeyPressed -= hook_KeyPressed;
                if (_isRegistered) { ClearHotkeys(); }
                SessionManager.ChatModeChanged -= CheckHotkeys;
                FocusManager.ForegroundChanged -= CheckHotkeys;
                _isInitialized = false;
            });
        }
        private void CheckHotkeys()
        {
            WindowManager.FloatingButton.Dispatcher.BeginInvoke(new Action(() =>
            {
                SetHotkeys(!SessionManager.InGameChatOpen && FocusManager.IsForeground);
            }), DispatcherPriority.Background);
        }

        private void Register()
        {
            RegisterHotKey(Settings.SettingsHolder.LfgHotkey.Modifier, Settings.SettingsHolder.LfgHotkey.Key);
            RegisterHotKey(Settings.SettingsHolder.InfoWindowHotkey.Modifier, Settings.SettingsHolder.InfoWindowHotkey.Key);
            RegisterHotKey(Settings.SettingsHolder.SettingsHotkey.Modifier, Settings.SettingsHolder.SettingsHotkey.Key);
            RegisterHotKey(Settings.SettingsHolder.LootSettingsHotkey.Modifier, Settings.SettingsHolder.LootSettingsHotkey.Key);
            RegisterHotKey(ModifierKeys.Control, Keys.K);
            RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt, Keys.R);
            //RegisterHotKey(Settings.ShowAllHotkey.Modifier, Settings.ShowAllHotkey.Key);

            _isRegistered = true;
        }

        // Registers a hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        ///     Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        private void RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            if (key == Keys.None)
            {
                return; //allow disable hotkeys using "None" key
            }
            // increment the counter.
            _currentId++;

            // register the hot key.
            if (!RegisterHotKey(_window.Handle, _currentId, (uint)modifier, (uint)key))
            {
            }
        }

        /// <summary>
        ///     A hot key has been pressed.
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        /// <summary>
        ///     Represents the window that is used internally to get the messages.
        /// </summary>
        private sealed class Window : NativeWindow, IDisposable
        {
            private static readonly int WmHotkey = 0x0312;

            public Window()
            {
                // create the handle for the window.
                CreateHandle(new CreateParams());
            }

            #region IDisposable Members

            public void Dispose()
            {
                DestroyHandle();
            }

            #endregion

            /// <summary>
            ///     Overridden to get the notifications.
            /// </summary>
            /// <param name="m"></param>
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                // check if we got a hot key pressed.
                if (m.Msg == WmHotkey)
                {
                    // get the keys.
                    var key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    var modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                    // invoke the event to notify the parent.
                    KeyPressed?.Invoke(this, new KeyPressedEventArgs(modifier, key));
                }
            }

            public event EventHandler<KeyPressedEventArgs> KeyPressed;
        }

        #region IDisposable Members

        public void Dispose()
        {
            // unregister all the registered hot keys.
            ClearHotkeys();

            // dispose the inner native window.
            _window.Dispose();
        }

        private void ClearHotkeys()
        {
            for (var i = _currentId; i > 0; i--) { UnregisterHotKey(_window.Handle, i); }
            _currentId = 0;
            _isRegistered = false;
        }

        #endregion
    }

    /// <summary>
    ///     Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
        {
            Modifier = modifier;
            Key = key;
        }

        public ModifierKeys Modifier { get; }

        public Keys Key { get; }
    }

}
