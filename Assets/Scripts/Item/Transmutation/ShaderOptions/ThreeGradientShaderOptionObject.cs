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
        public override void Apply(Material material)
        {
            int first = Shader.PropertyToID("_First");
            int second = Shader.PropertyToID("_Second");
            int third = Shader.PropertyToID("_Third");
            int outDominance =  Shader.PropertyToID("_OutDominance");
            int hueShift = Shader.PropertyToID("_HueShift");
                    
            material.SetColor(first,FirstColor);
            material.SetColor(second,SecondColor);
            material.SetColor(third, ThirdColor);
            material.SetFloat(outDominance,OutDominance);
            material.SetFloat(hueShift,HueShift);
        }
    }
}
