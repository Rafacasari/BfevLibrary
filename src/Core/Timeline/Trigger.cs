﻿using EvflLibrary.Common;
using EvflLibrary.Parsers;
using System.Text.Json.Serialization;

namespace EvflLibrary.Core
{
    public enum TriggerType : byte
    {
        Enter = 1, Leave = 2
    }

    public class Trigger : IEvflDataBlock
    {
        public short ClipIndex { get; set; }
        public TriggerType Type { get; set; }

        [JsonConstructor]
        public Trigger(short clipIndex, TriggerType type)
        {
            ClipIndex = clipIndex;
            Type = type;
        }

        public Trigger(EvflReader reader)
        {
            Read(reader);
        }

        public IEvflDataBlock Read(EvflReader reader)
        {
            ClipIndex = reader.ReadInt16();
            Type = (TriggerType)reader.ReadByte();
            reader.BaseStream.Position += 1; // Padding (byte)

            return this;
        }

        public void Write(EvflWriter writer)
        {
            writer.Write(ClipIndex);
            writer.Write((byte)Type);
            writer.Seek(1, SeekOrigin.Current);
        }
    }
}
