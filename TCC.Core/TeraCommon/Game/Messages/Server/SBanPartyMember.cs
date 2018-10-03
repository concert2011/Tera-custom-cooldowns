﻿using TCC.TeraCommon.Game.Services;

namespace TCC.TeraCommon.Game.Messages.Server
{
    public class S_BAN_PARTY_MEMBER : ParsedMessage
    {
        internal S_BAN_PARTY_MEMBER(TeraMessageReader reader) : base(reader)
        {
            reader.ReadUInt16();
            ServerId = reader.ReadUInt32();
            PlayerId = reader.ReadUInt32();
            reader.Skip(4); //unknown ffffffff
            Name = reader.ReadTeraString();
        }

        public uint ServerId { get; }
        public uint PlayerId { get; }
        public string Name { get; }
    }
}