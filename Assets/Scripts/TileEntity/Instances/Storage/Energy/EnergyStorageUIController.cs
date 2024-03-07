using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TileEntityModule.Instances.Storage {
    public class EnergyStorageUIController : MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI textMeshProUGUI;
        [SerializeField] public Slider energySlider;
        private Battery battery;
        public void display(Battery battery) {
            this.battery = battery;
            this.textMeshProUGUI.text = battery.name;
        }
        
        // Update is called once per frame
        void Update()
        {
            energySlider.value = ((float) battery.Energy)/battery.Storage;
        }
    }
}

