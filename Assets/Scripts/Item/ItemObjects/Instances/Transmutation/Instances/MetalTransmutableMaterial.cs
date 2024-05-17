using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Items.Transmutable {
    [CreateAssetMenu(fileName ="New Metal Material",menuName="Item/Instances/Transmutable/Metal")]
    public class MetalTransmutableMaterial : TransmutableItemMaterial
    {
        public List<TransmutableStateOptions> metalStates = new List<TransmutableStateOptions>{
            new TransmutableStateOptions(
                state: TransmutableItemState.Ingot,
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
                state: TransmutableItemState.Plate,
                sprites: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Wire,
                sprites: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Fine_Wire,
                sprites: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Double_Plate,
                sprites: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Small_Dust,
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
            new TransmutableStateOptions(
                state: TransmutableItemState.Small_Dust,
                sprites: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Rod,
                sprites: null,
                prefix: "",
                suffix: ""
            ),
            new TransmutableStateOptions(
                state: TransmutableItemState.Bolt,
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
            returnVal.AddRange(metalStates);
            return states;
        }

    }
}
