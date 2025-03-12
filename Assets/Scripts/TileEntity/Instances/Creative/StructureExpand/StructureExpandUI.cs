using System;
using System.Collections.Generic;
using Item.GrabbedItem;
using Item.Slot;
using Items;
using Items.Inventory;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace TileEntity.Instances.Creative.CreativeChest
{
    public class StructureExpandUI : MonoBehaviour, ITileEntityUI<StructureExpandInstance>
    {

        [SerializeField] private Button mSelectSerializedItemUI;
        [SerializeField] private ItemSlotUI mItemSlotUI;
        [SerializeField] private TMP_InputField mMinSizeField;
        [SerializeField] private TMP_InputField mMaxSizeField;
        [SerializeField] private Button mBackButton;
        [SerializeField] private SerializedItemSlotEditorUI serializedItemSlotEditorUIPrefab;
        private StructureExpandInstance structureExpandInstance;
        public void DisplayTileEntityInstance(StructureExpandInstance tileEntityInstance)
        {
            this.structureExpandInstance = tileEntityInstance;
            LoadItemSlot();
            mBackButton.onClick.AddListener(() =>
            {
                CanvasController.Instance.PopStack();
            });
            mSelectSerializedItemUI.onClick.AddListener(() =>
            {
                SerializedItemSlotEditorUI serializedItemSlotEditorUI = GameObject.Instantiate(serializedItemSlotEditorUIPrefab, transform, false);
                List<SerializedItemSlot> serializedItemSlots = new List<SerializedItemSlot>{new(tileEntityInstance.StructureExpandData.Id,1,null)};
                serializedItemSlotEditorUI.Init(serializedItemSlots,0,null,gameObject,callback:CallBack,displayAmount:false,displayTags:false,displayArrows:false);
            });
            mMinSizeField.text = tileEntityInstance.StructureExpandData.MinSize.ToString();
            mMaxSizeField.text = tileEntityInstance.StructureExpandData.MaxSize.ToString();
            
            mMinSizeField.onValueChanged.AddListener((value) =>
            {
                try
                {
                    tileEntityInstance.StructureExpandData.MinSize = System.Convert.ToInt32(value);
                }
                catch (Exception e) when (e is FormatException or OverflowException)
                {
                    mMinSizeField.text = tileEntityInstance.StructureExpandData.MinSize.ToString();
                }
            });
            
            mMaxSizeField.onValueChanged.AddListener((value) =>
            {
                try
                {
                    tileEntityInstance.StructureExpandData.MaxSize = System.Convert.ToInt32(value);
                }
                catch (Exception e) when (e is FormatException or OverflowException)
                {
                    mMaxSizeField.text = tileEntityInstance.StructureExpandData.MaxSize.ToString();
                }
               
            });
        }
        

        private void LoadItemSlot()
        {
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            ItemSlot itemSlot = new ItemSlot(itemRegistry.GetItemObject(structureExpandInstance.StructureExpandData.Id), 1, null);
            mItemSlotUI.Display(itemSlot);
        }

        private void CallBack(SerializedItemSlot serializedItemSlot)
        {
            structureExpandInstance.StructureExpandData.Id = serializedItemSlot?.id;
            LoadItemSlot();
        }
        
    }
}
