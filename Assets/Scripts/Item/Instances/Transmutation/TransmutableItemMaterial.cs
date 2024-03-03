using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ItemModule.Transmutable {
    /// <summary>
    /// Creates ItemObjects for each transmutable object state
    /// </summary>
    [CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item Register/Transmutable/Empty")]
    public class TransmutableItemMaterial : ScriptableObject
    {
        public string id;
        [Header("Color of default sprite")]
        public Color color;
        public List<TransmutableStateOptions> states;
        public bool test = false;
        public virtual List<TransmutableStateOptions> getStates() {
            return this.states;
        }
        [Header("Auto Generated")]
        public List<KVP<TransmutableItemState,string>> statesToID;
        private Dictionary<TransmutableItemState, string> stateToIDDict;
        public TransmutableItemObject transmute(TransmutableItemState output) {
            if (stateToIDDict == null) {
                initDict();
            }
            string outputID = stateToIDDict[output];
            return ItemRegistry.getInstance().getTransmutableItemObject(outputID);
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
        public TransmutableStateOptions(TransmutableItemState state, Sprite sprite, string prefix, string suffix) {
            this.state = state;
            this.sprite = sprite;
            this.prefix = prefix;
            this.suffix = suffix;
        }
        public TransmutableItemState state;
        
        [Header("Custom sprite\nIf left blank then\ncolor will be used on a default")]
        public Sprite sprite;
        
        [Header("Custom prefix\nIf left blank then\nstate name will be used")]
        public string prefix;
        [Header("Custom suffix\nIf left blank then\nstate name will be used")]
        public string suffix;
        
    }
}
