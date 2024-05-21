using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;


namespace Items.Transmutable {
    /// <summary>
    /// Creates ItemObjects for each transmutable object state
    /// </summary>
    [CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item/Instances/Transmutable/Empty")]
    public class TransmutableItemMaterial : ScriptableObject
    {
        public string id;
        public Tier tier;
        public Color color;
        
        //[HideInInspector]
        public List<TransmutableStateOptions> states;
        public virtual List<TransmutableStateOptions> getStates() {
            return this.states;
        }
        
        private List<KVP<TransmutableItemState,string>> statesToID;
        private Dictionary<TransmutableItemState, string> stateToIDDict;
        #if UNITY_EDITOR 
        public List<KVP<TransmutableItemState, string>> StatesToID { get => statesToID; set => statesToID = value; }
        #endif
        public TransmutableItemObject transmute(TransmutableItemState output) {
            if (!canTransmute(output)) {
                return null;
            }
            string outputID = stateToIDDict[output];
            return ItemRegistry.getInstance().getTransmutableItemObject(outputID);
        }

        public bool canTransmute(TransmutableItemState state) {
            if (stateToIDDict == null) {
                initDict();
            }
            return stateToIDDict.ContainsKey(state);
        }

        private void initDict() {
            stateToIDDict = new Dictionary<TransmutableItemState, string>();
            foreach (KVP<TransmutableItemState,string> kvp in statesToID) {
                stateToIDDict[kvp.key] = kvp.value;
            }
        }
    }

    [System.Serializable]
    public class KVP<K,V> {
        public K key;
        public V value;
        public KVP(K key, V value) {
            this.key = key;
            this.value = value;
        }
    }

    [System.Serializable]
    public class TransmutableStateOptions {
        public TransmutableStateOptions(TransmutableItemState state, Sprite[] sprites, string prefix, string suffix) {
            this.state = state;
            this.sprites = sprites;
            this.prefix = prefix;
            this.suffix = suffix;
        }
        public TransmutableItemState state;
        public Sprite[] sprites;
        public string prefix;
        public string suffix;
        
    }
}
