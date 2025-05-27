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
    }
}
