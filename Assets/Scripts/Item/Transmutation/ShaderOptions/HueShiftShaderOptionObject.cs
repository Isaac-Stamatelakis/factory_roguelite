using UnityEngine;

namespace Item.Transmutation.ShaderOptions
{
    [CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item/Instances/Transmutable/Shader Options/HueShift")]
    
    public class HueShiftShaderOptionObject : TransmutationShaderOptionObject
    {
        public float HueShift = -0.1f;
    }
}
