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
    public class StructureExpandUI : MonoBehaviour, ITileEntityUI
    {

        [SerializeField] private Button mSelectSerializedItemUI;
        [SerializeField] private ItemSlotUI mItemSlotUI;
        [SerializeField] private TMP_InputField mMaxSizeField;
        [SerializeField] private Button mBackButton;
        [SerializeField] private SerializedItemSlotEditorUI serializedItemSlotEditorUIPrefab;
        private StructureExpandInstance structureExpandInstance;
        public void DisplayTileEntityInstance(ITileEntityInstance tileEntityInstance)
        {
            if (tileEntityInstance is not StructureExpandInstance structureExpand) return;
            this.structureExpandInstance = structureExpand;
            LoadItemSlot();
            mBackButton.onClick.AddListener(() =>
            {
                CanvasController.Instance.PopStack();
            });
            mSelectSerializedItemUI.onClick.AddListener(() =>
            {
                SerializedItemSlotEditorUI serializedItemSlotEditorUI = GameObject.Instantiate(serializedItemSlotEditorUIPrefab);
                SerializedItemSlot serializedItemSlot = new(structureExpandInstance.StructureExpandData.Id, 1, null);
                SerializedItemSlotEditorParameters parameters = new SerializedItemSlotEditorParameters
                {
                    OnValueChange = CallBack
                };
                serializedItemSlotEditorUI.Initialize(serializedItemSlot,parameters);
                CanvasController.Instance.DisplayObject(serializedItemSlotEditorUI.gameObject,hideParent:false);
            });
           
            mMaxSizeField.text = structureExpandInstance.StructureExpandData.MaxSize.ToString();
            
            mMaxSizeField.onValueChanged.AddListener((value) =>
            {
                try
                {
                    structureExpandInstance.StructureExpandData.MaxSize = System.Convert.ToInt32(value);
                }
                catch (Exception e) when (e is FormatException or OverflowException)
                {
                    mMaxSizeField.text = structureExpandInstance.StructureExpandData.MaxSize.ToString();
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
