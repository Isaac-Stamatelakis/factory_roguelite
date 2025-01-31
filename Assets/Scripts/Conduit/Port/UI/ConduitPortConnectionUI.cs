using System.Collections.Generic;
using Conduits;
using Conduits.Ports;
using Conduits.Ports.UI;
using Conduits.Systems;
using Item.Slot;
using Items;
using Items.Inventory;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;

namespace Conduit.Port.UI
{
    public class ConduitPortConnectionUI : MonoBehaviour, IAmountIteratorListener
    {
        [SerializeField] private Button toggleActiveButton;
        [SerializeField] private PortColorButton colorButton;
        [SerializeField] private TextMeshProUGUI connectionText;
        [SerializeField] private Image disabledCover;
        [SerializeField] private AmountIteratorUI priorityIterator;
        [SerializeField] private TextMeshProUGUI priorityText;
        [SerializeField] private Transform colorButtonContainer;
        [SerializeField] private Image roundRobinImage;
        [SerializeField] private Button roundRobinButton;
        [SerializeField] private InventoryUI mFilterInventory;
        [SerializeField] private InventoryUI mUpgradeInventory;
        private IPortConduit displayedConduit;
        private ConduitPortData portConduitData;
        public void Display(IPortConduit conduit, ConduitPortData portData, PortConnectionType connectionType)
        {
            portConduitData = portData;
            priorityIterator.gameObject.SetActive(false);
            mFilterInventory.gameObject.SetActive(false);
            mUpgradeInventory.gameObject.SetActive(false);
            roundRobinImage.gameObject.SetActive(false);
            connectionText.text = connectionType.ToString();
            if (portData == null)
            {
                colorButtonContainer.gameObject.SetActive(false);
                SetEnabledImage(false);
                disabledCover.gameObject.SetActive(true);
                return;
            }
            displayedConduit = conduit;
            SetEnabledImage(portData.Enabled);
            toggleActiveButton.onClick.AddListener(()=>
            {
                portData.Enabled = !portData.Enabled;
                SetEnabledImage(portData.Enabled);
                conduit.GetConduitSystem().Rebuild(); // TODO make this more efficent
            });
            colorButton.Initialize(portData,displayedConduit);
            if (portData is PriorityConduitPortData priorityConduitPortData)
            {
                priorityIterator.gameObject.SetActive(true);
                SetPriorityText(priorityConduitPortData.Priority);
            }

            if (portData is ItemConduitInputPortData itemConduitInputPortData)
            {
                mFilterInventory.gameObject.SetActive(true);
                // TODO set filter
            }

            if (portData is ItemConduitOutputPortData itemConduitOutputPortData)
            {
                mUpgradeInventory.gameObject.SetActive(true);
                // All we have to consider is the number of upgrades in the slot
                const string upgradeId = "conduit_speed_upgrade";
                ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(upgradeId);
                ItemSlot itemSlot = new ItemSlot(itemObject, itemConduitOutputPortData.SpeedUpgrades, null);
                
                mUpgradeInventory.DisplayInventory(new List<ItemSlot>{itemSlot});
                mUpgradeInventory.SetItemRestriction(itemObject);
                
                mUpgradeInventory.AddCallback(OnUpgradeInventoryChange);
                // TODO set filter
                roundRobinImage.gameObject.SetActive(true);
                roundRobinButton.onClick.AddListener(() =>
                {
                    itemConduitOutputPortData.RoundRobin = !itemConduitOutputPortData.RoundRobin;
                    displayedConduit.GetConduitSystem().Rebuild(); // TODO make this more efficent
                });
            }
        }

        private void OnUpgradeInventoryChange(int index)
        {
            var itemConduitPortData = (ItemConduitOutputPortData)portConduitData;
            ItemSlot speedSlot = mUpgradeInventory.GetInventory()[index];
            itemConduitPortData.SpeedUpgrades = ItemSlotUtils.IsItemSlotNull(speedSlot) ? 0 : speedSlot.amount;
            
        }
        
        

        private void SetPriorityText(int priority)
        {
            priorityText.text = $"Priority: {priority.ToString()}";
        }

        private void SetEnabledImage(bool isEnabled)
        {
            toggleActiveButton.GetComponent<Image>().color = isEnabled ? Color.white : Color.red;
        }
        
        public void iterate(int amount)
        {
            PriorityConduitPortData priorityConduitPortData = portConduitData as PriorityConduitPortData;
            if (priorityConduitPortData == null) return;
            priorityConduitPortData.Priority += amount;
            SetPriorityText(priorityConduitPortData.Priority);
            displayedConduit.GetConduitSystem().Rebuild(); // TODO make this more efficent
        }
    }
}
