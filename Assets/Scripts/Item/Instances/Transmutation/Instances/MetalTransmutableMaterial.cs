using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ItemModule.Transmutable {
    [CreateAssetMenu(fileName ="New Metal Material",menuName="Item/Instances/Transmutable/Metal")]
    public class MetalTransmutableMaterial : TransmutableItemMaterial
    {
        public List<TransmutableStateOptions> metalStates = new List<TransmutableStateOptions>{
            new TransmutableStateOptions(
                state: TransmutableItemState.Ingot,
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
                state: TransmutableItemState.Plate,
                sprite: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Wire,
                sprite: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Fine_Wire,
                sprite: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Double_Plate,
                sprite: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Small_Dust,
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
            new TransmutableStateOptions(
                state: TransmutableItemState.Small_Dust,
                sprite: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Rod,
                sprite: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Bolt,
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
            returnVal.AddRange(metalStates);
            return states;
        }

    }
}
