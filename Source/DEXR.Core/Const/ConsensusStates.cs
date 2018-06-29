using System;

namespace DEXR.Core.Const
{
    public static class ConsensusStates
    {
        public const string Inactive = "Inactive";
        public const string Started = "Started";
        public const string SyncingChain = "SyncingChain";
        public const string WaitingForSpeaker = "WaitingForSpeaker";
        public const string CreatingBlock = "CreatingBlock";
        public const string SelectingSpeaker = "SelectingSpeaker";
        public const string BeginningEpoch = "BeginningEpoch";
        public const string UpdatingNodes = "UpdatingNodes"; 

    }
}
