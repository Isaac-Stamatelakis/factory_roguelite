using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UI;
using WorldModule.Caves;
using Items;
using System;

namespace DevTools.Structures {
    public class NewStructurePopUpUI : MonoBehaviour, IItemListReloadable
    {
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private Button backButton;
        [SerializeField] private Button createButton;
        [SerializeField] private Transform dynamicTextContainer;
        [SerializeField] private DynamicColorTextUI dynamicColorTextPrefab;
        [SerializeField] private Button tileSelector;
        [SerializeField] private SerializedItemSlotEditorUI itemSelectorUIPrefab;
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Dropdown optionDropdown;
        [SerializeField] private TextMeshProUGUI boundsText;
        [SerializeField] private IntervalVectorUI intervalVectorUIPrefab;
        [SerializeField] private Button editIntervalVectorButton;
        private StructureDevControllerUI controllerUI;
        // This only has one but is required to be a list to use the editor
        private List<SerializedItemSlot> itemSlots;
        private IntervalVector bounds;
        private StructureGenOptionType genOption = StructureGenOptionType.Empty;

        public void init(StructureDevControllerUI controllerUI) {
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
            tileSelector.gameObject.SetActive(false);

            StructureGenOptionType[] options = (StructureGenOptionType[])Enum.GetValues(typeof(StructureGenOptionType));
            List<TMP_Dropdown.OptionData> dropDownOptions = new List<TMP_Dropdown.OptionData>();
            foreach (StructureGenOptionType option in options) {
                dropDownOptions.Add(new TMP_Dropdown.OptionData(option.ToString()));
            }
            optionDropdown.options = dropDownOptions;
            optionDropdown.onValueChanged.AddListener((int index) => {
                genOption = (StructureGenOptionType) index;
                tileSelector.gameObject.SetActive(index!=0);
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
                StructureGenerationOption generationOption = StructureGeneratorOptionFactory.createOption(genOption,itemSlots[0].id);
                StructureGeneratorHelper.newStructure(nameField.text,generationOption,bounds);
                controllerUI.displayList();
                GameObject.Destroy(gameObject);
            });
            itemSlots = new List<SerializedItemSlot>{new SerializedItemSlot("stone",0,null)};
            
            tileSelector.onClick.AddListener(() => {
                SerializedItemSlotEditorUI itemSlotEditorUI = GameObject.Instantiate(itemSelectorUIPrefab);
                itemSlotEditorUI.transform.SetParent(transform.parent,false);
                itemSlotEditorUI.Init(itemSlots,0,this,gameObject,displayAmount:false,displayArrows:false,displayTags:false);
            });

            editIntervalVectorButton.onClick.AddListener(() => {
                IntervalVectorUI intervalVectorUI = GameObject.Instantiate(intervalVectorUIPrefab);
                intervalVectorUI.transform.SetParent(transform.parent,false);
                intervalVectorUI.display(bounds,reloadIntervalVector);
            });
            
        }

        public void reloadIntervalVector() {
            boundsText.text = bounds.ToString();
        }

        public void reload()
        {
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            ItemObject itemObject = itemRegistry.GetItemObject(itemSlots[0].id);
            if (itemObject == null) {
                itemImage.gameObject.SetActive(false);
            } else {
                itemImage.gameObject.SetActive(true);
                itemImage.sprite = itemObject.getSprite();
            }
            
        }

        public void reloadAll()
        {
            reload();
        }
    }
}

