﻿using System;
using System.Globalization;
using System.Numerics;
using Atomex.Blockchain.Ethereum;
using Atomex.Wallet.Bip;
using Microsoft.Extensions.Configuration;

namespace Atomex.EthereumTokens
{
    public class ERC20 : Ethereum
    {
        public decimal TransferGasLimit { get; private set; }
        public decimal TransferFeeAmount => TransferGasLimit * GasPriceInGwei / GweiInEth;

        public decimal ApproveGasLimit { get; private set; }
        public decimal ApproveFeeAmount => ApproveGasLimit * GasPriceInGwei / GweiInEth;
        public decimal RewardForRedeem { get; private set; }

        public string ERC20ContractAddress { get; private set; }
        public ulong ERC20ContractBlockNumber { get; private set; }

        public ERC20()
        {
        }

        public ERC20(IConfiguration configuration)
        {
            Update(configuration);
        }

        public override void Update(IConfiguration configuration)
        {
            Name = configuration["Name"];
            Description = configuration["Description"];
            DigitsMultiplier = long.Parse(configuration["DigitsMultiplier"]);
            DustDigitsMultiplier = long.Parse(configuration["DustDigitsMultiplier"]);
            Digits = (int)Math.Log10(DigitsMultiplier);
            Format = $"F{Digits}";

            FeeDigits = Digits;
            FeeCode = "ETH";
            FeeFormat = $"F{FeeDigits}";
            FeeCurrencyName = "ETH";

            HasFeePrice = true;
            FeePriceCode = DefaultGasPriceCode;
            FeePriceFormat = DefaultGasPriceFormat;

            TransferGasLimit = decimal.Parse(configuration["TransferGasLimit"], CultureInfo.InvariantCulture);
            ApproveGasLimit = decimal.Parse(configuration["ApproveGasLimit"], CultureInfo.InvariantCulture);
            InitiateGasLimit = decimal.Parse(configuration["InitiateGasLimit"], CultureInfo.InvariantCulture);
            InitiateWithRewardGasLimit = decimal.Parse(configuration["InitiateWithRewardGasLimit"], CultureInfo.InvariantCulture);
            AddGasLimit = decimal.Parse(configuration["AddGasLimit"], CultureInfo.InvariantCulture);
            RefundGasLimit = decimal.Parse(configuration["RefundGasLimit"], CultureInfo.InvariantCulture);
            RedeemGasLimit = decimal.Parse(configuration["RedeemGasLimit"], CultureInfo.InvariantCulture);
            GasPriceInGwei = decimal.Parse(configuration["GasPriceInGwei"], CultureInfo.InvariantCulture);

            RewardForRedeem = decimal.Parse(configuration[nameof(RewardForRedeem)], CultureInfo.InvariantCulture);

            Chain = ResolveChain(configuration);

            ERC20ContractAddress = configuration["ERC20Contract"];
            ERC20ContractBlockNumber = ulong.Parse(configuration["ERC20ContractBlockNumber"], CultureInfo.InvariantCulture);

            SwapContractAddress = configuration["SwapContract"];
            SwapContractBlockNumber = ulong.Parse(configuration["SwapContractBlockNumber"], CultureInfo.InvariantCulture);

            BlockchainApiBaseUri = configuration["BlockchainApiBaseUri"];
            BlockchainApi = ResolveBlockchainApi(
                configuration: configuration,
                currency: this,
                chain: Chain);

            TxExplorerUri = configuration["TxExplorerUri"];
            AddressExplorerUri = configuration["AddressExplorerUri"];
            TransactionType = typeof(EthereumTransaction);

            IsTransactionsAvailable = true;
            IsSwapAvailable = true;
            Bip44Code = Bip44.Ethereum;  //TODO ?
        }

        public BigInteger TokensToTokenDigits(decimal tokens)
        {
            return new BigInteger(tokens * DigitsMultiplier);
        }

        public decimal TokenDigitsToTokens(BigInteger tokenDigits)
        {
            return (decimal)tokenDigits / DigitsMultiplier;
        }

        public override decimal GetRewardForRedeem()
        {
            return RewardForRedeem / DigitsMultiplier;
        }

        public override decimal GetDefaultFee()
        {
            return TransferGasLimit;
        }
    }

    public class Tether : ERC20
    {
        public Tether()
        {
        }

        public Tether(IConfiguration configuration)
        {
            Update(configuration);
        }
    }

    public class USDC : ERC20
    {
        public USDC()
        {
        }

        public USDC(IConfiguration configuration)
        {
            Update(configuration);
        }
    }

}