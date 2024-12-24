using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;


namespace Items.Transmutable {
    /// <summary>
    /// Creates ItemObjects for each transmutable object state
    /// </summary>
    [CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item/Instances/Transmutable/Material")]
    public class TransmutableItemMaterial : ScriptableObject
    {
        public string id;
        public Tier tier;
        public Color color;

        public TransmutableMaterialOptions MaterialOptions;
        public virtual List<TransmutableStateOptions> GetStates()
        {
            return MaterialOptions.States;
        }
        
        private Dictionary<TransmutableItemState, TransmutableStateOptions> stateOptionDict;

        public Dictionary<TransmutableItemState, TransmutableStateOptions> GetOptionStateDict()
        {
            if (stateOptionDict != null) return stateOptionDict;
            stateOptionDict = new Dictionary<TransmutableItemState, TransmutableStateOptions>();
            foreach (TransmutableStateOptions stateOptions in MaterialOptions.States)
            {
                stateOptionDict[stateOptions.state] = stateOptions;
            }
            return stateOptionDict;
        }
    }
}
