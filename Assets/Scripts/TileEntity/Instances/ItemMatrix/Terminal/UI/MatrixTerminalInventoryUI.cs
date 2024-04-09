using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule.Tags.Matrix;
using ItemModule;
using ItemModule.Inventory;
using ItemModule.Tags;
using ItemModule.Tags.FluidContainers;
using System.Linq;

namespace TileEntityModule.Instances.Matrix {
    public interface IMatrixTerminalItemClickReciever {
        public void itemLeftClick(IItemSlotUIElement listener);
        public void itemRightClick(IItemSlotUIElement listener);
        public void itemMiddleClick(IItemSlotUIElement listener);
    } 

    public interface IMatrixRecipeClickReciever {
        public void rightClickRecipe(int n);
    }
    public class MatrixTerminalInventoryUI : MonoBehaviour, IMatrixTerminalItemClickReciever, IMatrixRecipeClickReciever
    {
        private ItemMatrixController controller;
        private Transform itemContainer;
        private MatrixDriveCollection matrixDriveCollection;
        private Queue<(MatrixDrive, Queue<MatrixDriveInventory>)> toRebuild;
        private Queue<MatrixDriveInventory> driveInventoryToRebuild;
        private int driveRebuildIndex;
        private Dictionary<string, Dictionary<ItemTagKey, (ItemSlot,MatrixTerminalItemSlotUI,EncodedRecipe)>> idTagItemSlotDict;
        public void init(ItemMatrixController controller, Transform itemContainer) {
            this.controller = controller;
            this.itemContainer = itemContainer;
            matrixDriveCollection = controller.getEntireDriveCollection();
            toRebuild = matrixDriveCollection.getQueueOfDrives();
            driveInventoryToRebuild = new Queue<MatrixDriveInventory>();
            GlobalHelper.deleteAllChildren(itemContainer);
            idTagItemSlotDict = new Dictionary<string, Dictionary<ItemTagKey, (ItemSlot, MatrixTerminalItemSlotUI, EncodedRecipe)>>();
            buildDict();
            createItemSlotGameObjects();
            sortItemSlots();
            // Add some extra blank slots
            for (int i = 0; i < 81; i++) {
                MatrixTerminalItemSlotUI slot = MatrixTerminalItemSlotUI.newInstance(null, this);
                slot.transform.SetParent(itemContainer, false);
            }
            
            
        }

        public void createItemSlotGameObjects() {
            foreach (var tagKeyDict in idTagItemSlotDict)
            {
                foreach (var itemSlotGameObject in tagKeyDict.Value.ToList()) // Convert to a list to avoid modifying the collection directly
                {
                    createItemSlotGameObject(itemSlotGameObject.Value,tagKeyDict.Key,itemSlotGameObject.Key);
                }
            }
        }

        private void loadItemIntoDict(ItemSlot itemSlot) {
            if (itemSlot == null || itemSlot.itemObject == null) {
                return;
            }
            if (!idTagItemSlotDict.ContainsKey(itemSlot.itemObject.id)) {
                idTagItemSlotDict[itemSlot.itemObject.id] = new Dictionary<ItemTagKey, (ItemSlot,MatrixTerminalItemSlotUI,EncodedRecipe)>();
            }
            ItemTagKey tagKey = new ItemTagKey(itemSlot.tags);
            if (idTagItemSlotDict[itemSlot.itemObject.id].ContainsKey(tagKey)) {
                idTagItemSlotDict[itemSlot.itemObject.id][tagKey].Item1.amount += itemSlot.amount;
            } else {
                idTagItemSlotDict[itemSlot.itemObject.id][tagKey] = (ItemSlotFactory.copy(itemSlot),null,null);
            }
        }
        private void createItemSlotGameObject((ItemSlot,MatrixTerminalItemSlotUI,EncodedRecipe) tuple, string id, ItemTagKey tagKey) {
            if (tuple.Item2 != null) {
                return;
            }
            if (tuple.Item1.amount <= 0) {
                return;
            }
            MatrixTerminalItemSlotUI slot = MatrixTerminalItemSlotUI.newInstance(tuple.Item1, this);
            ItemSlotUIFactory.load(tuple.Item1, slot.transform);
            var updatedValue = (tuple.Item1, slot,tuple.Item3);
            idTagItemSlotDict[id][tagKey] = updatedValue;
            slot.transform.SetParent(itemContainer, false);
        }
        public void buildDict() {
            foreach (KeyValuePair<MatrixDrive,List<MatrixDriveInventory>> kvp in matrixDriveCollection.DriveInventories) {
                for (int driveIndex = 0; driveIndex < kvp.Value.Count; driveIndex++) {
                    List<ItemSlot> matrixDriveInventory = kvp.Value[driveIndex].inventories;
                    for (int i = 0; i < matrixDriveInventory.Count; i++) {
                        loadItemIntoDict(matrixDriveInventory[i]);
                    }
                }
            }

            foreach ((string, ItemTagKey, EncodedRecipe) itemSlotRecipe in controller.Recipes.toList()) {
                string id = itemSlotRecipe.Item1;
                ItemTagKey tagKey = itemSlotRecipe.Item2;
                EncodedRecipe encodedRecipe = itemSlotRecipe.Item3;
                
            }
        }

        public void rebuildDict() {
            // Reset amounts
            foreach (KeyValuePair<string, Dictionary<ItemTagKey, (ItemSlot,MatrixTerminalItemSlotUI,EncodedRecipe)>> idDictKVP in idTagItemSlotDict) {
                foreach (KeyValuePair<ItemTagKey, (ItemSlot,MatrixTerminalItemSlotUI,EncodedRecipe)> kvp in idDictKVP.Value) {
                    kvp.Value.Item1.amount = 0;
                }
            }
            // Rebuild amounts
            buildDict();
            // Destroy items with no amount, and no recipe
            foreach (KeyValuePair<string, Dictionary<ItemTagKey, (ItemSlot,MatrixTerminalItemSlotUI,EncodedRecipe)>> idDictKVP in idTagItemSlotDict) {
                foreach (KeyValuePair<ItemTagKey, (ItemSlot,MatrixTerminalItemSlotUI,EncodedRecipe)> kvp in idDictKVP.Value) {
                    (ItemSlot, MatrixTerminalItemSlotUI, EncodedRecipe) value = kvp.Value;
                    if (canDestroy(value)) {
                        GameObject.Destroy(kvp.Value.Item2.gameObject);
                    }
                }
            }
            createItemSlotGameObjects();
            

        }

        private bool canDestroy((ItemSlot, MatrixTerminalItemSlotUI, EncodedRecipe) tuple) {
            return tuple.Item2 != null && tuple.Item1.amount <= 0 && tuple.Item3 == null;
        }
        public void FixedUpdate() {
            if (toRebuild.Count == 0) {
                matrixDriveCollection = controller.getEntireDriveCollection();
                toRebuild = matrixDriveCollection.getQueueOfDrives();
                rebuildDict();
                return;
            }
            if (driveInventoryToRebuild.Count == 0) {
                (MatrixDrive,Queue<MatrixDriveInventory>) drive = toRebuild.Dequeue();
                driveInventoryToRebuild = drive.Item2;
                return;
            }
            MatrixDriveInventory matrixDriveInventory = driveInventoryToRebuild.Peek();
            if (driveRebuildIndex >= matrixDriveInventory.inventories.Count) {
                driveInventoryToRebuild.Dequeue();
                driveRebuildIndex = 0;
                return;
            } 
            ItemSlot itemSlot = matrixDriveInventory.inventories[driveRebuildIndex];
            driveRebuildIndex++;
            if (itemSlot == null || itemSlot.itemObject == null) {
                return;
            }
            if (!idTagItemSlotDict.ContainsKey(itemSlot.itemObject.id)) {
                return;
            }
            ItemTagKey key = new ItemTagKey(itemSlot.tags);
            if (!idTagItemSlotDict[itemSlot.itemObject.id].ContainsKey(key)) {
                return;
            }
            (ItemSlot, MatrixTerminalItemSlotUI, EncodedRecipe) value = idTagItemSlotDict[itemSlot.itemObject.id][key];
            MatrixTerminalItemSlotUI slot = value.Item2;
            if (slot != null) {
                sortItemSlots();
                ((IItemSlotUIElement)slot).reload(value.Item1);
            }
            
        }

        private void sortItemSlots() {
            List<(ItemSlot,Transform)> slots = new List<(ItemSlot,Transform)>();
            for (int i = 0; i < itemContainer.childCount; i++) {
                Transform child = itemContainer.GetChild(i);
                MatrixTerminalItemSlotUI matrixTerminalItemSlotUI = child.GetComponent<MatrixTerminalItemSlotUI>();
                ItemSlot itemSlot = matrixTerminalItemSlotUI.getItemSlot();
                if (itemSlot == null || itemSlot.itemObject == null) {
                    continue;
                }
                slots.Add((itemSlot,child));
            }
            slots.Sort((b, a) =>
            {
                int amountComparison = a.Item1.amount.CompareTo(b.Item1.amount);
                if (amountComparison == 0)
                {
                    // If amounts are equal, sort by name
                    return string.Compare(a.Item1.itemObject.name, b.Item1.itemObject.name);
                }
                return amountComparison;
            });
            for (int i = 0; i < slots.Count; i++) {
                (ItemSlot, Transform) value = slots[i];
                value.Item2.SetSiblingIndex(i);
            }
        }

        private bool itemInDict(ItemSlot itemSlot) {
            if (itemSlot == null || itemSlot.itemObject == null) {
                return false;
            }
            return itemInDict(itemSlot.itemObject.id,new ItemTagKey(itemSlot.tags));
        }
        private bool itemInDict(string id, ItemTagKey tagKey) {
            return idTagItemSlotDict.ContainsKey(id) && idTagItemSlotDict[id].ContainsKey(tagKey);
        }

        private void leftClickInsert(GrabbedItemProperties grabbedItemProperties, ItemSlot grabbedSlot, IItemSlotUIElement slotUIElement) {
            int preAmount = grabbedSlot.amount;
            ItemSlot grabbedItemCopy = ItemSlotFactory.copy(grabbedSlot);
            matrixDriveCollection.send(grabbedSlot);
            grabbedItemCopy.amount -= grabbedSlot.amount;
            if (grabbedItemCopy.amount == 0) { // Was not inserted
                return;
            }
            grabbedItemProperties.updateSprite();
            string grabbedSlotId = grabbedItemCopy.itemObject.id;
            ItemTagKey grabbedSlotTagKey = new ItemTagKey(grabbedItemCopy.tags); 
            loadItemIntoDict(grabbedItemCopy);
            (ItemSlot, MatrixTerminalItemSlotUI, EncodedRecipe) tuple = idTagItemSlotDict[grabbedSlotId][grabbedSlotTagKey];
            if (tuple.Item2 == null) {
                createItemSlotGameObject(tuple,grabbedSlotId,grabbedSlotTagKey);
            } else {
                ((IItemSlotUIElement)tuple.Item2).reload(grabbedItemCopy);
            }
            sortItemSlots();
        }

        private void leftClickExtract(ItemSlot inventorySlot,GrabbedItemProperties grabbedItemProperties, IItemSlotUIElement slotUIElement) {
            if (inventorySlot == null || inventorySlot.itemObject == null) {
                return;
            }
            ItemTagKey itemTagKey = new ItemTagKey(inventorySlot.tags);
            grabbedItemProperties.itemSlot = matrixDriveCollection.take(inventorySlot.itemObject.id,itemTagKey,Global.MaxSize);
            inventorySlot.amount -= grabbedItemProperties.itemSlot.amount;
            (ItemSlot, MatrixTerminalItemSlotUI, EncodedRecipe) tuple = idTagItemSlotDict[inventorySlot.itemObject.id][itemTagKey];
            if (canDestroy(tuple)) {
                GameObject.Destroy(tuple.Item2.gameObject);
            } else {
                slotUIElement.reload(inventorySlot);
                sortItemSlots();
            }
            
            grabbedItemProperties.updateSprite();
     
        }
        public void itemLeftClick(IItemSlotUIElement slotUIElement)
        {
            GrabbedItemProperties grabbedItemProperties = GrabbedItemContainer.getGrabbedItem();
            ItemSlot grabbedSlot = grabbedItemProperties.itemSlot;
            ItemSlot inventorySlot = slotUIElement.getItemSlot();
            if (grabbedSlot == null || grabbedSlot.itemObject == null) {
                leftClickExtract(inventorySlot,grabbedItemProperties,slotUIElement);
            } else {
                leftClickInsert(grabbedItemProperties,grabbedSlot,slotUIElement);
            }

            
            
            /*
            MatrixDrive matrixDrive = listener.getMatrixDrive();
            int n = listener.getIndex();
            int driveIndex = listener.getDriveIndex();
            GameObject slot = listener.getGameObject();
            List<MatrixDriveInventory> matrixDriveInventoryList = driveInventories[matrixDrive];
            MatrixDriveInventory matrixDriveInventory = matrixDriveInventoryList[driveIndex];
            ItemSlot itemSlot = matrixDriveInventory.inventories[n];
            List<ItemSlot> inventory = matrixDriveInventoryList[driveIndex].inventories;
            
            if (grabbedSlot == null || grabbedSlot.itemObject == null && (inventorySlot != null || inventorySlot.itemObject != null)) {
                
                //takeFromInventory(inventorySlot,slot,inventory,n,grabbedItemProperties);
                return;
            }
            ItemState itemState = ItemState.Solid;
            if (itemSlot != null && itemSlot.itemObject != null && itemSlot.itemObject is IStateItem stateItem) {
                itemState = stateItem.getItemState();
            }
            if (itemState == ItemState.Fluid && grabbedSlot.itemObject is IFluidContainer fluidContainer && grabbedSlot.tags != null && grabbedSlot.tags.Dict.ContainsKey(ItemTag.FluidContainer)) {
                FluidContainerHelper.handleClick(grabbedItemProperties,inventory,n);
                return;
            }
            controller.sendItem(inventorySlot);
            */
        }

        

        private void takeFromInventory(ItemSlot inventorySlot, GameObject slot, List<ItemSlot> inventory, int n, GrabbedItemProperties grabbedItemProperties) {
            int size = Mathf.Min(Global.MaxSize,inventorySlot.amount);
            inventorySlot.amount -= size;
            grabbedItemProperties.itemSlot = new ItemSlot(inventorySlot.itemObject,size,inventorySlot.tags);
            grabbedItemProperties.updateSprite();
            if (inventorySlot.amount <= 0) {
                ItemSlotUIFactory.unload(slot.transform);
                ItemSlotUIFactory.load(inventory[n],slot.transform);
            } else {
                ItemSlotUIFactory.reload(slot,inventorySlot);
            }
        }

        private void giveToInventory(ItemSlot grabbedSlot, GrabbedItemProperties grabbedItemProperties) {
            /*
            foreach (KeyValuePair<MatrixDrive,List<MatrixDriveInventory>> kvp in driveInventories) {
                for (int driveIndex = 0; driveIndex < kvp.Value.Count; driveIndex++) {
                    List<ItemSlot> matrixDriveInventory = kvp.Value[driveIndex].inventories;
                    for (int i = 0; i < matrixDriveInventory.Count; i++) {
                        if (grabbedSlot.amount <= 0) {
                            grabbedItemProperties.updateSprite();
                            return;
                        }
                        ItemSlot itemSlot = matrixDriveInventory[i];
                        if (itemSlot == null || itemSlot.itemObject == null) {
                            continue;
                        }
                        if (!ItemSlotFactory.Equals(grabbedSlot,itemSlot)) {
                            continue;
                        }
                        ItemSlotHelper.insertIntoSlot(itemSlot,grabbedSlot,kvp.Value[driveIndex].maxSize);
                        //ItemSlotUIFactory.reload()
                    }
                }
            }
            // Has only gotten here if grabbed slot amount is greater than 0
            foreach (KeyValuePair<MatrixDrive,List<MatrixDriveInventory>> kvp in driveInventories) {
                for (int driveIndex = 0; driveIndex < kvp.Value.Count; driveIndex++) {
                    List<ItemSlot> matrixDriveInventory = kvp.Value[driveIndex].inventories;
                    for (int i = 0; i < matrixDriveInventory.Count; i++) {
                        ItemSlot itemSlot = matrixDriveInventory[i];
                        if (itemSlot != null && itemSlot.itemObject != null) {
                            continue;
                        }
                        matrixDriveInventory[i] = grabbedSlot;
                        grabbedItemProperties.itemSlot = null;
                        return;
                    }
                }
            }          
            */
        }

        public void itemMiddleClick(IItemSlotUIElement slotUIElement)
        {
            
        }

        public void itemRightClick(IItemSlotUIElement slotUIElement)
        {
            
        }

        public void rightClickRecipe(int n)
        {
            
        }
    }
}

