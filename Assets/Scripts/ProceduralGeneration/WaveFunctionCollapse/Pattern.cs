using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using WaveFunctionCollapseHelpers;
using WaveFunctionCollapse;
using System.Reflection;


namespace WaveFunctionCollapseManagers {

public class PatternManager {
    Dictionary<int, PatternData> patternDataIndexDict;
    Dictionary<int, PatternNeighbors> patternPossibleNeighborsDictionary;
    private int patternSize = -1;
    IFindNeighborStrategy strategy;

    public PatternManager(int patternSize) {
        this.patternSize = patternSize;
    }
    public void processGrid<T>(ValueManager<T> valueManager, bool equalWeights, string strategyName = null) {
        NeighborStrategyFactory neighborStrategyFactory = new NeighborStrategyFactory();
        strategy = neighborStrategyFactory.createInstance(strategy == null ? patternSize + " " : strategyName);
    }

    public void createPatterns<T>(ValueManager<T> valueManager, IFindNeighborStrategy strategy, bool equalWeights) {
        PatternDataResults patternFindResult = PatternFinder.getPatternDataFromGrid(valueManager, patternSize, equalWeights);
        patternDataIndexDict = patternFindResult.PatternIndexDict;
        getPatternNeighbours(patternFindResult, strategy);
    }

    private void getPatternNeighbours(PatternDataResults patternDataResults, IFindNeighborStrategy strategy) {

    }
    public PatternData getPatternDataFromIndex(int index) {
        return patternDataIndexDict[index];
    }
    public HashSet<int> getPossibleNeighborsForPatternInDirection(int patternIndex, Direction direction) {
        return patternPossibleNeighborsDictionary[patternIndex].getNeighborsInDirection(direction);
    }
    public float getPatternFrequency(int index) {
        return getPatternDataFromIndex(index).FrequencyRelative;
    }
    public float getPatternFrequencyLog2(int index) {
        return getPatternDataFromIndex(index).FrequencyRelativeLog2;
    }
    public int getNumberOfPatterns() {
        return patternDataIndexDict.Count;
    }
}
public class PatternNeighbors {
    public Dictionary<Direction, HashSet<int>> directionPatternNeighborDictionary = new Dictionary<Direction, HashSet<int>>();
    internal HashSet<int> getNeighborsInDirection(Direction direction) {
        if (directionPatternNeighborDictionary.ContainsKey(direction)) {
            return this.directionPatternNeighborDictionary[direction];
        }
        return new HashSet<int>();
    }
    public void addPatternToDicitonary(Direction direction, int patternIndex) {
        if (directionPatternNeighborDictionary.ContainsKey(direction)) {
            directionPatternNeighborDictionary[direction].Add(patternIndex);
        } else {
            directionPatternNeighborDictionary.Add(direction,new HashSet<int>(){patternIndex});
        }
    }

    public void addNeighbor(PatternNeighbors neighbors) {
        foreach (var item in neighbors.directionPatternNeighborDictionary) {
            if (!directionPatternNeighborDictionary.ContainsKey(item.Key)) {
                directionPatternNeighborDictionary.Add(item.Key,new HashSet<int>());
            } 
            directionPatternNeighborDictionary[item.Key].UnionWith(item.Value);
        }
    }
    
}

public class PatternDataResults {

}
public static class PatternFinder
{
    public static PatternDataResults GetPatternDataFromGrid<T>(ValueManager<T> valuesManager, int patternSize, bool equalWeights)
    {
        Dictionary<string, PatternData> patternHashcodeDictionary = new Dictionary<string, PatternData>();
        Dictionary<int, PatternData> patternIndexDictionary = new Dictionary<int, PatternData>();
        Vector2 sizeOfGrid = valuesManager.getGridSize();
        int patternGridSizeX = 0;
        int patternGridSizeY = 0;
        int rowMin = -1, colMin = -1, rowMax =-1, colMax =-1;
        if (patternSize < 3)
        {
            patternGridSizeX = (int)sizeOfGrid.x + 3 - patternSize;
            patternGridSizeY = (int)sizeOfGrid.y + 3 - patternSize;
            rowMax = patternGridSizeY - 1;
            colMax = patternGridSizeX - 1;
        }
        else
        {
            patternGridSizeX = (int)sizeOfGrid.x + patternSize - 1;
            patternGridSizeY = (int)sizeOfGrid.y + patternSize - 1;
            rowMin = 1 - patternSize;
            colMin = 1 - patternSize;
            rowMax = (int)sizeOfGrid.y;
            colMax = (int)sizeOfGrid.x;
        }

        int[][] patternIndicesGrid = MyCollectionExtension.CreateJaggedArray<int[][]>(patternGridSizeY, patternGridSizeX);
        int totalFrequency = 0;

        //we loop with offset -1 / +1 to get patterns. At the same time we have to account for patten size.
        //If pattern is of size 2 we will be reaching x+1 and y+1 to check all 4 values. Need visual aid.

        int patternIndex = 0;
        for (int row = rowMin; row < rowMax; row++)
        {
            for (int col = colMin; col < colMax; col++)
            {
                int[][] gridValues = valuesManager.GetPatternValuesFromGridAt(col, row, patternSize);
                string hashValue = HashCodeCalculator.CalculateHashCode(gridValues);

                if (patternHashcodeDictionary.ContainsKey(hashValue) == false)
                {
                    Pattern pattern = new Pattern(gridValues, hashValue, patternIndex);
                    patternIndex++;
                    AddNewPattern(patternHashcodeDictionary, patternIndexDictionary, hashValue, pattern);
                }
                else
                {

                    if (equalWeights == false)
                        patternIndexDictionary[patternHashcodeDictionary[hashValue].Pattern.Index].AddToFrequency();


                }
                //if (patternSize > colMin || row >= rowMin && row < rowMax-1 && col >= colMin && col < colMax-1)
                //{

                //    totalFrequency++;

                //}
                totalFrequency++;
                if (patternSize<3)
                    patternIndicesGrid[row + 1][col + 1] = patternHashcodeDictionary[hashValue].Pattern.Index;
                else
                    patternIndicesGrid[row + patternSize - 1][col + patternSize - 1] = patternHashcodeDictionary[hashValue].Pattern.Index;
            }
        }

        CalculateRelativeFrequency(patternIndexDictionary, totalFrequency);

        return new PatternDataResults(patternIndicesGrid, patternIndexDictionary);
    }

    private static void CalculateRelativeFrequency(Dictionary<int, PatternData> patternIndexDictionary, int totalFrequency)
    {
        foreach (var item in patternIndexDictionary.Values)
        {
            item.CalculateRelativeFrequency(totalFrequency);
        }
    }

    public static Dictionary<int, PatternNeighbours> FindPossibleNeighbursForAllPatterns(IFindNeighboutStrategy patternFinder, PatternDataResults patterndataResults)
    {

        return patternFinder.FIndNeighbours(patterndataResults);
    }

    public static PatternNeighbours CheckNeighboursInEachDirection(int x, int y, PatternDataResults patterndataResults)
    {
        PatternNeighbours neighbours = new PatternNeighbours();
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            int possiblePatternIndex = patterndataResults.GetNeighbourInDirection(x, y, dir);
            if (possiblePatternIndex >= 0)
            {
                neighbours.AddPatternToDirection(dir, possiblePatternIndex);
            }
        }
        return neighbours;
    }

    public static void AddNeighboursToDictionary(Dictionary<int, PatternNeighbours> dictionary, int patternIndex, PatternNeighbours neighbours)
    {
        if (dictionary.ContainsKey(patternIndex) == false)
        {

            dictionary.Add(patternIndex, neighbours);

        }
        dictionary[patternIndex].AddNeighbours(neighbours);

    }

    private static void AddNewPattern(Dictionary<string, PatternData> patternHashcodeDictionary, Dictionary<int, PatternData> patternIndexDictionary, string hashValue, Pattern pattern)
    {

        PatternData patternData = new PatternData(pattern);
        patternHashcodeDictionary.Add(hashValue, patternData);
        patternIndexDictionary.Add(pattern.Index, patternData);
    }

    public static bool AreArraysTheSame(int[][] arr1, int[][] arr2)
    {
        string arr1hash = HashCodeCalculator.CalculateHashCode(arr1);
        string arr2hash = HashCodeCalculator.CalculateHashCode(arr2);
        return arr1hash.Equals(arr2hash);

    }
}

public interface IFindNeighborStrategy {
    Dictionary<int, PatternNeighbors> FindNeighbours(PatternDataResults patternFinderResult);
}
public class NeighborStrategyFactory {
    public class NeighbourStrategyFactory
    {
        Dictionary<string, Type> strategies;

        public NeighbourStrategyFactory()
        {
            LoadTypesIFindNeighbourStrategy();
        }

        private void LoadTypesIFindNeighbourStrategy()
        {
            strategies = new Dictionary<string, Type>();
            Type[] typesInThisAssembly = Assembly.GetExecutingAssembly().GetTypes();

            foreach(var type in typesInThisAssembly)
            {
                if (type.GetInterface(typeof(IFindNeighborStrategy).ToString()) != null)
                {
                    strategies.Add(type.Name.ToLower(), type);
                }
            }
        }

        internal IFindNeighborStrategy CreteInstance(string nameOfStrategy)
        {
            Type t = GetTypeToCreate(nameOfStrategy);
            if (t == null)
            {
                t = GetTypeToCreate("more");
            }
            return Activator.CreateInstance(t) as IFindNeighborStrategy;
        }

        private Type GetTypeToCreate(string nameOfStrategy)
        {
            foreach (var possibleStrategy in strategies)
            {
                if (possibleStrategy.Key.Contains(nameOfStrategy)){
                    return possibleStrategy.Value;
                }
            }
            return null;
        }
    }
}

public class PatternData {
    private Pattern pattern;
    private int frequency;
    private float frequencyRelative;
    private float frequencyRelativeLog2;
    public float FrequencyRelative {get => frequencyRelative;}
    public Pattern Pattern {get => pattern;}
    public float FrequencyRelativeLog2 {get => frequencyRelativeLog2;}
    public PatternData(Pattern pattern) {
        this.pattern = pattern;
    }
    public void addToFrequency() {
        frequency++;
    }
    public void calculateRelativeFrequency(int total) {
        frequencyRelative = (float) frequency/total;
        frequencyRelativeLog2 = Mathf.Log(frequencyRelative,2);
    }
    public bool compareGrid(Direction direction, PatternData data) {
        return pattern.comparePatternToAnotherPattern(direction, data.pattern);
    }
}

public class Pattern {
    private int index;
    private int[][] grid;
    public string HashIndex {get; set;}
    public Pattern(int[][] grid, string hashIndex, int index) {
        this.grid = grid;
        this.HashIndex = hashIndex;
        this.index = index;
    }

    public void setGridValue(int x, int y, int value) {
        grid[x][y] = value;
    }
    public int getGridValue(int x, int y) {
        return grid[x][y];
    }
    public bool checkValueAtPosition(int x, int y, int value) {
        return value.Equals(getGridValue(x,y));
    }
    internal bool comparePatternToAnotherPattern(Direction direction, Pattern pattern) {
        int[][] myGrid = getGridValuesInDirection(direction);
        int[][] otherGrid = pattern.getGridValuesInDirection(direction.oppositeDirection());
        for (int row = 0; row < myGrid.Length; row ++) {
            for (int col = 0; col < myGrid[0].Length; col++) {
                if (myGrid[row][col] != otherGrid[row][col]) {
                    return false;
                }
            }
        }
        return true;
    }
    
    private int[][] getGridValuesInDirection(Direction direction) {
        int[][] gridPartsToCompare = null;
        switch (direction) {
            case Direction.Up:
                gridPartsToCompare = MyCollectionExtension.CreateJaggedArray<int[][]>(grid.Length-1,grid.Length);
                createPartOfGrid(0,grid.Length,1,grid.Length,gridPartsToCompare);
                break;
            case Direction.Down:
                gridPartsToCompare = MyCollectionExtension.CreateJaggedArray<int[][]>(grid.Length-1,grid.Length);
                createPartOfGrid(0,grid.Length,0,grid.Length-1,gridPartsToCompare);
                break;
            case Direction.Left:
                gridPartsToCompare = MyCollectionExtension.CreateJaggedArray<int[][]>(grid.Length,grid.Length-1);
                createPartOfGrid(0,grid.Length-1,0,grid.Length,gridPartsToCompare);
                break;
            case Direction.Right:
                gridPartsToCompare = MyCollectionExtension.CreateJaggedArray<int[][]>(grid.Length,grid.Length-1);
                createPartOfGrid(1,grid.Length,0,grid.Length,gridPartsToCompare);
                break;
        }
        return gridPartsToCompare;
    }

    private void createPartOfGrid(int xMin, int xMax, int yMin, int yMax, int[][] gridPartsToCompare) {
        List<int> tempList = new List<int>();
        for (int row = yMin; row < yMax; row++) {
            for (int col = xMin; col < xMax; col++) {
                tempList.Add(grid[row][col]);
            }
        }
        for (int n = 0; n < tempList.Count; n++) {
            int x = n % gridPartsToCompare.Length;
            int y = n / gridPartsToCompare.Length;
            gridPartsToCompare[x][y] = tempList[n];
        }
    }
}

public enum Direction {
    Up,
    Right,
    Down,
    Left
}

public static class DirectionHelper {
    public static Direction oppositeDirection(this Direction direction) {
        switch (direction) {
            case Direction.Up:
                return Direction.Down;
            case Direction.Down:
                return Direction.Up;
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
            default:
                // Will never get here
                return direction;
        }
    }
}

} // namespace end