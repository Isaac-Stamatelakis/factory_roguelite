using Items.Transmutable;
using UnityEngine;

namespace Item.Transmutation
{
    
    [CreateAssetMenu(fileName ="New Transmutation Shader Pair",menuName="Item/Instances/Transmutable/ShaderPair")]
    public class TransmutationShaderPairObject : ScriptableObject
    {
        public Material UIMaterial;
        public Material WorldMaterial;
    }
}
