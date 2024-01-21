using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum TransmutableItemState {
    Ingot,
    Dust,
    Plate,
    Wire,
    Fine_Wire,
    Double_Plate,
    Small_Dust,
    Tiny_Dust,
    Rod,
    Bolt,
    Screw,
    Liquid,
    Gas,
    Plasma
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
///
/// Creates ItemObjects for each transmutable object state
///
[CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item Register/Transmutable Material")]
public class TransmutableItemMaterial : ScriptableObject
{
    public string id;
    [Header("Color of default sprite")]
    public Color color;
    [Header("If clicked, all states are used")]
    public bool useAllStates;
    [Header("If clicked, all metal states are used")]
    public bool useAllMetalStates;
    [Header("If clicked, all gas states are used")]
    public bool useAllGasStates;
    public List<TransmutableStateOptions> states;
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
    public TransmutableItemState state;
    
    [Header("Custom sprite\nIf left blank then\ncolor will be used on a default")]
    public Sprite sprite;
    
    [Header("Custom prefix\nIf left blank then\nstate name will be used")]
    public string prefix;
    [Header("Custom suffix\nIf left blank then\nstate name will be used")]
    public string suffix;
    
}