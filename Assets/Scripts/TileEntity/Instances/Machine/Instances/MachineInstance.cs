using Chunks;
using Conduits.Ports;
using Items.Inventory;
using LibNoise.Operator;
using Recipe.Data;
using Recipe.Processor;
using TileEntity;
using TileEntity.Instances.Machine.UI;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace TileEntity.Instances.Machine.Instances
{
    public abstract class MachineInstance<TMachine, TRecipe> : TileEntityInstance<TMachine>, ITickableTileEntity, 
        IRightClickableTileEntity, ISerializableTileEntity, IConduitTileEntityAggregator, ISignalConduitInteractable, IInventoryListener, IMachineInstance
        where TMachine : MachineObject where TRecipe : ItemRecipe
    {
        protected TRecipe currentRecipe;
        public int Mode;
        public MachineItemInventory Inventory;
        public MachineEnergyInventory EnergyInventory;
        
        protected MachineInstance(TMachine tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        protected void InitializeItemInventory()
        {
            if (Inventory != null) return;
            var layoutObject = GetMachineLayout();
            if (layoutObject == null) return;
            Inventory = MachineInventoryFactory.InitializeItemInventory(this, layoutObject);
        }

        protected void InitializeEnergyInventory()
        {
            if (EnergyInventory != null) return;
            EnergyInventory = new MachineEnergyInventory(0, this);
        }

        public abstract void tickUpdate();
        
        public void onRightClick()
        {
            GameObject uiPrefab = TileEntityObject.RecipeProcessor.UIPrefab;
            GameObject ui = Object.Instantiate(uiPrefab);
            MachineBaseUI machineUI = ui.GetComponent<MachineBaseUI>();
            machineUI.DisplayTileEntityInstance(this);
            MainCanvasController.TInstance.DisplayUIWithPlayerInventory(ui);
        }

        public abstract string serialize();

        public abstract void unserialize(string data);

        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitPortLayout;
        }

        public IConduitInteractable GetConduitInteractable(ConduitType conduitType)
        {
            switch (conduitType)
            {
                case ConduitType.Energy:
                    return EnergyInventory;
                case ConduitType.Item:
                case ConduitType.Fluid:
                    return Inventory;
                case ConduitType.Signal:
                    return this;
                default:
                    return null;
            }
        }

        public bool ExtractSignal(Vector2Int portPosition)
        {
            return false;
        }

        public void InsertSignal(bool active, Vector2Int portPosition)
        {
            
        }

        public abstract void InventoryUpdate(int n);

        public abstract float GetProgressPercent();
        public MachineLayoutObject GetMachineLayout()
        {
            MachineRecipeProcessor recipeProcessor = tileEntityObject.RecipeProcessor;
            if (recipeProcessor == null)
            {
                Debug.LogWarning($"Tried to initialize inventory of MachineInstance with null recipe processor: {tileEntityObject.name}");
                return null;
            }
            return recipeProcessor.MachineLayout;
        }
    }
}
