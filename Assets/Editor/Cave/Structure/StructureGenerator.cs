using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using WorldModule.Caves;
using DevTools.Structures;
using Chunks;
using WorldModule;
using Chunks.Partitions;
using Chunks.IO;

public class StructureGenerator : EditorWindow {
    private static string[] structureNames = new string[]{"Restart this Editor"};
    private int index;
    [MenuItem("Tools/Caves/Structure")]
    public static void ShowWindow()
    {
        structureNames = StructureGeneratorHelper.getAllStructureFolders();
        StructureGenerator window = (StructureGenerator)EditorWindow.GetWindow(typeof(StructureGenerator));
        window.titleContent = new GUIContent("Structure Generator");
    }

    void OnGUI()
    {
        
        EditorGUILayout.BeginHorizontal();
        index = EditorGUILayout.Popup(index, structureNames);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate"))
        {
            generateStructure(structureNames[index]);
        }
    }

    /// <summary>
    /// This generates all structures inside a structure dev tool system.
    /// </summary>
    private void generateStructure(string structureName) {
        Structure structure = ScriptableObject.CreateInstance<Structure>();
        structure.name = structureName;
        
        string path = StructureGeneratorHelper.getPath(structureName);
        string dimPath = WorldLoadUtils.getDimPath(path, 0);

        List<SoftLoadedConduitTileChunk> chunks = ChunkIO.getUnloadedChunks(0,dimPath);
        Dictionary<Vector2Int, SoftLoadedConduitTileChunk> chunkDict = new Dictionary<Vector2Int, SoftLoadedConduitTileChunk>();
        IntervalVector coveredArea = new IntervalVector(new Interval<int>(0,0),new Interval<int>(0,0));
        foreach (SoftLoadedConduitTileChunk chunk in chunks) {
            if (chunkDict.ContainsKey(chunk.Position)) {
                Debug.LogError($"Chunk dict already contains a chunk at position {chunk.Position}");
                continue;
            }
            coveredArea.add(chunk.Position);
            chunkDict[chunk.Position] = chunk;
        }   
        
        Vector2Int size = coveredArea.getSize()*Global.ChunkSize;
        //Debug.Log($"Generating structures from system of size {coveredArea}");
        Vector2Int offset = new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound)*Global.ChunkSize;
        bool[,] perimeter = new bool[size.x,size.y];
        foreach (SoftLoadedConduitTileChunk softLoadedConduitTileChunk in chunks) {
            foreach (IChunkPartition partition in softLoadedConduitTileChunk.Partitions) {
                SeralizedWorldData data = partition.getData();
                for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                    for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                        string baseId = data.baseData.ids[x,y];
                        if (baseId == StructureGeneratorHelper.ParimeterId) {
                            Vector2Int normalizedPosition = partition.getRealPosition()*Global.ChunkPartitionSize+new Vector2Int(x,y)-offset;
                            perimeter[normalizedPosition.x,normalizedPosition.y] = true;
                        }
                    }
                }
            }
        }

        bool[,] visited = new bool[size.x,size.y];
        List<Vector2Int> directions = new List<Vector2Int>{
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down,
            Vector2Int.right
        };
        List<List<Vector2Int>> areas = new List<List<Vector2Int>>();
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                if (!visited[x,y] && !perimeter[x,y]) {
                    List<Vector2Int> filledArea = dfsEnclosedArea(new Vector2Int(x,y),perimeter,visited,directions,size);
                    if (filledArea != null) {
                        areas.Add(filledArea);
                    }
                }
            }
        }
        Debug.Log($"Structure generator found {areas.Count} structures");

        foreach (List<Vector2Int> area in areas) {
            if (area.Count == 0) {
                continue;
            }
            Vector2Int first = area[0];
            IntervalVector boundingBox = new IntervalVector(new Interval<int>(first.x,first.x),new Interval<int>(first.y,first.y));
            for (int i = 1; i < area.Count; i++) {
                boundingBox.add(area[i]);
            }

            Vector2Int areaOffset = new Vector2Int(boundingBox.X.LowerBound,boundingBox.Y.LowerBound);
            Vector2Int areaSize = boundingBox.getSize();
            WorldTileConduitData areaData = WorldGenerationFactory.createEmpty(areaSize);
            for (int x = 0; x < areaSize.x; x++) {
                for (int y = 0; y < areaSize.y; y++) {
                    areaData.baseData.ids[x,y] = StructureGeneratorHelper.FillId;
                }
            }
            foreach (Vector2Int vector in area) {
                Vector2Int adjustedVector = vector+offset;
                Vector2Int chunkPosition = Global.getChunkFromCell(adjustedVector);
                IChunk chunk = chunkDict[chunkPosition];
                Vector2Int partitionPosition = Global.getPartitionFromCell(adjustedVector)-chunkPosition*Global.PartitionsPerChunk;
                IChunkPartition partition = chunk.getPartition(partitionPosition);
                WorldTileConduitData partitionData = (WorldTileConduitData) partition.getData();
                Vector2Int posInPartition = Global.getPositionInPartition(adjustedVector);
                Vector2Int posInArea = vector-areaOffset;
                Debug.Log($"Vector {adjustedVector}, Chunk: {chunkPosition}, Partition {partitionPosition}, Position {posInPartition}");
                WorldGenerationFactory.mapWorldTileConduitData(areaData,partitionData,posInArea,posInPartition);
            }
            Debug.Log($"Added structure variant of size {areaSize} with {area.Count} points");
            structure.variants.Add(new StructureVariant(
                Newtonsoft.Json.JsonConvert.SerializeObject(areaData),
                areaSize,
                1
            ));
        }
        if (structure.variants.Count == 0) {
            Debug.LogWarning("Structure has no enclosed areas. Creation aborted");
            return;
        }
        string creationPath = Path.Combine(Global.EditorCreationPath,structureName);
        if (Directory.Exists(creationPath)) {
            Directory.Delete(creationPath,true);
        }
        AssetDatabase.CreateFolder(Global.EditorCreationPath,structureName);
        AssetDatabase.Refresh();
        Debug.Log($"Folder for structure created at {creationPath}");
        string savePath = Path.Combine(creationPath,structureName) + ".asset";
        AssetDatabase.CreateAsset(structure,savePath);
        Debug.Log($"Successfully created structure with {structure.variants.Count} variants at path {savePath}");
        AssetDatabase.Refresh();
    }

    private List<Vector2Int> dfsEnclosedArea(Vector2Int position, bool[,] perimeter, bool[,] visited, List<Vector2Int> directions, Vector2Int size) {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(position);
        visited[position.x,position.y] = true;
        bool enclosed = true;
        List<Vector2Int> points = new List<Vector2Int>();
        points.Add(position);
        while (stack.Count > 0)
        {
            Vector2Int next = stack.Pop();
            foreach (Vector2Int direction in directions) {
                Vector2Int directedPosition = next + direction;
                bool outOfBounds = directedPosition.x < 0 || directedPosition.y < 0 || directedPosition.x >= size.x || directedPosition.y >= size.y;
                if (outOfBounds) {
                    enclosed = false;
                    points = null;
                    continue;
                }
                if (perimeter[directedPosition.x,directedPosition.y] || visited[directedPosition.x,directedPosition.y]) {
                    continue;
                }
                visited[directedPosition.x,directedPosition.y] = true;
                stack.Push(directedPosition);
                if (enclosed) {
                    points.Add(directedPosition);
                }
            }
        }
        // points will be null if it is not enclosed, ie touched any out of bounds area
        return points;
    }
}
