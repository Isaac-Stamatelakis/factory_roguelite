using System.Collections.Generic;
using Conduit.Filter;
using Conduits;
using Conduits.Ports;
using Conduits.Ports.UI;
using Conduits.Systems;
using Item.GrabbedItem;
using Item.Slot;
using Items;
using Items.Inventory;
using Items.Tags;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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
        [SerializeField] private Button mEditFilterButton;
        private IPortConduit displayedConduit;
        private ConduitPortData portConduitData;
        private ItemFilterUI itemFilterUIPrefab;
        public void Display(IPortConduit conduit, ConduitPortData portData, PortConnectionType connectionType)
        {
            portConduitData = portData;
            priorityIterator.gameObject.SetActive(false);
            mFilterInventory.gameObject.SetActive(false);
            mUpgradeInventory.gameObject.SetActive(false);
            roundRobinImage.gameObject.SetActive(false);
            mEditFilterButton.gameObject.SetActive(false);
            
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
                conduit?.GetConduitSystem().Rebuild(); // TODO make this more efficent
            });
            colorButton.Initialize(portData,displayedConduit);
            if (portData is PriorityConduitPortData priorityConduitPortData)
            {
                priorityIterator.gameObject.SetActive(true);
                SetPriorityText(priorityConduitPortData.Priority);
            }

            if (conduit != null && portData is IFilterConduitPort filterConduitPort)
            {
                InitializeFilterInventory(filterConduitPort);
                mEditFilterButton.onClick.AddListener(EditFilterButtonPress);
                mEditFilterButton.gameObject.SetActive(true);
                var handle = Addressables.LoadAssetAsync<GameObject>(ItemFilterUI.ADDRESSABLE_PATH);
                handle.Completed += OnFilterPrefabLoad;
            }

            if (portData is ItemConduitOutputPortData itemConduitOutputPortData)
            {
                if (conduit != null)
                {
                    mUpgradeInventory.gameObject.SetActive(true);
                    // All we have to consider is the number of upgrades in the slot
                
                    ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(ItemTileEntityPort.UPGRADE_ID);
                    ItemSlot itemSlot = new ItemSlot(itemObject, itemConduitOutputPortData.SpeedUpgrades, null);
                
                    mUpgradeInventory.DisplayInventory(new List<ItemSlot>{itemSlot});
                    mUpgradeInventory.SetItemRestriction(itemObject);
                
                    mUpgradeInventory.AddCallback(OnUpgradeInventoryChange);
                }
                
                SetRoundRobinColor();
                roundRobinImage.gameObject.SetActive(true);
                roundRobinButton.onClick.AddListener(() =>
                {
                    
                    itemConduitOutputPortData.RoundRobin = !itemConduitOutputPortData.RoundRobin;
                    SetRoundRobinColor();
                    displayedConduit?.GetConduitSystem().Rebuild();
                });
            }
        }

        private void SetRoundRobinColor()
        {
            ItemConduitOutputPortData itemConduitOutputPortData = (ItemConduitOutputPortData)portConduitData;
            roundRobinButton.GetComponent<Image>().color = itemConduitOutputPortData.RoundRobin ? Color.green : Color.grey;
        }

        private void EditFilterButtonPress()
        {
            IFilterConduitPort filterConduitPort = (IFilterConduitPort)portConduitData;
            if (filterConduitPort.ItemFilter == null) return;
            ItemFilterUI itemFilterUI = GameObject.Instantiate(itemFilterUIPrefab);
            itemFilterUI.Initialize(filterConduitPort.ItemFilter);
            MainCanvasController.TInstance.DisplayUIWithPlayerInventory(itemFilterUI.gameObject);
        }
        
        private void OnFilterPrefabLoad(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                itemFilterUIPrefab = handle.Result.GetComponent<ItemFilterUI>();
            }
            else
            {
                Debug.LogError("Failed to load asset: " + handle.OperationException);
            }
            Addressables.Release(handle);
        }

        private void InitializeFilterInventory(IFilterConduitPort filterConduitPort)
        {
            mFilterInventory.gameObject.SetActive(true);
            
            ItemFilter filter = filterConduitPort.ItemFilter;
            ItemObject filterItem = ItemRegistry.GetInstance().GetItemObject(ItemTileEntityPort.FILTER_ID);
            ItemSlot itemSlot = filter == null
                ? null
                : new ItemSlot(filterItem, 1, null);
            mFilterInventory.DisplayInventory(new List<ItemSlot>{itemSlot}, clear: false);
            mFilterInventory.SetItemRestriction(filterItem);
            mFilterInventory.SetMaxSize(1);
            
            mFilterInventory.AddCallback(OnFilterInventoryChange);
        }
        private void OnFilterInventoryChange(int index)
        {
            IFilterConduitPort filterConduitPort = (IFilterConduitPort)portConduitData;
            ItemSlot itemFilterSlot = mFilterInventory.GetItemSlot(index);
            bool filterRemoved = ItemSlotUtils.IsItemSlotNull(itemFilterSlot);
            ItemFilter oldFilter = filterConduitPort.ItemFilter;
            ItemSlot grabbedSlot = GrabbedItemProperties.Instance.ItemSlot;
            if (filterRemoved)
            {
                ItemSlotUtils.AddTag(grabbedSlot, ItemTag.ItemFilter, oldFilter);
                filterConduitPort.ItemFilter = null;
            }
            else
            {
                if (itemFilterSlot?.tags?.Dict != null && itemFilterSlot.tags.Dict.TryGetValue(ItemTag.ItemFilter, out var tagObject))
                {
                    filterConduitPort.ItemFilter = tagObject as ItemFilter;
                }
                filterConduitPort.ItemFilter ??= new ItemFilter();
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
