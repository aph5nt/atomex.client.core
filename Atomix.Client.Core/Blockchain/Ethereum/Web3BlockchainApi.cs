﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Atomix.Blockchain.Abstract;
using Atomix.Common;
using Nethereum.Signer;
using Nethereum.Web3;
using Serilog;

namespace Atomix.Blockchain.Ethereum
{
    public class Web3BlockchainApi : IEthereumBlockchainApi
    {
        public const string InfuraMainNet = "https://mainnet.infura.io";
        public const string InfuraRinkeby = "https://rinkeby.infura.io";
        public const string InfuraRopsten = "https://ropsten.infura.io";

        public const string InfuraMainNetWebSocket = "wss://mainnet.infura.io/ws/v3/1f76c4ad2d4e4ad58d3b08d68c61cb0d";
        public const string InfuraRinkebyWebSocket = "wss://rinkeby.infura.io/ws/v3/1f76c4ad2d4e4ad58d3b08d68c61cb0d";
        public const string InfuraRopstenWebSocket = "wss://ropsten.infura.io/ws/v3/1f76c4ad2d4e4ad58d3b08d68c61cb0d";

        private readonly string _uri;

        public Web3BlockchainApi(Chain chain)
        {
            _uri = UriByChain(chain);

            if (_uri == null)
                throw new NotSupportedException($"Chain {chain} not supported");
        }

        public async Task<BigInteger> GetTransactionCountAsync(
            string address,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var web3 = new Web3(_uri);

            return await web3.Eth.Transactions
                .GetTransactionCount
                .SendRequestAsync(address)
                .ConfigureAwait(false);
        }

        public Task<IEnumerable<IBlockchainTransaction>> GetTransactionsAsync(
            string address,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public async Task<IBlockchainTransaction> GetTransactionAsync(
            string txId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var web3 = new Web3(_uri);

            var tx = await web3.Eth.Transactions
                .GetTransactionByHash
                .SendRequestAsync(txId)
                .ConfigureAwait(false);

            if (tx == null || tx.BlockHash == null)
                return null;

            var block = await web3.Eth.Blocks
                .GetBlockWithTransactionsHashesByHash
                .SendRequestAsync(tx.BlockHash)
                .ConfigureAwait(false);

            var utcTimeStamp = block != null
                ? ((long)block.Timestamp.Value).ToUtcDateTime()
                : DateTime.UtcNow;

            var txReceipt = await web3.Eth.Transactions
                .GetTransactionReceipt
                .SendRequestAsync(txId)
                .ConfigureAwait(false);

            if (txReceipt == null)
            {
                Log.Error("Tx not null, but txReceipt is null!");
                return null;
            }

            return new EthereumTransaction(tx, txReceipt, utcTimeStamp);
        }

        public async Task<string> BroadcastAsync(
            IBlockchainTransaction transaction,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!(transaction is EthereumTransaction ethTx))
                throw new NotSupportedException("Not supported transaction type");

            var web3 = new Web3(_uri);

            var txId = await web3.Eth.Transactions
                .SendRawTransaction
                .SendRequestAsync("0x" + ethTx.RlpEncodedTx)
                .ConfigureAwait(false);

            ethTx.Id = txId; // todo: wtf?
            return txId;
        }

        public static string UriByChain(Chain chain)
        {
            switch (chain)
            {
                case Chain.MainNet:
                    return InfuraMainNet;
                case Chain.Ropsten:
                    return InfuraRopsten;
                case Chain.Rinkeby:
                    return InfuraRinkeby;
                default:
                    return null;
            }
        }

        public static string WsUriByChain(Chain chain)
        {
            switch (chain)
            {
                case Chain.MainNet:
                    return InfuraMainNetWebSocket;
                case Chain.Ropsten:
                    return InfuraRopstenWebSocket;
                case Chain.Rinkeby:
                    return InfuraRinkebyWebSocket;
                default:
                    return null;
            }
        }
    }
}