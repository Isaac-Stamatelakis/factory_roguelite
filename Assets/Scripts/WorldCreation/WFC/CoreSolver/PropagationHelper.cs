using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WaveFunctionCollaps
{
    public class PropagationHelper
    {
        OutputGrid outputGrid;
        CoreHelper coreHelper;
        bool cellWithNoSolutionPresent = false;
        SortedSet<LowEntropyCell> lowestEntropySet = new SortedSet<LowEntropyCell>();
        Queue<VectorPair> pairsToPropagate = new Queue<VectorPair>();

        public SortedSet<LowEntropyCell> LowestEntropySet { get => lowestEntropySet;}
        public Queue<VectorPair> PairsToPropagate { get => pairsToPropagate;}

        public PropagationHelper(OutputGrid outputGrid, CoreHelper coreHelper)
        {
            this.outputGrid = outputGrid;
            this.coreHelper = coreHelper;
        }

        public bool CheckIfPairShouldBeProcessed(VectorPair propagatePair)
        {
            return outputGrid.CheckIfValidPosition(propagatePair.CellToPropagatePosition) && propagatePair.AreWeCheckingPreviousCellAgain() == false;
        }

        public void AnalyzePropagatonResults(VectorPair propagatePair, int startCount, int newPossiblePatternCount)
        {
            if (newPossiblePatternCount > 1 && startCount > newPossiblePatternCount)
            {
                AddNewPairsToPropagateQueue(propagatePair.CellToPropagatePosition, propagatePair.BaseCellPosition);
                AddToLowestEntropySet(propagatePair.CellToPropagatePosition, coreHelper);
            }
            if (newPossiblePatternCount == 0)
            {
                cellWithNoSolutionPresent = true;
            }
            if (newPossiblePatternCount == 1)
            {
                cellWithNoSolutionPresent = coreHelper.CheckCellSOlutionForCollisions(propagatePair.CellToPropagatePosition, outputGrid);
            }
        }

        public void AddToLowestEntropySet(Vector2Int cellToPropagatePosition, CoreHelper coreHelper)
        {
            var elementIdLowEntropySet = lowestEntropySet.Where(x => x.Position == cellToPropagatePosition).FirstOrDefault();

            if (elementIdLowEntropySet == null && outputGrid.CheckIfCellIsCollapsed(cellToPropagatePosition) == false)
            {
                float entropy = coreHelper.CalculateEntropy(cellToPropagatePosition, outputGrid);
                lowestEntropySet.Add(new LowEntropyCell(cellToPropagatePosition, entropy));

            }
            else
            {
                lowestEntropySet.Remove(elementIdLowEntropySet);
                elementIdLowEntropySet.Entropy = coreHelper.CalculateEntropy(cellToPropagatePosition, outputGrid);
                lowestEntropySet.Add(elementIdLowEntropySet);



            }
        }

        public void AddNewPairsToPropagateQueue(Vector2Int cellCoordinates, Vector2Int previousCell)
        {
            var list = coreHelper.Create4DirectionNeighbours(cellCoordinates, previousCell);
            foreach (var item in list)
            {
                pairsToPropagate.Enqueue(item);
            }

        }

        

        public void EnqueueUncollapsedNeighbours(VectorPair propagatePair)
        {
            var uncollapsedNeighbours = coreHelper.CheckIfNeighboursAreCollapsed(propagatePair, outputGrid);
            foreach (var uncollapsed in uncollapsedNeighbours)
            {
                pairsToPropagate.Enqueue(uncollapsed);
            }
        }

        public bool CheckForConflics()
        {
            return cellWithNoSolutionPresent;
        }

        public void SetConflictFlag()
        {
            cellWithNoSolutionPresent=true;
        }
    }

}
