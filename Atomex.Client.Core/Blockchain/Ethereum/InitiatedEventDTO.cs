using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Atomex.Blockchain.Ethereum
{
    [Event("Initiated")]
    public class InitiatedEventDTO : IEventDTO
    {
        [Parameter("bytes32", "_hashedSecret", 1, true)]
        public byte[] HashedSecret { get; set; }

        [Parameter("address", "_participant", 2, true)]
        public string Participant { get; set; }

        [Parameter("address", "_initiator", 3, false)]
        public string Initiator { get; set; }

        [Parameter("uint256", "_refundTimestamp", 4, false)]
        public BigInteger RefundTimestamp { get; set; }

        [Parameter("uint256", "_countdown", 5, false)]
        public BigInteger Countdown { get; set; }

        [Parameter("uint256", "_value", 6, false)]
        public BigInteger Value { get; set; }

        [Parameter("uint256", "_payoff", 7, false)]
        public BigInteger RedeemFee { get; set; }

        [Parameter("bool", "_active", 8, false)]
        public bool Active { get; set; }
    }
}