using System;

namespace DEXR.Core.Configuration
{
    public static class ConstantConfig
    {
        /// <summary>
        /// Interval between each block in seconds
        /// </summary>
        public const int BlockInterval = 15;

        /// <summary>
        /// The network fee rate per TransactionsCount/NetworkFeeUnit
        /// Eg: 5000 Transactions in a block with NetworkFeeUnit 1000 would yield
        /// MinimumFee = 0.0002 * (5000/1000) = 0.001
        /// </summary>
        public const decimal NetworkFeeRate = 0.0002M;

        /// <summary>
        /// The network fee unit per Transactions
        /// </summary>
        public const long NetworkFeeUnit = 1000;

        /// <summary>
        /// The maximum number of background threads to use when 
        /// broadcasting a message to connected nodes
        /// </summary>
        public const int BroadcastThreadCount = 4;

        /// <summary>
        /// Determines whether the node will verify transactions signature
        /// before pushing the transaction to queue. Disable this setting only
        /// for debugging reasons.
        /// </summary>
        public const bool EnableSignatureVerification = false;
    }
}
