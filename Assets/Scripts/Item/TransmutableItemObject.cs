using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum TransmutableItemState {
    Ingot,
    Dust,
    Plate,
    Wire,
}
[CreateAssetMenu(fileName ="New Transmutable Object",menuName="Item Register/Transmutable")]
public class TransmutableItemObject : ItemObject
{
    public List<TransmutableItemState> states;
    public List<TransmutableStateOptions> stateOptions;
}

[System.Serializable]
    public class TransmutableStateOptions {
        public Sprite sprite;
        public string prefix;
        public TransmutableItemState state;
    }