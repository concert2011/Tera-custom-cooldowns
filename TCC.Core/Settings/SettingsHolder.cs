﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using TCC.Data;
using Key = System.Windows.Forms.Keys;

namespace TCC.Settings
{
    public static class SettingsHolder
    {
        private static string _lastLanguage = "";
        private static bool _chatEnabled;
        private static ClickThruMode _chatClickThruMode;
        private static DateTime _statSentTime = DateTime.MinValue;
        private static string _statSentVersion = GetVersion();

        private static string GetVersion() //no idea, just use this for now
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return $"TCC v{v.Major}.{v.Minor}.{v.Build}{(App.Experimental ? "-e" : "")}";

        }
        public static double ScreenW => SystemParameters.VirtualScreenWidth;
        public static double ScreenH => SystemParameters.VirtualScreenHeight;

        public static WindowSettings GroupWindowSettings { get; set; } = new WindowSettings(0, 0, 0, 0, true, ClickThruMode.Never, 1, true, .5, false, true, false, null, nameof(GroupWindowSettings));
        public static WindowSettings CooldownWindowSettings { get; set; } = new WindowSettings(.4, .7, 0, 0, true, ClickThruMode.Never, 1, true, .5, false, true, false, null, nameof(CooldownWindowSettings));
        public static WindowSettings BossWindowSettings { get; set; } = new WindowSettings(.4, 0, 0, 0, true, ClickThruMode.Never, 1, true, .5, false, true, false, null, nameof(BossWindowSettings));
        public static WindowSettings BuffWindowSettings { get; set; } = new WindowSettings(1, .7, 0, 0, true, ClickThruMode.Never, 1, true, .5, false, true, false, null, nameof(BuffWindowSettings));
        public static WindowSettings CharacterWindowSettings { get; set; } = new WindowSettings(.4, 1, 0, 0, true, ClickThruMode.Never, 1, true, .5, false, true, false, null, nameof(CharacterWindowSettings));
        public static WindowSettings ClassWindowSettings { get; set; } = new WindowSettings(.25, .6, 0, 0, true, ClickThruMode.Never, 1, true, .5, false, true, false, null, nameof(ClassWindowSettings));
        public static WindowSettings FlightGaugeWindowSettings { get; set; } = new WindowSettings(0, 0, 0, 0, true, ClickThruMode.Always, 1, false, 1, false, true, false);
        public static WindowSettings FloatingButtonSettings { get; set; } = new WindowSettings(0, 0, 0, 0, true, ClickThruMode.Never, 1, false, 1, false, true, true);
        public static WindowSettings CivilUnrestWindowSettings { get; set; } = new WindowSettings(1, .45, 0, 0, true, ClickThruMode.Never, 1, true, .5, false, true, false, null, nameof(CivilUnrestWindowSettings));

        public static SynchronizedObservableCollection<ChatWindowSettings> ChatWindowsSettings { get; } = new SynchronizedObservableCollection<ChatWindowSettings>(App.BaseDispatcher);

        // Group window
        public static bool IgnoreMeInGroupWindow { get; set; }
        public static bool IgnoreGroupBuffs { get; set; }
        public static bool IgnoreGroupDebuffs { get; set; }
        public static bool DisablePartyMP { get; set; }
        public static bool DisablePartyHP { get; set; }
        public static bool DisablePartyAbnormals { get; set; }
        public static bool ShowOnlyAggroStacks { get; set; } = true;

        public static uint GroupSizeThreshold { get; set; } = 7;
        public static uint HideBuffsThreshold { get; set; } = 7;
        public static uint HideDebuffsThreshold { get; set; } = 7;
        public static uint DisableAbnormalitiesThreshold { get; set; } = 7;
        public static uint HideHpThreshold { get; set; } = 7;
        public static uint HideMpThreshold { get; set; } = 7;

        public static bool ShowMembersLaurels { get; set; }
        public static bool ShowAllGroupAbnormalities { get; set; }
        public static bool ShowGroupWindowDetails { get; set; } = true;
        public static bool ShowAwakenIcon { get; set; } = true;
        public static Dictionary<Class, List<uint>> GroupAbnormals { get; } = new Dictionary<Class, List<uint>>()
        {
            {       0, new List<uint>()},
            {(Class)1, new List<uint>()},
            {(Class)2, new List<uint>()},
            {(Class)3, new List<uint>()},
            {(Class)4, new List<uint>()},
            {(Class)5, new List<uint>()},
            {(Class)6, new List<uint>()},
            {(Class)7, new List<uint>()},
            {(Class)8, new List<uint>()},
            {(Class)9, new List<uint>()},
            {(Class)10, new List<uint>()},
            {(Class)11, new List<uint>()},
            {(Class)12, new List<uint>()},
            {(Class)255, new List<uint>()},
        };
        // Buff window
        public static FlowDirection BuffsDirection { get; set; } = FlowDirection.RightToLeft;
        //Add My Abnormals Setting by HQ ===========================================================
        public static bool ShowAllMyAbnormalities { get; set; } = true;
        public static Dictionary<Class, List<uint>> MyAbnormals { get; } = new Dictionary<Class, List<uint>>()
        {
            {       0, new List<uint>{ 100800, 100801 }},
            {(Class)1, new List<uint>{ 200230, 200231, 200232, 201701 }},
            {(Class)2, new List<uint>{ 300800, 300801, 300805 }},
            {(Class)3, new List<uint>{ 401705, 401706, 401710, 400500, 400501, 400508, 400710, 400711 }},
            {(Class)4, new List<uint>{ 21170, 22120, 23180, 26250, 29011, 25170, 25171, 25201, 25202, 500100, 500150, 501600, 501650, 502001, 502051, 502070, 502071, 502072 }},
            {(Class)5, new List<uint>{ 601400, 601450, 601460, 88608101, 88608102, 88608103, 88608104, 88608105, 88608106, 88608107, 88608108, 88608109, 88608110,602101,602102,602103,601611 }},
            {(Class)6, new List<uint>()},
            {(Class)7, new List<uint>()},
            {(Class)8, new List<uint>{ 10151010, 10151131, 10151192 }},
            {(Class)9, new List<uint>{ 89105101, 89105102, 89105103, 89105104, 89105105, 89105106, 89105107, 89105108, 89105109, 89105110, 89105111, 89105112, 89105113, 89105114, 89105115, 89105116, 89105117, 89105118, 89105119, 89105120, 10152340, 10152351 }},
            {(Class)10, new List<uint>{ 31020, 10153210 }},
            {(Class)11, new List<uint>{ 89314201, 89314202, 89314203, 89314204, 89314205, 89314206, 89314207, 89314208, 89314209, 89314210, 89314211, 89314212, 89314213, 89314214, 89314215, 89314216, 89314217, 89314218, 89314219, 89314220, 10154480, 10154450 }},
            {(Class)12, new List<uint>{ 10155130, 10155551, 10155510, 10155512, 10155540, 10155541, 10155542 }},
            {(Class)255, new List<uint>{ 6001, 6002, 6003, 6004, 6012, 6013, 702004, 805800, 805803, 200700, 200701, 200731, 800300, 800301, 800302, 800303, 800304, 702001 }},
        };
        //==========================================================================================

        // Cooldown window
        public static CooldownBarMode CooldownBarMode { get; set; } = CooldownBarMode.Fixed;
        public static bool ShowItemsCooldown { get; set; } = true;

        // Boss window
        public static bool ShowOnlyBosses { get; set; }
        public static EnrageLabelMode EnrageLabelMode { get; set; } = EnrageLabelMode.Remaining;
        public static bool AccurateHp { get; set; } = true;
        public static bool KylosHelper { get; set; } = true; //by HQ 20190324

        // Chat window
        public static bool ChatEnabled
        {
            get => ChatWindowsSettings.Count > 0 ? ChatWindowsSettings[0].Enabled : _chatEnabled;
            set
            {
                if (ChatWindowsSettings.Count > 0)
                {
                    if (ChatWindowsSettings[0].Enabled == value) return;
                    ChatWindowsSettings.ToList().ForEach(x => x.Enabled = value);
                }
                else
                {
                    _chatEnabled = value;
                }
            }
        }
        public static int MaxMessages { get; set; } = 500;
        public static int SpamThreshold { get; set; } = 2;
        public static bool ShowChannel { get; set; } = true;
        public static bool ShowTimestamp { get; set; } = true;
        public static bool ChatTimestampSeconds { get; internal set; }
        public static int FontSize { get; set; } = 15;
        public static bool AnimateChatMessages { get; set; }
        public static ClickThruMode ChatClickThruMode
        {
            get
            {
                if (ChatWindowsSettings.Count > 0) return ChatWindowsSettings[0].ClickThruMode;
                return _chatClickThruMode;
            }

            set
            {
                if (ChatWindowsSettings.Count > 0)
                {
                    if (ChatWindowsSettings[0].ClickThruMode == value) return;
                    ChatWindowsSettings.ToList().ForEach(x => x.ClickThruMode = value);
                }
                else
                {
                    _chatClickThruMode = value;
                }
            }
        }
        public static int ChatScrollAmount { get; set; } = 1;
        // Character window
        public static bool CharacterWindowCompactMode { get; set; } = true;
        // Class window
        public static bool WarriorShowTraverseCut { get; set; } = true;
        public static bool WarriorShowEdge { get; set; } = true;
        public static WarriorEdgeMode WarriorEdgeMode { get; set; } = WarriorEdgeMode.Bar;
        public static bool SorcererReplacesElementsInCharWindow { get; set; } = true;


        // Misc
        public static DateTime LastRun { get; set; } = DateTime.MinValue;

        public static DateTime StatSentTime
        {
            get => _statSentTime;
            set
            {
                _statSentTime = value;
                SettingsWriter.Save();
            }
        }

        public static string StatSentVersion
        {
            get
            {
                Log.CW(_statSentVersion);
                return _statSentVersion; 

            }
            set => _statSentVersion = value;
        }

        public static string LastLanguage
        {
            get => LanguageOverride != "" ? LanguageOverride : _lastLanguage;
            set => _lastLanguage = value;
        }
        public static string TwitchName { get; set; } = ""; //TODO: re-add this
        public static string TwitchToken { get; set; } = ""; //TODO: re-add this
        public static string TwitchChannelName { get; set; } = ""; //TODO: re-add this
        public static bool LfgEnabled { get; set; } = true;
        public static bool ShowTradeLfg { get; set; } = true;
        //public static bool StatSent { get; set; } = false;
        public static bool ShowFlightEnergy { get; set; } = true;
        public static bool UseHotkeys { get; set; } = true;
        public static bool EthicalMode { get; set; } = false;
        public static HotKey LfgHotkey { get; } = new HotKey(Key.Y, ModifierKeys.Control);
        public static HotKey InfoWindowHotkey { get; } = new HotKey(Key.I, ModifierKeys.Control);
        public static HotKey SettingsHotkey { get; } = new HotKey(Key.O, ModifierKeys.Control);
        public static HotKey LootSettingsHotkey { get; } = new HotKey(Key.L, ModifierKeys.Control);
        public static string LanguageOverride { get; set; } = "";
        public static double FlightGaugeRotation { get; set; } = 0;
        public static bool FlipFlightGauge { get; set; } = false;
        public static bool HideHandles { get; set; } = false;
        public static bool HighPriority { get; set; } = false;
        public static bool ForceSoftwareRendering { get; set; } = true;
        public static ControlShape AbnormalityShape { get; set; } = ControlShape.Round;
        public static ControlShape SkillShape { get; set; } = ControlShape.Round;
        public static bool Npcap { get; set; } = true;
        public static bool CheckOpcodesHash { get; set; } = true;
        public static bool ShowNotificationBubble { get; set; } = true;
        public static List<string> UserExcludedSysMsg { get; set; } = new List<string>();
        public static bool ExperimentalNotification { get; set; } = true;
        public static bool FpsAtGuardian { get; set; } = true;
        public static bool EnableProxy { get; set; } = true;
        public static bool ShowMembersHpNumbers { get; set; } = true;
        public static bool DisableLfgChatMessages { get; set; } = true;


        public static bool WebhookEnabledFieldBoss { get; set; } = false;
        public static string WebhookUrlFieldBoss { get; set; } = "";
        public static string WebhookMessageFieldBossSpawn { get; set; } = "@here {bossName} spawned in {regionName}!";
        public static string WebhookMessageFieldBossDie { get; set; } = "{bossName} is dead.";

        public static bool WebhookEnabledGuildBam { get; set; } = false;
        public static string WebhookUrlGuildBam { get; set; } = "";
        public static string WebhookMessageGuildBam { get; set; } = "@here Guild BAM will spawn soon!";
    }
}
