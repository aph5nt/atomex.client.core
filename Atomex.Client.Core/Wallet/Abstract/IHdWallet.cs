﻿using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Atomex.Blockchain.Abstract;
using Atomex.Core;
using Atomex.Core.Entities;

namespace Atomex.Wallet.Abstract
{
    public interface IHdWallet
    {
        /// <summary>
        /// Path to wallet
        /// </summary>
        string PathToWallet { get; }

        /// <summary>
        /// Gets wallet's network
        /// </summary>
        Network Network { get; }

        /// <summary>
        /// Gets the value that indicates whether the wallet is locked
        /// </summary>
        bool IsLocked { get; }

        /// <summary>
        /// Lock wallet
        /// </summary>
        void Lock();

        /// <summary>
        /// Unlock wallet
        /// </summary>
        /// <param name="password">Password</param>
        void Unlock(SecureString password);

        /// <summary>
        /// Encrypt wallet keys by <paramref name="password"/>
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>Encryption task</returns>
        Task EncryptAsync(SecureString password);

        /// <summary>
        /// Gets address for <paramref name="currency"/>, <paramref name="chain"/> and key <paramref name="index"/>
        /// </summary>
        /// <param name="currency">Currency</param>
        /// <param name="chain">Chain</param>
        /// <param name="index">Key index</param>
        /// <returns>Address</returns>
        WalletAddress GetAddress(Currency currency, int chain, uint index);

        /// <summary>
        /// Gets public key for service key with <paramref name="index"/>
        /// </summary>
        /// <param name="index">Service key index</param>
        /// <returns>Public key bytes</returns>
        byte[] GetServicePublicKey(uint index);

        /// <summary>
        /// Sign <paramref name="data"/> using public key corresponding to <paramref name="address"/>
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="address">Address</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Signature</returns>
        Task<byte[]> SignAsync(
            byte[] data,
            WalletAddress address,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sign input/output <paramref name="tx"/> corresponding to <paramref name="spentOutputs"/>
        /// </summary>
        /// <param name="tx">In/out transaction to sign</param>
        /// <param name="spentOutputs">Spent outputs</param>
        /// <param name="addressResolver">Address resolver</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True, if transaction successfully signed, otherwise else</returns>
        Task<bool> SignAsync(
            IInOutTransaction tx,
            IEnumerable<ITxOutput> spentOutputs,
            IAddressResolver addressResolver,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sign address based <paramref name="tx"/> by key for <paramref name="address"/>
        /// </summary>
        /// <param name="tx">Address based transaction to sign</param>
        /// <param name="address">Address</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True, if transaction successfully signed, otherwise else</returns>
        Task<bool> SignAsync(
            IAddressBasedTransaction tx,
            WalletAddress address,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sign transaction <paramref name="hash"/> using public key corresponding to <paramref name="address"/>
        /// </summary>
        /// <param name="hash">Transaction hash</param>
        /// <param name="address">Address</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Signature</returns>
        Task<byte[]> SignHashAsync(
            byte[] hash,
            WalletAddress address,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sign <paramerf name="data"/> using service key with <paramref name="keyIndex"/>
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="keyIndex">Service key index</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Signature</returns>
        Task<byte[]> SignByServiceKeyAsync(
            byte[] data,
            uint keyIndex,
            CancellationToken cancellationToken = default(CancellationToken));

        //bool Verify(
        //    WalletAddress walletAddress,
        //    byte[] data,
        //    byte[] signature);

        byte[] GetDeterministicSecret(Currency currency, DateTime timeStamp);
    }
}