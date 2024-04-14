using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule.Tags.Matrix;
using ItemModule;
using ItemModule.Inventory;
using ItemModule.Tags;
using ItemModule.Tags.FluidContainers;
using System.Linq;
using PlayerModule;
using DimensionModule;
using ChunkModule;

namespace TileEntityModule.Instances.Matrix {
    public interface IMatrixTerminalItemClickReciever {
        public void itemLeftClick(IItemSlotUIElement listener);
        public void itemRightClick(IItemSlotUIElement listener);
        public void itemMiddleClick(IItemSlotUIElement listener);
    } 
    public class MatrixTerminalInventoryUI : MonoBehaviour, IMatrixTerminalItemClickReciever
    {
        private ItemMatrixController controller;
        private Transform itemContainer;
        private MatrixDriveCollection matrixDriveCollection;
        private Queue<(MatrixDrive, Queue<MatrixDriveInventory>)> toRebuild;
        private Queue<MatrixDriveInventory> driveInventoryToRebuild;
        private int driveRebuildIndex;
        private Dictionary<string, Dictionary<ItemTagKey, (ItemSlot,MatrixTerminalItemSlotUI,EncodedRecipe)>> idTagItemSlotDict;
        private MatrixTerminalUI matrixTerminalUI;
        public void init(ItemMatrixController controller, Transform itemContainer, MatrixTerminalUI matrixTerminalUI) {
            this.controller = controller;
            this.itemContainer = itemContainer;
            this.matrixTerminalUI = matrixTerminalUI;
            matrixDriveCollection = controller.getEntireDriveCollection();
            toRebuild = matrixDriveCollection.getQueueOfDrives();
            driveInventoryToRebuild = new Queue<MatrixDriveInventory>();
            GlobalHelper.deleteAllChildren(itemContainer);
            idTagItemSlotDict = new Dictionary<string, Dictionary<ItemTagKey, (ItemSlot, MatrixTerminalItemSlotUI, EncodedRecipe)>>();
            buildDict();
            putRecipesInDict();
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
                (ItemSlot, MatrixTerminalItemSlotUI, EncodedRecipe) tuple = idTagItemSlotDict[itemSlot.itemObject.id][tagKey];
                if (tuple.Item1 == null || tuple.Item1.itemObject == null) {
                    tuple.Item1 = ItemSlotFactory.copy(itemSlot);
                } else {
                    tuple.Item1.amount += itemSlot.amount;
                }
            } else {
                idTagItemSlotDict[itemSlot.itemObject.id][tagKey] = (ItemSlotFactory.copy(itemSlot),null,null);
            }
        }
        private void createItemSlotGameObject((ItemSlot,MatrixTerminalItemSlotUI,EncodedRecipe) tuple, string id, ItemTagKey tagKey) {
            ItemSlot itemSlot = tuple.Item1;
            MatrixTerminalItemSlotUI itemSlotUI = tuple.Item2;
            EncodedRecipe encodedRecipe = tuple.Item3;
            if (tuple.Item2 != null) {
                return;
            }
            ItemSlot toDisplay = null;
            bool recipeOnly = false;
            if (itemSlot == null || itemSlot.itemObject == null) {
                if (encodedRecipe == null) {
                    return;
                }
                ItemSlot outputItem = encodedRecipe.getOutput(id,tagKey);
                if (outputItem == null) {
                    return;
                }
                recipeOnly = true;
                outputItem.amount = 1;
                toDisplay = outputItem;
            } else {
                if (tuple.Item1.amount <= 0) {
                    return;
                }
                toDisplay = itemSlot;
            }
            MatrixTerminalItemSlotUI slot = MatrixTerminalItemSlotUI.newInstance(toDisplay, this);
            ItemSlotUIFactory.load(toDisplay, slot.transform);
            if (recipeOnly) {
                slot.showCraftText();
            }
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

            
        }

        private void putRecipesInDict() {
            foreach ((string, ItemTagKey, EncodedRecipe) itemSlotRecipe in controller.Recipes.toList()) {
                string id = itemSlotRecipe.Item1;
                ItemTagKey tagKey = itemSlotRecipe.Item2;
                EncodedRecipe encodedRecipe = itemSlotRecipe.Item3;
                if (itemInDict(id,tagKey)) {
                    (ItemSlot,MatrixTerminalItemSlotUI,EncodedRecipe) tuple = idTagItemSlotDict[id][tagKey];
                    tuple.Item3 = encodedRecipe;
                    idTagItemSlotDict[id][tagKey] = tuple;
                } else {
                    if (!idTagItemSlotDict.ContainsKey(id)) {
                        idTagItemSlotDict[id] = new Dictionary<ItemTagKey, (ItemSlot, MatrixTerminalItemSlotUI, EncodedRecipe)>();
                    }
                    idTagItemSlotDict[id][tagKey] = (null,null,encodedRecipe); 
                }
            }
        }

        public void rebuildDict() {
            // Reset amounts
            foreach (KeyValuePair<string, Dictionary<ItemTagKey, (ItemSlot,MatrixTerminalItemSlotUI,EncodedRecipe)>> idDictKVP in idTagItemSlotDict) {
                foreach (KeyValuePair<ItemTagKey, (ItemSlot,MatrixTerminalItemSlotUI,EncodedRecipe)> kvp in idDictKVP.Value) {
                    if (kvp.Value.Item1 == null) {
                        continue;
                    }
                    kvp.Value.Item1.amount = 0;
                }
            }
            // Rebuild amounts
            buildDict();
            
            foreach (KeyValuePair<string, Dictionary<ItemTagKey, (ItemSlot,MatrixTerminalItemSlotUI,EncodedRecipe)>> idDictKVP in idTagItemSlotDict) {
                foreach (KeyValuePair<ItemTagKey, (ItemSlot,MatrixTerminalItemSlotUI,EncodedRecipe)> kvp in idDictKVP.Value) {
                    (ItemSlot, MatrixTerminalItemSlotUI, EncodedRecipe) value = kvp.Value;
                    handleDestruction(kvp.Value);
                }
            }
            createItemSlotGameObjects();
        }



        private bool handleDestruction((ItemSlot, MatrixTerminalItemSlotUI, EncodedRecipe) tuple) {
            // Destroy items with no amount, and no recipe
            ItemSlot itemSlot = tuple.Item1;
            MatrixTerminalItemSlotUI matrixTerminalItemSlotUI = tuple.Item2;
            EncodedRecipe encodedRecipe = tuple.Item3;
            if (matrixTerminalItemSlotUI == null) {
                return true;
            }
            if (itemSlot == null || itemSlot.itemObject == null || itemSlot.amount == 0) {
                if (encodedRecipe == null) {
                    GameObject.Destroy(matrixTerminalItemSlotUI.gameObject);
                    return true;
                } else {
                    matrixTerminalItemSlotUI.showCraftText();
                }
            }
            return false; 
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

        private void updateSlotOnRemoval(ItemSlot inventorySlot,string id, ItemTagKey itemTagKey, GrabbedItemProperties grabbedItemProperties, IItemSlotUIElement slotUIElement) {
            (ItemSlot, MatrixTerminalItemSlotUI, EncodedRecipe) tuple = idTagItemSlotDict[inventorySlot.itemObject.id][itemTagKey];
            bool destroyed = handleDestruction(tuple);
            if (!destroyed && inventorySlot.amount != 0) {
                slotUIElement.reload(inventorySlot);   
            } 
            sortItemSlots();
            grabbedItemProperties.updateSprite();
        }

        private bool insertItemIntoSystemFromTerminal(ItemSlot itemSlot, GrabbedItemProperties grabbedItemProperties) {
            int preAmount = itemSlot.amount;
            ItemSlot grabbedItemCopy = ItemSlotFactory.copy(itemSlot);
            controller.sendItem(itemSlot);
            grabbedItemCopy.amount -= itemSlot.amount;
            if (grabbedItemCopy.amount == 0) { // Was not inserted
                return false;
            }
            
            string grabbedSlotId = grabbedItemCopy.itemObject.id;
            ItemTagKey grabbedSlotTagKey = new ItemTagKey(grabbedItemCopy.tags); 
            loadItemIntoDict(grabbedItemCopy);
            if (!itemInDict(grabbedSlotId,grabbedSlotTagKey)) {
                Debug.LogWarning("Item was inserted into system but was not in itemDict");
                return false;
            }
            (ItemSlot, MatrixTerminalItemSlotUI, EncodedRecipe) tuple = idTagItemSlotDict[grabbedSlotId][grabbedSlotTagKey];
            if (tuple.Item2 == null) {
                createItemSlotGameObject(tuple,grabbedSlotId,grabbedSlotTagKey);
            } else {
                ((IItemSlotUIElement)tuple.Item2).reload(tuple.Item1,force:true);
            }
            grabbedItemProperties.updateSprite();
            sortItemSlots();
            return true;
        }

        private void giveAmountToGrabbedItem(ItemSlot inventorySlot, GrabbedItemProperties grabbedItemProperties, IItemSlotUIElement slotUIElement, int amount) {
            if (inventorySlot == null || inventorySlot.itemObject == null) {
                return;
            }
            if (inventorySlot.itemObject is IStateItem stateItem && stateItem.getItemState() != ItemState.Solid) {
                return;
            }
            
            ItemTagKey itemTagKey = new ItemTagKey(inventorySlot.tags);
            grabbedItemProperties.itemSlot = matrixDriveCollection.take(inventorySlot.itemObject.id,itemTagKey,amount);
            if (grabbedItemProperties.itemSlot == null || grabbedItemProperties.itemSlot.itemObject == null) {
                return;
            }
            inventorySlot.amount -= grabbedItemProperties.itemSlot.amount;
            updateSlotOnRemoval(inventorySlot,inventorySlot.itemObject.id,itemTagKey,grabbedItemProperties,slotUIElement);
        }

        private bool handleCraftClick(ItemSlot inventorySlot) {
            string id = inventorySlot.itemObject.id;
            ItemTagKey tagKey = new ItemTagKey(inventorySlot.tags);
            if (!itemInDict(id,tagKey)) {
                return false;
            }
            (ItemSlot, MatrixTerminalItemSlotUI, EncodedRecipe) tuple = idTagItemSlotDict[id][tagKey];
            EncodedRecipe encodedRecipe = tuple.Item3;
            if (encodedRecipe == null) {
                return false;
            }
            ItemSlot itemSlot = tuple.Item1;
            bool canNavigateToCraft = itemSlot == null || itemSlot.itemObject == null || Input.GetKeyDown(KeyCode.LeftShift);
            if (!canNavigateToCraft) {
                return false;
            }
            CraftAmountPopUpUI craftAmountPopUpUI = CraftAmountPopUpUI.newInstance();
            craftAmountPopUpUI.init(controller,inventorySlot,encodedRecipe);
            GlobalUIContainer.getInstance().getUiController().addGUI(craftAmountPopUpUI.gameObject);
            return true;
        }
        private bool fluidCellClick(GrabbedItemProperties grabbedItemProperties, ItemSlot inventorySlot, IItemSlotUIElement slotUIElement) {
            
            
            ItemSlot grabbedSlot = grabbedItemProperties.itemSlot;
            if (grabbedSlot.itemObject is not IFluidContainer fluidContainer || grabbedSlot.tags == null || !grabbedSlot.tags.Dict.ContainsKey(ItemTag.FluidContainer)) {
                return false;
            }
            object tagData = grabbedSlot.tags.Dict[ItemTag.FluidContainer];
            if (tagData == null) {
                if (inventorySlot == null || inventorySlot.itemObject == null || inventorySlot.itemObject is not IStateItem stateItem) {
                    return false;
                }
                ItemState itemState = stateItem.getItemState();
                if (itemState != ItemState.Fluid) {
                    return false;
                }
                string id = inventorySlot.itemObject.id;
                ItemTagKey itemTagKey = new ItemTagKey(inventorySlot.tags);
                ItemSlot extracted = controller.takeItem(id,itemTagKey,fluidContainer.getStorage());
                if (extracted.amount == 0) {
                    return false;
                }
                ItemSlot spliced = ItemSlotFactory.deepSlice(grabbedSlot,1);
                spliced.tags.Dict[ItemTag.FluidContainer] = extracted;
                PlayerInventory playerInventory = PlayerContainer.getInstance().getInventory();
                playerInventory.give(spliced);
                updateSlotOnRemoval(inventorySlot,inventorySlot.itemObject.id,itemTagKey,grabbedItemProperties,slotUIElement);
                return true;
            }
            if (!Input.GetKey(KeyCode.LeftShift)) { // User must be holding shift to send fluid contents into system
                return false;
            }
            if (tagData is not ItemSlot fluidCellContent) {
                return false;
            }
            insertItemIntoSystemFromTerminal(fluidCellContent,grabbedItemProperties);
            if (fluidCellContent == null || fluidCellContent.amount == 0 || fluidCellContent.itemObject == null) {
                
            }
            return true;
        }
        public void itemLeftClick(IItemSlotUIElement slotUIElement)
        {
            GrabbedItemProperties grabbedItemProperties = GrabbedItemContainer.getGrabbedItem();
            ItemSlot grabbedSlot = grabbedItemProperties.itemSlot;
            ItemSlot inventorySlot = slotUIElement.getItemSlot();
            
            if (grabbedSlot == null || grabbedSlot.itemObject == null) {
                if (inventorySlot != null && inventorySlot.itemObject != null) {
                    bool inCraftMenu = handleCraftClick(inventorySlot);
                    if (inCraftMenu) {
                        return;
                    }
                } 
                giveAmountToGrabbedItem(inventorySlot,grabbedItemProperties,slotUIElement,Global.MaxSize);
            } else {
                bool fluidClick = fluidCellClick(grabbedItemProperties,inventorySlot,slotUIElement);
                if (fluidClick) {
                    return;
                }
                insertItemIntoSystemFromTerminal(grabbedSlot,grabbedItemProperties);
            }

        }
        public void itemMiddleClick(IItemSlotUIElement slotUIElement)
        {
            // Maybe add this later not sure
        }

        public void itemRightClick(IItemSlotUIElement slotUIElement)
        {
            GrabbedItemProperties grabbedItemProperties = GrabbedItemContainer.getGrabbedItem();
            ItemSlot grabbedSlot = grabbedItemProperties.itemSlot;
            ItemSlot inventorySlot = slotUIElement.getItemSlot();
            if (grabbedSlot == null || grabbedSlot.itemObject == null || ItemSlotHelper.areEqual(grabbedSlot,inventorySlot)) {
                giveAmountToGrabbedItem(inventorySlot,grabbedItemProperties,slotUIElement,1);
            } else {
                ItemSlot spliced = ItemSlotFactory.splice(grabbedSlot,1);
                insertItemIntoSystemFromTerminal(spliced,grabbedItemProperties);
                if (spliced.amount == 0) {
                    grabbedItemProperties.itemSlot.amount--;
                }
                if (grabbedItemProperties.itemSlot.amount == 0) {
                    grabbedItemProperties.itemSlot = null;
                    grabbedItemProperties.updateSprite();
                }
            }
        }
    }
}

