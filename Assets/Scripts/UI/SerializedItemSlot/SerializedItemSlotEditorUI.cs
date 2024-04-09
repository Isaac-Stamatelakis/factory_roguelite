using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ItemModule;
using ItemModule.Inventory;
using System;

namespace UI {

    public interface IItemListReloadable {
        public void reload();
        public void reloadAll();
    }
    public class SerializedItemSlotEditorUI : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private GridLayoutGroup itemSearchResultContainer;
        [SerializeField] private TMP_InputField itemSearch;
        [SerializeField] private TMP_InputField tagInput;
        [SerializeField] private Image selectedImage;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Button upButton;
        [SerializeField] private Button downButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Image deleteButtonPanel;
        [SerializeField] private TMP_InputField amountField;
        private int size = 7*5;
        private float initalSize;
        private SerializedItemSlot SerializedItemSlot {get => itemSlots[index];}
        private IItemListReloadable container;
        private List<SerializedItemSlot> itemSlots;
        private int index;

        private List<ItemObject> currentItems = new List<ItemObject>();
        private IItemListReloadable reloadable;
        private float timeSinceLastDeletePress = 2f;
        public void init(List<SerializedItemSlot> serializedItemSlots, int index, IItemListReloadable reloadable, GameObject goBackTo) {
            this.itemSlots = serializedItemSlots;
            this.initalSize = scrollRect.content.sizeDelta.y;
            this.index = index;
            this.reloadable = reloadable;
            setImage();
            
            backButton.onClick.AddListener(() => {
                if (gameObject != null) {
                    GameObject.Destroy(gameObject);
                }
                if (goBackTo != null) {
                    goBackTo.SetActive(true);
                }
                
            });
            loadItems("");
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            scrollRect.verticalNormalizedPosition=1f;
            
            scrollRect.onValueChanged.AddListener((Vector2 value) => {
                
                if (value.y <= 0.05f && size < currentItems.Count) {
                    loadOneMoreRow();
                    Vector2 contentSize = scrollRect.content.sizeDelta;
                    scrollRect.verticalNormalizedPosition += 100f/contentSize.y;
                }
            });
            
            itemSearch.onValueChanged.AddListener((string value) => {
                loadItems(value);
            });
            tagInput.onValueChanged.AddListener((string value) => {
                SerializedItemSlot.tags = value;
            });
            amountField.text = SerializedItemSlot.amount.ToString();
            amountField.onValueChanged.AddListener((string value) => {
                if (value.Length == 0) {
                    return;
                }
                try {
                    int amount = System.Convert.ToInt32(value);
                    if (amount <= 0) {
                        amountField.text = "";
                        return;
                    }
                    if (amount >= 10000) {
                        amountField.text = "9999";
                        SerializedItemSlot.amount = 9999;
                    } else {
                        SerializedItemSlot.amount = amount;
                    }
                    reloadable.reload();
                } catch (FormatException) {
                    if (value.Length <= 1) {
                        amountField.text = "";
                    } else {
                        amountField.text = SerializedItemSlot.amount.ToString();
                    }
                }
            });
            upButton.onClick.AddListener(() => {
                int newIndex = Global.modInt(index-1,itemSlots.Count);
                SerializedItemSlot swap = itemSlots[index];
                itemSlots[index] = itemSlots[newIndex];
                itemSlots[newIndex] = swap;
                index = newIndex;
                reloadable.reloadAll();
            });
            downButton.onClick.AddListener(() => {
                int newIndex = Global.modInt(index+1,itemSlots.Count);
                SerializedItemSlot swap = itemSlots[index];
                itemSlots[index] = itemSlots[newIndex];
                itemSlots[newIndex] = swap;
                index = newIndex;
                reloadable.reloadAll();
            });
            deleteButton.onClick.AddListener(() =>{
                if (timeSinceLastDeletePress <= 1f) {
                    itemSlots.RemoveAt(index);
                    reloadable.reloadAll();
                    GameObject.Destroy(gameObject);
                }
                timeSinceLastDeletePress = 0f;
            });
            
        }

        private void setImage() {
            ItemObject itemObject = ItemRegistry.getInstance().getItemObject(SerializedItemSlot.id);
            if (itemObject != null) {
                selectedImage.sprite = itemObject.getSprite();
                selectedImage.transform.localScale = ItemSlotUIFactory.getItemScale(selectedImage.sprite);
            }
        }
        public void selectItem(ItemObject itemObject) {
            SerializedItemSlot serializedItemSlot = new SerializedItemSlot(
                itemObject.id,
                1,
                null
            );
            itemSlots[index] = serializedItemSlot;
            reloadable.reload();
            setImage();
        }

        private void loadOneMoreRow() {
            for (int i = 0; i < 7; i++) {
                if (size + i >= currentItems.Count) {
                    break;
                }
                ItemObject itemObject = currentItems[size+i];
                size ++;
                GameObject panel = GlobalHelper.instantiateFromResourcePath("UI/SerializedItemSlot/SerializedItemSlotPanel");
                panel.transform.SetParent(itemSearchResultContainer.transform,false);
                SerializedItemSlotEditItemPanel editItemPanel = panel.GetComponent<SerializedItemSlotEditItemPanel>();
                editItemPanel.init(SerializedItemSlot,this,itemObject);
                ItemSlotUIFactory.getItemImage(itemObject,panel.transform);
            }
        }
        private void loadItems(string value) {
            currentItems = ItemRegistry.getInstance().query(value,int.MaxValue);
            GlobalHelper.deleteAllChildren(itemSearchResultContainer.transform);
            Vector2 contentSize = scrollRect.content.sizeDelta;
            contentSize.y = initalSize;
            scrollRect.verticalNormalizedPosition = 1f;
            scrollRect.content.sizeDelta = contentSize;
            for (int i = 0; i < Mathf.Min(currentItems.Count,size); i++) {
                ItemObject itemObject = currentItems[i];
                GameObject panel = GlobalHelper.instantiateFromResourcePath("UI/SerializedItemSlot/SerializedItemSlotPanel");
                panel.transform.SetParent(itemSearchResultContainer.transform,false);
                SerializedItemSlotEditItemPanel editItemPanel = panel.GetComponent<SerializedItemSlotEditItemPanel>();
                editItemPanel.init(SerializedItemSlot,this,itemObject);
                ItemSlotUIFactory.getItemImage(itemObject,panel.transform);
            }
        }

        public void Update() {
            timeSinceLastDeletePress += Time.deltaTime;
            if (timeSinceLastDeletePress <= 1f) {
                deleteButtonPanel.color = Color.red;
            } else {
                deleteButtonPanel.color = Color.white;
            }
        }

        public static SerializedItemSlotEditorUI createNewInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/SerializedItemSlot/SerializedItemSlotEditor").GetComponent<SerializedItemSlotEditorUI>();
        }
    }
    
}

