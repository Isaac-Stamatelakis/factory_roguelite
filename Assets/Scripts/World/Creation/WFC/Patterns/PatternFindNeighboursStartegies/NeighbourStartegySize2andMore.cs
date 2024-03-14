using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollaps
{
    public class NeighbourStartegySize2andMore : IFindNeighboutStrategy
    {
        public Dictionary<int, PatternNeighbours> FIndNeighbours(PatternDataResults patterndataResults)
        {
            Dictionary<int, PatternNeighbours> patternDictionary = new Dictionary<int, PatternNeighbours>();
            foreach (var patternDataToCheck in patterndataResults.PatternIndexDictionary)
            {
                foreach (var possibleNeighbourPatternData in patterndataResults.PatternIndexDictionary)
                {
                    FindNeighboursInAllDirections(patternDictionary, patternDataToCheck, possibleNeighbourPatternData);
                }
            }
            return patternDictionary;
        }

        private static void FindNeighboursInAllDirections(Dictionary<int, PatternNeighbours> patternDictionary, KeyValuePair<int, PatternData> patternDataToCheck, KeyValuePair<int, PatternData> possibleNeighbourPatternData)
        {
            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            {

                if (patternDataToCheck.Value.CompareGrid(dir, possibleNeighbourPatternData.Value))
                {
                    if (patternDictionary.ContainsKey(patternDataToCheck.Key) == false)
                    {
                        patternDictionary.Add(patternDataToCheck.Key, new PatternNeighbours());
                    }
                    patternDictionary[patternDataToCheck.Key].AddPatternToDirection(dir, possibleNeighbourPatternData.Key);
                }
            }
        }
    }
}

