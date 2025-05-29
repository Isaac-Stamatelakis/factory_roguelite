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

        public bool HasShaders => ShaderMaterial;
        
        public TransmutationShaderPair GetShaderPair()
        {
            
            if (!ShaderOption)
            {
                return new TransmutationShaderPair(ShaderMaterial.UIMaterial, ShaderMaterial.WorldMaterial);
            }

            switch (ShaderOption)
            {
                case TwoGradientShaderOptionObject twoGradientOption:
                {
                    int first = Shader.PropertyToID("_First");
                    int second = Shader.PropertyToID("_Second");
                    Material worldCopy = new Material(ShaderMaterial.WorldMaterial);
                    worldCopy.SetColor(first,twoGradientOption.FirstColor);
                    worldCopy.SetColor(second,twoGradientOption.SecondColor);
                
                    Material uiCopy = new Material(ShaderMaterial.UIMaterial);
                    uiCopy.SetColor(first,twoGradientOption.FirstColor);
                    uiCopy.SetColor(second,twoGradientOption.SecondColor);
                    return new TransmutationShaderPair(uiCopy, worldCopy);
                }
                case ThreeGradientShaderOptionObject threeGradientOption:
                {
                    int first = Shader.PropertyToID("_First");
                    int second = Shader.PropertyToID("_Second");
                    int third = Shader.PropertyToID("_Third");
                    int outDominance =  Shader.PropertyToID("_OutDominance");
                    int hueShift = Shader.PropertyToID("_HueShift");
                    Material worldCopy = new Material(ShaderMaterial.WorldMaterial);
                    worldCopy.SetColor(first,threeGradientOption.FirstColor);
                    worldCopy.SetColor(second,threeGradientOption.SecondColor);
                    worldCopy.SetColor(third, threeGradientOption.ThirdColor);
                    worldCopy.SetFloat(outDominance,threeGradientOption.OutDominance);
                    worldCopy.SetFloat(hueShift,threeGradientOption.HueShift);
                    
                    Material uiCopy = new Material(ShaderMaterial.UIMaterial);
                    uiCopy.SetColor(first,threeGradientOption.FirstColor);
                    uiCopy.SetColor(second,threeGradientOption.SecondColor);
                    uiCopy.SetColor(third,threeGradientOption.ThirdColor);
                    uiCopy.SetFloat(outDominance,threeGradientOption.OutDominance);
                    uiCopy.SetFloat(hueShift,threeGradientOption.HueShift);
                    return new TransmutationShaderPair(uiCopy, worldCopy);
                }
                default:
                    return null;
            }
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
