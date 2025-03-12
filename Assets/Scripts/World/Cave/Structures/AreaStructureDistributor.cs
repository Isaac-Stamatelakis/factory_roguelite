using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Misc.RandomFrequency;
using Newtonsoft.Json;
using WorldModule.Caves;
using WorldModule;
using Misc;
using DevTools.Structures;
using TileEntity.Instances.Creative.CreativeChest;
using UnityEngine.Serialization;
using Random = System.Random;


namespace WorldModule.Caves {
    [CreateAssetMenu(fileName ="New Structure Distributor",menuName="Generation/Structure/Distributor")]
    public class AreaStructureDistributor : CaveTileGenerator
    {
        public List<PresetStructure> constantStructures;
        [FormerlySerializedAs("structures")] public List<StructureFrequency> randomStructures;
        public override void distribute(SeralizedWorldData worldTileData, int width, int height, Vector2Int bottomLeftCorner) {
            Dictionary<Vector2Int,StructureVariant> placedStructures = new Dictionary<Vector2Int,StructureVariant>();

            foreach (PresetStructure presetStructure in constantStructures)
            {
                Structure structure = StructureGeneratorHelper.LoadStructure(presetStructure.structureName); 
                int index = UnityEngine.Random.Range(0, structure.variants.Count);
                StructureVariant variant = structure.variants[index];
                Vector2Int normalizedPlacementPosition = new Vector2Int(width,height)/2 + presetStructure.location - variant.Size / 2 - new Vector2Int(Global.CHUNK_SIZE,Global.CHUNK_SIZE)/2;
                AreaStructureDistributorUtils.PlaceStructure(worldTileData,normalizedPlacementPosition, variant.Data, variant.Size);
            }
            foreach (StructureFrequency structureFrequency in randomStructures) {
                int amount = StatUtils.getAmount(structureFrequency.mean,structureFrequency.standardDeviation);
                Structure structure = StructureGeneratorHelper.LoadStructure(structureFrequency.structureName);
                var variants = structure.variants;
  
                if (variants.Count == 0) continue;
                while (amount > 0)
                {
                    int index = UnityEngine.Random.Range(0, variants.Count);
                    StructureVariant variant = variants[index];
                    TryPlaceStructure(worldTileData,placedStructures,variant,width,height,index);
                    
                    amount--;
                }
            }
            Debug.Log($"Generated {placedStructures.Count} structures");

        }

        private void TryPlaceStructure(SeralizedWorldData worldTileData, Dictionary<Vector2Int,StructureVariant> placedStructures, StructureVariant variant, int width, int height, int index)
        {
            int placementAttempts = 10;
            while (placementAttempts > 0) {
                Vector2Int? randomPosition = AreaStructureDistributorUtils.getRandomPlacementPosition(
                    width,
                    height,
                    variant.Size,
                    index
                );
                if (randomPosition == null || Overlap(placedStructures,variant,(Vector2Int)randomPosition))
                {
                    placementAttempts--;
                    continue;
                }
                
                placedStructures[(Vector2Int)randomPosition] = variant;
                AreaStructureDistributorUtils.PlaceStructure(worldTileData,(Vector2Int)randomPosition, variant.Data, variant.Size);
                return;
            }
        }

        private bool Overlap(Dictionary<Vector2Int,StructureVariant> placedStructures, StructureVariant variant, Vector2Int randomPosition)
        {
            foreach (KeyValuePair<Vector2Int,StructureVariant> kvp in placedStructures) {
                if (AreaStructureDistributorUtils.structureVariantsOverLap(
                        variant,
                        kvp.Value,
                        randomPosition,
                        kvp.Key
                    ))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static class AreaStructureDistributorUtils {
        public static bool structureVariantsOverLap(StructureVariant a, StructureVariant b, Vector2Int aPosition, Vector2Int bPosition) {
            Vector2Int aBottomLeft = aPosition;
            Vector2Int aTopRight = aPosition + a.Size;

            Vector2Int bBottomLeft = bPosition;
            Vector2Int bTopRight = bPosition + b.Size;
                
            bool overlapHorizontally = aBottomLeft.x < bTopRight.x && bBottomLeft.x < aTopRight.x;
            bool overlapVertically = aBottomLeft.y < bTopRight.y && bBottomLeft.y < aTopRight.y;
            if (!overlapHorizontally || !overlapVertically) {
                return false;
            }
            int areaA = a.Size.x * a.Size.y;
            int areaB = b.Size.x * b.Size.y;
            StructureVariant smallest, largest;
            Vector2Int smallestPosition, largestPosition;
            (smallest, largest) = areaA < areaB ? (a,b) : (b,a); 
            (smallestPosition,largestPosition) = areaA < areaB ? (aPosition,bPosition) : (bPosition,aPosition);
            Vector2Int largestTopRight = largestPosition + largest.Size;
            SeralizedWorldData smallestData = smallest.Data;
            SeralizedWorldData largestData = largest.Data;

            for (int x = 0; x < smallest.Size.x; x ++) {
                for (int y = 0; y < smallest.Size.y; y++) {
                    Vector2Int cellPosition = new Vector2Int(x,y);
                    string smallestBaseId = smallestData.baseData.ids[cellPosition.x,cellPosition.y];
                    if (smallestBaseId == StructureGeneratorHelper.FILL_ID) {
                        continue;
                    }
                    Vector2Int largestRelativePosition = largestPosition - smallestPosition + cellPosition;
                    if (largestRelativePosition.x < 0  || largestRelativePosition.y < 0 || 
                        largestRelativePosition.x >= smallest.Size.x || largestRelativePosition.y >= smallest.Size.y) {
                        continue;
                    }
                    string largestBaseId = largestData.baseData.ids[largestRelativePosition.x,largestRelativePosition.y];
                    if (largestBaseId != StructureGeneratorHelper.FILL_ID) {
                        return false;
                    }
                }
            }
            return true;
        }

        public static Vector2Int? getRandomPlacementPosition(int width, int height, Vector2Int structureSize, int variantIndex) {
            if (structureSize.x > width || structureSize.y > height) {
                Debug.LogWarning($"Tried to place structure variant {variantIndex} inside too small of cave");
                return null;
            } 
            int ranX = UnityEngine.Random.Range(0,width-structureSize.x);
            int ranY = UnityEngine.Random.Range(0,height-structureSize.y);
            return new Vector2Int(ranX,ranY);
        }

        public static void PlaceStructure(SeralizedWorldData caveData, Vector2Int position, WorldTileConduitData variantData, Vector2Int structureSize) {
            if (caveData is WorldTileConduitData worldTileConduitData) {
                for (int x = 0; x < structureSize.x; x++) {
                    for (int y = 0; y < structureSize.y; y++) {
                        Vector2Int vector = new Vector2Int(x,y);
                        WorldGenerationFactory.MapWorldTileConduitData(worldTileConduitData,variantData,position+vector,vector);
                    }
                }
                for (int x = 0; x < structureSize.x; x++) {
                    for (int y = 0; y < structureSize.y; y++) {
                        Vector2Int vector = new Vector2Int(x,y);
                        PlaceStructureId(worldTileConduitData,variantData,position+vector,vector);
                    }
                }
            } else {
                for (int x = 0; x < structureSize.x; x++) {
                    for (int y = 0; y < structureSize.y; y++) {
                        Vector2Int vector = new Vector2Int(x,y);
                        WorldGenerationFactory.MapWorldTileData(caveData,variantData,position+vector,vector);
                    }
                }
                for (int x = 0; x < structureSize.x; x++) {
                    for (int y = 0; y < structureSize.y; y++) {
                        Vector2Int vector = new Vector2Int(x,y);
                        PlaceStructureId(caveData,variantData,position+vector,vector);
                    }
                }
            }
        }
        
        private static void PlaceStructureId(SeralizedWorldData copyTo, SeralizedWorldData copyFrom, Vector2Int positionTo, Vector2Int positionFrom) 
        {
            string fromBaseId = copyFrom.baseData.ids[positionFrom.x,positionFrom.y];
            if (fromBaseId == StructureGeneratorHelper.EXPAND_ID)
            {
                ExpandStructureId(copyTo,copyFrom,positionTo,positionFrom);
            }
        }

        private static void ExpandStructureId(SeralizedWorldData copyTo, SeralizedWorldData copyFrom, Vector2Int positionTo, Vector2Int positionFrom)
        {
            StructureExpandData expandData;
            try
            {
                expandData = JsonConvert.DeserializeObject<StructureExpandData>(copyFrom.baseData.sTileEntityOptions[positionFrom.x, positionFrom.y]);
            }
            catch (JsonSerializationException)
            {
                return;
            }
            
            bool left = copyFrom.baseData.ids[positionFrom.x - 1, positionFrom.y] == null;
            bool right = copyFrom.baseData.ids[positionFrom.x + 1, positionFrom.y] == null;
            bool down = copyFrom.baseData.ids[positionFrom.x, positionFrom.y - 1] == null;
            bool up = copyFrom.baseData.ids[positionFrom.x, positionFrom.y + 1] == null;
            
            int yIter = 0;
            int xIter = 0;
            if (left && !right && down && up)
            {
                xIter = -1;
            }

            if (!left && right && down && up)
            {
                xIter = 1;
            }
            if (left && right && down && !up)
            {
                yIter = -1;
            }
            if (left && right && !down && up)
            {
                yIter = 1;
            }
            
            if (xIter == 0 && yIter == 0)
            {
                copyTo.baseData.sTileEntityOptions[positionTo.x,positionTo.y] = null;
                copyTo.baseData.sTileOptions[positionTo.x,positionTo.y] = null;
                copyTo.baseData.ids[positionTo.x, positionTo.y] = null;
                return;
            }
            
            string expandId = expandData.Id;
            Vector2Int change = new Vector2Int(xIter, yIter);
            int maxX = copyTo.baseData.ids.GetLength(0);
            int maxY = copyTo.baseData.ids.GetLength(1);
            int iterations = 0;
            
            while (iterations < expandData.MaxSize)
            {
                copyTo.baseData.sTileEntityOptions[positionTo.x,positionTo.y] = null;
                copyTo.baseData.sTileOptions[positionTo.x,positionTo.y] = null;
                copyTo.baseData.ids[positionTo.x, positionTo.y] = expandId;
                positionTo += change;
                positionFrom += change;
                if (positionTo.x < 0 || positionTo.y < 0 || positionTo.x >= maxX || positionTo.y >= maxY) break;
                if (iterations > 0 && copyTo.baseData.ids[positionTo.x, positionTo.y] != null) break;
                iterations++;
                
            }
        }
    }
    [System.Serializable]
    public class StructureFrequency {
        public string structureName;
        public int mean;
        public int standardDeviation;
    }

    [System.Serializable]
    public class PresetStructure
    {
        public string structureName;
        public Vector2Int location;
    }
}

