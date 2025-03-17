using System;
using UnityEngine;

namespace UI.TitleScreen
{
    public class TitleScreenParticleManager : MonoBehaviour
    {
        private ParticleSystem mParticleSystem;
        private const float PURPLE_HUE = 0.8f;
        private float hue = PURPLE_HUE;
        public void Start()
        {
            mParticleSystem = GetComponent<ParticleSystem>();
        }

        void FixedUpdate()
        {
            const float HUE_CHANGE_RATE = 1 / 100f;
            hue += HUE_CHANGE_RATE*Time.fixedDeltaTime;
            if (hue > 1f)
            {
                hue -= 1f;
            }
            Color newColor = Color.HSVToRGB(hue, 1f, 1f);
            var main = mParticleSystem.main;
            main.startColor = newColor;
        }
    }
}
