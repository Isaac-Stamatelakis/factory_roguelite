using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ItemModule;
using ItemModule.Inventory;
using ItemModule.Tags;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixInterfaceUI : MonoBehaviour, IInventoryListener
    {
        [SerializeField] private Button plusPriorityButton;
        [SerializeField] private Button minusProrityButton;
        [SerializeField] private TextMeshProUGUI priorityText;
        [SerializeField] private GridLayoutGroup recipeContainer;
        [SerializeField] private GridLayoutGroup upgradeContainer;
        private MatrixInterfaceUpgradeRestrictedInventoryUI typeRestrictedInventory;
        private TagRestrictedInventoryUI recipeRestrictedInventory;
        private MatrixInterface matrixInterface;

        public void init(MatrixInterface matrixInterface) {
            this.matrixInterface = matrixInterface;

            GlobalHelper.deleteAllChildren(upgradeContainer.transform);
            typeRestrictedInventory = upgradeContainer.gameObject.AddComponent<MatrixInterfaceUpgradeRestrictedInventoryUI>();
            ItemSlotUIFactory.getSlotsForInventory(matrixInterface.Upgrades,upgradeContainer.transform);
            typeRestrictedInventory.initalize(matrixInterface.Upgrades);

            GlobalHelper.deleteAllChildren(recipeContainer.transform);
            recipeRestrictedInventory = recipeContainer.gameObject.AddComponent<TagRestrictedInventoryUI>();
            recipeRestrictedInventory.addListener(this);
            ItemSlotUIFactory.getSlotsForInventory(matrixInterface.Recipes,recipeContainer.transform);
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

        public void inventoryUpdate(int n)
        {
            List<EncodedRecipe> recipes = matrixInterface.getRecipes();
            if (n >= recipes.Count) {
                return;
            }
            EncodedRecipe encodedRecipe = recipes[n];
            matrixInterface.recipeUpdate(encodedRecipe,n);
        }
    }
}

