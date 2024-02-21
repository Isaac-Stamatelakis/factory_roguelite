using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Tiles {
    public enum IntItemOption {
        Hardness = TileOption.Hardness,
    }
    public enum BoolItemOption {
        Rotation = TileOption.Rotation,
        Chisel = TileOption.Chisel,
        State = TileOption.State
    }
    public enum DropItemOption {
        Drop = TileOption.Drop
    }
    public enum TileOption {
        Rotation,
        Hardness,
        Chisel,
        State,
        Drop
    }
    public class TileOptions {
        [JsonProperty] private Dictionary<TileOption,object> sOptions;
        private Dictionary<TileOption,object> options;
        public TileOptions(Dictionary<TileOption,object> options, Dictionary<TileOption,object> sOptions) {

        }
        [JsonIgnore] public Dictionary<TileOption, object> Options { get => options; set => options = value; }
    }
    public static class TileOptionFactoryExtension {
        public static bool isDynamic(this TileOption option) {
            switch (option) {
                case TileOption.Rotation:
                    return true;
                case TileOption.Hardness:
                    return true;
                case TileOption.Chisel:
                    return true;
                case TileOption.State:
                    return false;
                default:
                    return false;
            }

        }
        public static bool isSerizable(this TileOption option) {
            switch (option) {
                case TileOption.Rotation:
                    return true;
                case TileOption.Hardness:
                    return false;
                case TileOption.Chisel:
                    return true;
                case TileOption.State:
                    return true;
                default:
                    return false;
            }
        }

    
        public static string serializeOptions(TileOptions tileOptions) {
            return Newtonsoft.Json.JsonConvert.SerializeObject(tileOptions);
        }

        public static TileOptions deseralizeOptions(string data, TileItem tileItem) {
            if (data == null) {
                return null;
            }
            TileOptions tileOptions = Newtonsoft.Json.JsonConvert.DeserializeObject<TileOptions>(data);
            tileOptions.Options = tileItem.getNonSOptions();
            return tileOptions;
        }
    }

    
}