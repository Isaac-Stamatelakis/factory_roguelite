using UnityEngine;

namespace Item.Transmutation.ShaderOptions
{
    [CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item/Instances/Transmutable/Shader Options/ThreeGrad")]
    public class ThreeGradientShaderOptionObject : TransmutationShaderOptionObject
    {
        public float HueShift;
        public float OutDominance;
        public Color FirstColor;
        public Color SecondColor;
        public Color ThirdColor;
    }
}
