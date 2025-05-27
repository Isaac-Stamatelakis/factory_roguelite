using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using Item.Transmutation;
using UnityEngine;
using TileEntity;
using UI.Chat;
using UnityEngine.Serialization;


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
        [FormerlySerializedAs("ShaderMaterial")] public Material UIShaderMaterial;
        public Material WorldShaderMaterial;
        
        public TransmutableMaterialOptions MaterialOptions;
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
