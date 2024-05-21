using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Items.Transmutable {
    [CreateAssetMenu(fileName ="New Gem Material",menuName="Item/Instances/Transmutable/Gem")]
    public class GemTransmutableMaterial : TransmutableItemMaterial
    {
        public List<TransmutableStateOptions> gemStates = new List<TransmutableStateOptions>{
            new TransmutableStateOptions(
                state: TransmutableItemState.Plate,
                sprites: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Dust,
                sprites: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Magnificent_Gem,
                sprites: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Exceptional_Gem,
                sprites: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Gem,
                sprites: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Mediocre_Gem,
                sprites: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Gem,
                sprites: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Screw,
                sprites: null,
                prefix: "",
                suffix: ""
            ),
        };


        public override List<TransmutableStateOptions> getStates()
        {
            List<TransmutableStateOptions> states = base.getStates();
            List<TransmutableStateOptions> returnVal = new List<TransmutableStateOptions>();
            returnVal.AddRange(states); 
            returnVal.AddRange(gemStates);
            return returnVal;
        }

    }
}
