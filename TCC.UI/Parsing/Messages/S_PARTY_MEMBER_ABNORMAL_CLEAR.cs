﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;
using Tera.Game.Messages;

namespace TCC.Parsing.Messages
{
    public class S_PARTY_MEMBER_ABNORMAL_CLEAR : ParsedMessage
    {
        public S_PARTY_MEMBER_ABNORMAL_CLEAR(TeraMessageReader reader) : base(reader)
        {
            ServerId = reader.ReadUInt32();
            PlayerId = reader.ReadUInt32();
        }

        public uint ServerId { get; private set; }
        public uint PlayerId { get; private set; }

    }
}
