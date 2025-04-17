using System;
using System.Collections.Generic;
using Items;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ItemEditor
{
    public class SlotValueEditorUI : MonoBehaviour
    {
        [SerializeField] private Button upButton;
        [SerializeField] private Button downButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private TMP_InputField tagInput;
        [SerializeField] private TMP_InputField amountField;
        [SerializeField] private ItemSlotUI itemSlotUI;
        private Action changeCallback;
        private Action listChangeCallback;
        private int index;
        
        public void DisplayArrows(List<SerializedItemSlot> itemSlots, int itemIndex)
        {
            index = itemIndex;
            upButton.onClick.AddListener(() => {
                int newIndex = Global.ModInt(index-1,itemSlots.Count);
                (itemSlots[index], itemSlots[newIndex]) = (itemSlots[newIndex], itemSlots[index]);
                index = newIndex;
                listChangeCallback.Invoke();
            });
            downButton.onClick.AddListener(() => {
                int newIndex = Global.ModInt(index+1,itemSlots.Count);
                (itemSlots[index], itemSlots[newIndex]) = (itemSlots[newIndex], itemSlots[index]);
                index = newIndex;
                listChangeCallback.Invoke();
            });
        }

        public void Display(SerializedItemSlot serializedItemSlot, DisplayParameters parameters)
        {
            this.changeCallback = parameters.ChangeCallback;
            this.listChangeCallback = parameters.ListChangeCallback;
            upButton.gameObject.SetActive(parameters.EnableArrows);
            downButton.gameObject.SetActive(parameters.EnableArrows);
            tagInput.gameObject.SetActive(parameters.DisplayTags);
            amountField.gameObject.SetActive(parameters.DisplayAmount);
            deleteButton.gameObject.SetActive(parameters.EnableDelete && parameters.ItemSlots != null);
            
            tagInput.onValueChanged.AddListener((string value) => {
                serializedItemSlot.tags = value;
            });
            amountField.text = serializedItemSlot?.amount.ToString();
            amountField.onValueChanged.AddListener(OnAmountChange);
            
            deleteButton.onClick.AddListener(() =>
            {
                parameters.ItemSlots.RemoveAt(parameters.Index);
                listChangeCallback.Invoke();
                CanvasController.Instance.PopStack();
            });
             
            return;
            void OnAmountChange(string value)
            {
                if (serializedItemSlot == null) return;
                if (value.Length == 0) {
                    return;
                }
                try
                {
                    uint amount = System.Convert.ToUInt32(value);
                    if (amount <= 0) {
                        amountField.text = "";
                        return;
                    }
                    if (amount >= 10000) {
                        amountField.text = "9999";
                        serializedItemSlot.amount = 9999;
                    } else {
                        serializedItemSlot.amount = amount;
                    }
                    changeCallback?.Invoke();
                } catch (FormatException) {
                    if (value.Length <= 1) {
                        amountField.text = "";
                    } else {
                        amountField.text = serializedItemSlot.amount.ToString();
                    }
                }
            }
        }
    }
}
