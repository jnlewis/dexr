using System;
using System.Collections.Generic;
using System.Text;

namespace DEXR.Core.Consensus
{
    public static class ListMerge
    {
        public static List<string> MergeWithEvenDistribution(List<string> listA, List<string> listB)
        {
            if (listA == null || listA.Count == 0)
                return listB;
            if (listB == null || listB.Count == 0)
                return listA;

            List<string> result = new List<string>();
            List<string> mergeFrom;
            List<string> mergeTo;

            if(listA.Count > listB.Count)
            {
                mergeFrom = listB;
                mergeTo = listA;
            }
            else
            {
                mergeFrom = listA;
                mergeTo = listB;
            }

            int spread = Convert.ToInt32(mergeTo.Count / mergeFrom.Count);
            if (spread == 0)
                spread = 1;

            int j = 0;
            for(int i=0; i<mergeTo.Count; i++)
            {
                if(j < mergeFrom.Count)
                {
                    if (i % spread == 0)
                    {
                        result.Add(mergeFrom[j]);
                        j++;
                    }
                }

                result.Add(mergeTo[i]);
            }

            //add last item or remaining items
            while(j < mergeFrom.Count)
            {
                result.Add(mergeFrom[j]);
                j++;
            }

            return result;
        }
    }
}
