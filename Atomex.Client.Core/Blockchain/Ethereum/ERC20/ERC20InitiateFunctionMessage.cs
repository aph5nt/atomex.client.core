﻿using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Atomex.Blockchain.Ethereum.ERC20
{
    [Function("initiate")]
    public class ERC20InitiateFunctionMessage : FunctionMessage
    {
        [Parameter("bytes32", "_hashedSecret", 1)]
        public byte[] HashedSecret { get; set; }

        [Parameter("address", "_contract", 2)]
        public string ERC20Contract { get; set; }

        [Parameter("address", "_participant", 3)]
        public string Participant { get; set; }

        [Parameter("uint256", "_refundTimestamp", 4)]
        public long RefundTimestamp { get; set; }

        [Parameter("uint256", "_countdown", 5)]
        public long Countdown { get; set; }

        [Parameter("uint256", "_value", 6)]
        public BigInteger Value { get; set; }

        [Parameter("uint256", "_payoff", 7)]
        public BigInteger RedeemFee { get; set; }

        [Parameter("bool", "_active", 8)]
        public bool Active { get; set; }
    }
}
