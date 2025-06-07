using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using Item.Slot;
using Items.Inventory;
using LibNoise.Operator;
using Recipe.Data;
using Recipe.Objects.Restrictions;
using Recipe.Processor;
using TileEntity;
using TileEntity.Instances.Machine.UI;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace TileEntity.Instances.Machine.Instances
{
    public abstract class MachineInstance<TMachine, TRecipe> : TileEntityInstance<TMachine>, ITickableTileEntity, 
        IStopPlayerRightClickableTileEntity, ISerializableTileEntity, IConduitPortTileEntityAggregator, ISignalConduitInteractable, IMachineInstance,
        IBluePrintPlaceInitializedTileEntity, IRecipeRestrictionTileEntity, IDropItemsOnBreakTileEntity
        where TMachine : MachineObject where TRecipe : ItemRecipe
    {
        protected TRecipe currentRecipe;
        public int Mode;
        public int Restrictions;
        public MachineItemInventory Inventory;
        public MachineEnergyInventory MachineEnergyInventory;
        
        protected MachineInstance(TMachine tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        protected void InitializeItemInventory()
        {
            if (Inventory != null) return;
            var layoutObject = GetMachineLayout();
            Inventory = new MachineItemInventory(this, TileEntityInventoryFactory.Initialize(layoutObject));
        }

        protected void InitializeEnergyInventory()
        {
            if (MachineEnergyInventory != null) return;
            MachineEnergyInventory = new MachineEnergyInventory(0, this);
        }

        public abstract void TickUpdate();
        
        public virtual void OnRightClick()
        {
            GameObject uiPrefab = TileEntityObject.RecipeProcessor.UIPrefab;
            GameObject ui = Object.Instantiate(uiPrefab);
            MachineBaseUI machineUI = ui.GetComponent<MachineBaseUI>();
            machineUI.DisplayTileEntityInstance(this);
            MainCanvasController.TInstance.DisplayUIWithPlayerInventory(ui);
        }

        public abstract string Serialize();

        public abstract void Unserialize(string data);

        
        public IConduitInteractable GetConduitInteractable(ConduitType conduitType)
        {
            switch (conduitType)
            {
                case ConduitType.Energy:
                    return MachineEnergyInventory.GetEnergyConduitInteractable();
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

        public abstract void InventoryUpdate();

        public abstract float GetProgressPercent();
        public TileEntityLayoutObject GetMachineLayout()
        {
            RecipeProcessor recipeProcessor = tileEntityObject.RecipeProcessor;
            if (!ReferenceEquals(recipeProcessor, null)) return recipeProcessor.LayoutObject;
            Debug.LogWarning($"Tried to initialize inventory of MachineInstance with null recipe processor: {tileEntityObject.name}");
            return null;
        }

        public MachineItemInventory GetItemInventory()
        {
            return Inventory;
        }

        public MachineEnergyInventory GetEnergyInventory()
        {
            return MachineEnergyInventory;
        }

        public void SetMode(int mode)
        {
            this.Mode = mode;
        }

        public int GetMode()
        {
            return Mode;
        }

        public void IterateMode(int amount)
        {
            Mode += amount;
        }

        public int GetModeCount()
        {
            RecipeProcessor processor = tileEntityObject.RecipeProcessor;
            if (!processor) return 0;
            return 0; // TODO
        }

        public void ResetRecipe()
        {
            currentRecipe = null;
        }

        public void EnergyInsertUpdate()
        {
            // May need this later
        }

        public abstract void PlaceInitialize();
        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitPortLayout;
        }

        public void AddPassRestriction(BooleanRecipeRestriction recipeRestriction)
        {
            bool passes = (Restrictions & (int) recipeRestriction) != 0;
            if (passes) return;
            Restrictions += (int)recipeRestriction;
        }

        public void RemoveRestriction(BooleanRecipeRestriction recipeRestriction)
        {
            bool passes = (Restrictions & (int) recipeRestriction) != 0;
            if (!passes) return;
            Restrictions -= (int)recipeRestriction;
        }

        public virtual List<ItemSlot> GetDroppableItems()
        {
            return Inventory.Content.GetDroppableItems();
        }
    }
}
