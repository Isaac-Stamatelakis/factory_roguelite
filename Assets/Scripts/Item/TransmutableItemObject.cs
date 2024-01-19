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
    List<TransmutableItemState> getAllMetalStates() {
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
}
[CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item Register/Transmutable Material")]
public class TransmutableItemObject : ItemObject
{
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

[System.Serializable]
public class TransmutableStateOptions {
    public TransmutableItemState state;
    
    [Header("Custom sprite\nIf left blank then\ncolor will be used on a default")]
    public Sprite sprite;
    
    [Header("Custom prefix\nIf left blank then\nstate name will be used")]
    public string prefix;
    
}