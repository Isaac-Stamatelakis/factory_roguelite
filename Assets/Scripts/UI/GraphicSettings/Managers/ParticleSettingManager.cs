using UnityEngine;

namespace UI.GraphicSettings.Managers
{
    internal enum ParticleSetting
    {
        Off,
        Decreased,
        On
    }
   
    internal class ParticleSettingManager : GraphicSettingManager
    {
        public override void ApplyGraphicSettings(int value)
        {
            ParticleSetting particleSetting = (ParticleSetting)value;
            
        }

        public override string GetValueName(int value)
        {
            ParticleSetting particleSetting = (ParticleSetting)value;
            return particleSetting.ToString();
        }
    }
}