using UnityEngine;

namespace Item.Transmutation.ShaderOptions
{
    [CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item/Instances/Transmutable/Shader Options/TwoGrad")]
    public class TwoGradientShaderOptionObject : TransmutationShaderOptionObject
    {
        public float HueShift;
        public Color FirstColor;
        public Color SecondColor;
        public override void Apply(Material material)
        {
            int first = Shader.PropertyToID("_First");
            int second = Shader.PropertyToID("_Second");
                    
            material.SetColor(first,FirstColor);
            material.SetColor(second,SecondColor);
        }
    }
}
