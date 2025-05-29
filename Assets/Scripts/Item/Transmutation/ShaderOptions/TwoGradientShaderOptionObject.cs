using UnityEngine;

namespace Item.Transmutation.ShaderOptions
{
    [CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item/Instances/Transmutable/Shader Options/TwoGrad")]
    public class TwoGradientShaderOptionObject : TransmutationShaderOptionObject
    {
        public float HueShift;
        public Color FirstColor;
        public Color SecondColor;
    }
}
