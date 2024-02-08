using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WaveFunctionCollaps
{
    public class CoreHelper
    {
        float totalFrequenc = 0;
        float totalFrequencyLog = 0;
        PatternManager patternManager;
        public CoreHelper(PatternManager patternManager)
        {
            this.patternManager = patternManager;
            for (int i = 0; i < this.patternManager.GetNumberOfPatterns(); i++)
            {
                totalFrequenc += this.patternManager.GetPatternFrequency(i);
            }
            totalFrequencyLog = Mathf.Log(totalFrequenc, 2);
        }
        public int SelectSolutionPatternFromFrequency(List<int> possibleValues)
        {
            List<float> valueFrequenciesFractions = GetListOfWeightsFromIndices(possibleValues);
            float randomValue = Random.Range(0, valueFrequenciesFractions.Sum());
            float sum = 0;
            int index = 0;
            foreach (var item in valueFrequenciesFractions)
            {
                sum += item;
                if (randomValue <= sum)
                {
                    return index;
                }
                index++;
            }
            return index;
        }

        private List<float> GetListOfWeightsFromIndices(List<int> possibleValues)
        {

            var valueFrequencies = possibleValues.Aggregate(new List<float>(), (acc, val) =>
            {
                acc.Add(patternManager.GetPatternFrequency(val));
                return acc;
            },
                                acc => acc).ToList();
            return valueFrequencies;
        }

        public List<VectorPair> Create4DirectionNeighbours(Vector2Int cellCoordinates, Vector2Int previousCell)
        {
            List<VectorPair> list = new List<VectorPair>()
            {
                new VectorPair(cellCoordinates, cellCoordinates + new Vector2Int(1, 0), Direction.Right,previousCell),
                new VectorPair(cellCoordinates, cellCoordinates + new Vector2Int(-1, 0), Direction.Left, previousCell),
                new VectorPair(cellCoordinates, cellCoordinates + new Vector2Int(0, 1), Direction.Up, previousCell),
                new VectorPair(cellCoordinates, cellCoordinates + new Vector2Int(0, -1), Direction.Down, previousCell)
            };
            return list;
        }

        public List<VectorPair> Create4DirectionNeighbours(Vector2Int cellCoordinates)
        {
            return Create4DirectionNeighbours(cellCoordinates, cellCoordinates);
        }

        public float CalculateEntropy(Vector2Int position, OutputGrid outputGrid)
        {
            float sum = 0;
            foreach (var possibleIndex in outputGrid.GetPossibleValuesForPositon(position))
            {
                sum += patternManager.GetPatternFrequencyLog2(possibleIndex);
            }
            return totalFrequencyLog - (sum / totalFrequenc);
        }

        public List<VectorPair> CheckIfNeighboursAreCollapsed(VectorPair pairToCheck, OutputGrid outputGrid)
        {

            return Create4DirectionNeighbours(pairToCheck.CellToPropagatePosition, pairToCheck.BaseCellPosition)
                .Where(x => outputGrid.CheckIfValidPosition(x.CellToPropagatePosition) && outputGrid.CheckIfCellIsCollapsed(x.CellToPropagatePosition) == false)
                .ToList();

        }

        public bool CheckCellSOlutionForCollisions(Vector2Int cellCoordinates, OutputGrid outputGrid)
        {
            foreach (var neighbour in Create4DirectionNeighbours(cellCoordinates))
            {
                if (outputGrid.CheckIfValidPosition(neighbour.CellToPropagatePosition) == false)
                {
                    continue;
                }
                HashSet<int> possibleIndices = new HashSet<int>();
                foreach (var patternIndexAtNeighbour in outputGrid.GetPossibleValuesForPositon(neighbour.CellToPropagatePosition))
                {
                    var possibleNeighborusForBase = patternManager.GetPossibleNeighboursForPatternInDIrection(patternIndexAtNeighbour, neighbour.DiectionFromBase.GetOppositeDirectionTo());
                    possibleIndices.UnionWith(possibleNeighborusForBase);
                }
                if (possibleIndices.Contains(outputGrid.GetPossibleValuesForPositon(cellCoordinates).First()) == false)
                {

                    return true;
                }
            }

            return false;
        }
    }

}
