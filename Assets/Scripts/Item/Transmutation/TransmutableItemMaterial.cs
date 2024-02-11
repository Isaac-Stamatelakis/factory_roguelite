using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum TransmutableItemState {
    Ingot,
    Dust,
    Plate,
    Wire,
    Block,
    Fine_Wire,
    Double_Plate,
    Small_Dust,
    Tiny_Dust,
    Rod,
    Bolt,
    Screw,
    Liquid,
    Gas,
    Plasma,
    Magnificent_Gem,
    Exceptional_Gem,
    Gem,
    Mediocre_Gem,
    Poor_Gem
}

public class TransmutableItemStateFactory {
    private static List<TransmutableItemState> getAllMetalStates() {
        return new List<TransmutableItemState>{
            TransmutableItemState.Ingot,
            TransmutableItemState.Dust,
            TransmutableItemState.Plate,
            TransmutableItemState.Wire,
            TransmutableItemState.Fine_Wire,
            TransmutableItemState.Double_Plate,
            TransmutableItemState.Small_Dust,
            TransmutableItemState.Tiny_Dust,
            TransmutableItemState.Rod,
            TransmutableItemState.Bolt,
            TransmutableItemState.Screw,
        };
            
    }

    private static Dictionary<TransmutableItemState, string> suffixs = new Dictionary<TransmutableItemState, string>{
        {TransmutableItemState.Ingot, "Ingot"},
        {TransmutableItemState.Dust, "Dust"},
        {TransmutableItemState.Plate, "Plate"},
        {TransmutableItemState.Wire, "Wire"},
        {TransmutableItemState.Fine_Wire, "Wire"},
        {TransmutableItemState.Double_Plate, "Plate"},
        {TransmutableItemState.Tiny_Dust, "Dust"},
        {TransmutableItemState.Rod, "Rod"},
        {TransmutableItemState.Bolt, "Bolt"},
        {TransmutableItemState.Screw, "Screw"},
        {TransmutableItemState.Liquid, "Liquid"},
        {TransmutableItemState.Gas, "Gas"},
        {TransmutableItemState.Plasma, "Plasma"},
        {TransmutableItemState.Magnificent_Gem, "Magnificent"},
        {TransmutableItemState.Exceptional_Gem, "Exceptional"},
        {TransmutableItemState.Gem, ""},
        {TransmutableItemState.Mediocre_Gem, "Mediocre"},
        {TransmutableItemState.Poor_Gem, "Poor"},


    };
    private static Dictionary<TransmutableItemState, string> prefixs = new Dictionary<TransmutableItemState, string>{
        {TransmutableItemState.Ingot, ""},
        {TransmutableItemState.Dust, ""},
        {TransmutableItemState.Plate, ""},
        {TransmutableItemState.Wire, ""},
        {TransmutableItemState.Fine_Wire, "Fine"},
        {TransmutableItemState.Double_Plate, "Double"},
        {TransmutableItemState.Tiny_Dust, "Tiny"},
        {TransmutableItemState.Rod, ""},
        {TransmutableItemState.Bolt, ""},
        {TransmutableItemState.Screw, ""},
        {TransmutableItemState.Liquid, ""},
        {TransmutableItemState.Gas, ""},
        {TransmutableItemState.Plasma, ""},
        {TransmutableItemState.Magnificent_Gem, ""},
        {TransmutableItemState.Exceptional_Gem, ""},
        {TransmutableItemState.Gem, ""},
        {TransmutableItemState.Mediocre_Gem, ""},
        {TransmutableItemState.Poor_Gem, ""},
    };

    public static string getPrefix(TransmutableItemState state) {
        if (prefixs.ContainsKey(state)) {
            return prefixs[state];
        }
        return "";
    }

    public static string getSuffix(TransmutableItemState state) {
        if (suffixs.ContainsKey(state)) {
            return suffixs[state];
        }
        return "";
    }
}

public class TransmutableItemSprites {
    private static Dictionary<TransmutableItemState, Sprite> dict = new Dictionary<TransmutableItemState, Sprite>();
    private static TransmutableItemSprites instance;
    private TransmutableItemSprites() {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Items/Main/TransmutableItems/TransmutableSprites");
        Debug.Log("Loaded " + sprites.Length + " Base Transmutation Sprites");
        foreach (TransmutableItemState state in (TransmutableItemState.GetValues(typeof(TransmutableItemState)))) {
            foreach (Sprite sprite in sprites) {
                if (sprite.name == state.ToString()) {
                    if (!dict.ContainsKey(state)) {
                        dict[state] = sprite;
                        break;
                    }
                }
            }
        }
    }
    public static TransmutableItemSprites getInstance() {
        if (instance == null) {
            instance = new TransmutableItemSprites();
        }
        return instance;
    }

    public Sprite getSprite(TransmutableItemState state) {
        if (dict.ContainsKey(state)) {
            return dict[state];
        }
        return null;
    }
}
///
/// Creates ItemObjects for each transmutable object state
///
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
public class TransmutableMaterialDict {
    public TransmutableItemMaterial material;
    public Dictionary<TransmutableItemState, TransmutableStateOptions> dict = new Dictionary<TransmutableItemState, TransmutableStateOptions>();
    public TransmutableMaterialDict(TransmutableItemMaterial material) {
        this.material = material;
        foreach (TransmutableStateOptions options in material.states) {
            if (!dict.ContainsKey(options.state)) {
                dict[options.state] = options;
            }
        }
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