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

        public void Display(List<SerializedItemSlot> itemSlots, int index,
            SerializedItemSlotEditorParameters parameters)
        {
            Display(itemSlots[index],parameters);
            upButton.gameObject.SetActive(parameters.EnableArrows);
            downButton.gameObject.SetActive(parameters.EnableArrows);
            deleteButton.gameObject.SetActive(parameters.EnableDelete);
            if (parameters.EnableArrows)
            {
                downButton.onClick.AddListener(() =>
                {
                    ShiftValue(1);
                });
                upButton.onClick.AddListener(() =>
                {
                    ShiftValue(-1);
                });
            }
            deleteButton.onClick.AddListener(() =>
            {
                itemSlots.RemoveAt(index);
                listChangeCallback.Invoke();
                CanvasController.Instance.PopStack();
            });
            void ShiftValue(int dir)
            {
                int newIndex = Global.ModInt(index+1,itemSlots.Count);
                (itemSlots[index], itemSlots[newIndex]) = (itemSlots[newIndex], itemSlots[index]);
                index = newIndex;
                listChangeCallback.Invoke();
            }
        }
        public void Display(SerializedItemSlot serializedItemSlot, SerializedItemSlotEditorParameters parameters)
        {
            this.listChangeCallback = parameters.ListChangeCallback;
            
            tagInput.gameObject.SetActive(parameters.DisplayTags);
            amountField.gameObject.SetActive(parameters.DisplayAmount);
            upButton.gameObject.SetActive(false);
            downButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);
            
            tagInput.onValueChanged.AddListener((string value) => {
                serializedItemSlot.tags = value;
            });
            amountField.text = serializedItemSlot?.amount.ToString();
            amountField.onValueChanged.AddListener(OnAmountChange);
            
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
