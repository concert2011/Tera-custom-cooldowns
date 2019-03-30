﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TCC.Data;
using TCC.Data.Chat;
using TCC.Settings;
using TCC.ViewModels;

namespace TCC.Windows
{
    /// <summary>
    /// Logica di interazione per SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {

        public IntPtr Handle => Dispatcher.Invoke(() => new WindowInteropHelper(this).Handle);

        public SettingsWindow()
        {
            DataContext = new SettingsWindowViewModel();
            InitializeComponent();
            TitleBarGrid.MouseLeftButtonDown += (_, __) => DragMove();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            HideWindow();
            SettingsWriter.Save();
        }

        private void OpenPlayerBuffSettings(object sender, RoutedEventArgs e)
        {
            //Add My Abnormals Setting by HQ ============================================================
            new MyAbnormalConfigWindow().ShowWindow();
            //===========================================================================================
        }

        private void OpenGroupBuffSettings(object sender, RoutedEventArgs e)
        {
            new GroupAbnormalConfigWindow().ShowWindow();

            //WindowManager.GroupAbnormalConfigWindow.ShowWindow();
        }
        private void ResetChatWindowsPosition(object sender, RoutedEventArgs e)
        {
            foreach (var cw in ChatWindowManager.Instance.ChatWindows)
            {
                cw.ResetToCenter();
            }
        }

        private void SendWebhookTest(object sender, RoutedEventArgs e)
        {
            TimeManager.Instance.ExecuteGuildBamWebhook(testMessage: true);
        }

        private void MakePositionsGlobal(object sender, RoutedEventArgs e)
        {
            WindowManager.MakeGlobal();
        }

        private void WindowPositionsReset(object sender, RoutedEventArgs e)
        {
            WindowManager.ResetToCenter();
        }

        private void OpenResourcesFolder(object sender, RoutedEventArgs e)
        {
            Process.Start(Path.Combine(App.BasePath, "resources/config"));
        }

        private void GoToPaypal(object sender, RoutedEventArgs e)
        {
            Process.Start("https://paypal.me/foglio1024");
        }

        private void GoToBamTimes(object sender, RoutedEventArgs e)
        {
            Process.Start("https://tcc-web-99a64.firebaseapp.com/");
        }

        private void GoToRestyle(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Foglio1024/tera-restyle/wiki");
        }

        private void GoToChat2(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Foglio1024/S1UI_chat2/blob/master/p75/S1UI_Chat2.gpk");
        }

        private void GoToTccStub(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Foglio1024/tcc-stub");
        }

        private void GoToCaaliModsDiscord(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discord.gg/dUNDDtw");
        }

        private void GoToTeraDpsDiscord(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discord.gg/anUXQTp");
        }

        private void GoToIssues(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Foglio1024/Tera-custom-cooldowns/issues");
        }

        private void GoToReleases(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Foglio1024/Tera-custom-cooldowns/releases");
        }

        private void GoToWiki(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Foglio1024/Tera-custom-cooldowns/wiki");
        }

        private void EventSetter_OnHandler(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement t)) return;
            t.Opacity = 0;
            t.RenderTransform = new TranslateTransform(-20, 0);
            var ease = new QuadraticEase();
            var slideAnim = new DoubleAnimation(-20, 0, TimeSpan.FromMilliseconds(750)) {EasingFunction = ease};
            var fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(750)) {EasingFunction = ease};
            t.BeginAnimation(OpacityProperty, fadeAnim);
            t.RenderTransform.BeginAnimation(TranslateTransform.XProperty, slideAnim);
        }

        private void ClearChatMessages(object sender, RoutedEventArgs e)
        {
            foreach (var chatMessage in ChatWindowManager.Instance.ChatMessages)
            {
                chatMessage.Dispose();
            }

            ChatWindowManager.Instance.ChatMessages.Clear();
        }

        private async void ForceExperimentalBuildDownlaod(object sender, RoutedEventArgs e)
        {
            if (TccMessageBox.Show("Warning: experimental build could be unstable. Proceed?", Data.MessageBoxType.ConfirmationWithYesNo) == MessageBoxResult.Yes)
            {
                await Task.Factory.StartNew(() => UpdateManager.ForceUpdateExperimental());
            }
        }

        private void TestWebhookGuildBam(object sender, RoutedEventArgs e)
        {
            TimeManager.Instance.ExecuteGuildBamWebhook(testMessage: true);
        }

        private void TestWebhookFieldBoss(object sender, RoutedEventArgs e)
        {
            SessionManager.DB.SystemMessagesDatabase.Messages.TryGetValue("SMT_FIELDBOSS_APPEAR", out var sysMsg);
            var srvMsg = "@4157\vregionName\v@rgn:213\vnpcName\v@creature:26#5001";
            var cm = new ChatMessage(srvMsg, sysMsg, ChatChannel.TCC);
            TimeManager.Instance.ExecuteFieldBossSpawnWebhook("Boss", "Region",cm.RawMessage,  testMessage: true);

            SessionManager.DB.SystemMessagesDatabase.Messages.TryGetValue("SMT_FIELDBOSS_DIE_GUILD", out var dieSysMsg);
            var dieSrvMsg = "@4158\vguildName\vWish\vuserName\v쿤\vnpcname\v@creature:26#5001";
            var cmd = new ChatMessage(dieSrvMsg, dieSysMsg, ChatChannel.TCC);
            TimeManager.Instance.ExecuteFieldBossDieWebhook("Boss", cmd.RawMessage,  testMessage: true);
        }
    }
}
