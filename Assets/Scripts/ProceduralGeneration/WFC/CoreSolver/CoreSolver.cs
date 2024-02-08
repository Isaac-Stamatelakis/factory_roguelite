using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WaveFunctionCollaps
{
    public class CoreSolver
    {
        
        OutputGrid outputGrid;
        PatternManager patternManager;
        CoreHelper coreHelper;
        PropagationHelper propagationHelper;

        public CoreSolver(OutputGrid outputGrid, PatternManager patternManager)
        {
            this.outputGrid = outputGrid;
            this.patternManager = patternManager;
            coreHelper = new CoreHelper(this.patternManager);
            this.propagationHelper = new PropagationHelper(this.outputGrid, this.coreHelper);
        }

        public void Propagate()
        {
            //Debug.Log("Propagation result:");
            while (propagationHelper.PairsToPropagate.Count > 0)
            {
                var propagatePair = propagationHelper.PairsToPropagate.Dequeue();
                if (propagationHelper.CheckIfPairShouldBeProcessed(propagatePair))
                {
                    ProcessCells(propagatePair);
                }
                if (propagationHelper.CheckForConflics() || outputGrid.CheckIfGridIsSolved())
                {
                    return;
                }
            }
            if (propagationHelper.CheckForConflics() && propagationHelper.PairsToPropagate.Count == 0 && propagationHelper.LowestEntropySet.Count == 0)
            {
                propagationHelper.SetConflictFlag();
            }
            //outputGrid.PrintResultsToConsol();
        }
        

        private void ProcessCells(VectorPair propagatePair)
        {
            if (outputGrid.CheckIfCellIsCollapsed(propagatePair.CellToPropagatePosition))
            {
                propagationHelper.EnqueueUncollapsedNeighbours(propagatePair);

            }
            else
            {
                PropagateNeighbours(propagatePair);
            }
        }


        private void PropagateNeighbours(VectorPair propagatePair)
        {
            var possibleValuesAtNeighbour = outputGrid.GetPossibleValuesForPositon(propagatePair.CellToPropagatePosition);
            int startCount = possibleValuesAtNeighbour.Count();

            RemoverImpossibleNeighbours(propagatePair, possibleValuesAtNeighbour);

            int newPossiblePatternCount = possibleValuesAtNeighbour.Count;
            propagationHelper.AnalyzePropagatonResults(propagatePair, startCount, newPossiblePatternCount);

        }



        private void RemoverImpossibleNeighbours(VectorPair propagatePair, HashSet<int> possibleValuesAtNeighbour)
        {
            HashSet<int> possibleIndices = new HashSet<int>();


            foreach (var patternIndexAtBase in outputGrid.GetPossibleValuesForPositon(propagatePair.BaseCellPosition))
            {
                var possibleNeighborusForBase = patternManager.GetPossibleNeighboursForPatternInDIrection(patternIndexAtBase, propagatePair.DiectionFromBase);

                possibleIndices.UnionWith(possibleNeighborusForBase);


            }

            possibleValuesAtNeighbour.IntersectWith(possibleIndices);

        }


        public Vector2Int GetLowestEntropyCell()
        {
            if (propagationHelper.LowestEntropySet.Count <= 0)
            {
                return outputGrid.GetRandomCell();
            }
            else
            {

                var lowestEntropyElement = propagationHelper.LowestEntropySet.First();
                Vector2Int returnVEctor = lowestEntropyElement.Position;
                propagationHelper.LowestEntropySet.Remove(lowestEntropyElement);
                return returnVEctor;

            }
        }
        public void CollapseCell(Vector2Int cellCoordinates)
        {
            var possibleValues = outputGrid.GetPossibleValuesForPositon(cellCoordinates).ToList();

            if (possibleValues.Count == 0 || possibleValues.Count == 1)
            {
                return;
            }
            else
            {
                int index = coreHelper.SelectSolutionPatternFromFrequency(possibleValues);

                outputGrid.SetPatternOnPosition(cellCoordinates.x, cellCoordinates.y, possibleValues[index]);
            }

            if (coreHelper.CheckCellSOlutionForCollisions(cellCoordinates, outputGrid) == false)
            {
                propagationHelper.AddNewPairsToPropagateQueue(cellCoordinates, cellCoordinates);
            }
            else
            {
                propagationHelper.SetConflictFlag();
            }
        }

        public bool CheckIfSolved()
        {
            return outputGrid.CheckIfGridIsSolved();
        }

        public bool CheckForConflics()
        {
            return propagationHelper.CheckForConflics();
        }
    }
}

