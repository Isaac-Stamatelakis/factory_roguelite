using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TileEntity.Instances.Storage {
    public class EnergyStorageUIController : MonoBehaviour, ITileEntityUI<BatteryInstance>
    {
        [SerializeField] public TextMeshProUGUI textMeshProUGUI;
        [SerializeField] public Slider energySlider;
        private BatteryInstance battery;
    
        public void DisplayTileEntityInstance(BatteryInstance tileEntityInstance)
        {
            this.battery = tileEntityInstance;
            this.textMeshProUGUI.text = tileEntityInstance.GetName();
        }

        void Update()
        {
            energySlider.value = ((float) battery.Energy)/battery.TileEntityObject.Storage;
        }
    }
}

