﻿using System;
using System.Collections.Generic;
using System.Linq;
using TCC.Data;
using TCC.Data.Chat;
using TCC.Settings;
using TCC.ViewModels;
namespace TCC.Parsing
{
    public static class SystemMessagesProcessor
    {
        public static void AnalyzeMessage(string srvMsg, SystemMessage sysMsg, string opcodeName)
        {
            if (!Pass(opcodeName)) return;

            if (!Process(srvMsg, sysMsg, opcodeName))
            {
                ChatWindowManager.Instance.AddChatMessage(new ChatMessage(srvMsg, sysMsg, (ChatChannel)sysMsg.ChatChannel));
            }
        }

        private static bool Pass(string opcodeName)
        {
            return /*!ExclusionList.Contains(opcodeName) && */!SettingsHolder.UserExcludedSysMsg.Contains(opcodeName);
        }

        private static void HandleMaxEnchantSucceed(string x)
        {
            var sysMsg = ChatMessage.BuildEnchantSystemMessage(x);
            ChatWindowManager.Instance.AddChatMessage(sysMsg);
        }
        private static void HandleFriendLogin(string friendName, SystemMessage sysMsg)
        {
            var sysmsg = "@0\vUserName\v" + friendName;
            var msg = new ChatMessage(sysmsg, sysMsg, ChatChannel.Friend) { Author = friendName };
            ChatWindowManager.Instance.AddChatMessage(msg);
        }
        private static void HandleClearedGuardianQuestsMessage(string srvMsg, SystemMessage sysMsg)
        {
            var currChar = WindowManager.Dashboard.VM.CurrentCharacter;
            var standardCountString = $"<font color =\"#cccccc\">({currChar.ClearedGuardianQuests + 1}/40)</font>";
            var maxedCountString = $"<font color=\"#cccccc\">(</font><font color =\"#ff0000\">{currChar.ClearedGuardianQuests + 1}</font><font color=\"#cccccc\">/40)</font>";
            var newMsg = new SystemMessage($"{sysMsg.Message} {(currChar.ClearedGuardianQuests + 1 == 40 ? maxedCountString : standardCountString)}", sysMsg.ChatChannel);
            var msg = new ChatMessage(srvMsg, newMsg, ChatChannel.Guardian);
            if (currChar.ClearedGuardianQuests + 1 == 40) msg.ContainsPlayerName = true;
            ChatWindowManager.Instance.AddChatMessage(msg);

        }
        private static void HandleNewGuildMasterMessage(string srvMsg, SystemMessage sysMsg)
        {
            var msg = new ChatMessage(srvMsg, sysMsg, ChatChannel.GuildNotice);
            ChatWindowManager.Instance.AddChatMessage(msg);
            msg.ContainsPlayerName = true;
            WindowManager.FloatingButton.NotifyExtended("Guild", msg.ToString(), NotificationType.Success);

        }
        private static void HandleGuilBamSpawn(string srvMsg, SystemMessage sysMsg)
        {
            TimeManager.Instance.UploadGuildBamTimestamp();
            TimeManager.Instance.SetGuildBamTime(true);
            TimeManager.Instance.ExecuteGuildBamWebhook();
            var msg = new ChatMessage(srvMsg, sysMsg, (ChatChannel)sysMsg.ChatChannel);
            ChatWindowManager.Instance.AddChatMessage(msg);
            WindowManager.FloatingButton.NotifyExtended("Guild BAM", msg.ToString(), NotificationType.Normal);
        }
        private static void HandleDungeonEngagedMessage(string srvMsg, SystemMessage sysMsg)
        {
            const string s = "dungeon:";
            var dgId = Convert.ToUInt32(srvMsg.Substring(srvMsg.IndexOf(s, StringComparison.Ordinal) + s.Length));
            WindowManager.Dashboard.VM.CurrentCharacter.EngageDungeon(dgId);

            var msg = new ChatMessage(srvMsg, sysMsg, (ChatChannel)sysMsg.ChatChannel);
            ChatWindowManager.Instance.AddChatMessage(msg);
        }
        private static void HandleFriendInAreaMessage(string srvMsg, SystemMessage sysMsg)
        {
            var msg = new ChatMessage(srvMsg, sysMsg, ChatChannel.Friend);
            var start = srvMsg.IndexOf("UserName\v", StringComparison.InvariantCultureIgnoreCase) + "UserName\v".Length;
            var end = srvMsg.IndexOf("\v", start, StringComparison.InvariantCultureIgnoreCase);
            var friendName = srvMsg.Substring(start, end - start);
            msg.Author = friendName;
            ChatWindowManager.Instance.AddChatMessage(msg);
        }
        private static void HandleRessMessage(string srvMsg, SystemMessage sysMsg)
        {
            var newSysMsg = new SystemMessage(sysMsg.Message.Replace("{UserName}", "<font color='#cccccc'>{UserName}</font>"), (int)ChatChannel.Ress);
            var msg = new ChatMessage(srvMsg, newSysMsg, ChatChannel.Ress);
            ChatWindowManager.Instance.AddChatMessage(msg);
            if (ProxyInterop.Proxy.IsConnected) ProxyInterop.Proxy.ForceSystemMessage(srvMsg, "SMT_BATTLE_PARTY_RESURRECT");

        }
        private static void HandleDeathMessage(string srvMsg, SystemMessage sysMsg)
        {
            var newSysMsg = new SystemMessage(sysMsg.Message.Replace("{UserName}", "<font color='#cccccc'>{UserName}</font>"), (int)ChatChannel.Death);
            var msg = new ChatMessage(srvMsg, newSysMsg, ChatChannel.Death);
            ChatWindowManager.Instance.AddChatMessage(msg);
        }
        private static void HandleInvalidLink(string srvMsg, SystemMessage sysMsg)
        {
            ChatWindowManager.Instance.AddChatMessage(new ChatMessage(srvMsg, sysMsg, (ChatChannel)sysMsg.ChatChannel));
            ChatWindowManager.Instance.RemoveDeadLfg();
            if (SettingsHolder.LfgEnabled) WindowManager.LfgListWindow.VM.RemoveDeadLfg();
        }

        #region Factory

        private static readonly Dictionary<string, Delegate> Processor = new Dictionary<string, Delegate>
        {
            { "SMT_MAX_ENCHANT_SUCCEED",                new Action<string, SystemMessage>((srvMsg, sysMsg) => HandleMaxEnchantSucceed(srvMsg)) },
            { "SMT_FRIEND_IS_CONNECTED",                new Action<string, SystemMessage>(HandleFriendLogin) },
            { "SMT_FRIEND_WALK_INTO_SAME_AREA",         new Action<string, SystemMessage>(HandleFriendInAreaMessage) },
            { "SMT_CHAT_LINKTEXT_DISCONNECT",           new Action<string, SystemMessage>(HandleInvalidLink) },

            { "SMT_BATTLE_PARTY_DIE",                   new Action<string, SystemMessage>(HandleDeathMessage) },
            { "SMT_BATTLE_YOU_DIE",                     new Action<string, SystemMessage>(HandleDeathMessage) },
            { "SMT_BATTLE_PARTY_RESURRECT",             new Action<string, SystemMessage>(HandleRessMessage) },
            { "SMT_BATTLE_RESURRECT",                   new Action<string, SystemMessage>(HandleRessMessage) },

            { "SMT_ACCEPT_QUEST",                       new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_CANT_START_QUEST",                   new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_COMPLATE_GUILD_QUEST",               new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_COMPLETE_MISSION",                   new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_COMPLETE_QUEST",                     new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_FAILED_QUEST_COMPENSATION",          new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_FAILED_QUEST",                       new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_FAILED_QUEST_CANCLE",                new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_QUEST_FAILED_GET_FLAG_PARTY_LEVEL",  new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_QUEST_ITEM_DELETED",                 new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_QUEST_RESET_MESSAGE",                new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_UPDATE_QUEST_TASK",                  new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_QUEST_SHARE_MESSAGE2",               new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_QUEST_USE_SKILL",                    new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },
            { "SMT_QUEST_USE_ITEM",                     new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Quest)) },

            { "SMT_GRANT_DUNGEON_COOLTIME_AND_COUNT",   new Action<string, SystemMessage>(HandleDungeonEngagedMessage) },
            { "SMT_GQUEST_URGENT_NOTIFY",               new Action<string, SystemMessage>(HandleGuilBamSpawn) },

            { "SMT_PARTY_LOOT_ITEM_PARTYPLAYER",        new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Loot)) },

            { "SMT_GC_SYSMSG_GUILD_CHIEF_CHANGED",      new Action<string, SystemMessage>(HandleNewGuildMasterMessage) },

            { "SMT_ACCOMPLISH_ACHIEVEMENT_GRADE_ALL",   new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Laurel)) },

            { "SMT_DROPDMG_DAMAGE",                     new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Damage)) },

            { "SMT_FIELD_EVENT_REWARD_AVAILABLE",       new Action<string, SystemMessage>(HandleClearedGuardianQuestsMessage) },
            { "SMT_FIELD_EVENT_ENTER",                  new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guardian)) },
            { "SMT_FIELD_EVENT_LEAVE",                  new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guardian)) },
            { "SMT_FIELD_EVENT_COMPLETE",               new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guardian)) },
            { "SMT_FIELD_EVENT_FAIL_OVERTIME",          new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guardian)) },
            { "SMT_FIELD_EVENT_CLEAR_REWARD_SENT",      new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guardian)) },
            { "SMT_FIELD_EVENT_WORLD_ANNOUNCE",         new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guardian)) },

            { "SMT_ITEM_DECOMPOSE_COMPLETE",            new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Loot)) },
            { "SMT_WAREHOUSE_ITEM_DRAW",                new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Loot)) },
            { "SMT_WAREHOUSE_ITEM_INSERT",              new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Loot)) },

            { "SMT_GC_MSGBOX_APPLYLIST_1",              new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_MSGBOX_APPLYLIST_2",              new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_MSGBOX_APPLYRESULT_1",            new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_MSGBOX_APPLYRESULT_2",            new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_SYSMSG_ACCEPT_GUILD_APPLY",       new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_SYSMSG_GUILD_MEMBER_BANNED",      new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_SYSMSG_GUILD_MEMBER_BANNED_2",    new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_SYSMSG_LEAVE_GUILD",              new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_SYSMSG_LEAVE_GUILD_FRIEND",       new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },
            { "SMT_GC_SYSMSG_REFUSE_GUILD_APPLY",       new Action<string, SystemMessage>((sys, srv) => Redirect(sys, srv, ChatChannel.Guild)) },

            { "SMT_FIELDBOSS_APPEAR",                   new Action<string, SystemMessage>(HandleFieldBossAppear) },     //by HQ 20181224
            { "SMT_FIELDBOSS_DIE_GUILD",                new Action<string, SystemMessage>(HandleFieldBossDie) },        //by HQ 20181224
            { "SMT_FIELDBOSS_DIE_NOGUILD",              new Action<string, SystemMessage>(HandleFieldBossDie) },        //by HQ 20181224
        };

        private static void Redirect(string srvMsg, SystemMessage sysMsg, ChatChannel ch)
        {
            var msg = new ChatMessage(srvMsg, sysMsg, ch);
            ChatWindowManager.Instance.AddChatMessage(msg);
        }

        //by HQ 20181224
        private static void HandleFieldBossAppear(string srvMsg, SystemMessage sysMsg)
        {
            var msg = new ChatMessage(srvMsg, sysMsg, (ChatChannel)sysMsg.ChatChannel);
            ChatWindowManager.Instance.AddChatMessage(msg);

            if (!SettingsHolder.WebhookEnabledFieldBoss) return;

            // @4157 \v
            // regionName \v @rgn:213 \v
            // npcName \v @creature:26#5001

            var monsterName = GetFieldBossName(srvMsg);
            var regName = srvMsg.Split('\v')[2].Replace("@rgn:", "");
            var regId = uint.Parse(regName);

            SessionManager.DB.RegionsDatabase.Names.TryGetValue(regId, out var regionName);

            TimeManager.Instance.ExecuteFieldBossSpawnWebhook(monsterName, regionName, msg.RawMessage);

        }

        private static string GetFieldBossName(string srvMsg)
        {
            // only for 'SMT_FIELDBOSS_*'
            var srvMsgSplit = srvMsg.Split('\v');
            var npcName = srvMsgSplit.Last().Replace("@creature:", "");
            var zoneId = uint.Parse(npcName.Split('#')[0]);
            var templateId = uint.Parse(npcName.Split('#')[1]);
            SessionManager.DB.MonsterDatabase.TryGetMonster(templateId, zoneId, out var m);
            return m.Name;

        }
        //by HQ 20181224
        private static void HandleFieldBossDie(string srvMsg, SystemMessage sysMsg)
        {
            var msg = new ChatMessage(srvMsg, sysMsg, (ChatChannel)sysMsg.ChatChannel);
            ChatWindowManager.Instance.AddChatMessage(msg);

            if (!SettingsHolder.WebhookEnabledFieldBoss) return;

            //@4158
            //guildNameWish
            //userName쿤
            //npcname@creature:26#5001

            //@????
            //userName쿤
            //npcname@creature:26#5001

            var monsterName = GetFieldBossName(srvMsg);

            TimeManager.Instance.ExecuteFieldBossDieWebhook(monsterName, msg.RawMessage);



            //if (srvMsg.Contains("@creature:39#501"))     // Hazar
            //{
            //    TimeManager.Instance.ExecuteFieldBossWebhook(501, 2);
            //    //Log.F("FieldBoss.log", $"[{nameof(HandleFieldBossDie)}] {srvMsg}"); //by HQ 20181228
            //}
            //else if (srvMsg.Contains("@creature:51#4001"))    // Kelos
            //{
            //    TimeManager.Instance.ExecuteFieldBossWebhook(4001, 2);
            //    //Log.F("FieldBoss.log", $"[{nameof(HandleFieldBossDie)}] {srvMsg}"); //by HQ 20181228
            //}
            //else if (srvMsg.Contains("@creature:26#5001"))    // Ortan
            //{
            //    TimeManager.Instance.ExecuteFieldBossWebhook(5001, 2);
            //    //Log.F("FieldBoss.log", $"[{nameof(HandleFieldBossDie)}] {srvMsg}"); //by HQ 20181228
            //}
            //else
            //{
            //    //Log.F("FieldBoss.log", $"[{nameof(HandleFieldBossDie)}] {srvMsg}"); //by HQ 20181228
            //}
        }

        private static bool Process(string serverMsg, SystemMessage sysMsg, string opcodeName)
        {
            if (!Processor.TryGetValue(opcodeName, out var type) || type == null) return false;
            type.DynamicInvoke(serverMsg, sysMsg);
            return true;
        }
        #endregion
    }
}