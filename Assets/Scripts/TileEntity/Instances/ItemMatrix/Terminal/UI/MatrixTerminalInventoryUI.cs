using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule.Tags.Matrix;
using ItemModule;
using ItemModule.Tags;
using ItemModule.Tags.FluidContainers;

namespace TileEntityModule.Instances.Matrix {
    public interface IMatrixTerminalItemClickReciever {
        public void itemLeftClick(IMatrixTerminalItemSlotClickListener listener);
        public void itemRightClick(IMatrixTerminalItemSlotClickListener listener);
        public void itemMiddleClick(IMatrixTerminalItemSlotClickListener listener);
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
        private Dictionary<string, Dictionary<ItemTagKey, (ItemSlot,GameObject)>> idTagItemSlotDict;
        public void init(ItemMatrixController controller, List<EncodedRecipeItem> recipes, Transform itemContainer) {
            this.controller = controller;
            this.itemContainer = itemContainer;
            matrixDriveCollection = controller.getEntireDriveCollection();
            toRebuild = matrixDriveCollection.getQueueOfDrives();
            driveInventoryToRebuild = new Queue<MatrixDriveInventory>();
            GlobalHelper.deleteAllChildren(itemContainer);
            idTagItemSlotDict = new Dictionary<string, Dictionary<ItemTagKey, (ItemSlot, GameObject)>>();
            foreach (KeyValuePair<MatrixDrive,List<MatrixDriveInventory>> kvp in matrixDriveCollection.DriveInventories) {
                for (int driveIndex = 0; driveIndex < kvp.Value.Count; driveIndex++) {
                    List<ItemSlot> matrixDriveInventory = kvp.Value[driveIndex].inventories;
                    for (int i = 0; i < matrixDriveInventory.Count; i++) {
                        ItemSlot itemSlot = matrixDriveInventory[i];
                        if (itemSlot == null || itemSlot.itemObject == null) {
                            continue;
                        }
                        if (!idTagItemSlotDict.ContainsKey(itemSlot.itemObject.id)) {
                            idTagItemSlotDict[itemSlot.itemObject.id] = new Dictionary<ItemTagKey, (ItemSlot,GameObject)>();
                        }
                        ItemTagKey tagKey = new ItemTagKey(itemSlot.tags);
                        if (idTagItemSlotDict[itemSlot.itemObject.id].ContainsKey(tagKey)) {
                            idTagItemSlotDict[itemSlot.itemObject.id][tagKey].Item1.amount += itemSlot.amount;
                        } else {
                            idTagItemSlotDict[itemSlot.itemObject.id][tagKey] = (ItemSlotFactory.copy(itemSlot),null);
                        }
                    }
                }
            }
            
            foreach (EncodedRecipeItem encodedRecipeItem in recipes) {
                
            }
            foreach (Dictionary<ItemTagKey, (ItemSlot, GameObject)> tagKeyDict in idTagItemSlotDict.Values) {
                foreach ((ItemSlot, GameObject) itemSlotGameObject in tagKeyDict.Values) {
                    MatrixTerminalItemSlotUI slot = MatrixTerminalItemSlotUI.newInstance(itemSlotGameObject.Item1,this);
                    ItemSlotUIFactory.load(itemSlotGameObject.Item1,slot.transform);
                    slot.transform.SetParent(itemContainer,false);
                }               
            }
        }

        public void FixedUpdate() {
            if (toRebuild.Count == 0) {
                matrixDriveCollection = controller.getEntireDriveCollection();
                toRebuild = matrixDriveCollection.getQueueOfDrives();
                return;
            }
            if (driveInventoryToRebuild.Count == 0) {
                (MatrixDrive,Queue<MatrixDriveInventory>) drive = toRebuild.Dequeue();
                driveInventoryToRebuild = drive.Item2;
                return;
            }
            MatrixDriveInventory matrixDriveInventory = driveInventoryToRebuild.Dequeue();
            foreach (ItemSlot itemSlot in matrixDriveInventory.inventories) {
                if (itemSlot == null || itemSlot.itemObject == null) {
                    continue;
                }
                
                ItemTagKey key = new ItemTagKey(itemSlot.tags);
                (ItemSlot, GameObject) value = idTagItemSlotDict[itemSlot.itemObject.id][key];
                value.Item1 = itemSlot;
                idTagItemSlotDict[itemSlot.itemObject.id][key] = value;
                GameObject slot = value.Item2;
                print("Reloaded " + value.Item1.amount);
                ItemSlotUIFactory.reload(slot,value.Item1);
            }

        }

        public void itemLeftClick(IMatrixTerminalItemSlotClickListener listener)
        {
            /*
            MatrixDrive matrixDrive = listener.getMatrixDrive();
            int n = listener.getIndex();
            int driveIndex = listener.getDriveIndex();
            GameObject slot = listener.getGameObject();
            List<MatrixDriveInventory> matrixDriveInventoryList = driveInventories[matrixDrive];
            MatrixDriveInventory matrixDriveInventory = matrixDriveInventoryList[driveIndex];
            ItemSlot itemSlot = matrixDriveInventory.inventories[n];
            List<ItemSlot> inventory = matrixDriveInventoryList[driveIndex].inventories;
            GameObject grabbedItem = GameObject.Find("GrabbedItem");
            GrabbedItemProperties grabbedItemProperties = grabbedItem.GetComponent<GrabbedItemProperties>();
            ItemSlot inventorySlot = inventory[n];
            ItemSlot grabbedSlot = grabbedItemProperties.itemSlot;
            if (grabbedItem == null) {
                Debug.LogError("Inventory GrabbedItem is null");
            }
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

        public void itemMiddleClick(IMatrixTerminalItemSlotClickListener listener)
        {
            
        }

        public void itemRightClick(IMatrixTerminalItemSlotClickListener listener)
        {
            
        }

        public void rightClickRecipe(int n)
        {
            
        }
    }
}

