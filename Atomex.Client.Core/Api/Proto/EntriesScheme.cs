﻿using System.Collections.Generic;
using Atomex.Common.Proto;
using Atomex.MarketData;

namespace Atomex.Api.Proto
{
    public class EntriesScheme : ProtoScheme<List<Entry>>
    {
        public EntriesScheme(byte messageId)
            : base(messageId)
        {
            Model.Add(typeof(Entry), true)
                .AddRequired(nameof(Entry.TransactionId))
                .AddRequired(nameof(Entry.Symbol))
                .AddRequired(nameof(Entry.Side))
                .AddRequired(nameof(Entry.Price))
                .AddRequired(nameof(Entry.QtyProfile));
        }
    }
}