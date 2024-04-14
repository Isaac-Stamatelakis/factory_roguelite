using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ItemModule;

namespace TileEntityModule.Instances.Matrix {
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
        private int amount;
        private ItemMatrixController controller;

        public void init(ItemMatrixController controller,  ItemSlot toCraft, int amount) {
            this.controller = controller;
            this.toCraft = toCraft;
            this.amount = amount;
            displayCraftingTree();
            HashSet<MatrixAutoCraftCore> cores = new HashSet<MatrixAutoCraftCore>();
            cancelButton.onClick.AddListener(() => {
                GameObject.Destroy(gameObject);
            });
        }
        public static AutoCraftPopupUI newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/AutoCrafting/RecipeCraftPopup").GetComponent<AutoCraftPopupUI>();
        }

        private void displayCraftingTree() {
            GlobalHelper.deleteAllChildren(itemElementContainer.transform);
            Tree<PreparedRecipePreview> craftingTree = AutoCraftingSequenceFactory.createRecipeTree(controller,toCraft,amount);
            List<TreeNode<PreparedRecipePreview>> items = TreeHelper.postOrderTraversal<PreparedRecipePreview>(craftingTree);
            foreach (TreeNode<PreparedRecipePreview> preparedRecipePreview in items) {
                foreach ((ItemSlot,int,bool) tuple in preparedRecipePreview.Value.AvailableInputs) {
                    ItemSlot itemSlot = tuple.Item1;
                    string id = itemSlot.itemObject.id;
                    ItemTagKey tagKey = new ItemTagKey(itemSlot.tags);
                    int amount = tuple.Item2;
                    bool crafted = tuple.Item3;
                    if (tuple.Item2 == 0 && crafted) {
                        continue;
                    }
                    AutoCraftItemUIElement autoCraftItemUIElement = AutoCraftItemUIElement.newInstance();
                    autoCraftItemUIElement.init(
                        AutoCraftElementMode.Input, 
                        tuple.Item1,
                        tuple.Item2,tuple.Item1.amount*preparedRecipePreview.Value.Amount
                    );
                    autoCraftItemUIElement.transform.SetParent(itemElementContainer.transform,false);
                }
                foreach ((ItemSlot,int) tuple in preparedRecipePreview.Value.OutputAmounts) {
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

