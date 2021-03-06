﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Atomex.Abstract;
using Atomex.Blockchain.Abstract;
using Atomex.Blockchain.Helpers;
using Atomex.Common;
using Atomex.Core;
using Atomex.Cryptography;
using Serilog;

namespace Atomex.Swaps.Abstract
{
    public abstract class CurrencySwap : ICurrencySwap
    {
        public const int DefaultSecretSize = 32;
        public const int DefaultSecretHashSize = 32; //20;

        public const int DefaultInitiatorLockTimeInSeconds = 10 * 60 * 60; // 10 hours
        public const int DefaultAcceptorLockTimeInSeconds = 5 * 60 * 60; // 5 hours
        protected const int DefaultGetTransactionAttempts = 10;

        protected static TimeSpan ConfirmationCheckInterval = TimeSpan.FromSeconds(60);
        protected static TimeSpan OutputSpentCheckInterval = TimeSpan.FromSeconds(60);
        protected static TimeSpan GetTransactionInterval = TimeSpan.FromSeconds(60);
        protected static TimeSpan RefundTimeCheckInterval = TimeSpan.FromSeconds(60);
        protected static TimeSpan ForceRefundInterval = TimeSpan.FromMinutes(5);
        public static TimeSpan RedeemTimeReserve = TimeSpan.FromMinutes(90);
        protected static TimeSpan PartyRedeemTimeReserve = TimeSpan.FromMinutes(95);
        public static TimeSpan PaymentTimeReserve = TimeSpan.FromMinutes(60);

        public OnSwapUpdatedDelegate InitiatorPaymentConfirmed { get; set; }
        public OnSwapUpdatedDelegate AcceptorPaymentConfirmed { get; set; }
        public OnSwapUpdatedDelegate AcceptorPaymentSpent { get; set; }
        public OnSwapUpdatedDelegate SwapUpdated { get; set; }

        public string Currency { get; }
        protected readonly ISwapClient SwapClient;
        protected readonly ICurrencies Currencies;

        protected CurrencySwap(
            string currency,
            ISwapClient swapClient,
            ICurrencies currencies)
        {
            Currency = currency;
            SwapClient = swapClient ?? throw new ArgumentNullException(nameof(swapClient));
            Currencies = currencies ?? throw new ArgumentNullException(nameof(currencies));
        }

        public abstract Task PayAsync(
            Swap swap,
            CancellationToken cancellationToken = default);

        public abstract Task StartPartyPaymentControlAsync(
            Swap swap,
            CancellationToken cancellationToken = default);

        public abstract Task RedeemAsync(
            Swap swap,
            CancellationToken cancellationToken = default);

        public abstract Task RedeemForPartyAsync(
            Swap swap,
            CancellationToken cancellationToken = default);

        public abstract Task RefundAsync(
            Swap swap,
            CancellationToken cancellationToken = default);

        public abstract Task StartWaitForRedeemAsync(
            Swap swap,
            CancellationToken cancellationToken = default);

        public abstract Task StartWaitForRedeemBySomeoneAsync(
            Swap swap,
            CancellationToken cancellationToken = default);

        public virtual Task HandlePartyPaymentAsync(
            Swap swap,
            Swap clientSwap,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        protected void RaiseInitiatorPaymentConfirmed(Swap swap)
        {
            InitiatorPaymentConfirmed?.Invoke(this, new SwapEventArgs(swap));
        }

        protected void RaiseAcceptorPaymentConfirmed(Swap swap)
        {
            AcceptorPaymentConfirmed?.Invoke(this, new SwapEventArgs(swap));
        }

        protected void RaiseAcceptorPaymentSpent(Swap swap)
        {
            AcceptorPaymentSpent?.Invoke(this, new SwapEventArgs(swap));
        }

        protected void RaiseSwapUpdated(Swap swap, SwapStateFlags changedFlag)
        {
            SwapUpdated?.Invoke(this, new SwapEventArgs(swap, changedFlag));
        }

        public static byte[] CreateSwapSecret()
        {
            return Rand.SecureRandomBytes(DefaultSecretSize);
        }

        public static byte[] CreateSwapSecretHash(byte[] secretBytes)
        {
            return Sha256.Compute(secretBytes, 2);
        }

        public static byte[] CreateSwapSecretHash160(byte[] secretBytes)
        {
            return Ripemd160.Compute(Sha256.Compute(secretBytes));
        }

        protected Task TrackTransactionConfirmationAsync(
            Swap swap,
            Currency currency,
            string txId,
            Action<Swap, IBlockchainTransaction, CancellationToken> confirmationHandler = null,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await currency
                        .IsTransactionConfirmed(
                            txId: txId,
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    if (result.HasError)
                        break;

                    if (result.Value.IsConfirmed)
                    {
                        confirmationHandler?.Invoke(swap, result.Value.Transaction, cancellationToken);
                        break;
                    }

                    await Task.Delay(ConfirmationCheckInterval, cancellationToken)
                        .ConfigureAwait(false);
                }
            }, cancellationToken);
        }

        protected Task ControlRefundTimeAsync(
            Swap swap,
            DateTime refundTimeUtc,
            Action<Swap, CancellationToken> refundTimeReachedHandler = null,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Log.Debug("Refund time check for swap {@swapId}", swap.Id);

                    var refundTimeReached = DateTime.UtcNow >= refundTimeUtc;

                    if (refundTimeReached)
                    {
                        refundTimeReachedHandler?.Invoke(swap, cancellationToken);
                        break;
                    }

                    await Task.Delay(RefundTimeCheckInterval, cancellationToken)
                        .ConfigureAwait(false);
                }
            }, cancellationToken);
        }

        protected bool CheckPayRelevance(Swap swap)
        {
            if (swap.IsAcceptor)
            {
                var acceptorRefundTimeUtc = swap.TimeStamp
                    .ToUniversalTime()
                    .AddSeconds(DefaultAcceptorLockTimeInSeconds);

                var paymentDeadline = acceptorRefundTimeUtc - PaymentTimeReserve;

                if (DateTime.UtcNow > paymentDeadline)
                {
                    Log.Error("Payment deadline reached for swap {@swap}", swap.Id);

                    swap.Cancel();
                    RaiseSwapUpdated(swap, SwapStateFlags.IsCanceled);

                    return false;
                }
            }

            return true;
        }
    }
}