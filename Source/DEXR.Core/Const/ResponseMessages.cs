using System;

namespace DEXR.Core.Const
{
    public static class ResponseMessages
    {
        public const string Success = "Success";
        public const string Error = "The node is unable to process your request at the moment.";

        public const string InsufficientFundsForFee = "Insufficient Funds For Fee";
        public const string InsufficientFunds = "Sender has insufficient funds.";
        public const string SpeakerNotRecognized = "Speaker not recognized.";
        public const string ServerNotStarted = "Server not started.";
        public const string InvalidSignature = "Invalid signature.";
        public const string TokenAlreadyExists = "Token already exists.";
        public const string InsufficientPermission = "Insufficient permission to perform transaction.";
        public const string NotEnoughOrdersAvailable = "Not Enough Orders Available";
        public const string InvalidHash = "Invalid Hash";
        public const string PendingTransactionAndBlockMismatch = "Pending Transaction And Block Mismatch";
        public const string OrderDoesNotBelongToRequester = "Order Does Not Belong To Requester";
        public const string ConsensusAlreadyActive = "Consensus Already Active";
        public const string NoChainExistOnNetwork = "No Chain Exist On Network";
        public const string TokenNotRecognized = "Token Not Recognized";
        public const string ViewBlocksInvalidIndex = "To index must be greater than from index";
        public const string ViewBlocksTooMany = "You can only view up to 100 blocks at a time";
        public const string NodeNotConsensusReady = "Node is not consensus ready.Transaction will be sent to other network nodes.";
    }
}