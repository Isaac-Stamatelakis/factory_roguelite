using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntityModule.Instances.CompactMachines {
    [CreateAssetMenu(fileName = "E~New Compact Machine", menuName = "Tile Entity/Compact Machine/Teleporter")]
    public class CompactMachineTeleporter : TileEntity, IClickableTileEntity, ICompactMachineInteractable, ISoftLoadable
    {
        [SerializeField] public GameObject uiPrefab;
        private CompactMachine compactMachine;
        
        public void onClick()
        {
            if (uiPrefab == null) {
                CompactMachineHelper.teleportOutOfCompactMachine(compactMachine);
                return;
            }
            // Not null show ui prefab
            /*
            GameObject instantiated = GameObject.Instantiate(uiPrefab);
            CompactMachineUIController uIController = instantiated.GetComponent<CompactMachineUIController>();
            if (uIController == null) {
                Debug.LogError(name + "ui prefab doesn't have controller");
                return;
            }
            uIController.display(this);
            GlobalUIContainer.getInstance().getUiController().setGUI(instantiated);
            */
        }

        public void syncToCompactMachine(CompactMachine compactMachine)
        {
            this.compactMachine = compactMachine;
            compactMachine.Teleporter = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}

