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
        private TagRestrictedInventoryUI recipeRestrictedInventory;

        public void init(MatrixInterface matrixInterface) {
            typeRestrictedInventory = upgradeContainer.gameObject.AddComponent<MatrixInterfaceUpgradeRestrictedInventoryUI>();
            typeRestrictedInventory.initalize(matrixInterface.Upgrades);

            recipeRestrictedInventory = recipeContainer.gameObject.AddComponent<TagRestrictedInventoryUI>();
            recipeRestrictedInventory.initalize(matrixInterface.Recipes,ItemTag.EncodedRecipe);

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

