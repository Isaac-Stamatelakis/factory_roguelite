using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ItemModule;
using ItemModule.Inventory;
using ItemModule.Tags;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixInterfaceUI : MonoBehaviour
    {
        [SerializeField] private Button plusPriorityButton;
        [SerializeField] private Button minusProrityButton;
        [SerializeField] private TextMeshProUGUI priorityText;
        [SerializeField] private GridLayoutGroup recipeContainer;
        [SerializeField] private GridLayoutGroup upgradeContainer;
        private MatrixInterfaceUpgradeRestrictedInventoryUI typeRestrictedInventory;
        private EncodedRecipeTagInventoryUI recipeRestrictedInventory;
        private MatrixInterface matrixInterface;

        public void init(MatrixInterface matrixInterface) {
            this.matrixInterface = matrixInterface;

            GlobalHelper.deleteAllChildren(upgradeContainer.transform);
            typeRestrictedInventory = upgradeContainer.gameObject.AddComponent<MatrixInterfaceUpgradeRestrictedInventoryUI>();
            ItemSlotUIFactory.getSlotsForInventory(matrixInterface.Upgrades,upgradeContainer.transform);
            typeRestrictedInventory.initalize(matrixInterface.Upgrades);

            GlobalHelper.deleteAllChildren(recipeContainer.transform);
            recipeRestrictedInventory = recipeContainer.gameObject.AddComponent<EncodedRecipeTagInventoryUI>();
            ItemSlotUIFactory.getSlotsForInventory(matrixInterface.Recipes,recipeContainer.transform);
            recipeRestrictedInventory.initalize(matrixInterface.Recipes,matrixInterface);

            priorityText.text = matrixInterface.Priority.ToString();
            plusPriorityButton.onClick.AddListener(() => {
                matrixInterface.iteratePriority(1);
                priorityText.text = matrixInterface.Priority.ToString();
            });
            minusProrityButton.onClick.AddListener(() => {
                matrixInterface.iteratePriority(-1);
                priorityText.text = matrixInterface.Priority.ToString();
            });
            
        }

        public static MatrixInterfaceUI newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/Interface/InterfaceUI").GetComponent<MatrixInterfaceUI>();
        }
    }
}
