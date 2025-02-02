using System.Collections.Generic;
using Conduit.Filter;
using Conduits.Ports;
using Item.Slot;
using Items.Tags;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Item.Tags
{
    public static class ItemSlotTagEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <note>Itemslot should already be null checked</note>
        /// <param name="itemSlot"></param>
        public static void EditItemTag(ItemSlot itemSlot)
        {
            List<ItemTag> tags = itemSlot?.itemObject.ApplyableTags;
            if (tags == null) return;
            
            
            // TODO unhard code this if more item tags are editable 
            if (tags.Contains(ItemTag.ItemFilter))
            {
                Addressables.LoadAssetAsync<GameObject>(ItemFilterUI.ADDRESSABLE_PATH).Completed += (handle) => OnAssetLoaded(handle, itemSlot);
            }
        }

        private static void OnAssetLoaded(AsyncOperationHandle<GameObject> handle, ItemSlot itemSlot)
        {
            // Semi scuffed
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject clone = GameObject.Instantiate(handle.Result);
                ItemFilterUI filterUI = clone.GetComponent<ItemFilterUI>();
                bool nullTag = ItemSlotUtils.BuildTagDictIfNull(itemSlot);
                if (nullTag)
                {
                    itemSlot.tags.Dict[ItemTag.ItemFilter] = new ItemFilter();
                }
                else
                {
                    itemSlot.tags.Dict[ItemTag.ItemFilter] ??= new ItemFilter();
                }
                filterUI.Initialize(itemSlot.tags.Dict[ItemTag.ItemFilter] as ItemFilter);
                MainCanvasController.TInstance.DisplayUIWithPlayerInventory(filterUI.gameObject);

            }
            else
            {
                Debug.LogError("Failed to load asset: " + handle.Status);
            }
            Addressables.Release(handle);
        }
    }
}
