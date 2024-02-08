using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WaveFunctionCollaps
{
    
    public class WFCCore
    {
        OutputGrid outputGrid;

        PatternManager patternManager;

        private int maxIterations = 0;
        public WFCCore(int outputWidth, int outputHeight, PatternManager patternManager, int maxIterations)
        {
            this.outputGrid = new OutputGrid(outputWidth, outputHeight, patternManager.GetNumberOfPatterns());
            this.patternManager = patternManager;
            this.maxIterations = maxIterations;
        }

        public int[][] CreateOutputGrid()
        {
            int iteration = 0;
            while (iteration < this.maxIterations)
            {
                CoreSolver solver = new CoreSolver(this.outputGrid, this.patternManager);
                int innerIteration = 100;
                while (!solver.CheckForConflics() && !solver.CheckIfSolved())
                {
                    Vector2Int position = solver.GetLowestEntropyCell();
                    solver.CollapseCell(position);
                    solver.Propagate();
                    innerIteration--;
                }
                
                if (solver.CheckForConflics())
                {

                    Debug.Log("\nCOnflict occured. Iteration: " + iteration);
                    iteration++;
                    outputGrid.ResetAllPossibilities();
                    solver = new CoreSolver(this.outputGrid, this.patternManager);
                }
               
                else
                {
                    Debug.Log("Solved on " + iteration+" iteration");
                    break;
                }
                
            }
            if (iteration >= this.maxIterations)
            {
                Debug.Log("COuldn't solve in " + this.maxIterations + " iterations");
            }
            return outputGrid.GetSolvedOutputGrid();
        }




    }
}

