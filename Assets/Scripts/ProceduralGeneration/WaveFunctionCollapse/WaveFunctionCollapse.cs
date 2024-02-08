using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using WaveFunctionCollapseHelpers;

namespace WaveFunctionCollapse {

/// <summary>
/// This implements wave function collapse 
/// Thanks to Sunny Value Studio: https://www.youtube.com/@SunnyValleyStudio
/// </summary>
public class WaveFunctionCollapse
{

    public static int patternSize = 2;
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
public class ValueManager<T> {
    public ValueManager(T[,] input, int rows, int columns) {
        this.rows = rows;
        this.columns = columns;
        createGridOfInput(input);   
    }
    private int rows;
    private int columns;
    private int[,] grid;
    private List<T> indexToString;

    private void createGridOfInput(T[,] input) {
        indexToString = new List<T>();
        Dictionary<T, int> stringToIndex = new Dictionary<T, int>();
        for (int y = 0; y < rows; y ++) {
            for (int x = 0; x < columns; x++) {
                T str = input[x,y];
                
                if (!indexToString.Contains(str)) {
                    indexToString.Add(str);
                    stringToIndex.Add(str,indexToString.Count-1);
                }
                grid[x,y] = stringToIndex[str];
            }
        }
    }

    public int getGridValue(int x, int y) {
        if (x >= columns || y >= rows) {
            throw new System.IndexOutOfRangeException("Cannot get value from grid, x or y out of range");
        }
        return grid[x,y];
    }
    private void getStringFromIndex(int index) {
        if (index < 0 || index >= indexToString.Count) {
            throw new System.IndexOutOfRangeException("Index not in indexToString");
        }
    }

    public int getGridValuesIncludingOffset(int x, int y) {
        if (x < 0 && y < 0) {
            return getGridValue(x+columns,y+rows);
        }
        if (x < 0 && y >= rows) {
            return getGridValue(x+columns,y-rows);
        }
        if (x >= columns && y < 0) {
            return getGridValue(x-columns,y+rows);
        }
        if (x >= columns && y >= rows) {
            return getGridValue(x-columns,y-rows);
        }
        if (x < 0) {
            return getGridValue(x+columns,y);
        }
        if (x >= columns) {
            return getGridValue(x-columns,y);
        }
        if (y < 0) {
            return getGridValue(x,y+columns);
        }
        if (y >= rows) {
            return getGridValue(x,y-columns);
        }
        return getGridValue(x,y);
    }

    public int[,] getPatternValuesFromGridAt(int x, int y, int patternSize) {
        int[,] arrayToReturn = MyCollectionExtension.CreateJaggedArray<int[,]>(patternSize,patternSize);
        for (int x2 = 0; x2 < patternSize; x2 ++) {
            for (int y2 =0; y2 < patternSize; y2++) {
                arrayToReturn[x2,y2] = getGridValuesIncludingOffset(x+x2,y+y2);
            }
        }
        return arrayToReturn;
    }

    public Vector2 getGridSize() {
        return new Vector2(columns,rows);
    }
} 
} // namespace end
