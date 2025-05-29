using Items.Transmutable;
using UnityEngine;

namespace Item.Transmutation
{
    public enum TransmutableItemShaderType
    {
        None = 0,
        HueShift = 1,
        TwoGrad = 2,
        ThreeGrad = 3,
        Rainbow = 4,
    }
    
    
    
    [CreateAssetMenu(fileName ="New Transmutation Shader Pair",menuName="Item/Instances/Transmutable/ShaderPair")]
    public class TransmutationShaderPairObject : ScriptableObject
    {
        private enum ShaderType
        {
            HueShift = TransmutableItemShaderType.HueShift,
            TwoGrad = TransmutableItemShaderType.TwoGrad,
            ThreeGrad = TransmutableItemShaderType.ThreeGrad,
            Rainbow = TransmutableItemShaderType.Rainbow,
        }

        [SerializeField] private ShaderType shaderType = ShaderType.HueShift;
        public TransmutableItemShaderType TransmutationShaderType => (TransmutableItemShaderType)shaderType;
        public Material UIMaterial;
        public Material WorldMaterial;
    }
}
