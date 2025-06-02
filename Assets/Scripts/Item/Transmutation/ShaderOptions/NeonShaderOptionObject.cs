using UnityEngine;

namespace Item.Transmutation.ShaderOptions
{
    [CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item/Instances/Transmutable/Shader Options/Neon")]
    public class NeonShaderOptionObject : TransmutationShaderOptionObject
    {
        public Color Color;
        public float WaveSpeed;
        public float WaveDulling;
        public float PulseSpeed;
        public float PulseDulling;
        public float GreyScaleFactor;
    }
}
