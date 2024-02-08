using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollaps
{
    public class PatternNeighbours
    {
        public Dictionary<Direction, HashSet<int>> directionNeighbourPatternDictionary = new Dictionary<Direction, HashSet<int>>();

        public void AddPatternToDirection(Direction dir, int patternIndex)
        {
            if (directionNeighbourPatternDictionary.ContainsKey(dir))
            {
                directionNeighbourPatternDictionary[dir].Add(patternIndex);
            }
            else
            {
                directionNeighbourPatternDictionary.Add(dir, new HashSet<int>() { patternIndex });
            }
        }

        public HashSet<int> GetNeighboursInDirection(Direction dir)
        {
            if (directionNeighbourPatternDictionary.ContainsKey(dir))
            {
                return directionNeighbourPatternDictionary[dir];
            }
            return new HashSet<int>();
        }

        public void AddNeighbours(PatternNeighbours neighbours)
        {
            foreach (var item in neighbours.directionNeighbourPatternDictionary)
            {
                if (directionNeighbourPatternDictionary.ContainsKey(item.Key) == false)
                {
                    directionNeighbourPatternDictionary.Add(item.Key, new HashSet<int>());

                }
                directionNeighbourPatternDictionary[item.Key].UnionWith(item.Value);

            }
        }

    }

}
