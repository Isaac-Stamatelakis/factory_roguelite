using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using UnityEngine;
using TileEntity;
using UI.Chat;


namespace Items.Transmutable {
    /// <summary>
    /// Creates ItemObjects for each transmutable object state
    /// </summary>
    [CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item/Instances/Transmutable/Material")]
    public class TransmutableItemMaterial : ScriptableObject
    {
        public Color color;
        public string chemicalFormula;
        public TieredGameStage gameStageObject;
        public Sprite OverlaySprite;
        public Material ShaderMaterial;
        
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
