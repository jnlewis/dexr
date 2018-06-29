using System;
using System.Collections.Generic;
using System.Linq;

namespace DEXR.Core.Utility
{
    public class MerkleTree
    {
        public MerkleTreeNode Root;
        public int Depth;

        public MerkleTree(List<string> hashes)
        {
            if (hashes.Count == 0)
                throw new ArgumentException();

            Root = Build(hashes.Select(p => new MerkleTreeNode { Hash = p }).ToArray());

            int depth = 1;
            for (MerkleTreeNode i = Root; i.LeftChild != null; i = i.LeftChild)
                depth++;
            this.Depth = depth;
        }

        private static MerkleTreeNode Build(MerkleTreeNode[] leaves)
        {
            if (leaves.Length == 0)
                throw new ArgumentException();
            if (leaves.Length == 1)
                return leaves[0];

            MerkleTreeNode[] parents = new MerkleTreeNode[(leaves.Length + 1) / 2];
            for (int i = 0; i < parents.Length; i++)
            {
                parents[i] = new MerkleTreeNode();
                parents[i].LeftChild = leaves[i * 2];
                leaves[i * 2].Parent = parents[i];
                if (i * 2 + 1 == leaves.Length)
                {
                    parents[i].RightChild = parents[i].LeftChild;
                }
                else
                {
                    parents[i].RightChild = leaves[i * 2 + 1];
                    leaves[i * 2 + 1].Parent = parents[i];
                }

                parents[i].Hash = HashUtility.SHA256(parents[i].LeftChild.Hash + parents[i].RightChild.Hash);
            }
            return Build(parents); //TailCall
        }
    }

    public class MerkleTreeNode
    {
        public string Hash { get; set; }
        public MerkleTreeNode Parent { get; set; }
        public MerkleTreeNode LeftChild { get; set; }
        public MerkleTreeNode RightChild { get; set; }

        public bool IsLeaf => LeftChild == null && RightChild == null;

        public bool IsRoot => Parent == null;
    }
}
