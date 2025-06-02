using UnityEngine;

namespace Item.Transmutation.ShaderOptions
{
    [CreateAssetMenu(fileName ="New Transmutable Material",menuName="Item/Instances/Transmutable/Shader Options/Neon")]
    public class NeonShaderOptionObject : TransmutationShaderOptionObject
    {
        public Color Color;
        public float WaveSpeed = 0.125f;
        public float WaveDulling = 128f;
        public float PulseSpeed = 1.7f;
        public float PulseDulling = 16;
        public float GreyScaleFactor = 0.8f;
        public override void Apply(Material material)
        {
            int color = Shader.PropertyToID("_Color");
            int pulseSpeed = Shader.PropertyToID("_PulseSpeed");
            int waveDulling = Shader.PropertyToID("_WaveDulling");
            int waveSpeed = Shader.PropertyToID("_WaveSpeed");
            int flashDulling = Shader.PropertyToID("_FlashDulling");
            int greyScaleFactor = Shader.PropertyToID("_Dulling");
            
            material.SetColor(color, Color);
            material.SetFloat(pulseSpeed, PulseSpeed);
            material.SetFloat(waveDulling, WaveDulling);
            material.SetFloat(flashDulling,PulseDulling);
            material.SetFloat(greyScaleFactor,GreyScaleFactor);
            material.SetFloat(waveSpeed, WaveSpeed);
        }
    }
}
