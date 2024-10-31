using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UI;
using WorldModule.Caves;
using Items;

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
        private StructureDevControllerUI controllerUI;
        private SerializedItemSlot serializedItemSlot;

        public void init(StructureDevControllerUI controllerUI) {
            this.controllerUI = controllerUI;
            DynamicColorTextUI dynamicColorTextUI = GameObject.Instantiate(dynamicColorTextPrefab);
            dynamicColorTextUI.init(
                colors: DynamicTextColorFactory.getRainbow(),
                positions: DynamicTextPositionFactory.getWave(),
                "CREATE",
                10
            );
            
            
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
                
                StructureGeneratorHelper.newStructure(nameField.text,serializedItemSlot.id);
                controllerUI.displayList();
                GameObject.Destroy(gameObject);
            });

            serializedItemSlot = new SerializedItemSlot(null,0,null);

            tileSelector.onClick.AddListener(() => {
                SerializedItemSlotEditorUI itemSlotEditorUI = GameObject.Instantiate(itemSelectorUIPrefab);
                itemSlotEditorUI.transform.SetParent(transform.parent,false);
                List<SerializedItemSlot> itemSlots = new List<SerializedItemSlot>{serializedItemSlot};
                itemSlotEditorUI.init(itemSlots,0,this,gameObject);
            });
            StartCoroutine(ItemRegistry.loadItems());
        }

        public static NewStructurePopUpUI newInstance() {
            return AddressableLoader.getPrefabComponentInstantly<NewStructurePopUpUI>("Assets/UI/DevTools/Structure/NewStructurePopUp.prefab");
        }

        public void reload()
        {
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            ItemObject itemObject = itemRegistry.getItemObject(serializedItemSlot.id);
            Image image = tileSelector.GetComponentInChildren<Image>();
            TextMeshProUGUI text = tileSelector.GetComponentInChildren<TextMeshProUGUI>();
            if (itemObject == null) {
                image.sprite = null;
                text.text = "Default Tile:\nNull";
            } else {
                image.sprite = itemObject.getSprite();
                text.text = $"Default Tile:\n{itemObject.name}";
            }
            
        }

        public void reloadAll()
        {
            reload();
        }
    }
}

