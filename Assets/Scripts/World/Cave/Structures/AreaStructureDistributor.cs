using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Misc.RandomFrequency;
using Newtonsoft.Json;
using WorldModule.Caves;
using WorldModule;


namespace WorldModule.Caves {
    [CreateAssetMenu(fileName ="New Structure Distributor",menuName="Generation/Structure/Distributor")]
    public class AreaStructureDistributor : ScriptableObject, IDistributor
    {
        public List<StructureFrequency> structures;
        public void distribute(SeralizedWorldData worldTileData, int seed, int width, int height) {
            Dictionary<Vector2Int,StructureVariant> placedStructures = new Dictionary<Vector2Int,StructureVariant>();
            System.Random rand = new System.Random();
            Dictionary<StructureVariant,WorldTileConduitData> structureDataDict = new Dictionary<StructureVariant, WorldTileConduitData>();
            foreach (StructureFrequency structureFrequency in structures) {
                double u1 = 1.0 - rand.NextDouble();
                double u2 = 1.0 - rand.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                int amount = Mathf.RoundToInt((float)(structureFrequency.mean + structureFrequency.standardDeviation * randStdNormal));
                //Debug.Log($"Generating {amount} of {structureFrequency.generatedStructure.name}");
                while (amount > 0) {
                    StructureVariant variant = RandomFrequencyListUtils.getRandomFromList<StructureVariant>(
                        elements: structureFrequency.generatedStructure.variants
                    );
                    int index = structureFrequency.generatedStructure.variants.IndexOf(variant);
                    if (!structureDataDict.ContainsKey(variant)) {
                        try {
                            structureDataDict[variant] = JsonConvert.DeserializeObject<WorldTileConduitData>(variant.Data);
                        } catch (JsonSerializationException e) {
                            structureDataDict[variant] = null;
                            
                            Debug.LogError($"Variant {index} of structure {structureFrequency.generatedStructure.name} has invalid data\nError {e}");
                        }   
                    }
                    if (structureDataDict[variant] != null) {
                        int placementAttempts = 10;
                        while (placementAttempts > 0) {
                            Vector2Int? randomPosition = AreaStructureDistributorUtils.getRandomPlacementPosition(
                                width,
                                height,
                                variant.Size,
                                structureFrequency.generatedStructure.name,
                                index
                            );
                            if (randomPosition == null) {
                                break;
                            }
                            foreach (KeyValuePair<Vector2Int,StructureVariant> kvp in placedStructures) {
                                if (AreaStructureDistributorUtils.structureVariantsOverLap(
                                    variant,
                                    kvp.Value,
                                    (Vector2Int)randomPosition,
                                    kvp.Key,
                                    structureDataDict
                                    
                                )) {
                                    placementAttempts--;
                                    break;
                                }
                            }
                            Debug.Log(randomPosition);
                            placedStructures[(Vector2Int)randomPosition] = variant;
                            AreaStructureDistributorUtils.placeStructure(worldTileData,(Vector2Int)randomPosition,structureDataDict[variant],variant.Size);
                            break;
                        }
                    }
                    amount--;
                }
            }
            Debug.Log($"Generated {placedStructures.Count} structures");

        }
    }

    public static class AreaStructureDistributorUtils {
        public static bool structureVariantsOverLap(StructureVariant a, StructureVariant b, Vector2Int aPosition, Vector2Int bPosition,Dictionary<StructureVariant,WorldTileConduitData> variantData) {
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
            SeralizedWorldData smallestData = variantData[smallest];
            SeralizedWorldData largestData = variantData[largest];

            for (int x = 0; x < smallest.Size.x; x ++) {
                for (int y = 0; y < smallest.Size.y; y++) {
                    Vector2Int cellPosition = new Vector2Int(x,y);
                    string smallestBaseId = smallestData.baseData.ids[cellPosition.x,cellPosition.y];
                    if (smallestBaseId == StructureGeneratorHelper.FillId) {
                        continue;
                    }
                    Vector2Int largestRelativePosition = largestPosition - smallestPosition + cellPosition;
                    if (largestRelativePosition.x < 0  || largestRelativePosition.y < 0 || 
                        largestRelativePosition.x >= smallest.Size.x || largestRelativePosition.y >= smallest.Size.y) {
                        continue;
                    }
                    string largestBaseId = largestData.baseData.ids[largestRelativePosition.x,largestRelativePosition.y];
                    if (largestBaseId != StructureGeneratorHelper.FillId) {
                        return false;
                    }
                }
            }
            return true;
        }

        public static Vector2Int? getRandomPlacementPosition(int width, int height, Vector2Int structureSize, string structureName, int variantIndex) {
            if (structureSize.x > width || structureSize.y > height) {
                Debug.LogWarning($"Tried to place structure {structureName} variant {variantIndex} inside too small of cave");
                return null;
            } 
            int ranX = UnityEngine.Random.Range(0,width-structureSize.x);
            int ranY = UnityEngine.Random.Range(0,height-structureSize.y);
            return new Vector2Int(ranX,ranY);
        }

        public static void placeStructure(SeralizedWorldData caveData, Vector2Int position, WorldTileConduitData variantData, Vector2Int structureSize) {
            if (caveData is WorldTileConduitData worldTileConduitData) {
                for (int x = 0; x < structureSize.x; x++) {
                    for (int y = 0; y < structureSize.y; y++) {
                        Vector2Int vector = new Vector2Int(x,y);
                        WorldGenerationFactory.mapWorldTileConduitData(worldTileConduitData,variantData,position+vector,vector);
                    }
                }
            } else {
                for (int x = 0; x < structureSize.x; x++) {
                    for (int y = 0; y < structureSize.y; y++) {
                        Vector2Int vector = new Vector2Int(x,y);
                        Debug.Log(vector);
                        WorldGenerationFactory.mapWorldTileData(caveData,variantData,position+vector,vector);
                    }
                }
            }
        }
    }
    [System.Serializable]
    public class StructureFrequency {
        public Structure generatedStructure;
        public int mean;
        public int standardDeviation;
    }
}

