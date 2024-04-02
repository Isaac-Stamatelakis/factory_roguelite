using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using ConduitModule.Ports;
using Newtonsoft.Json;
using GUIModule;

namespace TileEntityModule.Instances.Storage {
    [CreateAssetMenu(fileName = "E~New Battery", menuName = "Tile Entity/Storage/Battery")]
    public class Battery : TileEntity, ITickableTileEntity, IRightClickableTileEntity, ISerializableTileEntity, IConduitInteractable, IEnergyConduitInteractable
    {
        private int energy;
        [SerializeField] private int storage;
        [SerializeField] public ConduitPortLayout conduitPortLayout;
        [SerializeField] public GameObject uiPrefab;

        public int Energy { get => energy; set => energy = value; }
        public int Storage { get => storage; set => storage = value; }

        public ConduitPortLayout getConduitPortLayout()
        {
            return conduitPortLayout;
        }

        public ref int getEnergy(Vector2Int portPosition)
        {
            return ref energy;
        }

        public int insertEnergy(int input,Vector2Int portPosition)
        {
            if (Energy >= storage) {
                return 0;
            }
            int sum = input + Energy;
            if (sum > storage) {
                Energy = storage;
                return sum-storage;
            }
            Energy += input;
            return input;
        }

        public void onRightClick()
        {
            if (uiPrefab == null) {
                Debug.LogError(name + " uiPrefab is null");
                return;
            }
            GameObject instantiated = GameObject.Instantiate(uiPrefab);
            EnergyStorageUIController uIController = instantiated.GetComponent<EnergyStorageUIController>();
            if (uIController == null) {
                Debug.LogError(name + " uiPrefab doesn't have EnergyStorageUIController component");
                return;
            }
            GlobalUIController globalUIController = GlobalUIContainer.getInstance().getUiController();
            uIController.display(this);
            globalUIController.setGUI(instantiated);
        }

        public string serialize()
        {
            return JsonConvert.SerializeObject(Energy);
        }

        public void tickUpdate()
        {
            
        }

        public void unserialize(string data)
        {
            Energy = JsonConvert.DeserializeObject<int>(data);
        }
    }
}

