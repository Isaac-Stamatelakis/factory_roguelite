using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items;
using Items.Inventory;
using Items.Tags;

namespace TileEntity.Instances.Matrix {
    public class MatrixInterfaceUI : MonoBehaviour
    {
        [SerializeField] private Button plusPriorityButton;
        [SerializeField] private Button minusProrityButton;
        [SerializeField] private TextMeshProUGUI priorityText;
        [SerializeField] private GridLayoutGroup recipeContainer;
        [SerializeField] private GridLayoutGroup upgradeContainer;
        private MatrixInterfaceUpgradeRestrictedInventoryUI typeRestrictedInventory;
        private EncodedRecipeTagInventoryUI recipeRestrictedInventory;
        private MatrixInterfaceInstance matrixInterface;

        public void init(MatrixInterfaceInstance matrixInterface) {
            /*
            this.matrixInterface = matrixInterface;

            GlobalHelper.deleteAllChildren(upgradeContainer.transform);
            typeRestrictedInventory = upgradeContainer.gameObject.AddComponent<MatrixInterfaceUpgradeRestrictedInventoryUI>();
            ItemSlotUIFactory.getSlotsForInventory(matrixInterface.Upgrades,upgradeContainer.transform,ItemDisplayUtils.SolidItemPanelColor);
            typeRestrictedInventory.initalize(matrixInterface.Upgrades);

            GlobalHelper.deleteAllChildren(recipeContainer.transform);
            recipeRestrictedInventory = recipeContainer.gameObject.AddComponent<EncodedRecipeTagInventoryUI>();
            ItemSlotUIFactory.getSlotsForInventory(matrixInterface.Recipes,recipeContainer.transform,ItemDisplayUtils.SolidItemPanelColor);
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
            */
            
        }

        public static MatrixInterfaceUI newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/Interface/InterfaceUI").GetComponent<MatrixInterfaceUI>();
        }
    }
}

