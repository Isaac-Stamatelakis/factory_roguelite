using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TileEntityModule.Instances.Storage {
    public class EnergyStorageUIController : MonoBehaviour, ITileEntityUI<BatteryInstance>
    {
        [SerializeField] public TextMeshProUGUI textMeshProUGUI;
        [SerializeField] public Slider energySlider;
        private BatteryInstance battery;
    
        public void display(BatteryInstance tileEntityInstance)
        {
            this.battery = tileEntityInstance;
            this.textMeshProUGUI.text = tileEntityInstance.getName();
        }

        void Update()
        {
            energySlider.value = ((float) battery.Energy)/battery.TileEntity.Storage;
        }
    }
}

