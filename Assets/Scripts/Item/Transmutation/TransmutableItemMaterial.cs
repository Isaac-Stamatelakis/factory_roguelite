using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using Item.Transmutation;
using Item.Transmutation.ShaderOptions;
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
        public TransmutableMaterialOptions MaterialOptions;
        [SerializeField] private TransmutationShaderPairObject ShaderMaterial;
        [SerializeField] private TransmutationShaderOptionObject ShaderOption;

        public Tier GetTier()
        {
            return !gameStageObject ? Tier.Basic : gameStageObject.Tier;
        }

        public bool HasShaders => ShaderMaterial;
        
        public TransmutationShaderPair GetShaderPair()
        {
            
            if (!ShaderOption)
            {
                return new TransmutationShaderPair(ShaderMaterial.UIMaterial, ShaderMaterial.WorldMaterial);
            }

            Material uiCopy = new Material(ShaderMaterial.UIMaterial)
            {
                name = ShaderMaterial.UIMaterial.name + ShaderOption.name
            };
            
            ShaderOption.Apply(uiCopy);
            bool sameMaterial = ReferenceEquals(ShaderMaterial.WorldMaterial, ShaderMaterial.UIMaterial);
            if (sameMaterial)
            {
                return new TransmutationShaderPair(uiCopy, uiCopy);
            }
            
            Material worldCopy = new Material(ShaderMaterial.WorldMaterial)
            {
                name = ShaderMaterial.WorldMaterial.name + ShaderOption.name
            };
            ShaderOption.Apply(worldCopy);
            return new TransmutationShaderPair(uiCopy, worldCopy);
        }
    }

    public class  TransmutationShaderPair
    {
        public Material UIMaterial;
        public Material WorldMaterial;

        public  TransmutationShaderPair(Material uiMaterial, Material worldMaterial)
        {
            UIMaterial = uiMaterial;
            WorldMaterial = worldMaterial;
        }
    }
}
