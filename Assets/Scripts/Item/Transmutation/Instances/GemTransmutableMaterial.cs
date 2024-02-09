using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ItemModule.Transmutable {
    [CreateAssetMenu(fileName ="New Gem Material",menuName="Item Register/Transmutable/Gem")]
    public class GemTransmutableMaterial : TransmutableItemMaterial
    {
        public List<TransmutableStateOptions> gemStates = new List<TransmutableStateOptions>{
            new TransmutableStateOptions(
                state: TransmutableItemState.Plate,
                sprite: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Dust,
                sprite: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Magnificent_Gem,
                sprite: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Exceptional_Gem,
                sprite: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Gem,
                sprite: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Mediocre_Gem,
                sprite: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Gem,
                sprite: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Screw,
                sprite: null,
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
