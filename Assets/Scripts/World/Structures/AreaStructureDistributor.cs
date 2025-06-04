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
using World.Structures.Restriction;
using Random = System.Random;


namespace WorldModule.Caves {
    [CreateAssetMenu(fileName ="New Structure Distributor",menuName="Generation/Structure/Distributor")]
    public class AreaStructureDistributor : CaveTileGenerator
    {
        public List<PresetStructure> constantStructures;
        public List<StructureFrequency> randomStructures;
        public override void Distribute(SeralizedWorldData worldData, int width, int height, Vector2Int bottomLeftCorner) {
            Dictionary<Vector2Int,StructureVariant> placedStructures = new Dictionary<Vector2Int,StructureVariant>();

            foreach (PresetStructure presetStructure in constantStructures)
            {
                Structure structure = StructureGeneratorHelper.LoadStructure(presetStructure.structureName); 
                int index = UnityEngine.Random.Range(0, structure.variants.Count);
                StructureVariant variant = structure.variants[index];
                Vector2Int normalizedPlacementPosition = new Vector2Int(width,height)/2 + presetStructure.location - variant.Size / 2 - new Vector2Int(Global.CHUNK_SIZE,Global.CHUNK_SIZE)/2;
                AreaStructureDistributorUtils.PlaceStructure(worldData,normalizedPlacementPosition, variant.Data, variant.Size);
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
                    TryPlaceStructure(worldData,placedStructures,variant,structureFrequency.positionRestriction,structureFrequency.restriction,width,height,index);
                    
                    amount--;
                }
            }
            Debug.Log($"Generated {placedStructures.Count} structures");

        }

        private void TryPlaceStructure(SeralizedWorldData worldTileData, Dictionary<Vector2Int,StructureVariant> placedStructures, 
            StructureVariant variant, StructurePositionRestriction positionRestriction, StructureRestriction restriction,  int width, int height, int index)
        {
            int placementAttempts = 10;
            while (placementAttempts > 0) {
                Vector2Int? randomPosition = AreaStructureDistributorUtils.GetRandomRestrictedPlacementPosition(
                    worldTileData, positionRestriction, restriction, width, height, variant.Size, index);
                
                if (randomPosition == null || Overlap(placedStructures,variant,(Vector2Int)randomPosition))
                {
                    placementAttempts--;
                    continue;
                }
                Debug.Log(randomPosition.Value);
                placedStructures[(Vector2Int)randomPosition] = variant;
                AreaStructureDistributorUtils.PlaceStructure(worldTileData,(Vector2Int)randomPosition, variant.Data, variant.Size);
                return;
            }
        }

        private bool Overlap(Dictionary<Vector2Int,StructureVariant> placedStructures, StructureVariant variant, Vector2Int randomPosition)
        {
            foreach (var (position, structureVariant) in placedStructures) {
                if (AreaStructureDistributorUtils.StructureVariantsOverLap(variant, structureVariant, randomPosition, position)) return true;
            }

            return false;
        }
    }

    public static class AreaStructureDistributorUtils {
        public static bool StructureVariantsOverLap(StructureVariant a, StructureVariant b, Vector2Int aPosition, Vector2Int bPosition) {
            Vector2Int aBottomLeft = aPosition;
            Vector2Int aTopRight = aPosition + a.Size;

            Vector2Int bBottomLeft = bPosition;
            Vector2Int bTopRight = bPosition + b.Size;
                
            bool overlapHorizontally = aBottomLeft.x < bTopRight.x && bBottomLeft.x < aTopRight.x;
            bool overlapVertically = aBottomLeft.y < bTopRight.y && bBottomLeft.y < aTopRight.y;
            return overlapHorizontally && overlapVertically;
        }

        public static Vector2Int? GetRandomRestrictedPlacementPosition(SeralizedWorldData worldData, StructurePositionRestriction positionRestriction, StructureRestriction restriction, int width, int height, Vector2Int structureSize, int variantIndex) {
            if (structureSize.x > width || structureSize.y > height) {
                Debug.LogWarning($"Tried to place structure variant {variantIndex} inside too small of cave");
                return null;
            }

            BaseStructureRestriction structureRestriction = GetStructureRestriction(restriction,worldData,structureSize, new Vector2Int(width,height));
            
            if (restriction == StructureRestriction.None) return GetPosition();
            
            const int ATTEMPTS = 100;
            int i = ATTEMPTS;
            while (i > 0)
            {
                Vector2Int randomPosition = GetPosition();
                if (structureRestriction.ValidateRestriction(randomPosition))
                {
                    return randomPosition;
                }
                i--;
            }

            return null;
            Vector2Int GetPosition()
            {
                return GetRandomPosition(positionRestriction, structureSize, width, height);
            }
        }

        public static BaseStructureRestriction GetStructureRestriction(StructureRestriction structureRestriction, SeralizedWorldData worldData, Vector2Int structureSize, Vector2Int areaSize)
        {
            return structureRestriction switch
            {
                StructureRestriction.None => null,
                StructureRestriction.OnGround => new StructureGroundRestriction(worldData, areaSize, structureSize,
                    StructureRestrictionTileType.Block, Direction.Down),
                StructureRestriction.InAir => new StructureTileRestriction(worldData, areaSize, structureSize,
                    StructureRestrictionTileType.All, false),
                StructureRestriction.Hanging => new StructureGroundRestriction(worldData, areaSize, structureSize,
                    StructureRestrictionTileType.Block, Direction.Up),
                StructureRestriction.InFluid => new StructureTileRestriction(worldData, areaSize, structureSize,
                    StructureRestrictionTileType.Fluid, true),
                _ => throw new ArgumentOutOfRangeException(nameof(structureRestriction), structureRestriction, null)
            };
        }

        private static Vector2Int GetRandomPosition(StructurePositionRestriction positionRestriction, Vector2Int structureSize, int width, int height)
        {
            if (positionRestriction == StructurePositionRestriction.None)
            {
                return new Vector2Int(UnityEngine.Random.Range(0, width - structureSize.x), UnityEngine.Random.Range(0, height - structureSize.y));
            }
            float areaRatio = positionRestriction switch
            {
                StructurePositionRestriction.NotInnerQuarter => 0.25f,
                StructurePositionRestriction.NotInnerHalf => 0.5f,
                StructurePositionRestriction.OuterQuarter => 0.75f,
                _ => throw new ArgumentOutOfRangeException(nameof(positionRestriction), positionRestriction, null)
            };
            
            bool restrictX = UnityEngine.Random.value < 0.5f;
            if (restrictX)
            {
                int xRestriction = (int)(width * areaRatio/2f);
                int ranX = GetRandomInBounds(xRestriction,width,structureSize.x);
                return new Vector2Int(ranX, UnityEngine.Random.Range(0, height - structureSize.y));
            }
            int yRestriction =  (int)(height * areaRatio/2f);
            int ranY = GetRandomInBounds(yRestriction,height,structureSize.y);
            return new Vector2Int(UnityEngine.Random.Range(0, width - structureSize.x),ranY);


            int GetRandomInBounds(int restricted, int areaSize, int size)
            {
                if (restricted == 0)
                {
                    return UnityEngine.Random.Range(0,areaSize-size);
                }
                bool pickUpper = UnityEngine.Random.value < 0.5f;
                return pickUpper 
                    ? UnityEngine.Random.Range(areaSize/2+restricted,areaSize-size) 
                    : UnityEngine.Random.Range(0,areaSize/2-size-restricted);
            }
            
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
            } else if (!left && right && down && up)
            {
                xIter = 1;
            } else if (left && right && down && !up)
            {
                yIter = -1;
            } else if (left && right && !down && up)
            {
                yIter = 1;
            } else if (left)
            {
                yIter = 1;
            } else if (right)
            {
                yIter = -1;
            } else if (down)
            {
                yIter = -1;
            } else if (up)
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

    public enum StructureRestriction
    {
        None,
        OnGround,
        InAir,
        Hanging,
        InFluid
    }

    public enum StructurePositionRestriction
    {
        None,
        NotInnerQuarter,
        NotInnerHalf,
        OuterQuarter
    }
    
    [System.Serializable]
    public class StructureFrequency {
        public string structureName;
        public StructureRestriction restriction;
        public StructurePositionRestriction positionRestriction;
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

