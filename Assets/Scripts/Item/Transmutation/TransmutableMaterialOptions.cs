using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Transmutable
{
    [CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item/Instances/Transmutable/Options")]
    public class TransmutableMaterialOptions : ScriptableObject
    {
        public TransmutableItemState BaseState = TransmutableItemState.Ingot;
        public List<TransmutableStateOptions> States;
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

