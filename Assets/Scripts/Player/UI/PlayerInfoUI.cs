using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerModule {
    public class PlayerInfoUI : MonoBehaviour
    {
        [SerializeField] private Scrollbar health;
        [SerializeField] private Scrollbar energy;
        
        public void display(float healthRatio, float energyRatio) {
            health.size = healthRatio;
            energy.size = energyRatio;
        }
    }
}

