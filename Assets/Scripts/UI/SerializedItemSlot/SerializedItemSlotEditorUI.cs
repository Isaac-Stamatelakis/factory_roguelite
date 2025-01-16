using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Items;
using Items.Inventory;
using System;
using Item.Slot;

namespace UI {

    public interface IItemListReloadable {
        public void reload();
        public void reloadAll();
    }
    public class SerializedItemSlotEditorUI : MonoBehaviour
    {
        [SerializeField] private ItemSlotUI itemSlotUIPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private GridLayoutGroup itemSearchResultContainer;
        [SerializeField] private TMP_InputField itemSearch;
        [SerializeField] private TMP_InputField tagInput;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Button upButton;
        [SerializeField] private Button downButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Image deleteButtonPanel;
        [SerializeField] private TMP_InputField amountField;
        [SerializeField] private Image selectedItemImage;
        private static readonly int ROWS = 7;
        private static readonly int INITAL_COLUMNS = 5;
        private int size = ROWS*INITAL_COLUMNS;
        private float initalSize;
        private SerializedItemSlot SerializedItemSlot {get => itemSlots[index];}
        private IItemListReloadable container;
        private List<SerializedItemSlot> itemSlots;
        private int index;
        private List<ItemObject> currentItems = new List<ItemObject>();
        private IItemListReloadable reloadable;
        private float timeSinceLastDeletePress = 2f;
        private Type itemType;
        private Action<SerializedItemSlot> callback;
        public void Init(
            List<SerializedItemSlot> serializedItemSlots, int index, IItemListReloadable reloadable, GameObject goBackTo, 
            Type itemType = null, bool displayTags = true, bool displayArrows = true, bool displayAmount = true, bool displayTrash = true, Action<SerializedItemSlot> callback = null
        ) {
            this.itemSlots = serializedItemSlots;
            this.initalSize = scrollRect.content.sizeDelta.y;
            this.index = index;
            this.reloadable = reloadable;
            this.itemType = itemType;
            this.callback = callback;

            upButton.gameObject.SetActive(displayArrows);
            downButton.gameObject.SetActive(displayArrows);
            tagInput.gameObject.SetActive(displayTags);
            amountField.gameObject.SetActive(displayAmount);
            deleteButton.gameObject.SetActive(displayTrash);
            SetImage();
            
            backButton.onClick.AddListener(() => {
                if (gameObject != null) {
                    GameObject.Destroy(gameObject);
                }
                if (goBackTo != null) {
                    goBackTo.SetActive(true);
                }
                
            });
            LoadItems("");
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            scrollRect.verticalNormalizedPosition=1f;
            
            scrollRect.onValueChanged.AddListener((Vector2 value) => {
                
                if (value.y <= 0.05f && size < currentItems.Count) {
                    loadOneMoreRow();
                    Vector2 contentSize = scrollRect.content.sizeDelta;
                    scrollRect.verticalNormalizedPosition += 100f/contentSize.y;
                }
            });
            itemSearch.onValueChanged.AddListener(LoadItems);
            tagInput.onValueChanged.AddListener((string value) => {
                SerializedItemSlot.tags = value;
            });
            amountField.text = SerializedItemSlot.amount.ToString();
            amountField.onValueChanged.AddListener((string value) => {
                if (value.Length == 0) {
                    return;
                }
                try {
                    uint amount = System.Convert.ToUInt32(value);
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
                (itemSlots[index], itemSlots[newIndex]) = (itemSlots[newIndex], itemSlots[index]);
                index = newIndex;
                reloadable.reloadAll();
            });
            downButton.onClick.AddListener(() => {
                int newIndex = Global.modInt(index+1,itemSlots.Count);
                (itemSlots[index], itemSlots[newIndex]) = (itemSlots[newIndex], itemSlots[index]);
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

        private void SetImage() {
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(SerializedItemSlot);
            if (itemSlot.itemObject != null) {
                selectedItemImage.sprite = itemSlot.itemObject.getSprite();
            } else {
                selectedItemImage.sprite = null;
            }
            
        }
        public void SelectItem(ItemObject itemObject) {
            SerializedItemSlot serializedItemSlot = new SerializedItemSlot(
                itemObject.id,
                itemSlots[index].amount,
                itemSlots[index].tags
            );
            itemSlots[index] = serializedItemSlot;
            callback?.Invoke(serializedItemSlot);
            reloadable?.reload();
            SetImage();
        }

        private void loadOneMoreRow() {
            for (int i = 0; i < ROWS; i++) {
                if (size + i >= currentItems.Count) {
                    break;
                }
                ItemObject itemObject = currentItems[size+i];
                size ++;
                ItemSlotUI itemSlotUI = Instantiate(itemSlotUIPrefab, itemSearchResultContainer.transform);
                ItemSlot itemSlot = new ItemSlot(itemObject, 1,null);
                itemSlotUI.Display(itemSlot);
                SerializedItemSlotEditItemPanel editItemPanel = itemSlotUI.gameObject.AddComponent<SerializedItemSlotEditItemPanel>();
                editItemPanel.init(SerializedItemSlot,this,itemObject);
            }
        }
        private void LoadItems(string value) {
            if (itemType != null) {
                
            } else {
                currentItems = ItemRegistry.GetInstance().Query(value,int.MaxValue);
            }
            
            GlobalHelper.deleteAllChildren(itemSearchResultContainer.transform);
            Vector2 contentSize = scrollRect.content.sizeDelta;
            contentSize.y = initalSize;
            scrollRect.verticalNormalizedPosition = 1f;
            scrollRect.content.sizeDelta = contentSize;
            for (int i = 0; i < Mathf.Min(currentItems.Count,size); i++) {
                ItemObject itemObject = currentItems[i];
                ItemSlotUI itemSlotUI = Instantiate(itemSlotUIPrefab, itemSearchResultContainer.transform);
                itemSlotUI.Display(new ItemSlot(itemObject,1,null));
                SerializedItemSlotEditItemPanel editItemPanel = itemSlotUI.gameObject.AddComponent<SerializedItemSlotEditItemPanel>();
                editItemPanel.init(SerializedItemSlot,this,itemObject);
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
    }
    
}

