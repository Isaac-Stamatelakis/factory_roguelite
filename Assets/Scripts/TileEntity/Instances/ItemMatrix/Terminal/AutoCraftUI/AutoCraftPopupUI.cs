using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items;

namespace TileEntity.Instances.Matrix {
    public class AutoCraftPopupUI : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup itemElementContainer;
        [SerializeField] private TextMeshProUGUI requiredMemoryText;
        [SerializeField] private TextMeshProUGUI maxUsableCores;
        [SerializeField] private TMP_Dropdown craftingProcessorSelector;
        [SerializeField] private TextMeshProUGUI currentProcessorMemory;
        [SerializeField] private TextMeshProUGUI currentProcessorCores;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button activateButton;

        private ItemSlot toCraft;
        private uint amount;
        private ItemMatrixControllerInstance controller;
        public void init(ItemMatrixControllerInstance controller,  ItemSlot toCraft, uint amount) {
            this.controller = controller;
            this.toCraft = toCraft;
            this.amount = amount;
            DisplayCraftingTree();
            HashSet<MatrixAutoCraftCore> cores = new HashSet<MatrixAutoCraftCore>();
            cancelButton.onClick.AddListener(() => {
                GameObject.Destroy(gameObject);
            });
        }
        public static AutoCraftPopupUI newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/AutoCrafting/RecipeCraftPopup").GetComponent<AutoCraftPopupUI>();
        }

        private void DisplayCraftingTree() {
            GlobalHelper.deleteAllChildren(itemElementContainer.transform);
            Tree<PreparedRecipePreview> craftingTree = AutoCraftingSequenceFactory.CreateRecipeTree(controller,toCraft,amount);
            List<TreeNode<PreparedRecipePreview>> items = TreeHelper.postOrderTraversal<PreparedRecipePreview>(craftingTree);
            foreach (TreeNode<PreparedRecipePreview> preparedRecipePreview in items) {
                foreach (var (itemSlot, previewAmount, crafted) in preparedRecipePreview.Value.AvailableInputs) {
                    string id = itemSlot.itemObject.id;
                    ItemTagKey tagKey = new ItemTagKey(itemSlot.tags);
                    if (amount == 0 && crafted) {
                        continue;
                    }
                    AutoCraftItemUIElement autoCraftItemUIElement = AutoCraftItemUIElement.newInstance();
                    autoCraftItemUIElement.init(
                        AutoCraftElementMode.Input, 
                        itemSlot,
                        previewAmount,
                        itemSlot.amount*preparedRecipePreview.Value.Amount
                    );
                    autoCraftItemUIElement.transform.SetParent(itemElementContainer.transform,false);
                }
                foreach ((ItemSlot,uint) tuple in preparedRecipePreview.Value.OutputAmounts) {
                    AutoCraftItemUIElement autoCraftItemUIElement = AutoCraftItemUIElement.newInstance();
                    autoCraftItemUIElement.init(
                        AutoCraftElementMode.Output, 
                        tuple.Item1,
                        tuple.Item2,
                        0
                    );
                    autoCraftItemUIElement.transform.SetParent(itemElementContainer.transform,false);
                }
            }
        }
    }
}

