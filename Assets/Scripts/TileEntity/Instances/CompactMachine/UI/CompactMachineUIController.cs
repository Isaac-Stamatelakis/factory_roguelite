using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Dimensions;

namespace TileEntityModule.Instances.CompactMachines {
    public class CompactMachineUIController : MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI title;
        [SerializeField] public Button teleportButton;
        [SerializeField] public Button toggleButton;
        [SerializeField] public TMP_InputField nameTextField;
        private CompactMachine compactMachine;

        public void Start() {
            toggleButton.onClick.AddListener(toggle);
            teleportButton.onClick.AddListener(teleport);
        }

        public void OnDestroy() {
            toggleButton.onClick.RemoveAllListeners();
            teleportButton.onClick.RemoveAllListeners();

        }
        
        public void display(CompactMachine compactMachine) {
            this.compactMachine = compactMachine;
        }

        private void teleport() {
            CompactMachineHelper.teleportIntoCompactMachine(compactMachine);
        }
        private void toggle() {
            
        }
    }
}

