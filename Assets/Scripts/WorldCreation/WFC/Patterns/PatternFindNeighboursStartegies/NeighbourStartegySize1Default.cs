using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollaps
{
    public class NeighbourStartegySize1Default : IFindNeighboutStrategy
    {
        public Dictionary<int, PatternNeighbours> FIndNeighbours(PatternDataResults patterndataResults)
        {
            Dictionary<int, PatternNeighbours> result = new Dictionary<int, PatternNeighbours>();
            FindNeighboursForEachPattern(patterndataResults, result);
            return result;
        }

        private static void FindNeighboursForEachPattern(PatternDataResults patterndataResults, Dictionary<int, PatternNeighbours> result)
        {
            for (int y = 0; y < patterndataResults.GetGridLengthInY(); y++)
            {
                for (int x = 0; x < patterndataResults.GetGridLengthInX(); x++)
                {
                    PatternNeighbours neighbours = PatternFinder.CheckNeighboursInEachDirection(x, y, patterndataResults);
                    PatternFinder.AddNeighboursToDictionary(result, patterndataResults.GetIndexAt(x, y), neighbours);
                }
            }
        }
    }
}

