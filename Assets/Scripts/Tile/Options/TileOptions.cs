using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Items;

namespace Tiles {
    /// <summary>
    /// Options which can be modified in the game
    /// </summary>
    [System.Serializable]
    public struct DynamicTileOptions {
        public int hardness;
        
    }
    /// <summary>
    /// Options which can only be modified in editor
    /// </summary>
    [System.Serializable]
    public class StaticTileOptions {
        public bool hitable = true;
        public bool rotatable = false;
        public bool hasStates = false;
        public List<DropOption> dropOptions;
        public bool requireTileBelow = false;
        public bool requireTileAbove = false;
        public bool requireTileSide = false;
    }
    /// </summary>
    /// Options which require serializaiton
    /// <summary>
    [System.Serializable]
    public struct SerializedTileOptions {
        public int rotation;
        public int state;
        public bool mirror;
        [JsonConstructor]
        public SerializedTileOptions(int rotation, int state, bool mirror) {
            this.rotation = rotation;
            this.state = state;
            this.mirror = mirror;
        }
    }
    [System.Serializable]
    public class TileOptions {
        [SerializeField] private StaticTileOptions staticOptions;
        [SerializeField] private DynamicTileOptions dynamicOptions;
        [JsonProperty] private SerializedTileOptions serializedOptions;
        
        
        [JsonIgnore] public SerializedTileOptions SerializedTileOptions { get => serializedOptions; set => serializedOptions = value; }
        [JsonIgnore] public DynamicTileOptions DynamicTileOptions { get => dynamicOptions; set => dynamicOptions = value; }
        [JsonIgnore] public StaticTileOptions StaticOptions { get => staticOptions; set => staticOptions = value; }

        public TileOptions(StaticTileOptions staticTileOptions, DynamicTileOptions dynamicTileOptions, SerializedTileOptions serializedTileOptions) {
            this.staticOptions = staticTileOptions;
            this.dynamicOptions = dynamicTileOptions;
            this.serializedOptions = serializedTileOptions;
        }
    } 
    [System.Serializable]
    public class DropOption {
        public ItemObject itemObject;
        public int weight;
        public int lowerAmount;
        public int upperAmount;
    }

    public static class TileOptionFactory {
        public static string serialize(TileOptions tileOptions) {
            if (tileOptions == null) {
                return null;
            }
            if (tileOptions.StaticOptions == null) {
                return null;
            }
            if (!tileOptions.StaticOptions.hasStates && !tileOptions.StaticOptions.rotatable) {
                return null;
            }
            return Serialize(tileOptions.SerializedTileOptions);
        }

        public static string Serialize(SerializedTileOptions serializedTileOptions)
        {
            return JsonConvert.SerializeObject(serializedTileOptions);
        }
        public static TileOptions deserialize(string data, TileItem tileItem) {
            if (data == null) {
                return new TileOptions(
                    tileItem.tileOptions.StaticOptions,
                    tileItem.tileOptions.DynamicTileOptions,
                    tileItem.tileOptions.SerializedTileOptions
                );
            }
            try {
                SerializedTileOptions serializedTileOptions = JsonConvert.DeserializeObject<SerializedTileOptions>(data);
                return new TileOptions(
                    tileItem.tileOptions.StaticOptions,
                    tileItem.tileOptions.DynamicTileOptions,
                    JsonConvert.DeserializeObject<SerializedTileOptions>(data)
                );
            } catch (JsonSerializationException ex) {
                Debug.LogError("TileOptionFactory method 'deserialize' error: " + ex);
                return getDefault(tileItem);
            }
        }

        public static SerializedTileOptions Deserialize(string data, TileItem tileItem)
        {
            if (data == null)
            {
                return tileItem.tileOptions.SerializedTileOptions;
            }
            try
            {
                return JsonConvert.DeserializeObject<SerializedTileOptions>(data);
            }
            catch (JsonSerializationException)
            {
                return tileItem.tileOptions.SerializedTileOptions;
            }
            
            
        }
        public static TileOptions getDefault(TileItem tileItem) {
            return new TileOptions(
                tileItem.tileOptions.StaticOptions,
                tileItem.tileOptions.DynamicTileOptions,
                tileItem.tileOptions.SerializedTileOptions
            );
        }
    }

    
}