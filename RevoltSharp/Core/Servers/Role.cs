﻿using Optional.Unsafe;
using System.Numerics;

namespace RevoltSharp
{
    public class Role
    {
        public string Id { get; internal set; }
        internal RevoltClient Client;
        public string ServerId { get; internal set; }
        public string Name { get; internal set; }
        public int[] Permissions { get; internal set; }
        public bool Hoist { get; internal set; }
        public BigInteger Rank { get; internal set; }
        public string Color { get; internal set; }

        internal void Update(PartialRoleJson json)
        {
            if (json.name.HasValue)
                Name = json.name.ValueOrDefault();

            if (json.permissions.HasValue)
                Permissions = json.permissions.ValueOrDefault();

            if (json.hoist.HasValue)
                Hoist = json.hoist.ValueOrDefault();

            if (json.rank.HasValue)
                Rank = json.rank.ValueOrDefault();

            if (json.colour.HasValue)
                Color = json.colour.ValueOrDefault();
        }

        internal Role Clone()
        {
            return (Role)this.MemberwiseClone();
        }
    }
}
