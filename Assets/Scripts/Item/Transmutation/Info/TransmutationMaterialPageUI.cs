using System;
using System.Collections.Generic;
using Item.Slot;
using Items;
using Items.Inventory;
using Items.Transmutable;
using TMPro;
using UnityEngine;

namespace Item.Transmutation.Info
{
    public class TransmutationMaterialPageUI : MonoBehaviour, ITransmutationMaterialPageUI
    {
        [SerializeField] private InventoryUI mDefaultStateInventory;
        [SerializeField] private TextMeshProUGUI mNameText;
        [SerializeField] private TextMeshProUGUI mTierText;
        [SerializeField] private TextMeshProUGUI mChemFormulaText;
        [SerializeField] private InventoryUI mStateInventory;
        
        public void Display(TransmutationMaterialInfo materialInfo)
        {
            var material = materialInfo.Material;
            mNameText.text = $"Name: {material.name}";
            mTierText.text = !material.gameStageObject ? $"Tier: None" : $"Tier: {material.gameStageObject.Tier}";
            
            mChemFormulaText.text = $"Chemical Formula: {TransmutableItemUtils.FormatChemicalFormula(material.chemicalFormula)}";
            TransmutableItemState defaultState = material.MaterialOptions.BaseState;
            ItemObject defaultStateItem = TransmutableItemUtils.GetMaterialItem(material, defaultState);
            ItemSlot defaultStateItemSlot = new ItemSlot(defaultStateItem,1,null);
            mDefaultStateInventory.DisplayInventory(new List<ItemSlot>{defaultStateItemSlot},clear:false);
            mDefaultStateInventory.SetInteractMode(InventoryInteractMode.Recipe);
            
            List<ItemSlot> stateInventory = new List<ItemSlot>();
            foreach (var stateOptions in material.MaterialOptions.States)
            {
                if (stateOptions.state == material.MaterialOptions.BaseState) continue;
                ItemObject stateItem = TransmutableItemUtils.GetMaterialItem(material, stateOptions.state);
                ItemSlot stateItemSlot = new ItemSlot(stateItem,1,null);
                stateInventory.Add(stateItemSlot);
            }
            mStateInventory.DisplayInventory(stateInventory);
            mStateInventory.SetInteractMode(InventoryInteractMode.Recipe);
        }
    }
}
