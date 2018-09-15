﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCC.TeraCommon.Game;
using TCC.TeraCommon.Game.Messages;
using TCC.TeraCommon.Game.Services;

namespace TCC.Parsing.Messages
{
    public class S_DESTROY_GUILD_TOWER : ParsedMessage
    {
        public Vector3f Location { get; }
        public uint SourceGuildId { get; }
        public uint TargetGuildId { get; }
        public string SourceGuildName { get; }
        public string PlayerName { get; }
        public string TargetGuildName { get; }

        public S_DESTROY_GUILD_TOWER(TeraMessageReader reader) : base(reader)
        {
            try
            {
                reader.Skip(2 + 2 + 2);
                Location = reader.ReadVector3f();
                SourceGuildId = reader.ReadUInt32();
                TargetGuildId = reader.ReadUInt32();
                SourceGuildName = reader.ReadTeraString();
                PlayerName = reader.ReadTeraString();
                TargetGuildName = reader.ReadTeraString();
            }
            catch (Exception e)
            {

            }
        }
    }
}
