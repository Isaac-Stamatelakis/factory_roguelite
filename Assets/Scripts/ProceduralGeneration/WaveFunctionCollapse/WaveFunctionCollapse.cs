using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This implements wave function collapse 
/// </summary>
public class WaveFunctionCollapse
{
    public void generate(string[,] input, int size) {
        string[,] offsetGrid = getOffsetGrid(input, size);
    }

    public string[,] getOffsetGrid(string[,] input, int size) {
        int ratio = 3;
        string[,] offsetGrid = new string[size*ratio,size*ratio]; // offset grid is triple the size of the original

        for (int offsetX = 0; offsetX < ratio; offsetX ++) {
            for (int offsetY = 0; offsetY < ratio; offsetY ++) {
                for (int x = 0; x < size; x ++) {
                    for (int y = 0; y < size; y ++) {
                        offsetGrid[offsetX*size + x,offsetY*size+y] = offsetGrid[x,y];
                    }
                }
            }
        }
        return offsetGrid;
    }
}
