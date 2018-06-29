using DEXR.Core.Models;

namespace DEXR.Core.Model.Requests
{
    public class AnnounceNewBlockRequest
    {
        public BlockHeader NewBlockHeader { get; set; }
    }
}
