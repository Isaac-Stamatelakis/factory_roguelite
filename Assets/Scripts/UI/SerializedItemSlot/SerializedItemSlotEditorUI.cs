using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ItemModule;

namespace UI {

    public interface ISerializedItemSlotContainer {
        public void setSeralizedItemSlot(SerializedItemSlot serializedItemSlot);
    }
    public class SerializedItemSlotEditorUI : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private GridLayoutGroup itemSearchResultContainer;
        [SerializeField] private TMP_InputField itemSearch;
        [SerializeField] private TMP_InputField tagInput;
        [SerializeField] private Image selectedImage;
        private int size = 35;
        private SerializedItemSlot serializedItemSlot;
        private ISerializedItemSlotContainer container;
        public void init(SerializedItemSlot serializedItemSlot, ISerializedItemSlotContainer container, GameObject goBackTo) {
            this.serializedItemSlot = serializedItemSlot;
            this.container = container;
            setImage();

            backButton.onClick.AddListener(() => {
                GameObject.Destroy(gameObject);
                goBackTo.SetActive(true);
            });
            loadItems("");
            itemSearch.onValueChanged.AddListener((string value) => {
                loadItems(value);
            });
            tagInput.onValueChanged.AddListener((string value) => {
                serializedItemSlot.tags = value;
            });
            
        }

        private void setImage() {
            ItemObject itemObject = ItemRegistry.getInstance().getItemObject(serializedItemSlot.id);
            if (itemObject != null) {
                selectedImage.sprite = itemObject.getSprite();
            }
        }
        public void selectItem(ItemObject itemObject) {
            serializedItemSlot = new SerializedItemSlot(
                itemObject.id,
                1,
                null
            );
            container.setSeralizedItemSlot(serializedItemSlot);
            setImage();
            
        }

        private void loadItems(string value) {
            List<ItemObject> items = ItemRegistry.getInstance().query(value,size);
            GlobalHelper.deleteAllChildren(itemSearchResultContainer.transform);
            foreach (ItemObject itemObject in items) {
                GameObject panel = GlobalHelper.loadFromResourcePath("UI/SerializedItemSlot/SerializedItemSlotPanel");
                panel.transform.SetParent(itemSearchResultContainer.transform,false);
                SerializedItemSlotEditItemPanel editItemPanel = panel.GetComponent<SerializedItemSlotEditItemPanel>();
                editItemPanel.init(serializedItemSlot,this,itemObject);
                ItemSlotUIFactory.getItemImage(itemObject,panel.transform);
            }
        }

        public static SerializedItemSlotEditorUI createNewInstance() {
            return GlobalHelper.loadFromResourcePath("UI/SerializedItemSlot/SerializedItemSlotEditor").GetComponent<SerializedItemSlotEditorUI>();
        }
    }
    
}

