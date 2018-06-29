using System;

namespace DEXR.Core.Const
{
    public static class ResponseCodes
    {
        public const string Success = "1";
        public const string Error = "500";

        public const string InsufficientFundsForFee = "9";
        public const string InsufficientFunds = "10";
        public const string SpeakerNotRecognized = "11";
        public const string ServerNotStarted = "12";
        public const string InvalidSignature = "13";
        public const string TokenAlreadyExists = "14";
        public const string InsufficientPermission = "15";
        public const string NotEnoughOrdersAvailable = "16";
        public const string InvalidHash = "17";
        public const string PendingTransactionAndBlockMismatch = "18";
        public const string OrderDoesNotBelongToRequester = "19";
        public const string ConsensusAlreadyActive = "20";
        public const string NoChainExistOnNetwork = "21";
        public const string TokenNotRecognized = "22";
        public const string ViewBlocksInvalidIndex = "23";
        public const string ViewBlocksTooMany = "24";


    }
}