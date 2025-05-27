using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UI;
using WorldModule.Caves;
using Items;
using System;
using Item.Inventory.ClickHandlers.Instances;
using Item.Slot;
using Items.Inventory;

namespace DevTools.Structures {
    public class NewStructurePopUpUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private Button backButton;
        [SerializeField] private Button createButton;
        [SerializeField] private Transform dynamicTextContainer;
        [SerializeField] private DynamicColorTextUI dynamicColorTextPrefab;
        [SerializeField] private SerializedItemSlotEditorUI itemSelectorUIPrefab;
        [SerializeField] private InventoryUI mInventoryUI;
        [SerializeField] private TMP_Dropdown optionDropdown;
        [SerializeField] private TextMeshProUGUI boundsText;
        [SerializeField] private IntervalVectorUI intervalVectorUIPrefab;
        [SerializeField] private Button editIntervalVectorButton;
        private StructureDevControllerUI controllerUI;
        // This only has one but is required to be a list to use the editor
        private List<SerializedItemSlot> itemSlots;
        private IntervalVector bounds;
        private StructureGenOptionType genOption = StructureGenOptionType.Empty;
        private SerializedItemSlot selectedItem;

        public void Init(StructureDevControllerUI controllerUI) {
            if (!ItemRegistry.IsLoaded) {
                StartCoroutine(ItemRegistry.LoadItems());
            }
            
            this.controllerUI = controllerUI;
            this.bounds = new IntervalVector(new Interval<int>(-4,4),new Interval<int>(-4,4));
            boundsText.text = bounds.ToString();

            DynamicColorTextUI dynamicColorTextUI = GameObject.Instantiate(dynamicColorTextPrefab);
            dynamicColorTextUI.init(
                colors: DynamicTextColorFactory.getRainbow(),
                positions: DynamicTextPositionFactory.getWave(),
                "CREATE",
                10
            );
            mInventoryUI.SetInteractMode(InventoryInteractMode.OverrideAction);
            selectedItem = new SerializedItemSlot("stone", 1, null);
            mInventoryUI.OverrideClickAction((button,index) =>
            {
                SerializedItemSlotEditorUI itemSlotEditorUI = GameObject.Instantiate(itemSelectorUIPrefab);
                List<ItemObject> tileItems = ItemRegistry.GetInstance().GetAllItemsOfTypeAsItem<TileItem>();
                itemSlotEditorUI.Initialize(selectedItem,Display,null,tileItems);
                CanvasController.Instance.DisplayObject(itemSlotEditorUI.gameObject,hideParent:false);
            });
            
            mInventoryUI.gameObject.SetActive(false);

            StructureGenOptionType[] options = (StructureGenOptionType[])Enum.GetValues(typeof(StructureGenOptionType));
            List<TMP_Dropdown.OptionData> dropDownOptions = new List<TMP_Dropdown.OptionData>();
            foreach (StructureGenOptionType option in options) {
                dropDownOptions.Add(new TMP_Dropdown.OptionData(option.ToString()));
            }
            optionDropdown.options = dropDownOptions;
            optionDropdown.onValueChanged.AddListener((int index) => {
                genOption = (StructureGenOptionType) index;
                mInventoryUI.gameObject.SetActive(index!=0);
            });
            
            dynamicTextContainer.localScale = new Vector3(1.5f,1.5f,1f);
            dynamicColorTextUI.transform.SetParent(dynamicTextContainer.transform,false);
            backButton.onClick.AddListener(() => {
                GameObject.Destroy(gameObject);
            });
            createButton.onClick.AddListener(() => {
                if (nameField.text.Length == 0) {
                    nameField.placeholder.color = Color.red;
                    return;
                }
                StructureGenerationOption generationOption = StructureGeneratorOptionFactory.createOption(genOption,selectedItem.id);
                StructureGeneratorHelper.NewStructure(nameField.text,generationOption,bounds);
                controllerUI.DisplayList();
                GameObject.Destroy(gameObject);
            });

            editIntervalVectorButton.onClick.AddListener(() => {
                IntervalVectorUI intervalVectorUI = GameObject.Instantiate(intervalVectorUIPrefab);
                intervalVectorUI.transform.SetParent(transform.parent,false);
                intervalVectorUI.display(bounds,ReloadIntervalVector);
            });
            Display(selectedItem);

        }

        public void ReloadIntervalVector() {
            boundsText.text = bounds.ToString();
        }

        public void Display(SerializedItemSlot serializedItemSlot)
        {
            selectedItem = serializedItemSlot;
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(serializedItemSlot);
            mInventoryUI.DisplayInventory(new List<ItemSlot>{itemSlot},clear:false);
        }
    }
}

